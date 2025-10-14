using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Player
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public class Player : Actor {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(Player))]
	class PlayerEditor : EditorExtensions {
		Player I => target as Player;
		public override void OnInspectorGUI() {
			Begin("Player");

			LabelField("Animator", EditorStyles.boldLabel);
			I.BodyAnimator    = ObjectField("Body Animator",    I.BodyAnimator);
			I.EmotionAnimator = ObjectField("Emotion Animator", I.EmotionAnimator);
			Space();
			LabelField("Physics", EditorStyles.boldLabel);
			I.Speed = FloatField("Speed", I.Speed);
			Space();

			End();
		}
	}
	#endif



	// Fields

	List<GameObject> m_NearObjects = new();



	// Properties

	protected List<GameObject> NearObjects => m_NearObjects;



	// Methods

	void FixedUpdate() => NearObjects.Clear();
	void OnTriggerStay2D(Collider2D other) => NearObjects.Add(other.gameObject);

	public (GameObject, IInteractable) GetNearestInteractable() {
		if (NearObjects.Count == 0) return (null, null);
		float distancesq = float.MaxValue;
		var gameObject = default(GameObject);
		var interactable = default(IInteractable);
		var stack = new Stack<Transform>();

		foreach (var nearObject in NearObjects) {
			stack.Push(nearObject.transform);
			var i = default(IInteractable);
			while (0 < stack.Count) {
				var transform = stack.Pop();
				if (transform.TryGetComponent(out i)) break;
				foreach (Transform child in transform) stack.Push(child);
			}
			if (i != null && i.IsInteractable) {
				float sq = GetDistanceSq(nearObject);
				if (sq < distancesq) {
					distancesq = sq;
					gameObject = nearObject;
					interactable = i;
				}
			}
		}
		return (gameObject, interactable);
	}



	protected override void Simulate() {
		MoveVector = InputManager.MoveDirection;
		if (InputManager.GetKeyDown(KeyAction.Interact)) {
			var interactable = GetNearestInteractable().Item2;
			if (interactable != null) {
				GetNearestInteractable().Item2?.Interact(gameObject);
			}
			else {
				GameManager.Player.GetComponent<ToolManager>()?.UseTool();
			}
		}
	}

	protected override void Act() {
		base.Act();
		if (PathPoints.Count == 0) {
			Body.linearVelocity = GameManager.GridMultiplier * MoveVector * Speed;
		}
	}
}
