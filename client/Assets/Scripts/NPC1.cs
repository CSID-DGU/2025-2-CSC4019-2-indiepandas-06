using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// NPC1
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public class NPC1 : Actor {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(NPC1))]
	class NPC1Editor : EditorExtensions {
		NPC1 I => target as NPC1;
		public override void OnInspectorGUI() {
			Begin("NPC1");

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
	}
}
