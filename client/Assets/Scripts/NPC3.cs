using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// NPC4
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public class NPC3 : Actor {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(NPC3))]
	class NPC3Editor : EditorExtensions {
		NPC3 I => target as NPC3;
		public override void OnInspectorGUI() {
			Begin("NPC3");

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
			I.EventTrigger = ObjectField("Event Trigger", I.EventTrigger);
			End();
		}
	}
	#endif



	// Fields

	[SerializeField] EventTrigger m_EventTrigger;



	// Properties

	EventTrigger EventTrigger {
		get => m_EventTrigger;
		set => m_EventTrigger = value;
	}



	// Lifecycle

	protected override void Simulate() {
		if (EventTrigger && !string.IsNullOrEmpty(BehaviorName)) {
			EventTrigger.gameObject.SetActive(!BehaviorName.Equals("Sleep"));
		}
		switch (GameManager.IntValue["NPC3State"]) {
			// State 0: -
			case 0: {
			} break;
			// State 1: 퀘스트 시작
			case 1: {
				new QuestRuntimeFactory.Builder("npc3_quest", "동수의 심부름")
				.Collect(
					itemId: "WheatItem", target: 3,
					title: "밀 모으기",
					activeDesc: "밀 3개를 모으자.",
					doneDesc: "밀을 충분히 모았다. 다시 동수에게 가보자!"
				)
				.Talk(
					dialogueId: "npc3_quest_talk1",
					title: "다시 동수에게 말 걸기",
					activeDesc: "밀을 모았다. 동수에게 가져다주자",
					doneDesc: "동수에게 밀을 가져다주었다."
				)
				.Start();
				GameManager.IntValue["NPC3State"] = 2;
			} break;
			// State 2: 퀘스트 진행 중
			case 2: {
			} break;
			// State 3: 퀘스트 완료
			case 3: {
			} break;
		}
	}
}
