using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class QuestDetailUI : MonoBehaviour {
    [SerializeField] private Transform container;
    [SerializeField] private QuestMissionItemUI itemPrefab;
    [SerializeField] private QuestListUI listUI;
    [SerializeField] private GameObject detailUIParent;
    [SerializeField] private TextMeshProUGUI titleText;

    private readonly List<QuestMissionItemUI> pool = new();
    private string boundQuestId;

    private void OnEnable() {
        Subscribe();
        Refresh();
    }

    private void OnDisable() {
        Unsubscribe();
    }

    public void BindQuest(string questId) {
        boundQuestId = questId;
        Refresh();
    }

    private void Subscribe() {
        if (QuestManager.Instance != null) {
            QuestManager.Instance.OnQuestStarted += OnQuestChanged;
            QuestManager.Instance.OnQuestProgress += OnQuestChanged;
            QuestManager.Instance.OnQuestCompleted += OnQuestChanged;
            QuestManager.Instance.OnQuestRemoved += OnQuestChanged;
        }
        if (listUI != null) {
            listUI.OnSelectionChanged += BindQuest;
        }
    }

    private void Unsubscribe() {
        if (QuestManager.Instance != null) {
            QuestManager.Instance.OnQuestStarted -= OnQuestChanged;
            QuestManager.Instance.OnQuestProgress -= OnQuestChanged;
            QuestManager.Instance.OnQuestCompleted -= OnQuestChanged;
            QuestManager.Instance.OnQuestRemoved -= OnQuestChanged;
        }
        if (listUI != null) {
            listUI.OnSelectionChanged -= BindQuest;
        }
    }

    private void OnQuestChanged(string questId) {
        // 내가 바인딩한 퀘스트일 때만 갱신
        if (questId == boundQuestId) {
            Refresh();
        }
    }

    public void Refresh() {
        ClearContainer();

        if (string.IsNullOrEmpty(boundQuestId)) {
            detailUIParent.SetActive(false);
            return;
        }

        // 선택된 퀘스트 데이터 찾기
        if (!QuestManager.Instance.TryGetDefinition(boundQuestId, out var def)) {
            detailUIParent.SetActive(false);
            return;
        }
        
        detailUIParent.SetActive(true);

        // 활성 중인 퀘스트 인스턴스
        QuestInstance qi = null;
        foreach (var q in QuestManager.Instance.GetActiveQuests()) {
            if (q.Def.questId == boundQuestId) { qi = q; break; }
        }

        // 제목 텍스트 설정
        if (titleText) {
            titleText.text = def.title;
        }

        // 보여줄 미션 범위 계산
        int missionCount = def.missions.Count;
        if (missionCount == 0) { return; }

        int endIndexInclusive = missionCount - 1;
        if (qi != null && qi.State == QuestState.Active && def.runMode == QuestRunMode.Sequential) {
            int currentIdx = GetCurrentMissionIndexByState(qi);
            endIndexInclusive = Mathf.Clamp(currentIdx, 0, missionCount - 1);
        }

        for (int i = 0; i <= endIndexInclusive; i++) {
            var md = def.missions[i];
            MissionState state = MissionState.Inactive;
            bool isCurrent = false;

            if (qi != null) {
                // 런타임 인스턴스의 상태를 사용
                if (i < qi.Missions.Count) {
                    state = qi.Missions[i].State;
                    isCurrent = (state == MissionState.Active);
                }
            }
            else {
                // 인스턴스 없으면 가정
                state = AssumeCompleted(boundQuestId) ? MissionState.Completed : MissionState.Inactive;
                isCurrent = false;
            }

            var item = GetFromPool(container);
            item.Setup(md.title, md.descriptionWhenActive, md.descriptionWhenDone, state, isCurrent);
        }
    }

    private int GetCurrentMissionIndexByState(QuestInstance qi) {
        // 가장 최근의 Active 미션 인덱스
        int lastCompleted = -1;
        for (int i = 0; i < qi.Missions.Count; i++) {
            switch (qi.Missions[i].State) {
                case MissionState.Active: return i;
                case MissionState.Completed: lastCompleted = i; break;
            }
        }
        return Mathf.Clamp(lastCompleted + 1, 0, qi.Missions.Count - 1);
    }

    private bool AssumeCompleted(string questId) {
        // 완료 리스트에 있다면 완료로 간주
        foreach (var id in QuestManager.Instance.GetCompletedQuestIds()) {
            if (id == questId) return true;
        }
        return false;
    }

    private void ClearContainer() {
        if (!container) { return; }
        for (int i = container.childCount - 1; i >= 0; --i) {
            var t = container.GetChild(i);
            if (t.TryGetComponent(out QuestMissionItemUI it)) {
                it.gameObject.SetActive(false);
                pool.Add(it);
            }
            else {
                Destroy(t.gameObject);
            }
        }
    }

    private QuestMissionItemUI GetFromPool(Transform parent) {
        for (int i = pool.Count - 1; i >= 0; --i) {
            var it = pool[i];
            pool.RemoveAt(i);
            it.transform.SetParent(parent, false);
            it.gameObject.SetActive(true);
            return it;
        }
        return Instantiate(itemPrefab, parent);
    }
}