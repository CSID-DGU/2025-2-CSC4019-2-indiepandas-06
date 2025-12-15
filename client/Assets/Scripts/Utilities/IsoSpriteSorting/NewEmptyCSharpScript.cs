using System;
using UnityEngine;
/*
public class IsoSpriteSorting_Temp : MonoBehaviour {
    public bool isMovable;
    public bool renderBelowAll;
    public SpriteDepthDatabase spriteDepthDatabase;

    private SpriteRenderer spriteRenderer;
    private Transform t;

    [NonSerialized] public Vector2 SortingPoint1;
    [NonSerialized] public Vector2 SortingPoint2;
    [NonSerialized] public Bounds2D cachedBounds;

    void Awake() {
        t = transform;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Start() {
        if (Application.isPlaying) {
            IsoSpriteSortingManager.RegisterSprite(this);
        }
        RefreshCache();
    }

    public void RefreshCache() {
        if (!spriteRenderer) return;

        Sprite sprite = spriteRenderer.sprite;
        if (sprite && spriteDepthDatabase && spriteDepthDatabase.TryGetDepthLine(sprite, out var localA, out var localB)) {
            Vector2 pos = t.position;
            SortingPoint1 = pos + localA;
            SortingPoint2 = pos + localB;
        } else {
            // fallback 기존 pivot 중심
            SortingPoint1 = t.position;
            SortingPoint2 = t.position;
        }

        cachedBounds = new Bounds2D(spriteRenderer.bounds);
    }

    private void OnDestroy() {
        if (Application.isPlaying)
            IsoSpriteSortingManager.UnregisterSprite(this);
    }
}
*/