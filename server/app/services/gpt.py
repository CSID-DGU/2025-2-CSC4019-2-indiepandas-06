from __future__ import annotations

import json
import re
from typing import Any, Dict, Tuple

from app.core.bridge import ai_bridge  # Bridge 임포트
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
# GPT 서비스 (Bridge 적용 버전)
# ----------------------------
class GPTService:
    """
    WebSocket Bridge를 통해 로컬 AI 워커에게 생성을 요청하는 서비스.
    """

    def __init__(self):
        # WebSocket 통신이므로 URL은 필요 없으나, 타임아웃 설정은 가져옴
        self.timeout = float(settings.REQUEST_TIMEOUT_S)

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
        1) 서버에서 프롬프트 구성 (로직 일관성 유지)
        2) Bridge를 통해 워커로 전송
        3) 응답 수신 후 후처리 및 안전 검사
        """
        # 1. 프롬프트 생성
        user_prompt = build_user_prompt(
            dialog_text=dialog_text,
            emotion=emotion,
            persona=persona,
            game_state=game_state,
            locale=locale,
        )

        payload = {
            "user_prompt": user_prompt,
            "system_prompt": SYSTEM_PROMPT,
            "temperature": temperature,
            "max_tokens": max_tokens,
        }

        # 2. 로컬 워커 요청 (Bridge)
        try:
            resp_data = await ai_bridge.send_request_and_wait(
                task_type="gpt",
                payload=payload,
                timeout=self.timeout,
            )

            # 3. 결과 파싱 및 후처리
            line = self._extract_text(resp_data)
            line = _postprocess(line)
            safety = _safety_check(line)

            if safety.get("blocked"):
                return _fallback_line(emotion, persona), {**safety, "fallback": True}

            return line, {**safety, "fallback": False}

        except Exception as e:
            # 에러 발생 시 폴백 반환
            return _fallback_line(emotion, persona), {
                "error": str(e),
                "fallback": True,
            }

    @staticmethod
    def _extract_text(data: Dict[str, Any]) -> str:
        """
        워커 응답 데이터에서 실제 텍스트 추출
        """
        return data.get("content", "")
