using UnityEngine;

public static class GridUtility {
    private static float spritePPU = 16f;
    private static float tileWidth = 32f / spritePPU;
    private static float tileHeight = 16f / spritePPU;

    public static Vector3 SnapToIsometricGrid(Vector3 rawPosition, int size = 1) {
        float gridX = Mathf.Round((rawPosition.x / (tileWidth / 2) + rawPosition.y / (tileHeight / 2)) / 2f);
        float gridY = Mathf.Round((rawPosition.y / (tileHeight / 2) - rawPosition.x / (tileWidth / 2)) / 2f);

        float snappedX = (gridX - gridY) * (tileWidth / 2f);
        float snappedY = (gridX + gridY) * (tileHeight / 2f);

        Vector3 offset = Vector3.zero;

        if (size % 2 == 1) {
            // 위까지는 그리드의 교차 점의 위치이기 때문에 오프셋을 줘야 함
            // 실제 위치가 교차 점을 기준으로 상하좌우 중 어느 그리드 칸에 적합한지 결정해 그 방향으로 오프셋을 줘야 함
            // 오프셋 방향 결정
            float offsetDirX = (rawPosition.x - snappedX) >= 0 ? 1f : -1f;
            float offsetDirY = (rawPosition.y - snappedY) >= 0 ? 1f : -1f;
            // 그리드 선의 기울기는 1/2이기 때문에 y방향 차이에는 2를 곱해 비교
            if (Mathf.Abs(rawPosition.x - snappedX) > Mathf.Abs(rawPosition.y - snappedY) * 2) {
                offsetDirY = 0f;
            }
            else {
                offsetDirX = 0f;
            }
            
            offset = new Vector3((tileWidth / 2f) * offsetDirX, (tileHeight / 2f) * offsetDirY, 0f);
        }

        return new Vector3(snappedX, snappedY, rawPosition.z) + offset;
    }
}
