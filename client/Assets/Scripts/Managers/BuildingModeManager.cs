using UnityEngine;
using System;

public class BuildingModeManager : MonoSingleton<BuildingModeManager> {
    [SerializeField] private GhostObject ghostPrefab;
    [SerializeField] private Transform mapObjectParent;
    [SerializeField] private GameObject buildingModeScreen;
    [SerializeField] private GameObject buildGridOverlay;
    public Transform MapObjectParent => mapObjectParent;
    private GhostObject ghostInstance;
    private GameObject ghostObject => ghostInstance.gameObject;

    public bool IsBuildingMode => buildingModeScreen.activeSelf;
    private bool prevIsBuildingMode;

    private MapObject selectedObject;
    public MapObject SelectedObject => selectedObject;
    
    public event Action OnSelectedChanged;

    protected override void Awake() {
        base.Awake();
        ghostInstance = Instantiate(ghostPrefab);
        ghostObject.SetActive(false);
        buildGridOverlay.SetActive(false);
        prevIsBuildingMode = IsBuildingMode;
        OnSelectedChanged?.Invoke();
    }

    private void Update() {
        // BuildingMode 변화 감지
        if (prevIsBuildingMode != IsBuildingMode) {
            prevIsBuildingMode = IsBuildingMode;
            OnSelectedChanged?.Invoke();
            buildGridOverlay.SetActive(IsBuildingMode);
        }
    }

    public void ShowGhost(MapObjectData data) {
        ghostInstance.Init(data);
        ghostObject.SetActive(true);
    }

    public void ShowGhost(MapObject mapObject) {
        ghostObject.SetActive(true);
        ghostInstance.Init(mapObject);
    }

    public void HideGhost() {
        ghostObject.SetActive(false);
    }

    public void ConfirmPlacement() {
        if (!ghostObject.activeSelf) { return; }

        // 실제 오브젝트 생성
        ghostInstance.ConfirmPlacement();

        HideGhost();
    }

    public void SelectObject(MapObject mapObject) {
        if (!IsBuildingMode || selectedObject == mapObject) { return; }

        selectedObject = mapObject;

        OnSelectedChanged?.Invoke();
    }
    
}