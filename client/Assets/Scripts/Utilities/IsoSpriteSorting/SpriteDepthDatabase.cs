using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct SpriteDepthInfo {
    public Sprite sprite;

    [Tooltip("피벗 기준 픽셀 단위 오프셋 (좌표 A)")]
    public Vector2 pixelA;
    [Tooltip("피벗 기준 픽셀 단위 오프셋 (좌표 B)")]
    public Vector2 pixelB;

    [HideInInspector] public Vector2 localA;
    [HideInInspector] public Vector2 localB;

    public void UpdateLocalFromPixels()
    {
        if (sprite == null)
            return;

        float ppu = sprite.pixelsPerUnit;
        Vector2 pivot = sprite.pivot;

        // 피벗 기준으로 로컬 좌표로 변환
        Vector2 spriteSize = sprite.rect.size;
        Vector2 pivotOffset = (pivot / ppu);

        localA = (pixelA - pivot) / ppu;
        localB = (pixelB - pivot) / ppu;
    }
}

[CreateAssetMenu(fileName = "SpriteDepthDatabase", menuName = "Isometric/Sprite Depth Database")]
public class SpriteDepthDatabase : ScriptableObject {
    [SerializeField]
    public List<SpriteDepthInfo> entries = new();

    private Dictionary<Sprite, SpriteDepthInfo> cache;

    private void OnValidate() {
        // 인스펙터에서 값 변경 시 자동으로 로컬 좌표 업데이트
        for (int i = 0; i < entries.Count; i++) {
            var e = entries[i];
            e.UpdateLocalFromPixels();
            entries[i] = e;
        }
        BuildCache();
    }

    private void OnEnable() {
        BuildCache();
    }

    private void BuildCache() {
        cache = new();
        foreach (var e in entries) {
            if (e.sprite != null)
                cache[e.sprite] = e;
        }
    }

    public bool TryGetDepthLine(Sprite sprite, out Vector2 a, out Vector2 b) {
        if (sprite != null && cache != null && cache.TryGetValue(sprite, out var info)) {
            a = info.localA;
            b = info.localB;
            return true;
        }
        a = b = Vector2.zero;
        return false;
    }
}
