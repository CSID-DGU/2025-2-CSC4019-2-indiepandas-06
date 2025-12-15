using UnityEngine;
using System;
using System.Collections.Generic;

public static class QuestRuntimeFactory {
    // 공통: 런타임 SO 생성 헬퍼
    static T NewSO<T>() where T : ScriptableObject {
        var so = ScriptableObject.CreateInstance<T>();
#if UNITY_EDITOR
        so.hideFlags = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy;
#else
        so.hideFlags = HideFlags.HideAndDontSave;
#endif
        return so;
    }
    
    public sealed class Builder {
        readonly QuestData q;

        public Builder(string questId, string title, string desc = null, QuestRunMode mode = QuestRunMode.Sequential) {
            q = NewSO<QuestData>();
            q.questId = string.IsNullOrWhiteSpace(questId) ? Guid.NewGuid().ToString() : questId;
            q.title = title ?? "(Untitled Quest)";
            q.description = desc ?? "";
            q.runMode = mode;
        }

        public Builder Talk(string dialogueId, string title, string activeDesc, string doneDesc) {
            var m = NewSO<TalkDialogueMissionData>();
            m.dialogueId = dialogueId;
            m.title = title;
            m.descriptionWhenActive = activeDesc;
            m.descriptionWhenDone   = doneDesc;
            q.missions.Add(m);
            return this;
        }

        public Builder Collect(string itemId, int target, string title, string activeDesc, string doneDesc) {
            var m = NewSO<CollectItemMissionData>();
            m.itemId = itemId;
            m.targetCount = Mathf.Max(1, target);
            m.title = title;
            m.descriptionWhenActive = activeDesc;
            m.descriptionWhenDone   = doneDesc;
            q.missions.Add(m);
            return this;
        }

        public Builder Reach(string locationId, string title, string activeDesc, string doneDesc) {
            var m = NewSO<ReachLocationMissionData>();
            m.locationId = locationId;
            m.title = title;
            m.descriptionWhenActive = activeDesc;
            m.descriptionWhenDone   = doneDesc;
            q.missions.Add(m);
            return this;
        }

        // 다음 퀘스트 체인 등록
        public Builder ChainTo(params string[] nextQuestIds) {
            if (nextQuestIds != null) q.nextQuestIds.AddRange(nextQuestIds);
            return this;
        }

        public QuestData Build() => q;

        public bool Start() => QuestManager.Instance.StartQuest(q);
    }


}
