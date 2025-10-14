using UnityEngine;
using System.Threading.Tasks;

[RequireComponent(typeof(MapObject))]
public class MapObjectOutliner : MonoBehaviour {
    [SerializeField] protected Transform visual;
    [SerializeField] private Material outlineMaterial;
    private Material noOutlineMaterial;
    private MapObject mapObject;
    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock materialPropertyBlock;

    private static readonly int sizeID = Shader.PropertyToID("_Thickness");
    private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor");

    private void Awake() {
        mapObject = GetComponent<MapObject>();
        spriteRenderer = visual.GetComponent<SpriteRenderer>();
        noOutlineMaterial = spriteRenderer.material;
        materialPropertyBlock = new MaterialPropertyBlock();
        HandleSelectionChanged();
    }

    private async void OnEnable() {
        // 빌딩 모드 매니저 초기화까지 대기
        await WaitForManagerInstance();

        if (BuildingModeManager.Instance != null) {
            BuildingModeManager.Instance.OnSelectedChanged += HandleSelectionChanged;
        }

        // 초기 상태 한 번 갱신
        HandleSelectionChanged();
    }

    private void OnDisable() {
        if (BuildingModeManager.Instance != null) {
            BuildingModeManager.Instance.OnSelectedChanged -= HandleSelectionChanged;
        }
    }

    private void HandleSelectionChanged() {
        var manager = BuildingModeManager.Instance;
        if (manager == null || !manager.IsBuildingMode) {
            ApplyOutline(false);
            return;
        }

        if (manager.SelectedObject == mapObject) {
            ApplyOutline(true, 2f);
            ApplyOutlineColor(Color.yellow);
        } else {
            ApplyOutline(true, 1f);
            ApplyOutlineColor(Color.white);
        }
    }

    private void ApplyOutline(bool enable, float thickness = 1f) {
        if (spriteRenderer == null) { return; }

        spriteRenderer.material = enable ? outlineMaterial : noOutlineMaterial;

        spriteRenderer.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetFloat(sizeID, thickness);
        spriteRenderer.SetPropertyBlock(materialPropertyBlock);
    }

    private void ApplyOutlineColor(Color color) {
        if (spriteRenderer == null) { return; }

        spriteRenderer.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetColor(OutlineColorID, color);
        spriteRenderer.SetPropertyBlock(materialPropertyBlock);
    }

    private async Task WaitForManagerInstance() {
        while (BuildingModeManager.Instance == null) {
            await Task.Yield();
        }
    }
}