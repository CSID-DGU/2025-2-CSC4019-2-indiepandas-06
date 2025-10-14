from __future__ import annotations

import asyncio
from typing import Any, Tuple, Union, cast

import httpx

from app.core.config import settings

Floaty = Union[str, float, int]


class EmotionService:
    """
    감정 추론 서비스 클라이언트.

    기대하는 외부 API 형식(예시):
      POST {EMOTION_API_URL}
      body: {"text": "<string>", "locale": "ko-KR"}
      response(예시1): {"emotion": "sadness", "confidence": 0.87}
      response(예시2): {"label": "joy", "score": 0.91}
      response(예시3): {"pred": {"label": "anger", "score": 0.73}}

    - 타임아웃: settings.EMOTION_TIMEOUT_S
    - 재시도: 네트워크/5xx 시 최대 2회, 지수 백오프(0.2s, 0.4s)
    """

    def __init__(self, *, api_url: str | None = None, timeout: float | None = None) -> None:
        self.api_url = api_url or str(settings.EMOTION_API_URL)
        self.timeout = timeout or float(settings.EMOTION_TIMEOUT_S)
        self._client = httpx.AsyncClient(timeout=self.timeout)

    async def infer(self, text: str, locale: str = "ko-KR") -> Tuple[str, float]:
        """
        :param text: 플레이어 대사 텍스트
        :param locale: 로캘(예: 'ko-KR')
        :return: (emotion, confidence)
        :raises httpx.RequestError, httpx.HTTPStatusError
        """
        payload = {"text": text, "locale": locale}

        # 최대 2회 재시도(총 3번 시도)
        attempts = 3
        backoff = 0.2

        last_exc: Exception | None = None
        for i in range(attempts):
            try:
                resp = await self._client.post(self.api_url, json=payload)
                resp.raise_for_status()
                data = resp.json()
                return self._extract_emotion(data)
            except (httpx.RequestError, httpx.HTTPStatusError) as e:
                last_exc = e
                # 4xx는 재시도하지 않음
                if isinstance(e, httpx.HTTPStatusError) and 400 <= e.response.status_code < 500:
                    raise
                if i < attempts - 1:
                    await asyncio.sleep(backoff)
                    backoff *= 2
                else:
                    # 최종 실패
                    raise
        # 논리상 도달하지 않음
        assert last_exc is not None
        raise last_exc  # type: ignore[misc]

    # ----------------------------
    # 내부: 다양한 응답 스키마 정규화
    # ----------------------------
    def _extract_emotion(self, data: Any) -> Tuple[str, float]:
        """
        다양한 키 이름을 허용하여 (emotion, confidence)로 정규화.
        """
        # 평평한 형태
        if isinstance(data, dict):
            # 케이스 1: {"emotion": "...", "confidence": 0.87}
            emo = data.get("emotion")
            conf = data.get("confidence")
            if isinstance(emo, str) and self._is_num(conf):
                return emo, float(cast(Floaty, conf))

            # 케이스 2: {"label": "...", "score": 0.91}
            emo = data.get("label")
            conf = data.get("score")
            if isinstance(emo, str) and self._is_num(conf):
                return emo, float(cast(Floaty, conf))

            # 케이스 3: {"pred": {"label": "...", "score": 0.91}}
            pred = data.get("pred")
            if isinstance(pred, dict):
                emo = pred.get("label") or pred.get("emotion")
                conf = pred.get("score") or pred.get("confidence")
                if isinstance(emo, str) and self._is_num(conf):
                    return emo, float(cast(Floaty, conf))

            # 케이스 4: {"results": [{"label": "...", "score": 0.91}, ...]}
            results = data.get("results")
            if isinstance(results, list) and results:
                first = results[0]
                if isinstance(first, dict):
                    emo = first.get("label") or first.get("emotion")
                    conf = first.get("score") or first.get("confidence")
                    if isinstance(emo, str) and self._is_num(conf):
                        return emo, float(cast(Floaty, conf))

        # 실패 시 명확한 에러
        raise ValueError(f"Unsupported emotion response schema: {data!r}")

    @staticmethod
    def _is_num(v: Any) -> bool:
        try:
            float(v)
            return True
        except Exception:
            return False

    async def aclose(self) -> None:
        await self._client.aclose()

    async def __aenter__(self) -> "EmotionService":
        return self

    async def __aexit__(self, exc_type, exc, tb) -> None:
        await self.aclose()
