using UnityEngine;

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DiaryListUI : MonoBehaviour {
    [SerializeField] private RectTransform containter;
    [SerializeField] private DiaryListItemUI itemPrefab;
    [SerializeField] private Button btnNewEntry;
    [SerializeField] private DiaryDetailUI detailView;

    readonly List<DiaryListItemUI> pool = new();
    private string selectedId;

    void Awake() {
        if (btnNewEntry) { btnNewEntry.onClick.AddListener(CreateNewEntry); }
        if (detailView) { detailView.SetRefreshCallback(RefreshList); }
    }

    void OnEnable() {
        RefreshList();
        if (DiaryManager.Instance.Entries.Count > 0) {
            var first = DiaryManager.Instance.Entries[^1];
            Select(first.id);
        } else {
            detailView.ClearView();
        }
    }

    void ClearItems() {
        foreach (var it in pool) {
            if (it) {
                it.gameObject.SetActive(false);
            }
        }
    }

    DiaryListItemUI GetItem() {
        foreach (var it in pool) if (!it.gameObject.activeSelf) {
            it.gameObject.SetActive(true);
            it.transform.SetAsLastSibling();
            return it;
        }
        var inst = Instantiate(itemPrefab, containter);
        pool.Add(inst);
        return inst;
    }

    public void RefreshList() {
        if (!containter || !itemPrefab) { return; }

        DiaryManager.Instance.SortByDateThenCreatedAsc();
        ClearItems();

        var list = DiaryManager.Instance.Entries;
        for (int i = 0; i < list.Count; i++) {
            var e  = list[i];
            var go = GetItem();
            go.Bind(e, Select);
        }
        UpdateSelectionVisuals();

        LayoutRebuilder.ForceRebuildLayoutImmediate(containter);
    }

    void UpdateSelectionVisuals() {
        foreach (var it in pool) {
            if (!it || !it.gameObject.activeSelf) { continue; }
            it.SetSelected(!string.IsNullOrEmpty(selectedId) && it.EntryId == selectedId);
        }
    }

    void Select(string entryId) {
        selectedId = entryId;
        var e = DiaryManager.Instance.FindById(entryId);
        if (detailView) { detailView.Bind(e); }

        UpdateSelectionVisuals();
    }

    void CreateNewEntry() {
        var e = DiaryManager.Instance.Create(
            title: "",
            dateLocal: System.DateTime.Now,
            situation: "", fact: "", emotion: "", thought: ""
        );
        DiaryManager.Instance.SortByDateThenCreatedAsc();
        DiaryManager.Instance.SaveAllNow();
        RefreshList();
        Select(e.id);
    }
}