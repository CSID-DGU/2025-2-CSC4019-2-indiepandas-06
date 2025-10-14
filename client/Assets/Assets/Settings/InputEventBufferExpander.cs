#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.InputSystem;

[InitializeOnLoad]
public static class InputEventBufferExpander {
    static InputEventBufferExpander() {
        // 이벤트 버퍼 확장 (8MB)
        InputSystem.settings.maxEventBytesPerUpdate = 8 * 1024 * 1024;
    }
}
#endif