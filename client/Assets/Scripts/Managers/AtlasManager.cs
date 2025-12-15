using UnityEngine;
using UnityEngine.U2D;
using System.Collections.Generic;

public class AtlasManager : MonoSingleton<AtlasManager> {
    [SerializeField] private AtlasDatabase db;
    [SerializeField] private bool warmupOnAwake = false;
    [SerializeField] private List<string> warmupKeys = new();

    private readonly Dictionary<(string, string), Sprite> spriteCache = new();
    private readonly Dictionary<string, SpriteAtlas> atlasMap = new();

    protected override void Awake() {
        BuildMapFromDB();
        if (warmupOnAwake) {
            foreach (var k in warmupKeys) Warmup(k);
        }
    }

    private void BuildMapFromDB() {
        atlasMap.Clear();
        if (!db) {
            Debug.LogWarning("AtlasDatabase not assigned.");
            return;
        }
        foreach (var e in db.entries) {
            if (!string.IsNullOrEmpty(e.key) && e.atlas) {
                atlasMap[e.key] = e.atlas;
            }
        }
    }

    private SpriteAtlas FindAtlas(string atlasKey) {
        if (string.IsNullOrEmpty(atlasKey)) { return null; }
        if (atlasMap.TryGetValue(atlasKey, out var a) && a) { return a; }

        BuildMapFromDB();
        atlasMap.TryGetValue(atlasKey, out a);
        return a;
    }

    public void Warmup(string atlasKey) {
        var atlas = FindAtlas(atlasKey);
        if (!atlas) {
            Debug.LogWarning($"Warmup fail: atlas '{atlasKey}' not found");
            return;
        }
        int count = atlas.spriteCount;
        if (count <= 0) return;
        var arr = new Sprite[count];
        int filled = atlas.GetSprites(arr);

        for (int i = 0; i < filled; i++) {
            var s = arr[i];
            if (s) spriteCache[(atlasKey, s.name)] = s;
        }
    }

    public Sprite Get(string atlasKey, string spriteName) {
        if (string.IsNullOrEmpty(spriteName)) { return null; }

        if (spriteCache.TryGetValue((atlasKey, spriteName), out var sprite) && sprite) {
            return sprite;
        }

        var atlas = FindAtlas(atlasKey);
        if (!atlas) { return null; }

        var loaded = atlas.GetSprite(spriteName);
        if (loaded) {
            spriteCache[(atlasKey, spriteName)] = loaded;
        }
        return loaded;
    }

    public void ClearCache() => spriteCache.Clear();
}
