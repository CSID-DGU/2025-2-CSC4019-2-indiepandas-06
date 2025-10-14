from typing import List, cast

from pydantic import AnyHttpUrl
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    """
    프로젝트 전역 설정.
    Pydantic v2 + pydantic-settings 기반.
    """

    # 환경 모드
    ENV: str = "dev"

    # Emotion 분석 API
    EMOTION_API_URL: AnyHttpUrl = cast(AnyHttpUrl, "http://localhost:9000/v1/predict")
    EMOTION_TIMEOUT_S: float = 2.0

    # GPT API
    GPT_API_URL: AnyHttpUrl = cast(AnyHttpUrl, "https://api.openai.com/v1/chat/completions")
    GPT_MODEL: str = "gpt-4o-mini"
    GPT_API_KEY: str = "REPLACE_WITH_YOUR_KEY"

    # 공통 설정
    REQUEST_TIMEOUT_S: float = 6.0
    MAX_INPUT_CHARS: int = 1000

    # CORS
    CORS_ORIGINS: List[str] = ["*"]

    # pydantic-settings 설정 (v2 방식)
    model_config = SettingsConfigDict(
        env_file=".env",  # 루트(Server/.env)
        env_file_encoding="utf-8",
        extra="ignore",  # 알 수 없는 환경변수 무시
    )


# 전역 인스턴스
settings = Settings()
