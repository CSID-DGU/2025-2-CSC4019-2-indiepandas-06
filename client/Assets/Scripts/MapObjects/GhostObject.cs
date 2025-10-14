using UnityEngine;

public class GhostObject : MapObjectBase {
    [SerializeField] private MapObject MapObjectPrefab;
    private Camera mainCam;
    private MapObject targetObject;
    private bool isActive;

    protected void Update() {
        base.Update();

        FollowMouse();

        // TODO 회전 버튼 따로 만들기
        if (InputManager.GetKeyDown(KeyAction.Jump)) {
            RotateNext();
        }
    }

    public void Init(MapObjectData newData = null) {
        base.Init(newData);
        
        if (newData != null) {
            isActive = true;
        }
    }

    public void Init(MapObject mapObject) {
        if (mapObject != null) {
            base.Init(mapObject.Data);
            SetRotationIndex(mapObject.RotationIndex);
            targetObject = mapObject;
            isActive = true;
        }
    }

    protected override void Awake() {
        base.Awake();

        mainCam = Camera.main;
        targetObject = null;

        // 반투명
        spriteRenderer.color = new Color(1f, 1f, 1f, 0.75f);

        isActive = false;
    }

    public void ConfirmPlacement() {
        if (!isActive && !gameObject.activeSelf) { return; }

        // 실제 오브젝트 생성
        MapObject inst;
        if (targetObject != null) {
            inst = targetObject;
            targetObject.transform.position = transform.position;
            targetObject.gameObject.SetActive(true);

            targetObject = null;
        }
        else {
            inst = Instantiate(MapObjectPrefab, transform.position, Quaternion.identity, BuildingModeManager.Instance.MapObjectParent);
            inst.Init(data);
        }
        inst.SetRotationIndex(rotationIndex);
        inst.PlayAppear();
        // 바로 선택
        BuildingModeManager.Instance.SelectObject(inst);

        isActive = false;
    }

    private void FollowMouse() {
        if (!isActive && !gameObject.activeSelf) { return; }

        Vector3 mousePos = InputManager.PointPosition;
        Vector3 worldPos = mainCam.ScreenToWorldPoint(mousePos);
        worldPos.z = 0f;
        // 그리드에 맞게 움직이기
        Vector3 gridPos = GridUtility.SnapToIsometricGrid(worldPos, data.Size);
        transform.position = gridPos;
        Debug.Log($"Mouse: {InputManager.PointPosition}");

        if (InputManager.GetKeyUp(KeyAction.Click)) {
            BuildingModeManager.Instance.ConfirmPlacement();
        }
    }
}
