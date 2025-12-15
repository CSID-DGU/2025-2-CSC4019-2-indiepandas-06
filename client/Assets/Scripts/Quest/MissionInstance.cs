using System;

public enum MissionState { Inactive, Active, Completed }
public enum QuestState { Inactive, Active, Completed }

public abstract class MissionInstance {
    public MissionData Def { get; }
    public QuestInstance Owner { get; internal set; }

    public MissionState State { get; private set; } = MissionState.Inactive;
    public int Progress { get; protected set; }
    public int Target { get; protected set; }
    public event Action<MissionInstance> OnProgress;
    public event Action<MissionInstance> OnCompleted;

    protected MissionInstance(MissionData def) { Def = def; }

    public virtual void Activate() { 
        State = MissionState.Active;
        Bind();

        // 토스트 UI 표시
        string questTitle = Owner?.Def?.title ?? "(알 수 없음)";
        UIManager.ShowToast(ToastIconType.MissionStart, "다음 목표", $"{questTitle} - {Def.title}");
    }
    public virtual void Deactivate() {
        Unbind();
        State = MissionState.Inactive;
    }
    protected void ReportProgress(int current, int target) {
        Progress = current; Target = target;
        OnProgress?.Invoke(this);
        if (Progress >= Target && State != MissionState.Completed) {
            State = MissionState.Completed;
            Unbind();
            // 토스트 UI 표시
            string questTitle = Owner?.Def?.title ?? "(알 수 없음)";
            UIManager.ShowToast(ToastIconType.MissionComplete, "목표 달성", $"{questTitle} - {Def.title}");
            OnCompleted?.Invoke(this);
        }
    }

    // TODO 세이브 로드용 (이후 수정)
    public virtual void LoadProgress(int progress, int target, bool completed) {
        Progress = progress;
        Target = target;
        if (completed) {
            State = MissionState.Completed;
            Unbind(); // 이벤트 정리
        } else if (State == MissionState.Inactive) {
            // 저장된 상태가 진행중이면 Activate()가 따로 호출될 수도 있음
        }
    }

    protected abstract void Bind();
    protected abstract void Unbind();
}

// ===== 수집 미션 ==============================
public class CollectItemMissionInstance : MissionInstance {
    private readonly CollectItemMissionData data;
    public CollectItemMissionInstance(CollectItemMissionData data) : base(data) { this.data = data; }

    protected override void Bind() {
        GameplayEvents.OnItemChanged += OnItemChanged;
        // 시작 시 한 번 합산
        Recompute();
    }
    protected override void Unbind() {
        GameplayEvents.OnItemChanged -= OnItemChanged;
    }
    private void OnItemChanged(string changedItemId) {
        if (changedItemId != null && changedItemId != data.itemId) { return; }
        Recompute();
    }
    private void Recompute() {
        // 인벤토리에서 해당 아이템 총 개수 합산
        int count = 0;
        
        // 안전하게 모든 인벤토리 순회해서 합산하는 편도 가능
        foreach (var kv in new[] {
            ItemType.Tool, ItemType.Seed, ItemType.Food, ItemType.Equipment, ItemType.MapObject, ItemType.Special, ItemType.ETC
        }) {
            var I = InventoryManager.Instance.GetInventory(kv);
            foreach (var it in I.Items) {
                if (it?.ItemData?.ItemId == data.itemId) {
                    count += it.Count;
                }
            }
        }
        ReportProgress(count, data.targetCount);
    }
}

// ===== NPC 대화 미션 ==============================
public class TalkToNpcMissionInstance : MissionInstance {
    private readonly TalkToNpcMissionData data;
    public TalkToNpcMissionInstance(TalkToNpcMissionData data) : base(data) { this.data = data; }
    protected override void Bind() {
        GameplayEvents.OnNpcTalked += OnNpc;
    }
    protected override void Unbind() {
        GameplayEvents.OnNpcTalked -= OnNpc;
    }
    private void OnNpc(string npcId) {
        if (npcId == data.npcId) {
            ReportProgress(1, 1);
        }
    }
}

public class TalkDialogueMissionInstance : MissionInstance {
    private readonly TalkDialogueMissionData data;
    public TalkDialogueMissionInstance(TalkDialogueMissionData data) : base(data) { this.data = data; }

    protected override void Bind() {
        GameplayEvents.OnDialogueCompleted += OnDialogueDone;
    }
    protected override void Unbind() {
        GameplayEvents.OnDialogueCompleted -= OnDialogueDone;
    }
    private void OnDialogueDone(string id) {
        if (!string.IsNullOrEmpty(id) && id == data.dialogueId) {
            ReportProgress(1, 1);
        }
    }
}


// ===== 위치 도달 미션 ==============================
public class ReachLocationMissionInstance : MissionInstance {
    private readonly ReachLocationMissionData data;
    public ReachLocationMissionInstance(ReachLocationMissionData data) : base(data) { this.data = data; }
    protected override void Bind() {
        GameplayEvents.OnLocationReached += OnLoc;
    }
    protected override void Unbind() {
        GameplayEvents.OnLocationReached -= OnLoc;
    }
    private void OnLoc(string locId) {
        if (locId == data.locationId) {
            ReportProgress(1, 1);
        }
    }
}