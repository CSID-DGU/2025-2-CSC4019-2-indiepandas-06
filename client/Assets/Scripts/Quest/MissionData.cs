using UnityEngine;
using System.Collections.Generic;

public enum QuestRunMode { Sequential, Parallel }
public enum MissionType { CollectItem, TalkToNpc, ReachLocation, BuildObject }

public abstract class MissionData : ScriptableObject {
    [TextArea] public string title;
    [TextArea] public string descriptionWhenActive;
    [TextArea] public string descriptionWhenDone;
    public abstract MissionInstance CreateRuntime();
}

[CreateAssetMenu(menuName = "Quest/Mission/Collect Item")]
public class CollectItemMissionData : MissionData {
    public string itemId;
    public int targetCount = 1;
    public override MissionInstance CreateRuntime() => new CollectItemMissionInstance(this);
}

[CreateAssetMenu(menuName = "Quest/Mission/Talk To NPC")]
public class TalkToNpcMissionData : MissionData {
    public string npcId;
    public override MissionInstance CreateRuntime() => new TalkToNpcMissionInstance(this);
}

[CreateAssetMenu(menuName = "Quest/Mission/Talk Dialogue")]
public class TalkDialogueMissionData : MissionData {
    public string dialogueId;
    public override MissionInstance CreateRuntime() => new TalkDialogueMissionInstance(this);
}

[CreateAssetMenu(menuName = "Quest/Mission/Reach Location")]
public class ReachLocationMissionData : MissionData {
    public string locationId;
    public override MissionInstance CreateRuntime() => new ReachLocationMissionInstance(this);
}

[CreateAssetMenu(menuName = "Quest/Quest Definition")]
public class QuestData : ScriptableObject {
    public string questId;
    public string title;
    [TextArea] public string description;
    public QuestRunMode runMode = QuestRunMode.Sequential;
    public List<MissionData> missions = new();
    public List<string> nextQuestIds = new();
    // TODO 보상 정보(골드, 아이템, 값 증가 등)
}