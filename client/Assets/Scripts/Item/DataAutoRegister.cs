using UnityEditor;
using UnityEngine;

public interface IRegisterable<T> {
    void ClearItems();
    void AddItem(T data);
}

#if UNITY_EDITOR
[InitializeOnLoad]
public static class DataAutoRegister {

    static DataAutoRegister() {
        EditorApplication.delayCall += RegisterAll;
    }

    private static void RegisterAssets<TAsset, TDatabase>()
        where TAsset : ScriptableObject
        where TDatabase : MonoBehaviour, IRegisterable<TAsset>
    {
        var db = GameObject.FindObjectOfType<TDatabase>();
        if (db == null) return;

        db.ClearItems();

        string[] guids = AssetDatabase.FindAssets($"t:{typeof(TAsset).Name}");
        foreach (var guid in guids) {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<TAsset>(path);
            if (asset != null) {
                db.AddItem(asset);
            }
        }

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/Register All Databases")]
    private static void RegisterAll() {
        RegisterAssets<ItemData, ItemDatabase>();
        RegisterAssets<PlantData, PlantDatabase>();
        RegisterAssets<MapObjectData, MapObjectDatabase>();
    }
}
#endif