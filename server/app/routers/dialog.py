from __future__ import annotations

import time
import uuid
from typing import Annotated

from fastapi import APIRouter, Depends, HTTPException, Request, status

from app.core.config import settings
from app.core.observability import current_request_id, get_logger
from app.core.security import (
    SecurityContext,
    api_key_or_hmac_dependency,
    rate_limit_dependency,
)
from app.schemas import DialogRequest, DialogResponse
from app.services.emotion import EmotionService
from app.services.gpt import GPTService

router = APIRouter(prefix="/v1/dialog", tags=["dialog"])

log = get_logger(__name__)


# ----------------------------
# 의존성 주입 팩토리 (테스트/모킹 용이)
# ----------------------------
def get_emotion_service() -> EmotionService:
    return EmotionService()


def get_gpt_service() -> GPTService:
    return GPTService()


AuthDep = Annotated[SecurityContext, Depends(api_key_or_hmac_dependency)]
RateDep = Annotated[SecurityContext, Depends(rate_limit_dependency)]
EmotionDep = Annotated[EmotionService, Depends(get_emotion_service)]
GptDep = Annotated[GPTService, Depends(get_gpt_service)]


# ----------------------------
# 간단 핑 엔드포인트 (연동/인증 확인용)
# ----------------------------
@router.get("/ping")
async def ping(
    ctx: AuthDep,
    _rl: RateDep,
):
    return {
        "ok": True,
        "auth": ctx.method,
        "rid": current_request_id() or "-",
        "env": settings.ENV,
    }


# ----------------------------
# 핵심: 대화 생성 엔드포인트
# ----------------------------
@router.post(
    "/generate",
    response_model=DialogResponse,
    status_code=status.HTTP_200_OK,
)
async def generate_dialog(
    payload: DialogRequest,
    request: Request,
    ctx: AuthDep,
    _rl: RateDep,
    emotion_svc: EmotionDep,
    gpt_svc: GptDep,
):
    """
    1) 감정 추론 서비스 호출
    2) GPT로 NPC 대사 생성
    3) 감정/대사/메타를 반환
    """
    req_id = current_request_id() or f"req_{uuid.uuid4().hex[:12]}"
    t0 = time.perf_counter()

    # 입력 길이 제한(서버 보호)
    if len(payload.dialog_text) > settings.MAX_INPUT_CHARS:
        raise HTTPException(
            status_code=413,
            detail=f"dialog_text too long (>{settings.MAX_INPUT_CHARS} chars)",
        )

    # 1) 감정 추론
    try:
        emo, conf = await emotion_svc.infer(payload.dialog_text, payload.locale)
    except Exception as e:
        log.exception("emotion service error", extra={"request_id": req_id})
        raise HTTPException(status_code=502, detail=f"Emotion service error: {e}")

    # 2) GPT 대사 생성
    try:
        npc_line, safety = await gpt_svc.generate_line(
            dialog_text=payload.dialog_text,
            emotion=emo,
            persona=payload.npc_persona,
            game_state=payload.game_state,
            locale=payload.locale,
        )
        # 간단 가드: 차단 플래그 시 안전한 대체문구
        if safety.get("blocked"):
            npc_line = "그 주제는 조심스럽게 다뤄야 해요. 다른 이야기도 함께 나눠볼까요?"
    except Exception as e:
        log.exception("gpt service error", extra={"request_id": req_id})
        raise HTTPException(status_code=502, detail=f"GPT service error: {e}")

    latency_ms = int((time.perf_counter() - t0) * 1000)

    # 3) 응답 구성
    resp = DialogResponse(
        emotion=str(emo),
        confidence=float(conf),
        npc_line=str(npc_line),
        safety=dict(safety or {}),
        latency_ms=latency_ms,
        request_id=req_id,
        usage={
            # 실제로는 각 서비스에서 측정값을 받아 채우는 것을 권장
            "emotion_ms": None,
            "gpt_ms": None,
            "model": getattr(settings, "GPT_MODEL", "unknown"),
            "auth": ctx.method,
        },
    )

    log.info(
        "dialog generated",
        extra={
            "request_id": req_id,
            "emotion": resp.emotion,
            "confidence": resp.confidence,
            "latency_ms": latency_ms,
        },
    )
    return resp
