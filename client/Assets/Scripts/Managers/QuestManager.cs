using UnityEngine;
using System;
using System.Collections.Generic;


public class QuestManager : MonoSingleton<QuestManager> {
    [SerializeField] private List<QuestData> questDefs = new();

    private readonly Dictionary<string, QuestData> defDict = new();
    private readonly Dictionary<string, QuestInstance> active = new();
    public IReadOnlyDictionary<string, QuestInstance> Active => active;
    private readonly HashSet<string> completed = new();

    public bool IsCompleted(string questId) => completed.Contains(questId);
    public bool TryGetDefinition(string id, out QuestData def) => defDict.TryGetValue(id, out def);

    public IEnumerable<QuestInstance> GetActiveQuests() => active.Values;
    public IEnumerable<string> GetCompletedQuestIds() => completed;
    
    public event System.Action<string> OnQuestStarted;
    public event System.Action<string> OnQuestCompleted;
    public event System.Action<string> OnQuestRemoved;
    public event System.Action<string> OnQuestProgress;

    // TODO 테스트 코드
    void Start() {
        /*
        var ok = new QuestRuntimeFactory.Builder("test_q1", "예진이의 버스킹", "런타임 생성 테스트 퀘스트")
            .Talk(
                dialogueId: "q1_yejin",
                title: "민수와 대화하기",
                activeDesc: "울고있는 예진이를 발견했다! 버스킹을 했는데 관객들이 떠나갔다고, 자기는 음악에 재능이 없는 것 같다고 한다.\n 진짜 별로여서 떠난 걸까? 관객을 찾아 대화해보자!\n우선 민수와 대화해보자.",
                doneDesc: "민수와 대화를 마쳤다.\n민수는 약속이 있어서 먼저 떠난 것이었다. 오히려 예진이의 음색을 칭찬했다."
            )
            .Collect(
                itemId: "CarrotItem", target: 4,
                title: "정민과 대화하기",
                activeDesc: "다음 관객인 정민을 만나 대화해보자!",
                doneDesc: "당근을 충분히 모았다. 다시 예진에게 가보자!"
            )
            .Talk(
                dialogueId: "q1_yejin2",
                title: "다시 예진이에게 말 걸기",
                activeDesc: "예진에게 말을 걸어보자",
                doneDesc: "예진과 대화를 마쳤다. 고생했다고 한다!"
            )
            .Start();

        ok = new QuestRuntimeFactory.Builder("test_q2", "쓸모 없는 선물?", "런타임 생성 테스트 퀘스트")
            .Talk(
                dialogueId: "q2_yejin",
                title: "관객을 찾아 대화하기 - 민수",
                activeDesc: "울고있는 예진이를 발견했다! 버스킹을 했는데 관객들이 떠나갔다고, 자기는 음악에 재능이 없는 것 같다고 한다.\n 진짜 별로여서 떠난 걸까? 관객을 찾아 대화해보자!\n우선 민수와 대화해보자.",
                doneDesc: "민수와 대화를 마쳤다.\n민수는 약속이 있어서 먼저 떠난 것이었다. 오히려 예진이의 음색을 칭찬했다."
            )
            .Start();
        
        ok = new QuestRuntimeFactory.Builder("test_q3", "낙담한 목수", "런타임 생성 테스트 퀘스트")
            .Talk(
                dialogueId: "q3_yejin",
                title: "관객을 찾아 대화하기 - 민수",
                activeDesc: "울고있는 예진이를 발견했다! 버스킹을 했는데 관객들이 떠나갔다고, 자기는 음악에 재능이 없는 것 같다고 한다.\n 진짜 별로여서 떠난 걸까? 관객을 찾아 대화해보자!\n우선 민수와 대화해보자.",
                doneDesc: "민수와 대화를 마쳤다.\n민수는 약속이 있어서 먼저 떠난 것이었다. 오히려 예진이의 음색을 칭찬했다."
            )
            .Start();
*/
        /*
        // 1) 퀘스트 SO 즉석 생성
        var qq = ScriptableObject.CreateInstance<QuestData>();
        qq.questId = System.Guid.NewGuid().ToString();    // 임시 ID
        qq.title = "테스트2";
        qq.description = "런타임 생성 테스트 퀘스트2";
        qq.runMode = QuestRunMode.Sequential;

        // 2) 미션들 생성: Talk -> Collect -> Talk
        var m11 = ScriptableObject.CreateInstance<TalkToNpcMissionData>();
        m11.title = "예진에게 말 걸기";
        m11.descriptionWhenActive = "Yejin에게 말을 걸어보자";
        m11.descriptionWhenDone = "예진과 대화를 마쳤다";
        m11.npcId = "yejin";
        qq.missions.Add(m11);
        ok = QuestManager.Instance.StartQuest(qq);
        */
    }


    protected override void Awake() {
        base.Awake();
        defDict.Clear();
        foreach (var q in questDefs) {
            if (q && !string.IsNullOrEmpty(q.questId)) {
                defDict[q.questId] = q;
            }
        }
    }

    // SO 에셋으로 등록된 퀘스트용
    // ID로 퀘스트 시작
    public bool StartQuest(string questId) {
        if (!defDict.TryGetValue(questId, out var def)) {
            return false;
        }
        if (completed.Contains(questId) || active.ContainsKey(questId)) {
            return false;
        }

        var qi = new QuestInstance(def);
        Hook(qi);
        active[questId] = qi;
        qi.Activate();

        // 퀘스트 시작 알림
        var id = qi.Def.questId;
        OnQuestStarted?.Invoke(id);

        // 토스트 UI 표시
        UIManager.ShowToast(ToastIconType.QuestStart, "퀘스트 시작", qi.Def.title);

        return true;
    }

    // SO로 직접 시작 (런타임 생성/외부 로딩 포함)
    public bool StartQuest(QuestData def) {
        if (def == null || string.IsNullOrEmpty(def.questId)) {
            return false;
        }
        if (completed.Contains(def.questId) || active.ContainsKey(def.questId)) {
            return false;
        }

        // 딕셔너리에 등록
        if (!defDict.ContainsKey(def.questId)) {
            defDict[def.questId] = def;
        }

        var qi = new QuestInstance(def);
        Hook(qi);
        active[def.questId] = qi;
        qi.Activate();

        // 퀘스트 시작 알림
        var id = qi.Def.questId;
        OnQuestStarted?.Invoke(id);

        // 토스트 UI 표시
        UIManager.ShowToast(ToastIconType.QuestStart, "퀘스트 시작", def.title);
        
        return true;
    }

    void Hook(QuestInstance qi) {
        // 퀘스트 인스턴스에 구독
        qi.OnQuestCompleted += OnCompleted;
        qi.OnQuestProgress += (q) => {
            OnQuestProgress?.Invoke(q.Def.questId);
        };
    }

    void OnCompleted(QuestInstance qi) {
        var id = qi.Def.questId;
        active.Remove(id);
        completed.Add(id);

        // 퀘스트 완료 알림
        OnQuestCompleted?.Invoke(id);

        // 토스트 UI 표시
        string questTitle = qi?.Def?.title ?? "(알 수 없음)";
        UIManager.ShowToast(ToastIconType.QuestComplete, "퀘스트 완료", questTitle);

        // TODO: 보상 지급
        // 다음 퀘스트 체인 자동 오픈
        foreach (var next in qi.Def.nextQuestIds) {
            StartQuest(next);
        }
        // TODO: 저장
    }

    // 미션 활성화 상태
    public bool IsMissionActive(string questId, int missionIndex) {
        if (string.IsNullOrEmpty(questId)) {
            return false;
        }
        if (!active.TryGetValue(questId, out var qi)) {
            return false;
        }

        var list = qi.Missions;
        if (missionIndex < 0 || missionIndex >= list.Count) {
            return false;
        }

        return list[missionIndex].State == MissionState.Active;
    }
    // 퀘스트 활성화 상태
    public bool IsQuestActive(string questId) {
        if (!active.TryGetValue(questId, out var qi)) {
            return false;
        }
        foreach (var m in qi.Missions) {
            if (m.State == MissionState.Active) {
                return true;
            }
        }
        return false;
    }

    // 현재 진행 중인 미션 인덱스 얻기 (Sequential 기준)
    public bool TryGetCurrentMissionIndex(string questId, out int index) {
        index = -1;
        if (!active.TryGetValue(questId, out var qi)) {
            return false;
        }

        if (qi.Def.runMode == QuestRunMode.Sequential) {
            int i = Mathf.Clamp(qi.CurrentIndex, 0, qi.Missions.Count - 1);
            if (qi.Missions.Count == 0) return false;
            if (qi.Missions[i].State == MissionState.Active) {
                index = i;
                return true;
            }
            // 복원 로직 등으로 인덱스/상태가 틀어졌을 때 방어적으로 스캔
            for (int k = 0; k < qi.Missions.Count; k++) {
                if (qi.Missions[k].State == MissionState.Active) {
                    index = k;
                    return true;
                }
            }
            return false;
        } else {
            // Parallel이면 Active인 것 중 첫 번째 반환
            for (int k = 0; k < qi.Missions.Count; k++) {
                if (qi.Missions[k].State == MissionState.Active) {
                    index = k;
                    return true;
                }
            }
            return false;
        }
    }

    // 저장/로드용 스냅샷
    [Serializable] class SaveData {
        public List<string> completed = new();
        public List<string> active = new();
        public List<int> currentIndex = new();
        public List<int[]> progresses = new();
    }

    public object CaptureState() {
        var sd = new SaveData();
        sd.completed.AddRange(completed);
        foreach (var kv in active) {
            sd.active.Add(kv.Key);
            sd.currentIndex.Add(kv.Value.CurrentIndex);
            var arr = new int[kv.Value.Missions.Count];
            for (int i = 0; i < arr.Length; i++) arr[i] = kv.Value.Missions[i].Progress;
            sd.progresses.Add(arr);
        }
        return sd;
    }

    public void RestoreState(object state) {
        var sd = state as SaveData;
        if (sd == null) return;
        completed.Clear();
        foreach (var c in sd.completed) completed.Add(c);
        active.Clear();
        for (int i = 0; i < sd.active.Count; i++) {
            if (!defDict.TryGetValue(sd.active[i], out var def)) continue;
            var qi = new QuestInstance(def);
            active[sd.active[i]] = qi;
            Hook(qi);
            qi.Activate();
            // 간단 복원(정확 복원은 MissionInstance마다 별 저장 필드 추가)
            if (def.runMode == QuestRunMode.Sequential) {
                // 앞 미션들을 강완 처리 후 현재 미션 활성화
                int idx = Mathf.Clamp(sd.currentIndex[i], 0, qi.Missions.Count);
                for (int k = 0; k < idx; k++) qi.Missions[k].Activate(); // Activate→Completed 강제로 만들려면 별도 API 필요
            }
        }
    }
}