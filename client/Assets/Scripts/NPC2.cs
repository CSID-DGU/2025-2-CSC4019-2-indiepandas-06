using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// NPC2
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public class NPC2 : Actor {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(NPC2))]
	class NPC2Editor : EditorExtensions {
		NPC2 I => target as NPC2;
		public override void OnInspectorGUI() {
			Begin("NPC2");

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
		switch (GameManager.IntValue["NPC2State"]) {
			// State 0: -
			case 0: {
			} break;
			// State 1: 퀘스트 시작
			case 1: {
				new QuestRuntimeFactory.Builder("npc2_quest", "다솔의 버스킹")
				.Talk(
					dialogueId: "npc2_quest_talk1",
					title: "버스킹 중 떠난 이유1",
					activeDesc: "사람들이 버스킹을 듣다가 떠난 이유를 알아내자.",
					doneDesc: "주민으로부터 버스킹을 듣다가 떠난 이유를 들었다!"
				)
				.Talk(
					dialogueId: "npc2_quest_talk2",
					title: "버스킹 중 떠난 이유2",
					activeDesc: "사람들이 버스킹을 듣다가 떠난 이유를 알아내자.",
					doneDesc: "주민으로부터 버스킹을 듣다가 떠난 이유를 들었다!"
				)
				.Talk(
					dialogueId: "npc2_quest_talk3",
					title: "떠난 이유 들려주기",
					activeDesc: "사람들이 버스킹을 듣다가 떠난 이유를 다솔에게 말해주자.",
					doneDesc: "다솔에게 사람들이 버스킹을 듣다가 떠난 이유를 말해주었다!"
				)
				.Start();
				GameManager.IntValue["NPC2State"] = 2;
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
