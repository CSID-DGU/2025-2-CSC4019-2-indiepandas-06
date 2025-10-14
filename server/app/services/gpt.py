from __future__ import annotations

import asyncio
import json
import re
from typing import Any, Dict, Tuple

import httpx

from app.core.config import settings

# ----------------------------
# 프롬프트 (시스템)
# ----------------------------
SYSTEM_PROMPT = """\
당신은 모바일 RPG의 NPC입니다. 플레이어의 감정 상태와 게임 맥락을 고려해,
짧고 자연스러운 한국어 대사를 '한 줄'만 생성하세요.

제약:
- 존댓말(반말 금지), 1인칭으로 자신을 과하게 강조하지 마세요.
- 공격적/차별적/선정적 표현 금지.
- 이모지 남발 금지(필요하면 0~1개).
- 60자 이내.
- 출력은 대사 '한 줄'만. 설명/접두사/코드블록 금지.
"""


# ----------------------------
# 사용자 프롬프트 빌더
# ----------------------------
def build_user_prompt(
    *,
    dialog_text: str,
    emotion: str,
    persona: str,
    game_state: dict | None,
    locale: str = "ko-KR",
) -> str:
    state = (
        f"게임상태: {json.dumps(game_state, ensure_ascii=False)}"
        if game_state
        else "게임상태: 없음"
    )
    return (
        f"[플레이어 입력]\n{dialog_text}\n\n"
        f"[추론된 감정]\n{emotion}\n\n"
        f"[NPC 페르소나]\n{persona}\n\n"
        f"{state}\n\n"
        f"[요청]\n위 조건을 지켜 NPC의 대사 한 줄만 출력하세요."
    )


# ----------------------------
# 간단 안전 필터 / 후처리
# ----------------------------
_PROFANITY = re.compile(
    r"(개새|씨발|좆|병신|꺼져|fuck|shit|bitch|asshole|retard)",
    re.IGNORECASE,
)


def _postprocess(line: str) -> str:
    # 개행/따옴표/여분 공백 제거
    line = line.strip().strip("`\"'").replace("\n", " ").strip()
    # 너무 긴 경우 절단(멀티바이트 안전)
    if len(line) > 60:
        line = line[:60].rstrip() + "…"
    # 마침표/문장부호 보정(선택)
    return line


def _safety_check(line: str) -> Dict[str, Any]:
    bad = bool(_PROFANITY.search(line))
    return {
        "profanity": bad,
        "blocked": bad,  # 간단 규칙: 비속어 포함 시 차단
        "length_over": len(line) > 60,
    }


def _fallback_line(emotion: str, persona: str) -> str:
    # 감정별 간단 폴백 템플릿
    emo = (emotion or "").lower()
    if "sad" in emo or "슬픔" in emo:
        return "오늘 정말 수고 많으셨어요. 잠시 쉬어가며 제가 곁에서 도울게요."
    if "ang" in emo or "분노" in emo:
        return "많이 답답하셨겠어요. 천천히 상황을 정리해 함께 해결해봐요."
    if "joy" in emo or "기쁨" in emo or "행복" in emo:
        return "좋은 소식이네요! 그 기운을 이어서 다음 걸음도 함께해요."
    if "fear" in emo or "두려" in emo:
        return "걱정이 크셨겠어요. 안전하니 한 걸음씩 함께 살펴볼게요."
    if "surprise" in emo or "놀" in emo:
        return "예상치 못한 일이었겠군요. 잠시 정리하고 차분히 대응해봐요."
    # 디폴트
    return "지금 마음에 공감해요. 천천히 이야기 나누며 제가 도울게요."


# ----------------------------
# GPT 서비스
# ----------------------------
class GPTService:
    """
    OpenAI 호환 Chat Completions API 호출기.
    - 엔드포인트: settings.GPT_API_URL
    - 모델: settings.GPT_MODEL
    - 키: settings.GPT_API_KEY (Authorization: Bearer)
    - 타임아웃: settings.REQUEST_TIMEOUT_S
    """

    def __init__(
        self, *, api_url: str | None = None, model: str | None = None, timeout: float | None = None
    ):
        self.api_url = api_url or str(settings.GPT_API_URL)
        self.model = model or str(settings.GPT_MODEL)
        self.timeout = timeout or float(settings.REQUEST_TIMEOUT_S)
        self._client = httpx.AsyncClient(
            timeout=self.timeout,
            headers={
                "Authorization": f"Bearer {settings.GPT_API_KEY}",
                "Content-Type": "application/json",
            },
        )

    async def generate_line(
        self,
        *,
        dialog_text: str,
        emotion: str,
        persona: str,
        game_state: dict | None,
        locale: str = "ko-KR",
        temperature: float = 0.7,
        max_tokens: int = 80,
    ) -> Tuple[str, Dict[str, Any]]:
        """
        NPC 대사 한 줄과 안전 정보 반환.
        실패/차단 시 감정 기반 폴백 문구를 제공.
        """
        user_prompt = build_user_prompt(
            dialog_text=dialog_text,
            emotion=emotion,
            persona=persona,
            game_state=game_state,
            locale=locale,
        )

        body = {
            "model": self.model,
            "messages": [
                {"role": "system", "content": SYSTEM_PROMPT},
                {"role": "user", "content": user_prompt},
            ],
            "temperature": temperature,
            "max_tokens": max_tokens,
        }

        # 간단 재시도(네트워크/5xx): 2회
        attempts = 3
        backoff = 0.25
        last_error: Exception | None = None

        for i in range(attempts):
            try:
                resp = await self._client.post(self.api_url, json=body)
                resp.raise_for_status()
                data = resp.json()
                line = self._extract_text(data)
                line = _postprocess(line)
                safety = _safety_check(line)

                if safety.get("blocked"):
                    # 폴백 대사로 대체
                    return _fallback_line(emotion, persona), {**safety, "fallback": True}

                return line, {**safety, "fallback": False}
            except (httpx.RequestError, httpx.HTTPStatusError) as e:
                last_error = e
                # 4xx는 재시도하지 않음
                if isinstance(e, httpx.HTTPStatusError) and 400 <= e.response.status_code < 500:
                    break
                if i < attempts - 1:
                    await asyncio.sleep(backoff)
                    backoff *= 2
                else:
                    break

        # 완전 실패 시 폴백
        return _fallback_line(emotion, persona), {
            "error": str(last_error) if last_error else "unknown",
            "fallback": True,
        }

    # ----------------------------
    # 내부: 다양한 OpenAI/호환 응답 스키마 처리
    # ----------------------------
    @staticmethod
    def _extract_text(data: Dict[str, Any]) -> str:
        """
        OpenAI 호환 응답에서 메시지 텍스트를 추출.
        기본: data["choices"][0]["message"]["content"]
        """
        try:
            choices = data.get("choices") or []
            if not choices:
                raise KeyError("choices is empty")
            msg = choices[0].get("message") or {}
            content = msg.get("content")
            if not isinstance(content, str) or not content.strip():
                raise KeyError("empty content")
            return content
        except Exception:
            # 다른 공급자 형태(예: {"output_text": "..."} 등)도 시도
            for k in ("output_text", "text", "answer"):
                v = data.get(k)
                if isinstance(v, str) and v.strip():
                    return v
            raise ValueError(f"Unsupported chat completion schema: {data!r}")

    async def aclose(self) -> None:
        await self._client.aclose()

    async def __aenter__(self) -> "GPTService":
        return self

    async def __aexit__(self, exc_type, exc, tb) -> None:
        await self.aclose()
