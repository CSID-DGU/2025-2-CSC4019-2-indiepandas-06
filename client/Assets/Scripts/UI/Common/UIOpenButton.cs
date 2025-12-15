using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Open UI Screen Button")]
public class UIOpenButton : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Button button;

    [Header("Target")]
    [SerializeField] private Screen targetScreen = Screen.Menu;

    [Tooltip("Options")]
    [SerializeField] private bool toggle = false;

    private void Reset() {
        // 같은 오브젝트에 붙은 Button 자동 할당
        if (!button) {
            TryGetComponent(out button);
        }
    }

    private void OnValidate() {
        if (!button) {
            TryGetComponent(out button);
        }
    }

    private void Awake() {
        if (!button) {
            return;
        }
        button.onClick.AddListener(OnClick);
    }

    private void OnDestroy() {
        if (button) {
            button.onClick.RemoveListener(OnClick);
        }
    }

    private void OnClick() {
        if (toggle && UIManager.CurrentScreen == targetScreen) {
            // 현재 최상단 스크린이 대상이면 닫기
            var top = UIManager.CurrentScreenBase;
            if (top != null) {
                UIManager.CloseScreen(top);
            }
            return;
        }

        UIManager.OpenScreen(targetScreen);
    }

}