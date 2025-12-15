using System.IO;
using System.Text;
using UnityEngine;

public interface IPersistence {
    void SaveJson(string relativePath, string json);
    bool TryLoadJson(string relativePath, out string json);
    bool Exists(string relativePath);
    string ResolvePath(string relativePath);
}

public class JsonFilePersistence : IPersistence {
    public void SaveJson(string relativePath, string json) {
        var path = ResolvePath(relativePath);
        var dir  = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(path, json, new UTF8Encoding(false));
    }

    public bool TryLoadJson(string relativePath, out string json) {
        var path = ResolvePath(relativePath);
        if (File.Exists(path)) {
            json = File.ReadAllText(path, Encoding.UTF8);
            return true;
        }
        json = null;
        return false;
    }

    public bool Exists(string relativePath) => File.Exists(ResolvePath(relativePath));

    public string ResolvePath(string relativePath) {
        return Path.Combine(Application.persistentDataPath, relativePath).Replace("\\","/");
    }
}

public class SaveManager : MonoSingleton<SaveManager> {
    IPersistence provider;

    protected override void Awake() {
        base.Awake();
        provider = new JsonFilePersistence();
    }

    public void SaveObject<T>(string path, T obj, bool pretty = true) {
        string json = pretty ? JsonUtility.ToJson(obj, true) : JsonUtility.ToJson(obj, false);
        provider.SaveJson(path, json);
    }

    public bool TryLoadObject<T>(string path, out T obj) where T : new() {
        if (provider.TryLoadJson(path, out var json) && !string.IsNullOrEmpty(json)) {
            try {
                obj = JsonUtility.FromJson<T>(json);
                if (obj == null) obj = new T();
                return true;
            } catch {
                obj = new T();
                return false;
            }
        }
        obj = new T();
        return false;
    }

    public bool Exists(string path) => provider.Exists(path);
    public string ResolvePath(string path) => provider.ResolvePath(path);
}