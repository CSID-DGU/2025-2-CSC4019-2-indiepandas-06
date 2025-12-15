from __future__ import annotations

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import Response

from app.core.config import settings
from app.core.observability import add_observability
from app.routers.dialog import router as dialog_router
from app.routers.websocket import router as ws_router


def create_app() -> FastAPI:
    app = FastAPI(
        title="Game Dialog Server",
        version="1.0.0",
        docs_url="/docs",
        redoc_url="/redoc",
    )

    # CORS
    app.add_middleware(
        CORSMiddleware,
        allow_origins=settings.CORS_ORIGINS or ["*"],
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"],
    )

    # Observability
    add_observability(app, log_level="INFO", json=True)

    # Routers
    app.include_router(dialog_router)
    app.include_router(ws_router)

    # Health
    @app.get("/healthz", tags=["health"])
    async def healthz():
        return {"status": "ok"}

    # Root
    @app.get("/", tags=["meta"])
    async def root():
        return {
            "name": "Game Dialog Server",
            "env": settings.ENV,
            "docs": "/docs",
            "health": "/healthz",
        }

    # favicon.ico 404 방지: 아이콘 없이 204로 응답
    @app.get("/favicon.ico", include_in_schema=False)
    async def favicon():
        return Response(status_code=204)

    return app


app = create_app()
