using System;
using System.Collections.Generic;
using UnityEngine;

public class DiaryManager : MonoSingleton<DiaryManager> {
    const string SavePath = "diary/diary.json";

    [SerializeField] private DiaryCollection data = new();
    public IReadOnlyList<DiaryEntry> Entries => data.entries;

    bool dirty;

    protected override void Awake() {
        base.Awake();
        LoadAll();
    }

    void OnApplicationPause(bool pause) {
        if (pause) {
            FlushIfDirty();
        }
    }

    void OnApplicationQuit() {
        FlushIfDirty();
    }

    public DiaryEntry Create(string title, DateTime dateLocal, string situation, string fact, string emotion, string thought) {
        var nowUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var e = new DiaryEntry {
            id = System.Guid.NewGuid().ToString("N"),
            title = title ?? "",
            dateIso = dateLocal.ToString("yyyy-MM-dd HH:mm"),
            situation = situation ?? "",
            fact = fact ?? "",
            emotion = emotion ?? "",
            thought = thought ?? "",
            createdUnix = nowUtc,
            updatedUnix = nowUtc
        };
        data.entries.Add(e);
        MarkDirty();
        return e;
    }

    public bool UpdateEntry(string id, Action<DiaryEntry> mutate) {
        var e = FindById(id);
        if (e == null) { return false; }
        mutate?.Invoke(e);
        e.updatedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        MarkDirty();
        return true;
    }

    public bool Delete(string id) {
        var idx = data.entries.FindIndex(x => x.id == id);
        if (idx < 0) return false;
        data.entries.RemoveAt(idx);
        MarkDirty();
        return true;
    }

    public DiaryEntry FindById(string id) => data.entries.Find(x => x.id == id);

    public List<DiaryEntry> FindByDate(string isoDate) {
        return data.entries.FindAll(e => string.Equals(e.dateIso, isoDate, StringComparison.Ordinal));
    }

    public List<DiaryEntry> FindByRange(DateTime fromLocal, DateTime toLocalInclusive) {
        string a = fromLocal.ToString("yyyy-MM-dd");
        string b = toLocalInclusive.ToString("yyyy-MM-dd");
        return data.entries.FindAll(e => string.Compare(e.dateIso, a, StringComparison.Ordinal) >= 0
                                      && string.Compare(e.dateIso, b, StringComparison.Ordinal) <= 0);
    }

    public void SortByDateThenCreatedAsc() {
        data.entries.Sort((x, y) => {
            int d = string.Compare(x.dateIso, y.dateIso, StringComparison.Ordinal);
            if (d != 0) return d;
            return x.createdUnix.CompareTo(y.createdUnix);
        });
    }

    public void LoadAll() {
        if (SaveManager.Instance.TryLoadObject(SavePath, out DiaryCollection loaded) && loaded != null) {
            data = loaded;
        } else {
            data = new DiaryCollection();
        }
        dirty = false;
        Debug.Log($"[DiaryManager] Loaded entries: {data.entries.Count}");
    }

    public void SaveAllNow() {
        SaveManager.Instance.SaveObject(SavePath, data, pretty: true);
        dirty = false;
    }

    void MarkDirty() { dirty = true; }

    void FlushIfDirty() {
        if (dirty) {
            SaveAllNow();
        }
    }
}