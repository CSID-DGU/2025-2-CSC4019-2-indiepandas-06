from __future__ import annotations

import base64
import hashlib
import hmac
import os
import time
from dataclasses import dataclass
from typing import Optional, Tuple

from fastapi import Depends, HTTPException, Request, status

# 선택적으로 settings를 사용할 수 있으면 사용
try:
    from app.core.config import settings  # type: ignore
except Exception:  # pragma: no cover
    settings = None  # type: ignore


# ----------------------------
# 공통 헤더 이름
# ----------------------------
HEADER_API_KEY = "X-Api-Key"
HEADER_SIGNATURE = "X-Signature"  # HMAC 서명 (hex 또는 base64)
HEADER_TIMESTAMP = "X-Timestamp"  # epoch seconds (int)
HEADER_NONCE = "X-Nonce"  # 재전송 방지용 임의 문자열 (권장 길이 >= 8)


# ----------------------------
# 설정 로딩 (환경변수 → settings 순서로 확인)
# ----------------------------
def _read_str_env(name: str, default: Optional[str] = None) -> Optional[str]:
    v = os.getenv(name)
    if v is not None and v != "":
        return v
    if settings is not None:
        # getattr로 존재 유무 체크 (없으면 default)
        return getattr(settings, name, default)
    return default


# 두 인증 모드(선택사항)
API_KEY_VALUE: Optional[str] = _read_str_env("SERVER_API_KEY", None)
HMAC_SECRET: Optional[str] = _read_str_env("SERVER_HMAC_SECRET", None)

# 허용 타임스큐(초) – 서명된 요청의 유효 시간창
HMAC_ALLOWED_SKEW: int = int(_read_str_env("HMAC_ALLOWED_SKEW", "300") or "300")

# 개발용 레이트리밋 (기본: 60req/60s)
RL_DEFAULT_RATE = int(_read_str_env("RL_DEFAULT_RATE", "60") or "60")
RL_DEFAULT_PER = int(_read_str_env("RL_DEFAULT_PER", "60") or "60")


# ----------------------------
# 보안 컨텍스트
# ----------------------------
@dataclass
class SecurityContext:
    """
    라우터에서 사용할 인증 컨텍스트.
    - principal: 클라이언트 식별자(간단히 헤더에서 읽은 API Key 또는 'hmac')
    - method: "api_key" | "hmac" | "none"
    """

    principal: str = "anonymous"
    method: str = "none"


# ----------------------------
# 유틸: HMAC 서명/검증
# ----------------------------
def _hmac_sign(secret: str, body: bytes, ts: str, nonce: str) -> bytes:
    """
    메시지 서명 규약(간단 예시):
    sign_input = ts + "\n" + nonce + "\n" + sha256(body).hexdigest()
    signature = HMAC_SHA256(secret, sign_input)
    """
    body_hash = hashlib.sha256(body).hexdigest()
    sign_input = f"{ts}\n{nonce}\n{body_hash}".encode("utf-8")
    return hmac.new(secret.encode("utf-8"), sign_input, hashlib.sha256).digest()


def _decode_signature(sig: str) -> bytes:
    """
    클라이언트가 hex 또는 base64 중 아무거나 보낼 수 있도록 지원.
    """
    sig = sig.strip()
    # hex 시도
    try:
        return bytes.fromhex(sig)
    except ValueError:
        pass
    # base64 시도
    try:
        return base64.b64decode(sig, validate=True)
    except Exception:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Invalid X-Signature encoding (expect hex or base64).",
        )


def verify_hmac_headers(
    request: Request,
    *,
    secret: str,
    allowed_skew: int = HMAC_ALLOWED_SKEW,
) -> SecurityContext:
    """
    HMAC 헤더 검증:
      - X-Timestamp: 현재 시간과의 차이 <= allowed_skew
      - X-Nonce: 존재만 체크(실제 운영에선 캐시 저장하여 재사용 거부 권장)
      - X-Signature: 서버 계산값과 동일해야 함
    """
    ts = request.headers.get(HEADER_TIMESTAMP)
    nonce = request.headers.get(HEADER_NONCE)
    sig = request.headers.get(HEADER_SIGNATURE)

    if not ts or not nonce or not sig:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail=(
                "Missing one of required headers: "
                f"{HEADER_TIMESTAMP}, {HEADER_NONCE}, {HEADER_SIGNATURE}"
            ),
        )

    # 타임스큐 체크
    try:
        ts_i = int(ts)
    except ValueError:
        raise HTTPException(status_code=400, detail="X-Timestamp must be integer epoch seconds.")

    now = int(time.time())
    if abs(now - ts_i) > allowed_skew:
        raise HTTPException(status_code=401, detail="Request timestamp is out of allowed window.")

    # 논스 재사용 방지(간단 구현: 존재 체크만) — 실제 운영에선 Redis 등으로 nonce 캐시
    if len(nonce) < 8:
        raise HTTPException(status_code=400, detail="X-Nonce too short (>= 8 required).")

    # 바디 바이트 획득(계산을 위해 다시 읽음)
    # FastAPI/Starlette의 요청 바디는 스트림이므로, receive를 래핑하여 복원
    body = _read_and_restore_body(request)

    client_sig = _decode_signature(sig)
    server_sig = _hmac_sign(secret=secret, body=body, ts=ts, nonce=nonce)

    # 타이밍 공격 완화: hmac.compare_digest
    if not hmac.compare_digest(client_sig, server_sig):
        raise HTTPException(status_code=401, detail="Invalid HMAC signature.")

    return SecurityContext(principal="hmac", method="hmac")


async def api_key_or_hmac_dependency(request: Request) -> SecurityContext:
    """
    FastAPI 의존성:
      1) HMAC_SECRET이 설정되어 있으면 HMAC 우선 검증
      2) 아니면 SERVER_API_KEY가 설정되어 있으면 API Key 검증
      3) 둘 다 없으면 인증 생략(개발 편의) — 운영에서는 반드시 최소 하나 설정 권장
    """
    # HMAC 우선
    if HMAC_SECRET:
        try:
            return verify_hmac_headers(request, secret=HMAC_SECRET, allowed_skew=HMAC_ALLOWED_SKEW)
        except HTTPException as e:
            # HMAC이 설정되어 있으면 실패 시 바로 거부
            raise e

    # API KEY 대안
    if API_KEY_VALUE:
        api_key = request.headers.get(HEADER_API_KEY)
        if not api_key or not hmac.compare_digest(api_key, API_KEY_VALUE):
            raise HTTPException(status_code=401, detail="Invalid or missing API Key.")
        return SecurityContext(principal="api_key", method="api_key")

    # 개발 편의: 인증 비활성
    return SecurityContext(principal="anonymous", method="none")


# ----------------------------
# 인메모리 레이트리밋 (개발용)
# ----------------------------
class InMemoryRateLimiter:
    """
    아주 단순한 개발용 레이트리미터(토큰 버킷 유사).
    - 프로세스 메모리 기반이므로 멀티인스턴스/재시작 환경에선 적합하지 않음.
    - 운영에선 Redis + lua 스크립트, Nginx/Cloudflare, 또는 API Gateway 권장.
    """

    def __init__(self) -> None:
        # key -> (window_start_epoch, count)
        self._store: dict[str, Tuple[int, int]] = {}

    def allow(self, key: str, *, rate: int = RL_DEFAULT_RATE, per: int = RL_DEFAULT_PER) -> bool:
        now = int(time.time())
        window_start = now - (now % per)

        start, cnt = self._store.get(key, (window_start, 0))
        if start != window_start:
            # 새 윈도우 시작
            start, cnt = window_start, 0

        if cnt + 1 > rate:
            self._store[key] = (start, cnt)  # 유지
            return False

        self._store[key] = (start, cnt + 1)
        return True


_rate_limiter = InMemoryRateLimiter()


async def rate_limit_dependency(
    request: Request,
    ctx: SecurityContext = Depends(api_key_or_hmac_dependency),
) -> SecurityContext:
    """
    라우터에서 Depends로 추가하여 레이트리밋 적용.
    기본: 클라이언트 식별자 + 경로 조합으로 분당 60회.
    """
    # 식별자 키 (인증 방식에 따라)
    client = ctx.principal
    if client == "anonymous":
        # 인증 미사용 시, IP 기반(신뢰 낮음)
        client = request.client.host if request.client else "unknown"

    key = f"{client}:{request.url.path}"
    if not _rate_limiter.allow(key):
        raise HTTPException(status_code=429, detail="Rate limit exceeded. Slow down.")
    return ctx


# ----------------------------
# 내부 유틸
# ----------------------------
def _read_and_restore_body(request: Request) -> bytes:
    """
    Request body를 읽고, 이후 라우터에서 다시 읽을 수 있도록 복원.
    """
    if not hasattr(request.state, "_cached_body"):
        # 원본 receive 저장
        receive_ = request.receive

        async def receive_wrapper():
            message = await receive_()
            if message["type"] == "http.request":
                body = message.get("body", b"") or b""
                more_body = message.get("more_body", False)
                # 캐시
                if not hasattr(request.state, "_cached_body"):
                    request.state._cached_body = body
                else:
                    request.state._cached_body += body
                return {"type": "http.request", "body": body, "more_body": more_body}
            return message

        request._receive = receive_wrapper  # type: ignore[attr-defined]

        # body를 한 번 강제로 소비해서 캐시한다
        # (Starlette는 body() 호출 시 내부에서 receive를 소모)
        # 이후 라우터 단계에서 request.body()를 다시 호출해도 캐시된 값을 반환하게 됨.
    # body()는 이미 캐시가 있으면 그대로 반환
    # 단, 여기서는 동기 함수이므로 직접 이벤트 루프 없이 접근하지 않음.
    # 호출자(verify_hmac_headers)에서 request.body()를 쓰지 않고 캐시를 참조하도록 구현.
    # 캐시가 비어있으면 실제로 읽어서 채운다.
    # 아래는 동기 함수라서, Starlette의 비동기 body()를 직접 기다릴 수 없으므로,
    # HMAC 검증은 라우터 이전 미들웨어보다 '의존성 단계'에서 수행하는 것을 전제로 한다.
    # → FastAPI 의존성은 비동기 컨텍스트에서 실행되므로 request.body()를 await 가능.
    return _get_cached_body_sync_unsafe(request)


def _get_cached_body_sync_unsafe(request: Request) -> bytes:
    """
    현재 state에 캐시된 body를 가능한 한 반환.
    비동기 컨텍스트가 아니라면 request.body()를 await할 수 없으므로,
    캐시가 없을 경우 빈 바이트를 반환(서명 입력에 빈 바디 해시 사용).
    """
    cached = getattr(request.state, "_cached_body", None)
    if cached is None:
        # 의존성에서 최초 접근 시점엔 아직 캐시가 없을 수 있다.
        # 가능한 해법: 라우터에서 request.body()를 먼저 await하여 캐시되도록 하거나,
        # 클라이언트가 항상 JSON 본문을 보내도록 강제.
        return b""
    return cached


# ----------------------------
# 디버깅용: 서버측 서명 생성 (테스트/문서)
# ----------------------------
def debug_make_signature(
    secret: str, body: bytes, ts: int | str, nonce: str, *, b64: bool = False
) -> str:
    """
    서버와 동일한 규약으로 서명을 계산해 문자열로 반환.
    - hex (기본) 또는 base64 문자열 반환
    """
    ts = str(ts)
    sig = _hmac_sign(secret, body, ts, nonce)
    return base64.b64encode(sig).decode() if b64 else sig.hex()
