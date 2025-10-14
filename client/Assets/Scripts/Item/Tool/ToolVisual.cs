using UnityEngine;

public class ToolVisual : MonoBehaviour {
    public enum SwingMode {
        Water,
        Swing
    }

    [SerializeField] private SpriteRenderer bodyRenderer;

    private SpriteRenderer spriteRenderer;
    private Vector3 handPosition = new Vector3(-0.3f, 0.1f, 0f);

    private float initialRotationZ;
    private float targetRotationZ;
    private float swingDuration = 0f;
    private float swingTimer = 0f;
    private bool isSwinging = false;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetSprite(ItemData data) {
        if (spriteRenderer != null) {
            spriteRenderer.sprite = data?.HoldingSprite;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        if (spriteRenderer != null) {
            float pivotYOffset = GetPivotYOffset();
            transform.localPosition = new Vector3(
                spriteRenderer.flipX ? -handPosition.x : handPosition.x,
                handPosition.y - pivotYOffset,
                handPosition.z
            );
            SwingAnimation();
        }
    }

    private float GetPivotYOffset() {
        if (bodyRenderer == null || bodyRenderer.sprite == null) { return 0f; }

        var sprite = bodyRenderer.sprite;
        // pivot.y는 픽셀 단위
        float pivotWorldY = sprite.pivot.y / sprite.pixelsPerUnit;

        return pivotWorldY;
    }

    private void SwingAnimation() {
        if (!isSwinging) { return; }
        
        swingTimer += Time.deltaTime;
        float t = swingTimer / swingDuration;

        if (t >= 1f) {
            transform.localRotation = Quaternion.identity;
            isSwinging = false;
            return;
        }

        float smoothT = Mathf.SmoothStep(0f, 1f, t);
        float angle = Mathf.Lerp(initialRotationZ, targetRotationZ, smoothT);
        transform.localRotation = Quaternion.Euler(0, 0, spriteRenderer.flipX ? -angle : angle);
    }

    public void PlaySwingAnimation(SwingMode mode) {
        isSwinging = true;
        swingTimer = 0f;

        switch (mode) {
            case SwingMode.Water:
                initialRotationZ = 0f;
                targetRotationZ = -30f;
                swingDuration = 0.5f;
                break;
            case SwingMode.Swing:
                initialRotationZ = 60f;
                targetRotationZ = -60f;
                swingDuration = 0.25f;
                break;
        }

        float startAngle = spriteRenderer.flipX ? -initialRotationZ : initialRotationZ;
        transform.localRotation = Quaternion.Euler(0, 0, spriteRenderer.flipX ? -initialRotationZ : initialRotationZ);
        
    }
}
