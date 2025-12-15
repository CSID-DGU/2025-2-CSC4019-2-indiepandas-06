using UnityEngine;
using System.Collections.Generic;



// ━

public enum State {
	Idle,
	Moving,
	Sleep,
	Fishing_Cast,
	Fishing_Wait,
	Fishing_Bite,
	Fishing_Struggle,
	Fishing_Reel,
	Fishing_Carp,
	Fishing_Mudfish,
	Fishing_Crawfish,
	Item_Guitar,
	Item_FishingRod,
	Item_Chair,
	Item_Hoe,
};

public enum Emotion {
	None,
	Empty,
	Thinking,
	Embarrassed,
	Surprised,
	Smiling,
	Crying,
	Moved,
	Serious,
};



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Actor
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[RequireComponent(typeof(Rigidbody2D))]
public abstract class Actor : MonoBehaviour {

	// Fields

	static List<Actor> s_Actors = new();

	[SerializeField] Animator m_BodyAnimator;
	[SerializeField] Animator m_EmotionAnimator;
	State m_State;
	Emotion m_Emotion;
	List<SpriteRenderer> m_Renderers;

	bool m_IsSimulated = true;

	Rigidbody2D m_Body;
	Vector2 m_MoveVector;
	[SerializeField] float m_Speed = 4f;
	Queue<Vector3> m_PathPoints = new();

	[SerializeField] Scheduler m_Scheduler;
	bool m_UseScheduler = true;
	string m_BehaviorName;
	float m_BehaviorStartTime;
	float m_BehaviorDuration;

	SpriteRenderer m_BodyRenderer;
	[SerializeField] HashMap<string, RuntimeAnimatorController> m_AccessoryTable = new();
	List<(SpriteRenderer, Animator)> m_AccessoryList = new();
	Stack<(SpriteRenderer, Animator)> m_AccessoryPool = new();



	// Properties

	public static List<Actor> Actors {
		get => s_Actors;
	}



	protected Animator BodyAnimator {
		get => m_BodyAnimator;
		set => m_BodyAnimator = value;
	}
	protected Animator EmotionAnimator {
		get => m_EmotionAnimator;
		set => m_EmotionAnimator = value;
	}

	public State State {
		get => m_State;
		set {
			if (m_State != value) {
				m_State = value;
				BodyAnimator.Play(value.ToString());
				BodyAnimator.Update(0f);
			}
		}
	}
	public Emotion Emotion {
		get => m_Emotion;
		set {
			if (m_Emotion != value) {
				m_Emotion = value;
				EmotionAnimator.Play(value.ToString());
				EmotionAnimator.Update(0f);
			}
		}
	}

	protected List<SpriteRenderer> Renderers {
		get {
			if (m_Renderers == null) {
				m_Renderers = new();
				AddRendererRecursive(transform, m_Renderers);
			}
			return m_Renderers;
		}
	}

	static void AddRendererRecursive(Transform transform, List<SpriteRenderer> renderers) {
		if (transform.TryGetComponent(out SpriteRenderer renderer)) renderers.Add(renderer);
		for (int i = 0; i < transform.childCount; i++) {
			AddRendererRecursive(transform.GetChild(i), renderers);
		}
	}

	protected bool FlipX {
		get => (0 < Renderers.Count) ? Renderers[0].flipX : default;
		set {
			for (int i = 0; i < Renderers.Count; i++) Renderers[i].flipX = value;
		}
	}

	protected bool Hide {
		get => (0 < Renderers.Count) ? !Renderers[0].enabled : default;
		set {
			for (int i = 0; i < Renderers.Count; i++) Renderers[i].enabled = !value;
		}
	}



	public bool IsSimulated {
		get => m_IsSimulated;
		set {
			if (m_IsSimulated != value) {
				m_IsSimulated = value;
				EnableTriggerRecursive(transform, value);
			}
		}
	}

	static void EnableTriggerRecursive(Transform transform, bool enable) {
		if (transform.TryGetComponent(out EventTrigger trigger)) trigger.enabled = enable;
		for (int i = 0; i < transform.childCount; i++) {
			EnableTriggerRecursive(transform.GetChild(i), enable);
		}
	}



	protected Rigidbody2D Body => !m_Body ?
		m_Body = GetComponent<Rigidbody2D>() :
		m_Body;

	protected Vector2 MoveVector {
		get => m_MoveVector;
		set => m_MoveVector = value;
	}
	public float Speed {
		get           => m_Speed;
		protected set => m_Speed = value;
	}
	public Queue<Vector3> PathPoints {
		get => m_PathPoints;
	}



	protected Scheduler Scheduler {
		get => m_Scheduler;
		set => m_Scheduler = value;
	}
	protected bool UseScheduler {
		get => m_UseScheduler;
		set => m_UseScheduler = value;
	}
	public string BehaviorName {
		get => m_BehaviorName;
		set => m_BehaviorName = value;
	}
	public float BehaviorStartTime {
		get => m_BehaviorStartTime;
		set => m_BehaviorStartTime = value;
	}
	public float BehaviorDuration {
		get => m_BehaviorDuration;
		set => m_BehaviorDuration = value;
	}



	SpriteRenderer BodyRenderer => !m_BodyRenderer ?
		m_BodyRenderer = BodyAnimator.GetComponent<SpriteRenderer>() :
		m_BodyRenderer;

	protected HashMap<string, RuntimeAnimatorController> AccessoryTable {
		get => m_AccessoryTable;
	}
	List<(SpriteRenderer, Animator)> AccessoryList {
		get => m_AccessoryList;
	}
	Stack<(SpriteRenderer, Animator)> AccessoryPool {
		get => m_AccessoryPool;
	}



	// Methods

	protected float GetDistance(GameObject target) {
		if (target == null) return float.MaxValue;
		var xDelta = transform.position.x - target.transform.position.x;
		var yDelta = transform.position.y - target.transform.position.y;
		xDelta /= GameManager.GridXMultiplier;
		yDelta /= GameManager.GridYMultiplier;
		return Mathf.Sqrt(xDelta * xDelta + yDelta * yDelta);
	}

	protected float GetDistanceSq(GameObject target) {
		if (target == null) return float.MaxValue;
		var xDelta = transform.position.x - target.transform.position.x;
		var yDelta = transform.position.y - target.transform.position.y;
		xDelta *= GameManager.GridYMultiplier;
		yDelta *= GameManager.GridXMultiplier;
		return xDelta * xDelta + yDelta * yDelta;
	}



	public void LookAt(GameObject target) {
		if (target == null) return;
		var direction = target.transform.position - transform.position;
		FlipX = direction.x < 0f;
	}

	public void CalculatePath(GameObject target, float threshold = 0f) {
		if (target == null) return;
		var sourcePosition = transform.position;
		var targetPosition = target.transform.position;
		if (0f < threshold) {
			var distance = Vector3.Distance(sourcePosition, targetPosition);
			if (threshold < distance) {
				targetPosition += threshold * (sourcePosition - targetPosition) / distance;
			}
		}
		NavigationManager.TryGetPath(sourcePosition, targetPosition, PathPoints);
	}

	public void ClearPath() {
		PathPoints.Clear();
	}



	protected virtual void Simulate() { }

	protected virtual void Act() {
		if (Scheduler && UseScheduler) {
			var behaviorName = BehaviorName;
			var position = Scheduler.GetNextBehavior(this);
			if (behaviorName != BehaviorName) {
				if (!NavigationManager.TryGetPath(Body.position, (Vector2)position, PathPoints)) {
					BehaviorName = null;
				}
			}
		}
		if (PathPoints.TryPeek(out var point)) {
			MoveVector = (((Vector2)point - Body.position) / GameManager.GridMultiplier).normalized;
			Body.linearVelocity = GameManager.GridMultiplier * MoveVector * Speed;
			if (Vector2.Distance(Body.position, point) < 0.1f) {
				PathPoints.Dequeue();
				if (PathPoints.Count == 0) MoveVector = default;
			}
		}
	}

	protected virtual void Draw() {
		bool doDefault = false;
		switch (BehaviorName) {
			case "Sleep":
				if (PathPoints.Count == 0) {
					if (Hide != true) Hide = true;
					State = State.Sleep;
				} else doDefault = true;
				break;
			default:
				doDefault = true;
				break;
		}
		if (doDefault) {
			if (Hide != false) Hide = false;
			State = (0.1f < MoveVector.magnitude) ? State.Moving : State.Idle;
			FlipX = (MoveVector.x != 0f) ? MoveVector.x < 0f : FlipX;
		}
	}



	protected bool HasAccessory(string name) {
		foreach (var instance in AccessoryList) {
			if (instance.Item1.gameObject.name == name) return true;
		}
		return false;
	}

	protected void AddAccessory(string name) {
		if (AccessoryTable.TryGetValue(name, out var controller)) {
			if (!AccessoryPool.TryPop(out var instance)) {
				var gameObject = new GameObject();
				gameObject.transform.SetParent(BodyAnimator.transform.parent);
				gameObject.transform.localPosition = Vector3.zero;
				instance.Item1 = gameObject.AddComponent<SpriteRenderer>();
				instance.Item1.spriteSortPoint = SpriteSortPoint.Pivot;
				instance.Item1.sortingLayerID = BodyRenderer.sortingLayerID;
				instance.Item2 = gameObject.AddComponent<Animator>();
			}
			instance.Item2.runtimeAnimatorController = controller;
			instance.Item1.gameObject.name = name;
			instance.Item1.gameObject.SetActive(true);
			AccessoryList.Add(instance);
			Renderers.Add(instance.Item1);
		}
	}

	protected void RemoveAccessory(string name) {
		foreach (var instance in AccessoryList) {
			if (instance.Item1.gameObject.name == name) {
				instance.Item1.gameObject.name = "Accessory";
				instance.Item1.gameObject.SetActive(false);
				AccessoryPool.Push(instance);
				AccessoryList.Remove(instance);
				Renderers.Remove(instance.Item1);
				break;
			}
		}
	}

	void UpdateAccessory() {
		if (AccessoryList.Count == 0) return;
		int sortingOrder = BodyRenderer.sortingOrder;
		float deltaTime = BodyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		for (int i = 0; i < AccessoryList.Count; i++) {
			var instance = AccessoryList[i];
			instance.Item1.sortingOrder = sortingOrder + i + 1;
			instance.Item2.Play(State.ToString(), 0, deltaTime);
		}
	}



	// Lifecycle

	void OnEnable() => Actors.Add(this);
	void OnDisable() => Actors.Remove(this);

	void Update() {
		switch (GameManager.GameState) {
			case GameState.Gameplay:
				if (IsSimulated) Simulate();
				Act();
				Draw();
				break;
			case GameState.Cutscene:
				Act();
				Draw();
				break;
			case GameState.Paused:
				Draw();
				break;
		}
	}

	void LateUpdate() {
		UpdateAccessory();
	}
}
