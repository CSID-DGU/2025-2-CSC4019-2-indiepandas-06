#!/usr/bin/env bash
set -e

# 기본 환경 변수 (필요 시 docker-compose.yml에서 override 가능)
: "${UVICORN_WORKERS:=4}"
: "${UVICORN_HOST:=0.0.0.0}"
: "${UVICORN_PORT:=8000}"
: "${APP_MODULE:=app.main:app}"
: "${ENV:=prod}"

echo "[start.sh] Starting server in $ENV mode..."

if [ "$ENV" = "dev" ]; then
  # 개발 모드: 핫리로드 지원
  exec uvicorn "$APP_MODULE" \
    --reload \
    --host "$UVICORN_HOST" \
    --port "$UVICORN_PORT"
else
  # 프로덕션 모드: Gunicorn + UvicornWorker
  exec gunicorn "$APP_MODULE" \
    -k uvicorn.workers.UvicornWorker \
    -w "$UVICORN_WORKERS" \
    -b "$UVICORN_HOST:$UVICORN_PORT" \
    --timeout 60 \
    --graceful-timeout 30 \
    --keep-alive 20 \
    --access-logfile '-' \
    --error-logfile '-'
fi
