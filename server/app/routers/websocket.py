import os

from fastapi import APIRouter, Header, WebSocket, WebSocketDisconnect, status

from app.core.bridge import ai_bridge

router = APIRouter()


@router.websocket("/ws/ai-worker")
async def websocket_endpoint(
    websocket: WebSocket, x_api_key: str | None = Header(None, alias="X-Api-Key")
):
    # 수정된 부분: settings 대신 os.getenv 사용
    server_api_key = os.getenv("SERVER_API_KEY")

    # 인증 체크
    if server_api_key and x_api_key != server_api_key:
        await websocket.close(code=status.WS_1008_POLICY_VIOLATION)
        return

    await ai_bridge.connect(websocket)
    try:
        while True:
            data = await websocket.receive_text()
            await ai_bridge.process_message(data)
    except WebSocketDisconnect:
        ai_bridge.disconnect(websocket)
