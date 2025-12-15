using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class QuestListUI : MonoBehaviour {
    [Header("Sections")]
    [SerializeField] private Transform container;

    [Header("Prefabs")]
    [SerializeField] private QuestListItemUI itemPrefab;

    // 풀링
    private readonly List<QuestListItemUI> pool = new List<QuestListItemUI>();

    private string selectedQuestId;
    public string SelectedQuestId => selectedQuestId;

    public event System.Action<string> OnSelectionChanged;

    private void OnEnable() {
        Subscribe();
        RefreshAll();
        RefreshSelectionVisual();
    }

    private void OnDisable() {
        Unsubscribe();
    }

    private void Subscribe() {
        if (QuestManager.Instance != null) {
            QuestManager.Instance.OnQuestStarted += HandleQuestChanged;
            QuestManager.Instance.OnQuestCompleted += HandleQuestChanged;
            QuestManager.Instance.OnQuestRemoved += HandleQuestChanged;
            QuestManager.Instance.OnQuestProgress += HandleQuestChanged;
        }
    }

    private void Unsubscribe() {
        if (QuestManager.Instance != null) {
            QuestManager.Instance.OnQuestStarted -= HandleQuestChanged;
            QuestManager.Instance.OnQuestCompleted -= HandleQuestChanged;
            QuestManager.Instance.OnQuestRemoved -= HandleQuestChanged;
            QuestManager.Instance.OnQuestProgress -= HandleQuestChanged;
        }
    }

    public void SelectQuest(string questId) {
        selectedQuestId = questId;
        OnSelectionChanged?.Invoke(questId);
        RefreshSelectionVisual();
    }

    private void HandleQuestChanged(string questId) {
        RefreshAll();
        RefreshSelectionVisual();
    }

    public void RefreshAll() {
        ClearContainer(container);

        // 진행중 리스트
        foreach (var qi in QuestManager.Instance.GetActiveQuests()) {
            var def = qi.Def;
            var item = GetFromPool(container);
            item.Setup(def.questId, def.title, OnItemClicked, selectedQuestId, false);
        }

        // 완료 리스트
        foreach (var id in QuestManager.Instance.GetCompletedQuestIds()) {
            if (QuestManager.Instance.TryGetDefinition(id, out var def)) {
                var item = GetFromPool(container);
                item.Setup(def.questId, def.title, OnItemClicked, selectedQuestId, true);
            }
            else {
                // 퀘스트 정의를 못 찾으면 ID만 출력
                var item = GetFromPool(container);
                item.Setup(id, $"[{id}]", OnItemClicked, selectedQuestId, true);
            }
        }
    }

    private void RefreshSelectionVisual() {
        // 현재 컨테이너에 있는 모든 아이템의 하이라이트를 갱신
        for (int i = 0; i < container.childCount; ++i) {
            if (container.GetChild(i).TryGetComponent(out QuestListItemUI it)) {
                it.RefreshSelection(selectedQuestId);
            }
        }
    }

    private void OnItemClicked(string questId) {
        SelectQuest(questId);
    }

    private void ClearAll() {
        ClearContainer(container);
    }

    private void ClearContainer(Transform container) {
        if (!container) return;
        for (int i = container.childCount - 1; 0 <= i; --i) {
            var t = container.GetChild(i);
            var item = t.GetComponent<QuestListItemUI>();
            if (item != null) {
                // 풀로 돌리기
                item.gameObject.SetActive(false);
                pool.Add(item);
            }
            else {
                // 혹시 다른게 섞여있다면 파괴
                Destroy(t.gameObject);
            }
        }
    }

    private QuestListItemUI GetFromPool(Transform parent) {
        for (int i = pool.Count - 1; i >= 0; --i) {
            var it = pool[i];
            pool.RemoveAt(i);
            it.transform.SetParent(parent, false);
            it.gameObject.SetActive(true);
            return it;
        }
        // 풀 비었으면 새로 생성
        return Instantiate(itemPrefab, parent);
    }
}