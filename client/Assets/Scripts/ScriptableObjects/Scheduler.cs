using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif



[Serializable]
public struct Behavior {
	public string name;

	public bool isTimeFixed;
	public float startTime;
	public float weight;
	public float duration;

	public bool isLocationBased;
	public Vector3 location;
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Scheduler
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[CreateAssetMenu(fileName = "Scheduler", menuName = "Scriptable Objects/Scheduler")]
public class Scheduler : ScriptableObject {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(Scheduler))]
	class SchedulerEditor : EditorExtensions {
		Scheduler I => target as Scheduler;
		public override void OnInspectorGUI() {
			Begin("Scheduler");

			HelpBox("All time values are in 24-hour format (0.0 to 24.0).", MessageType.Info);
			for (int i = 0; i < I.Behaviors.Count; i++) {
				var behavior = I.Behaviors[i];
				LabelField($" {behavior.name}", EditorStyles.boldLabel);

				BeginVertical(EditorStyles.helpBox);
				BeginHorizontal();
				behavior.name = TextField("Name", behavior.name);
				BeginDisabledGroup(i == 0);
				if (Button("↑", GUILayout.Width(18f))) {
					var temp = I.Behaviors[i - 1];
					I.Behaviors[i - 1] = I.Behaviors[i];
					I.Behaviors[i] = temp;
					break;
				}
				EndDisabledGroup();
				BeginDisabledGroup(i == I.Behaviors.Count - 1);
				if (Button("↓", GUILayout.Width(18f))) {
					var temp = I.Behaviors[i + 1];
					I.Behaviors[i + 1] = I.Behaviors[i];
					I.Behaviors[i] = temp;
					break;
				}
				EndDisabledGroup();
				if (Button("-", GUILayout.Width(18f))) {
					I.Behaviors.RemoveAt(i);
					break;
				}
				EndHorizontal();
				Space();

				BeginHorizontal();
				PrefixLabel("Is Time Fixed");
				behavior.isTimeFixed = ToggleLeft(behavior.isTimeFixed switch {
					true  => "Time Fixed",
					false => "Random",
				}, behavior.isTimeFixed);
				EndHorizontal();
				if (behavior.isTimeFixed) {
					behavior.startTime = FloatField("Start Time", behavior.startTime);
				} else {
					behavior.weight = Slider("Weight", behavior.weight, 0f, 1f);
				}
				behavior.duration = FloatField("Duration", behavior.duration);
				Space();

				BeginHorizontal();
				PrefixLabel("Is Location Based");
				behavior.isLocationBased = ToggleLeft(behavior.isLocationBased switch {
					true  => "Location Based",
					false => "Location Free",
				}, behavior.isLocationBased);
				EndHorizontal();
				if (behavior.isLocationBased) {
					var anchor = (GameObject)null;
					anchor = ObjectField("Anchor", anchor);
					if (anchor != null) behavior.location = anchor.transform.position;
					behavior.location = Vector3Field("Location", behavior.location);
				}
				I.Behaviors[i] = behavior;
				EndVertical();
			}
			if (Button("Add Behavior")) {
				I.Behaviors.Add(new Behavior() {
					name = "New Behavior",
					isTimeFixed = true,
					weight = 0.5f,
					isLocationBased = true,
				});
			}

			End();
		}
	}
	#endif



	// Fields

	[SerializeField] List<Behavior> m_Behaviors = new();



	// Properties

	List<Behavior> Behaviors {
		get => m_Behaviors;
	}



    // Methods

	public Vector3 GetNextBehavior(Actor actor) {
		float day = Mathf.Floor(EnvironmentManager.TimeOfDay);
		float timeOfDay = (EnvironmentManager.TimeOfDay - day) * 24f;

		foreach (var behavior in Behaviors) if (behavior.isTimeFixed) {
			float endTime = behavior.startTime + behavior.duration;
			if (behavior.startTime <= timeOfDay && timeOfDay < endTime) {
				actor.BehaviorName = behavior.name;
				actor.BehaviorStartTime = behavior.startTime;
				actor.BehaviorDuration = behavior.duration;
				return behavior.isLocationBased ? behavior.location : actor.transform.position;
			}
		}

		if (actor.BehaviorName == null) {
			float totalWeight = 0f;
			foreach (var behavior in Behaviors) if (!behavior.isTimeFixed) {
				totalWeight += Mathf.Max(behavior.weight);
			}
			float random = Random.Range(0f, totalWeight);
			foreach (var behavior in Behaviors) if (!behavior.isTimeFixed) {
				if (0f < (random -= Mathf.Max(behavior.weight))) continue;
				actor.BehaviorName = behavior.name;
				actor.BehaviorStartTime = timeOfDay;
				actor.BehaviorDuration = behavior.duration;
				return behavior.isLocationBased ? behavior.location : actor.transform.position;
			}
		} else {
			if (actor.BehaviorStartTime + actor.BehaviorDuration < timeOfDay) {
				actor.BehaviorName = null;
			} else foreach (var behavior in Behaviors) {
				if (behavior.name != actor.BehaviorName) continue;
				return behavior.isLocationBased ? behavior.location : actor.transform.position;
			}
		}

		return actor.transform.position;
	}
}
