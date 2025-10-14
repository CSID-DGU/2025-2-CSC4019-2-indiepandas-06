# =========================
# Build stage
# =========================
FROM python:3.11-slim AS builder

WORKDIR /build

# 선택: dev 의존성까지 설치할지 여부 (docker build --build-arg INSTALL_DEV=true)
ARG INSTALL_DEV=false

# 시스템 도구 (필요시 빌드 툴 추가)
RUN apt-get update && apt-get install -y --no-install-recommends \
    build-essential curl ca-certificates && \
    rm -rf /var/lib/apt/lists/*

# 의존성 사전 빌드 (wheel)
COPY requirements.txt .
# requirements-dev.txt가 있으면 함께 복사
COPY requirements-dev.txt requirements-dev.txt

RUN pip install --upgrade pip && \
    pip wheel --no-cache-dir --wheel-dir /wheels -r requirements.txt && \
    if [ "$INSTALL_DEV" = "true" ] && [ -f requirements-dev.txt ]; then \
        pip wheel --no-cache-dir --wheel-dir /wheels -r requirements-dev.txt ; \
    fi


# =========================
# Runtime stage
# =========================
FROM python:3.11-slim AS runtime

WORKDIR /app

# 보안: 비루트 사용자
RUN useradd -m appuser

# 런타임 최소 패키지
RUN apt-get update && apt-get install -y --no-install-recommends \
    ca-certificates && \
    rm -rf /var/lib/apt/lists/*

# 빌더에서 만들어둔 wheels 설치
COPY --from=builder /wheels /wheels
RUN pip install --no-cache-dir /wheels/* && rm -rf /wheels

# 애플리케이션 소스 복사
COPY app/ /app/app
COPY docker/start.sh /app/start.sh
RUN chmod +x /app/start.sh

# 기본 환경 변수
ENV PYTHONUNBUFFERED=1 \
    PIP_NO_CACHE_DIR=1 \
    UVICORN_WORKERS=4 \
    UVICORN_HOST=0.0.0.0 \
    UVICORN_PORT=8000 \
    APP_MODULE=app.main:app \
    ENV=prod

# 헬스/서비스 포트
EXPOSE 8000

# 권한 격하
USER appuser

# 실행 (prod: gunicorn, dev는 compose에서 ENV=dev로 바꿔서 uvicorn --reload)
CMD ["/app/start.sh"]
