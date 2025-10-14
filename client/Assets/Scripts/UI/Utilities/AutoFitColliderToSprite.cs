using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class AutoFitColliderToSprite : MonoBehaviour {
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D collider;
    private Sprite prevSprite;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<BoxCollider2D>();
    }

    private void LateUpdate() {
        if (prevSprite != spriteRenderer.sprite) {
            prevSprite = spriteRenderer.sprite;
            FitColliderToSprite();
        }
    }

    public void FitColliderToSprite() {
        if (spriteRenderer == collider || collider == null || spriteRenderer.sprite == null) {
            return;
        }

        // 스프라이트의 Bounds 정보 사용
        Bounds spriteBounds = spriteRenderer.sprite.bounds;

        collider.offset = spriteBounds.center;
        collider.size = spriteBounds.size;
    }
}