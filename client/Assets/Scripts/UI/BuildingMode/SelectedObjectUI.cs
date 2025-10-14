using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class SelectedObjectUI : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private RectTransform uiRoot;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, 0f);
    [SerializeField] private Button rotateNextButton;
    [SerializeField] private Button rotatePrevButton;

    private Transform targetTransform;
    private MapObject targetObject;

    void Awake() {
        if (!mainCamera) {
            mainCamera = Camera.main;
        }
    }

    private async void OnEnable() {
        // 빌딩 모드 매니저 초기화까지 대기
        await WaitForManagerInstance();

        if (BuildingModeManager.Instance != null) {
            BuildingModeManager.Instance.OnSelectedChanged += HandleSelectionChanged;
        }

        rotateNextButton.onClick.AddListener(OnClickRotateNext);
        rotatePrevButton.onClick.AddListener(OnClickRotatePrev);

        // 초기 상태 한 번 갱신
        HandleSelectionChanged();
    }
    
    private void OnDisable() {
        // 구독 해제
        if (BuildingModeManager.Instance != null) {
            BuildingModeManager.Instance.OnSelectedChanged -= HandleSelectionChanged;
        }

        rotateNextButton.onClick.RemoveListener(OnClickRotateNext);
        rotatePrevButton.onClick.RemoveListener(OnClickRotatePrev);
    }

    void Update() {
        if (!targetObject || !uiRoot) { return; }

        // 월드 좌표를 스크린 좌표 변환
        Vector3 worldPos = targetTransform.position + offset;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        // 화면 안에 있을 때만 표시
        bool isVisible = screenPos.z > 0 && screenPos.x >= 0 && screenPos.x <= UnityEngine.Screen.width && screenPos.y >= 0 && screenPos.y <= UnityEngine.Screen.height;
        uiRoot.gameObject.SetActive(isVisible);
        if (!isVisible) { return; }

        // UI RectTransform 위치 설정
        uiRoot.position = screenPos;
    }

    public void HandleSelectionChanged() {
        targetObject = BuildingModeManager.Instance.SelectedObject;
        if (targetObject != null) {
            targetTransform = targetObject.transform;
            rotateNextButton.gameObject.SetActive(targetObject.Data.IsRotatable);
            rotatePrevButton.gameObject.SetActive(targetObject.Data.IsRotatable);
            float xOffset = 24f * 0.5f * (targetObject.Data.Size + 1);
            rotateNextButton.transform.localPosition = new Vector3(xOffset, 0f, 0f);
            rotatePrevButton.transform.localPosition = new Vector3(-xOffset, 0f, 0f);
        }
        uiRoot.gameObject.SetActive(targetObject != null);
    }

    private void OnClickRotateNext() {
        if (targetObject != null) {
            targetObject.RotateNext();
            targetObject.PlayAppear();
        }
    }

    private void OnClickRotatePrev() {
        if (targetObject != null) {
            targetObject.RotatePrev();
            targetObject.PlayAppear();
        }
    }

    public void ClearTarget() {
        targetTransform = null;
        uiRoot.gameObject.SetActive(false);
    }

    private async Task WaitForManagerInstance() {
        while (BuildingModeManager.Instance == null) {
            await Task.Yield();
        }
    }
}