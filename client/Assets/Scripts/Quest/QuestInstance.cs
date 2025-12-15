using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestInstance {
    public QuestData Def { get; }
    public QuestState State { get; private set; } = QuestState.Inactive;
    public int CurrentIndex { get; private set; } = 0; // Sequential 모드에서의 현재 미션 인덱스
    public readonly List<MissionInstance> Missions = new();

    public event Action<QuestInstance> OnQuestProgress;
    public event Action<QuestInstance> OnQuestCompleted;

    public QuestInstance(QuestData def) {
        Def = def;
        foreach (var m in def.missions) {
            var mi = m.CreateRuntime();
            mi.Owner = this;
            mi.OnProgress += _ => OnQuestProgress?.Invoke(this);
            mi.OnCompleted += OnMissionCompleted;
            Missions.Add(mi);
        }
    }

    public void Activate() {
        if (State == QuestState.Active) return;
        State = QuestState.Active;
        if (Def.runMode == QuestRunMode.Sequential) {
            if (Missions.Count > 0) Missions[0].Activate();
        } else {
            foreach (var m in Missions) m.Activate();
        }
        OnQuestProgress?.Invoke(this);
    }

    void OnMissionCompleted(MissionInstance mi) {
        if (Def.runMode == QuestRunMode.Sequential) {
            // 다음 미션 활성화
            CurrentIndex = Mathf.Clamp(CurrentIndex + 1, 0, Missions.Count);
            if (CurrentIndex < Missions.Count) {
                Missions[CurrentIndex].Activate();
            } else {
                Complete();
            }
        } else {
            // 병렬모드: 모두 완료 시 퀘스트 완료
            foreach (var m in Missions) if (m.State != MissionState.Completed) return;
            Complete();
        }
        OnQuestProgress?.Invoke(this);
    }

    void Complete() {
        State = QuestState.Completed;
        foreach (var m in Missions) m.Deactivate();
        OnQuestCompleted?.Invoke(this);
    }
}