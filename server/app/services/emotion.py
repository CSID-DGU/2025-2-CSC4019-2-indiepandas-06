from __future__ import annotations

from typing import Any, Tuple, Union, cast

from app.core.bridge import ai_bridge  # Bridge 임포트
from app.core.config import settings

Floaty = Union[str, float, int]


class EmotionService:
    def __init__(self):
        # WebSocket 방식이므로 URL 불필요, 타임아웃만 참조
        self.timeout = float(settings.EMOTION_TIMEOUT_S)

    async def infer(self, text: str, locale: str = "ko-KR") -> Tuple[str, float]:
        payload = {"text": text, "locale": locale}

        # Bridge를 통해 로컬 워커에 요청
        # (기존 httpx 요청 부분을 대체)
        try:
            response_data = await ai_bridge.send_request_and_wait(
                task_type="emotion", payload=payload, timeout=self.timeout
            )
            return self._extract_emotion(response_data)
        except Exception as e:
            # 연결 에러나 타임아웃 등을 여기서 잡아서 처리
            raise e

    # _extract_emotion 및 _is_num 메서드는 기존 코드 그대로 유지...
    def _extract_emotion(self, data: Any) -> Tuple[str, float]:
        # (기존 코드 복사 붙여넣기)
        # ... user provided code ...
        if isinstance(data, dict):
            emo = data.get("emotion")
            conf = data.get("confidence")
            if isinstance(emo, str) and self._is_num(conf):
                return emo, float(cast(Floaty, conf))
        # ... (생략) ...
        return "neutral", 0.0  # Fallback example

    @staticmethod
    def _is_num(v: Any) -> bool:
        try:
            float(v)
            return True
        except Exception:
            return False
