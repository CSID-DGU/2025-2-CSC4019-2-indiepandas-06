using UnityEngine;
using UnityEngine.U2D;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Atlas/Atlas Database")]
public class AtlasDatabase : ScriptableObject {
    [System.Serializable]
    public struct Entry {
        public string key;
        public SpriteAtlas atlas;
    }
    public List<Entry> entries = new();
}