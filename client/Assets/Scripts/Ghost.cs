using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Ghost
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public class Ghost : Actor {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(Ghost))]
	class GhostEditor : EditorExtensions {
		Ghost I => target as Ghost;
		public override void OnInspectorGUI() {
			Begin("Ghost");

			LabelField("Animator", EditorStyles.boldLabel);
			I.BodyAnimator    = ObjectField("Body Animator",    I.BodyAnimator);
			I.EmotionAnimator = ObjectField("Emotion Animator", I.EmotionAnimator);
			Space();
			LabelField("Physics", EditorStyles.boldLabel);
			I.Speed = FloatField("Speed", I.Speed);
			Space();
			LabelField("Schedule", EditorStyles.boldLabel);
			I.Scheduler = ObjectField("Scheduler", I.Scheduler);
			if (I.Scheduler) {
				BeginDisabledGroup();
				LabelField("Behavior", I.BehaviorName);
				EndDisabledGroup();
			}
			I.SpawnProbability = Slider("Spawn Probability", I.SpawnProbability, 0f, 1f);
			I.EventTrigger = ObjectField("Event Trigger", I.EventTrigger);
			End();
		}
	}
	#endif



	// Fields

	[SerializeField] float m_SpawnProbability;
	[SerializeField] EventTrigger m_EventTrigger;



	// Properties

	float SpawnProbability {
		get => m_SpawnProbability;
		set => m_SpawnProbability = value;
	}

	EventTrigger EventTrigger {
		get => m_EventTrigger;
		set => m_EventTrigger = value;
	}



	// Lifecycle

	protected override void Simulate() {
		if (EventTrigger && !string.IsNullOrEmpty(BehaviorName)) {
			EventTrigger.gameObject.SetActive(!BehaviorName.Equals("Sleep"));
		}

		switch (GameManager.IntValue["GhostState"]) {
			// State 0: 미등장
			case 0: {
				float time = EnvironmentManager.TimeOfDay % 1f;
				if (time - Time.deltaTime / EnvironmentManager.DayLength < 0.5f && 0.5f <= time) {
					if (Random.value < SpawnProbability) GameManager.IntValue["GhostState"] = 1;
				}
				PathPoints.Clear();
				BehaviorName = "Sleep";
				UseScheduler = false;
			} break;
			// State 1: 등장
			case 1: {
				float time = EnvironmentManager.TimeOfDay % 1f;
				if (time - Time.deltaTime / EnvironmentManager.DayLength < 0.5f && 0.5f <= time) {
					GameManager.IntValue["GhostState"] = 0;
				}
				UseScheduler = true;
			} break;
			// State 2: 퀘스트 시작
			case 2: {
				new QuestRuntimeFactory.Builder("ghost_quest", "유령의 부탁")
				.Talk(
					dialogueId: "ghost_quest_talk1",
					//title: "전 마을이장의 도움을 받았던 주민들 찾기",
					title: "도움받은 주민 찾기",
					activeDesc: "주민들에게서 유령이 생전 남긴 성과들을 듣자.",
					doneDesc: "주민으로부터 유령에게 도움받았던 이야기를 들었다!"
				)
				.Talk(
					dialogueId: "ghost_quest_talk2",
					//title: "전 마을이장의 도움을 받았던 주민들 찾기",
					title: "도움받은 주민 찾기",
					activeDesc: "주민들에게서 유령이 생전 남긴 성과들을 듣자.",
					doneDesc: "주민으로부터 유령에게 도움받았던 이야기를 들었다!"
				)
				.Talk(
					dialogueId: "ghost_quest_talk3",
					//title: "사람들이 도움받았던 이야기를 말해주기",
					title: "도움받은 이야기 전하기",
					activeDesc: "전 마을이장에게 사람들이 도움받았던 이야기를 말해주자.",
					doneDesc: "전 마을이장에게 사람들이 도움받았던 이야기를 말해주었다!"
				)
				.Start();
				GameManager.IntValue["GhostState"] = 3;
				UseScheduler = true;
			} break;
			// State 3: 퀘스트 진행 중
			case 3: {
				UseScheduler = true;
			} break;
			// State 4: 퀘스트 완료
			case 4: {
				PathPoints.Clear();
				BehaviorName = "Sleep";
				UseScheduler = false;
			} break;
		}
	}
}
