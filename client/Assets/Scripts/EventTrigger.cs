using UnityEngine;
using UnityEngine.Events;


#if UNITY_EDITOR
using UnityEditor;
#endif



// Trigger Types

public enum TriggerType {
	OnInteract,
	InRange,
};



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Event Trigger
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("Component/Event Trigger")]
[RequireComponent(typeof(Collider2D))]
public sealed class EventTrigger : MonoBehaviour, IInteractable {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(EventTrigger))]
	class EventTriggerAuthoringEditor : EditorExtensions {
		EventTrigger I => target as EventTrigger;
		public override void OnInspectorGUI() {
			Begin("Event Trigger");

			LabelField("Event", EditorStyles.boldLabel);
			I.Event = ObjectField("Event Graph", I.Event);
			if (I.Event == null && Button("Create Event Graph")) {
				I.Event = CreateInstance<EventGraph>();
			}
			if (I.Event != null && Button("Open Event Graph")) {
				I.Event.name = I.gameObject.name;
				I.Event.Open();
			}
			Space();
			LabelField("Trigger", EditorStyles.boldLabel);
			I.InteractionType = EnumField("Interaction Type", I.InteractionType);
			I.TriggerType = EnumField("Trigger Type", I.TriggerType);
			I.PlayerOnly = Toggle("Player Only", I.PlayerOnly);
			var width = GUILayout.Width(18f);
			BeginHorizontal();
			PrefixLabel("Use Count Limit");
			I.UseCountLimit = EditorGUILayout.Toggle(I.UseCountLimit, width);
			if (I.UseCountLimit) I.Count = IntField(I.Count);
			EndHorizontal();
			BeginHorizontal();
			PrefixLabel("Use Cooldown");
			I.UseCooldown = EditorGUILayout.Toggle(I.UseCooldown, width);
			if (I.UseCooldown) I.Cooldown = FloatField(I.Cooldown);
			EndHorizontal();
			Space();
			PropertyField("m_OnInteract");
			Space();

			End();
		}

		void OnSceneGUI() {
			if (I.Event == null) return;
			if (I.Event.Clone != null) {
				Tools.current = Tool.None;
				foreach (var data in I.Event.Clone.GetEvents()) data.DrawHandles();
			}
		}
	}

	void OnDrawGizmosSelected() {
		if (Event == null) return;
		Gizmos.color = Color.green;
		foreach (var data in Event.Entry.GetEvents()) data.DrawGizmos();

		if (Event.Clone == null) return;
		Gizmos.color = Color.white;
		foreach (var data in Event.Clone.GetEvents()) data.DrawGizmos();
	}
	#endif



	// Fields

	[SerializeField] EventGraph m_Event;
	[SerializeField] InteractionType m_InteractableType;
	[SerializeField] TriggerType m_TriggerType;
	[SerializeField] bool m_PlayerOnly = true;
	[SerializeField] int m_Count;
	[SerializeField] float m_Cooldown;
	float m_Timer;

	[SerializeField] UnityEvent m_OnInteract = new();



	// Properties

	public EventGraph Event {
		get => m_Event;
		set => m_Event = value;
	}
	public InteractionType InteractionType {
		get => m_InteractableType;
		set => m_InteractableType = value;
	}
	public TriggerType TriggerType {
		get => m_TriggerType;
		set => m_TriggerType = value;
	}
	public bool PlayerOnly {
		get => m_PlayerOnly;
		set => m_PlayerOnly = value;
	}
	public bool UseCountLimit {
		get => 0 <= m_Count;
		set => m_Count = value ? Mathf.Max(1, m_Count) : -1;
	}
	public int Count {
		get => m_Count;
		set => m_Count = value;
	}
	public bool UseCooldown {
		get => 0f <= m_Cooldown;
		set => m_Cooldown = value ? Mathf.Max(float.Epsilon, m_Cooldown) : -1f;
	}
	public float Cooldown {
		get => m_Cooldown;
		set => m_Cooldown = value;
	}
	float Timer {
		get => m_Timer;
		set => m_Timer = value;
	}

	public bool IsInteractable => (!UseCountLimit || 0 < Count) && (!UseCooldown || Timer <= 0f);

	public UnityEvent OnInteract => m_OnInteract;

	uint eventID;



	// Methods

	public void Interact(GameObject interactor) {
		if (IsInteractable && eventID == default) {
			OnInteract.Invoke();
			eventID = GameManager.PlayEvent(Event);
			if (UseCountLimit) Count--;
			if (UseCooldown) Timer = Cooldown;
		}
	}



	// Lifecycle

	void Update() {
		if (eventID != default && GameManager.IsEventPlaying(eventID)) return;
		eventID = default;
		Timer -= Time.deltaTime;
	}

	void OnTriggerStay2D(Collider2D other) {
		if (TriggerType != TriggerType.InRange) return;
		if (!PlayerOnly || other.TryGetComponent(out Player _)) Interact(other.gameObject);
	}
}
