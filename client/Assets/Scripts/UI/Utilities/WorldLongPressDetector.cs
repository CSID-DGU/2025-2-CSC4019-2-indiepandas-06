using UnityEngine;
using UnityEngine.EventSystems;

public class WorldLongPressDetector : MonoBehaviour {
    [SerializeField] private float holdTime = 0.5f;
    private bool isPointerDown = false;
    private float timer = 0f;

    private ILongPressable longPressable;

    private void Awake() {
        longPressable = GetComponent<ILongPressable>();
        if (longPressable == null) {
            Debug.LogWarning($"{gameObject.name}에 ILongPressable 인터페이스 구현체가 없습니다.");
        }
    }

    private void Update() {
        if (isPointerDown) {
            timer += Time.unscaledDeltaTime;
            if (timer >= holdTime) {
                longPressable?.OnLongPressed();
                Reset();
            }
        }
    }

    public void OnPressStart() {
        isPointerDown = true;
        timer = 0f;
    }

    public void OnPressEnd() => Reset();

    private void Reset() {
        isPointerDown = false;
        timer = 0f;
    }
}