using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildGridOverlay : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Tilemap overlayTilemap;
    [SerializeField] private TileBase overlayCellTile;
    [SerializeField] private Tilemap groundTilemap;

    [Header("옵션")]
    [SerializeField] private bool autoBuildOnStart = true;
    [SerializeField] private int extraMargin = 0;

    private bool built;

    void Reset() {
        overlayTilemap = GetComponent<Tilemap>();
    }

    void Start() {
        if (autoBuildOnStart) { BuildOnce(); }
        Show(true);
    }

    /// 한 번만 칸 채우기
    public void BuildOnce() {
        if (built) return;
        if (overlayTilemap == null || overlayCellTile == null) {
            Debug.LogWarning("overlayTilemap/overlayCellTile 미지정");
            return;
        }

        // 범위 가져오기
        BoundsInt bounds;
        if (groundTilemap != null) {
            bounds = groundTilemap.cellBounds;
        } else {
            // 직접 범위 지정
            bounds = new BoundsInt(-20, -20, 0, 40, 40, 1);
        }

        // 여유 여백
        bounds.xMin -= extraMargin;
        bounds.yMin -= extraMargin;
        bounds.xMax += extraMargin;
        bounds.yMax += extraMargin;

        overlayTilemap.ClearAllTiles();

        // 빠른 대량 세팅 (루프도 OK, 여기선 루프로 이해 쉬움)
        for (int x = bounds.xMin; x < bounds.xMax; x++) {
            for (int y = bounds.yMin; y < bounds.yMax; y++) {
                var pos = new Vector3Int(x, y, 0);
                overlayTilemap.SetTile(pos, overlayCellTile);
            }
        }

        built = true;
    }

    /// 건축모드 토글에서 호출
    public void Show(bool on) {
        if (!built) BuildOnce();
        var r = overlayTilemap.GetComponent<TilemapRenderer>();
        if (r) r.enabled = on;
        else overlayTilemap.gameObject.SetActive(on); // 렌더러 없으면 오브젝트 토글
    }
}