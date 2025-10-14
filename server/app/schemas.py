from __future__ import annotations

from typing import Annotated, Any, Dict, Optional

from pydantic import BaseModel, Field

# ------------------------------------
# 타입 Alias (Annotated + Field)
# ------------------------------------
DialogText = Annotated[
    str,
    Field(
        strip_whitespace=True,
        min_length=1,
        max_length=2000,
        description="플레이어가 보낸 대화 텍스트",
    ),
]


# ------------------------------------
# Request Models
# ------------------------------------
class DialogRequest(BaseModel):
    """
    플레이어 입력을 받아 NPC 대사를 생성하기 위한 요청 페이로드.
    """

    player_id: str = Field(..., description="플레이어 식별자")
    session_id: str = Field(..., description="세션 식별자(게임 세션/대화 세션 등)")
    dialog_text: DialogText
    locale: str = Field("ko-KR", description="로캘 (예: 'ko-KR', 'en-US')")
    npc_persona: str = Field("기본 NPC", description="NPC 페르소나/말투/역할")
    game_state: Optional[Dict[str, Any]] = Field(
        default=None,
        description="게임 상태 맥락(퀘스트, 스테이지 등 메타데이터)",
        examples=[{"quest_id": "q10", "stage": 2}],
    )

    model_config = {
        "json_schema_extra": {
            "example": {
                "player_id": "p123",
                "session_id": "s456",
                "dialog_text": "오늘 너무 힘들었어…",
                "locale": "ko-KR",
                "npc_persona": "따뜻한 상담가",
                "game_state": {"quest_id": "q10", "stage": 2},
            }
        }
    }


# ------------------------------------
# Response Models
# ------------------------------------
class SafetyInfo(BaseModel):
    """
    간단한 안전 정보(비속어, 차단 여부 등)
    """

    profanity: bool = Field(False, description="비속어/유해 표현 검출 여부")
    blocked: bool = Field(False, description="응답 차단 여부(차단 시 폴백 문구 반환)")
    length_over: bool | None = Field(None, description="길이 제한 초과 여부(후처리 전 기준)")
    fallback: bool | None = Field(None, description="폴백 문구 사용 여부")
    error: str | None = Field(None, description="모델 호출 실패 메시지(있다면)")


class UsageInfo(BaseModel):
    """
    지연/모델/인증 등 진단 목적 메타데이터
    """

    emotion_ms: int | None = Field(None, description="감정 추론 소요(ms)")
    gpt_ms: int | None = Field(None, description="GPT 호출 소요(ms)")
    model: str | None = Field(None, description="사용된 생성 모델명")
    auth: str | None = Field(None, description="인증 방식(api_key/hmac/none)")


class DialogResponse(BaseModel):
    """
    감정 추론 결과 + GPT 생성 대사를 담아 반환.
    """

    emotion: str = Field(..., description="추론된 감정 레이블 (예: sadness, joy)")
    confidence: float = Field(..., ge=0.0, le=1.0, description="감정 추론 신뢰도 [0,1]")
    npc_line: str = Field(..., description="생성된 NPC 대사(한 줄)")
    safety: SafetyInfo = Field(default_factory=SafetyInfo, description="안전/가드레일 정보")
    latency_ms: int = Field(..., description="총 처리 지연(ms)")
    request_id: str = Field(..., description="요청 식별자(Request ID)")
    usage: UsageInfo = Field(default_factory=UsageInfo, description="진단/메타 정보")

    model_config = {
        "json_schema_extra": {
            "example": {
                "emotion": "sadness",
                "confidence": 0.87,
                "npc_line": "오늘 정말 고생했어요. 잠시 쉬어가며 제가 도와드릴게요.",
                "safety": {"profanity": False, "blocked": False, "fallback": False},
                "latency_ms": 412,
                "request_id": "req_01abc234def5",
                "usage": {"emotion_ms": 95, "gpt_ms": 280, "model": "gpt-4o-mini", "auth": "hmac"},
            }
        }
    }
