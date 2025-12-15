using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// NPCDetective
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public class NPCDetective : Actor {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(NPCDetective))]
	class NPCDetectiveEditor : EditorExtensions {
		NPCDetective I => target as NPCDetective;
		public override void OnInspectorGUI() {
			Begin("NPCDetective");

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
			I.ScenarioRunner = ObjectField("Scenario Runner", I.ScenarioRunner);
			I.Scenario = ObjectField("Scenario", I.Scenario);
			End();
		}
	}
	#endif



	// Fields

	[SerializeField] float m_SpawnProbability;
	[SerializeField] EventTrigger m_EventTrigger;
	[SerializeField] DetectiveScenarioRunner m_ScenarioRunner;
	[SerializeField] DetectiveScenarioSO m_Scenario;
	float m_Timer;



	// Properties

	float SpawnProbability {
		get => m_SpawnProbability;
		set => m_SpawnProbability = value;
	}

	EventTrigger EventTrigger {
		get => m_EventTrigger;
		set => m_EventTrigger = value;
	}
	DetectiveScenarioRunner ScenarioRunner {
		get => m_ScenarioRunner;
		set => m_ScenarioRunner = value;
	}
	DetectiveScenarioSO Scenario {
		get => m_Scenario;
		set => m_Scenario = value;
	}
	float Timer {
		get => m_Timer;
		set => m_Timer = value;
	}



	// Lifecycle

	protected override void Simulate() {
		if (EventTrigger && !string.IsNullOrEmpty(BehaviorName)) {
			EventTrigger.gameObject.SetActive(!BehaviorName.Equals("Sleep"));
		}

		switch (GameManager.IntValue["NPCDetectiveState"]) {
			// State 0: 미등장
			case 0: {
				float time = EnvironmentManager.TimeOfDay % 1f;
				if (time - Time.deltaTime / EnvironmentManager.DayLength < 0.1f && 0.1f <= time) {
					if (Random.value < SpawnProbability) GameManager.IntValue["NPCDetectiveState"] = 1;
				}
				PathPoints.Clear();
				BehaviorName = "Sleep";
				UseScheduler = false;
			} break;
			// State 1: 등장
			case 1: {
				float time = EnvironmentManager.TimeOfDay % 1f;
				if (time - Time.deltaTime / EnvironmentManager.DayLength < 0.1f && 0.1f <= time) {
					GameManager.IntValue["NPCDetectiveState"] = 0;
				}
				UseScheduler = true;
			} break;
			// State 2:
			case 2: {
				ScenarioRunner.Play(Scenario);
				GameManager.IntValue["NPCDetectiveState"] = 3;
				UseScheduler = false;
			} break;
			// State 3:
			case 3: {
				UseScheduler = false;
				if (UIManager.CurrentScreen == Screen.Game && GameManager.GameState == GameState.Gameplay) {
					GameManager.IntValue["NPCDetectiveState"] = 4;
					Timer = 1f;
				}
			} break;
			// State 4:
			case 4: {
				Timer -= Time.deltaTime;
				if (Timer <= 0f) {
					PathPoints.Clear();
					BehaviorName = "Sleep";
					UseScheduler = false;
				}
			} break;
		}
	}
}
