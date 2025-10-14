import contextvars
import logging
import sys
import time
import uuid
from typing import Callable, Optional

from fastapi import FastAPI, Request, Response
from starlette.middleware.base import BaseHTTPMiddleware
from starlette.types import ASGIApp

# 요청별 상관관계 ID(Request ID)를 contextvar로 보관
_request_id_ctx: contextvars.ContextVar[str] = contextvars.ContextVar("request_id", default="")


# ----------------------------
# 로깅 설정
# ----------------------------
def setup_logging(level: str = "INFO", json: bool = True) -> None:
    """
    애플리케이션 전역 로깅 초기화.
    - json=True면 python-json-logger가 있으면 JSON 포맷, 없으면 기본 포맷
    - uvicorn 기본 로거도 동일 레벨로 맞춤
    """
    root = logging.getLogger()
    # 중복 핸들러 방지
    for h in list(root.handlers):
        root.removeHandler(h)

    level_value = getattr(logging, level.upper(), logging.INFO)
    root.setLevel(level_value)

    handler = logging.StreamHandler(sys.stdout)

    if json:
        try:
            from pythonjsonlogger import jsonlogger  # type: ignore

            fmt = jsonlogger.JsonFormatter(
                "%(asctime)s %(levelname)s %(name)s %(message)s "
                "%(request_id)s %(method)s %(path)s %(status_code)s %(latency_ms)s"
            )
            handler.setFormatter(fmt)
        except Exception:
            handler.setFormatter(
                logging.Formatter(
                    "[%(asctime)s] %(levelname)s %(name)s " "[rid=%(request_id)s] %(message)s"
                )
            )
    else:
        handler.setFormatter(
            logging.Formatter(
                "[%(asctime)s] %(levelname)s %(name)s " "[rid=%(request_id)s] %(message)s"
            )
        )

    root.addHandler(handler)

    # uvicorn 로거 레벨 동기화
    for logger_name in ("uvicorn", "uvicorn.error", "uvicorn.access"):
        logging.getLogger(logger_name).setLevel(level_value)


def get_logger(name: Optional[str] = None) -> logging.LoggerAdapter:
    """
    logger 어댑터를 반환하여 모든 로그에 request_id 필드를 자동 주입.
    """
    base_logger = logging.getLogger(name or __name__)
    return logging.LoggerAdapter(base_logger, {"request_id": current_request_id()})


def current_request_id() -> str:
    return _request_id_ctx.get()


def _set_request_id(value: str) -> None:
    _request_id_ctx.set(value)


# ----------------------------
# 미들웨어
# ----------------------------
class RequestContextMiddleware(BaseHTTPMiddleware):
    """
    - 요청에 X-Request-ID 없으면 생성하여 헤더/컨텍스트에 주입
    - 요청/응답의 핵심 정보(메서드, 경로, 상태코드, 지연시간)를 구조화 로깅
    - /healthz 등 잡음 많은 경로는 스킵 가능
    """

    def __init__(
        self,
        app: ASGIApp,
        *,
        skip_paths: Optional[set[str]] = None,
        logger: Optional[logging.Logger] = None,
    ) -> None:
        super().__init__(app)
        self.skip_paths = skip_paths or {"/healthz"}
        self._logger = logger or get_logger("request")

    async def dispatch(self, request: Request, call_next: Callable) -> Response:  # type: ignore[override]
        # Request ID 설정
        request_id = request.headers.get("X-Request-ID") or _gen_request_id()
        _set_request_id(request_id)

        # 시간 측정
        start = time.perf_counter()
        method = request.method
        path = request.url.path

        if path not in self.skip_paths:
            self._logger.info(
                "request start",
                extra={
                    "method": method,
                    "path": path,
                    "status_code": None,
                    "latency_ms": None,
                    "request_id": request_id,
                },
            )

        try:
            response: Response = await call_next(request)
        except Exception:
            latency_ms = int((time.perf_counter() - start) * 1000)
            self._logger.exception(
                "request unhandled exception",
                extra={
                    "method": method,
                    "path": path,
                    "status_code": 500,
                    "latency_ms": latency_ms,
                    "request_id": request_id,
                },
            )
            raise

        # 응답 헤더에 Request ID 반영
        response.headers.setdefault("X-Request-ID", request_id)

        latency_ms = int((time.perf_counter() - start) * 1000)
        if path not in self.skip_paths:
            self._logger.info(
                "request end",
                extra={
                    "method": method,
                    "path": path,
                    "status_code": getattr(response, "status_code", None),
                    "latency_ms": latency_ms,
                    "request_id": request_id,
                },
            )

        return response


def _gen_request_id() -> str:
    return f"req_{uuid.uuid4().hex[:12]}"


# ----------------------------
# FastAPI에 붙이는 헬퍼
# ----------------------------
def add_observability(app: FastAPI, *, log_level: str = "INFO", json: bool = True) -> None:
    """
    애플리케이션 시작 시 한 번 호출하여:
    - 로깅 초기화
    - 요청 컨텍스트/로깅 미들웨어 장착
    """
    setup_logging(level=log_level, json=json)
    # 올바른 등록 방법: add_middleware 사용
    app.add_middleware(RequestContextMiddleware)
    logger = get_logger("startup")
    logger.info("observability initialized", extra={"request_id": current_request_id() or "-"})
