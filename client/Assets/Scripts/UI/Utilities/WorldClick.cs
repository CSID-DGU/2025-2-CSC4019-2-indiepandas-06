using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class WorldClick : MonoBehaviour {
    [SerializeField] private Camera camera;
    [SerializeField] private LayerMask clickMask = 8;
    private Dictionary<Sprite, Color32[]> spriteCache = new();
    private float lastClickTime;

    void OnApplicationFocus(bool hasFocus) {
        if (hasFocus) {
            UnityEngine.InputSystem.InputSystem.ResetDevice(UnityEngine.InputSystem.Mouse.current);
        }
    }

    void Awake() {
        lastClickTime = 0f;
        if (!camera) {
            camera = Camera.main;
        }
    }

    void Update() {
        HandleClick();
        HandleClickEnd();
    }

    private void HandleClickEnd() {
        if (!WasPointerReleasedThisFrame()) { return; }
        foreach (var detector in FindObjectsOfType<WorldLongPressDetector>()) {
            detector.OnPressEnd();
        }
    }

    private void HandleClick() {
        if (Time.unscaledTime - lastClickTime < 0.05f) { return; }
        if (!WasPointerPressedThisFrame()) { return; }
        lastClickTime = Time.unscaledTime;
        Debug.Log("클릭시작");
        // UI 위 클릭이면 무시
        //if (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) { return; }
        
        Vector2 screenPos = GetPointerScreenPosition();
        float dist = camera.orthographic
            ? 0f
            : Mathf.Abs(camera.transform.position.z - 0f);

        Vector3 worldPos3 = camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, dist));
        Vector2 worldPos  = new Vector2(worldPos3.x, worldPos3.y);

        // 해당 지점의 콜라이더 탐색
        var hit = Physics2D.OverlapPoint(worldPos, clickMask);
        if (!hit) { return; }

        var sr = hit.GetComponent<SpriteRenderer>();
        if (sr != null && !IsSpritePixelOpaque(sr, worldPos)) { return; }

        // 클릭 처리
        var clickable = hit.GetComponentInParent<IClickable>();
        if (clickable != null) {
            clickable.OnClick(worldPos3);
        }
        // 길게 누름 처리
        var detector = hit.GetComponentInParent<WorldLongPressDetector>();
        if (detector != null) {
            detector.OnPressStart();
        }
    }

    private bool IsSpritePixelOpaque(SpriteRenderer sr, Vector2 worldPos) {
        Sprite sprite = sr.sprite;
        if (sprite == null || sprite.texture == null) {
            return false;
        }

        if (!spriteCache.TryGetValue(sprite, out var pixels)) {
            if (!sprite.texture.isReadable) { return true; }
            pixels = sprite.texture.GetPixels32();
            spriteCache[sprite] = pixels;
        }

        Rect rect = sprite.rect;
        int texWidth = sprite.texture.width;

        Vector2 localPos = sr.transform.InverseTransformPoint(worldPos);
        Vector2 pivot = sprite.pivot;
        float ppu = sprite.pixelsPerUnit;

        int px = Mathf.RoundToInt(pivot.x + localPos.x * ppu);
        int py = Mathf.RoundToInt(pivot.y + localPos.y * ppu);

        if (px < 0 || py < 0 || px >= rect.width || py >= rect.height) {
            return false;
        }

        int pixelIndex = (int)(rect.y + py) * texWidth + (int)(rect.x + px);
        // 0~255 기준
        return pixels[pixelIndex].a > 32;
    }


    private static bool WasPointerPressedThisFrame() {
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame) { return true; }
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) { return true; }
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) { return true; }
        return false;
    }

    private static bool WasPointerReleasedThisFrame() {
        if (Pointer.current != null && Pointer.current.press.wasReleasedThisFrame) { return true; }
        if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame) { return true; }
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame) { return true; }
        return false;
    }

    private static Vector2 GetPointerScreenPosition() {
        if (Pointer.current != null) { return Pointer.current.position.ReadValue(); }
        if (Mouse.current != null) { return Mouse.current.position.ReadValue(); }
        if (Touchscreen.current != null) { return Touchscreen.current.primaryTouch.position.ReadValue(); }
        return Vector2.zero;
    }
    
}