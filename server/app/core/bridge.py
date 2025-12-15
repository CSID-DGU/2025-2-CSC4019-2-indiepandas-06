# app/core/bridge.py
import asyncio
import json
import uuid
from typing import Any, Dict, Optional

from fastapi import WebSocket


class AIConnectionManager:
    """
    로컬 AI 워커와 WebSocket 연결을 유지하고,
    HTTP 요청이 들어오면 워커에게 작업을 토스한 뒤 결과를 기다리는 브리지.
    """

    def __init__(self):
        self.active_connection: Optional[WebSocket] = None
        # 요청 ID : Future 객체 (응답을 기다리는 대기표)
        self.pending_requests: Dict[str, asyncio.Future] = {}

    async def connect(self, websocket: WebSocket):
        await websocket.accept()
        self.active_connection = websocket
        print("Local AI Worker connected!")

    def disconnect(self, websocket: WebSocket):
        if self.active_connection == websocket:
            self.active_connection = None
            print("Local AI Worker disconnected!")

    async def send_request_and_wait(
        self, task_type: str, payload: dict, timeout: float = 10.0
    ) -> Any:
        """
        1. 고유 ID 생성
        2. 워커에게 JSON 전송
        3. Future 객체를 생성해 대기
        4. 워커로부터 응답이 오면 Future에 값 설정 -> 반환
        """
        if not self.active_connection:
            raise ConnectionError("No AI Worker connected via WebSocket.")

        req_id = str(uuid.uuid4())
        loop = asyncio.get_running_loop()
        future = loop.create_future()

        # 대기열 등록
        self.pending_requests[req_id] = future

        message = {
            "request_id": req_id,
            "type": task_type,  # "emotion" or "gpt"
            "payload": payload,
        }

        try:
            # 워커에게 전송
            await self.active_connection.send_text(json.dumps(message))

            # 응답 대기 (Timeout 적용)
            result = await asyncio.wait_for(future, timeout=timeout)
            return result
        except asyncio.TimeoutError:
            del self.pending_requests[req_id]
            raise TimeoutError(f"AI Worker response timed out ({task_type})")
        except Exception as e:
            if req_id in self.pending_requests:
                del self.pending_requests[req_id]
            raise e

    async def process_message(self, raw_message: str):
        """
        워커로부터 온 응답 메시지를 처리.
        해당하는 request_id의 Future에 결과를 넣어줌으로써 대기 중인 요청을 깨움.
        """
        try:
            data = json.loads(raw_message)
            req_id = data.get("request_id")
            result = data.get("result")
            error = data.get("error")

            if req_id in self.pending_requests:
                future = self.pending_requests[req_id]
                if error:
                    future.set_exception(Exception(error))
                else:
                    future.set_result(result)
                del self.pending_requests[req_id]
        except Exception as e:
            print(f"Error processing message: {e}")


# 전역 인스턴스
ai_bridge = AIConnectionManager()
