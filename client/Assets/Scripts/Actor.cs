using UnityEngine;
using System.Collections.Generic;



// ━

public enum State {
	Idle,
	Moving,
}

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



	// Properties

	public static List<Actor> Actors => s_Actors;

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
			}
		}
	}
	public Emotion Emotion {
		get => m_Emotion;
		set {
			if (m_Emotion != value) {
				m_Emotion = value;
				EmotionAnimator.Play(value.ToString());
			}
		}
	}
	protected List<SpriteRenderer> Renderers {
		get {
			if (m_Renderers == null) {
				m_Renderers = new();
				var stack = new Stack<Transform>();
				stack.Push(transform);
				while (0 < stack.Count) {
					var body = stack.Pop();
					if (body.TryGetComponent(out SpriteRenderer renderer)) m_Renderers.Add(renderer);
					for (int i = 0; i < body.childCount; i++) stack.Push(body.GetChild(i));
				}
			}
			return m_Renderers;
		}
	}
	protected bool FlipX {
		get => Renderers[0].flipX;
		set {
			if (Renderers[0].flipX != value) {
				Renderers.ForEach(renderer => renderer.flipX = value);
			}
		}
	}



	public bool IsSimulated {
		get => m_IsSimulated;
		set {
			if (m_IsSimulated != value) {
				m_IsSimulated = value;
				var list = new List<EventTrigger>();
				transform.GetComponentsInChildren(true, list);
				foreach (var trigger in list) trigger.gameObject.SetActive(value);
			}
		}
	}



	protected Rigidbody2D Body => m_Body || TryGetComponent(out m_Body) ? m_Body : null;

	protected Vector2 MoveVector {
		get => m_MoveVector;
		set => m_MoveVector = value;
	}
	public float Speed {
		get => m_Speed;
		protected set => m_Speed = value;
	}
	public Queue<Vector3> PathPoints => m_PathPoints;



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



	protected virtual void Simulate() {
	}

	protected virtual void Act() {
		if (PathPoints.TryPeek(out var point)) {
			MoveVector = (((Vector2)point - Body.position) / GameManager.GridMultiplier).normalized;
			Body.linearVelocity = GameManager.GridMultiplier * MoveVector * Speed;

			if (Vector2.Distance(Body.position, point) < 0.1f) {
				PathPoints.Dequeue();
				if (PathPoints.Count == 0) State = State.Idle;
			}
		}
	}

	protected virtual void Draw() {
		State = 0.01f < MoveVector.sqrMagnitude ? State.Moving : State.Idle;
		FlipX = MoveVector.x != 0f ? MoveVector.x < 0f : FlipX;
		MoveVector = default;
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
}
