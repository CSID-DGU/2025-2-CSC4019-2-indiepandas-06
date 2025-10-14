using UnityEngine;
using UnityEngine.Events;


#if UNITY_EDITOR
using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Minsu
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public class Minsu : Actor {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(Minsu))]
	class MinsuEditor : EditorExtensions {
		Minsu I => target as Minsu;
		public override void OnInspectorGUI() {
			Begin("Minsu");

			LabelField("Animator", EditorStyles.boldLabel);
			I.BodyAnimator    = ObjectField("Body Animator",    I.BodyAnimator);
			I.EmotionAnimator = ObjectField("Emotion Animator", I.EmotionAnimator);
			Space();
			LabelField("Physics", EditorStyles.boldLabel);
			I.Speed = FloatField("Speed", I.Speed);
			Space();
			LabelField("Group Event", EditorStyles.boldLabel);
			I.Event = ObjectField("Event", I.Event);
			I.EventProbability = Slider("Event Probability", I.EventProbability, float.Epsilon, 1f);
			I.EventInterval = FloatField("Event Interval", I.EventInterval);
			I.SearchRange   = FloatField("Search Range",   I.SearchRange);
			Space();

			End();
		}
	}
	#endif



	// Constants

	const float CommunicationRange = 4f;



	// Fields

	[SerializeField] GameObject m_Event;
	[SerializeField] float m_EventProbability = 0.2f;
	[SerializeField] float m_EventInterval = 1f;
	[SerializeField] float m_SearchRange = 8f;
	int m_EventState;
	float m_Timer;
	Actor m_Target;



	// Properties

	public GameObject Event {
		get => m_Event;
		set => m_Event = value;
	}
	public float EventProbability {
		get => m_EventProbability;
		set => m_EventProbability = value;
	}
	public float EventInterval {
		get => m_EventInterval;
		set => m_EventInterval = value;
	}
	public float SearchRange {
		get => m_SearchRange;
		set => m_SearchRange = value;
	}
	int EventState {
		get => m_EventState;
		set => m_EventState = value;
	}
	float Timer {
		get => m_Timer;
		set => m_Timer = value;
	}
	Actor Target {
		get => m_Target;
		set => m_Target = value;
	}



	// Methods

	void Start() => Event.gameObject.SetActive(false);

	protected override void Simulate() {
		Timer -= Time.deltaTime;
		switch (EventState) {

			case 0:
				if (Timer <= 0f) {
					Timer = EventInterval;
					if (Random.value < EventProbability) {
						Target = SearchTarget(SearchRange);
						if (Target) {
							Timer = 10f;
							Target.IsSimulated = false;
							CalculatePath(Target.gameObject);
							Target.CalculatePath(gameObject);
							EventState = 1;
						}
					}
				}
				break;

			case 1:
				if (Timer <= 0f) {
					Timer = EventInterval;
					ClearPath();
					Target.ClearPath();
					Target.IsSimulated = true;
					Target = null;
					EventState = 0;
				} else {
					if (GetDistance(Target.gameObject) < CommunicationRange) {
						Timer = 10f;
						ClearPath();
						Target.ClearPath();
						BeginCommunication(Target);
						EventState = 2;
						if (Event.TryGetComponent(out EventTrigger trigger)) {
							trigger.OnInteract.AddListener(() => {
								trigger.OnInteract.RemoveAllListeners();
								Timer = EventInterval;
								EndCommunication(Target);
								Target.IsSimulated = true;
								Target = null;
								EventState = 3;
							});
						}
					}
				}
				break;

			case 2:
				if (Timer <= 0f) {
					Timer = EventInterval;
					EndCommunication(Target);
					Target.IsSimulated = true;
					Target = null;
					EventState = 0;
				}
				break;
		}
	}



	Actor SearchTarget(float range) {
		Actor target = null;
		foreach (var actor in Actors) {
			if (actor == this || actor is Player || !actor.IsSimulated) continue;
			float distance = GetDistance(actor.gameObject);
			if (distance < range) {
				range = distance;
				target = actor;
			}
		}
		return target;
	}

	void BeginCommunication(Actor target) {
		Emotion = target.Emotion = Emotion.Thinking;
		LookAt(target.gameObject);
		target.LookAt(gameObject);
		Event.gameObject.SetActive(true);
		var a = transform.position;
		var b = target.transform.position;
		Event.transform.position = Vector3.Lerp(a, b, 0.5f);
		EventState = 2;
	}

	void EndCommunication(Actor target) {
		Emotion = target.Emotion = Emotion.None;
		Event.gameObject.SetActive(false);
		EventState = 0;
	}
}
