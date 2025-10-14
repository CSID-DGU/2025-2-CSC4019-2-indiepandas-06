.PHONY: dev test fmt lint up down logs build clean

# ------------------------
# 로컬 개발
# ------------------------
dev:
	uvicorn app.main:app --reload --host 0.0.0.0 --port 8000 --env-file .env

test:
	pytest -q --maxfail=1 --disable-warnings -v

fmt:
	black .
	ruff check --fix .

lint:
	ruff check .
	mypy app || true

# ------------------------
# Docker / Compose
# ------------------------
up:
	docker compose up -d --build

down:
	docker compose down

logs:
	docker compose logs -f --tail=100

build:
	docker build -t game-dialog-server:latest .

clean:
	find . -type d -name "__pycache__" -exec rm -rf {} +
	find . -type d -name ".mypy_cache" -exec rm -rf {} +
	find . -type d -name ".pytest_cache" -exec rm -rf {} +
	rm -rf dist build *.egg-info
