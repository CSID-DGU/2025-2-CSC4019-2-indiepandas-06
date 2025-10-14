/*
using UnityEngine;

#if UNITY_EDITOR
using static EditorVisualElement;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Actor | Set Emotion
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Actor/Set Emotion")]
public class SetEmotionEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public class SetEmotionEventNode : EventNodeBase {
		SetEmotionEvent I => target as SetEmotionEvent;

		public SetEmotionEventNode() : base() {
			mainContainer.style.minWidth = mainContainer.style.maxWidth = Node1U;
		}

		public override void ConstructData() {
			var instance = ObjectField(I.instance, value => I.instance = value);
			var emotion  = EnumField(I.emotion, value => I.emotion = value);
			mainContainer.Add(instance);
			mainContainer.Add(emotion);
		}
	}
	#endif



	// Fields

	public GameObject instance;
	public Emotion emotion;



	// Methods

	public override void End() {
		if (instance && instance.TryGetComponent(out Actor actor)) actor.Emotion = emotion;
	}

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is SetEmotionEvent setEmotionEvent) {
			instance = setEmotionEvent.instance;
			emotion  = setEmotionEvent.emotion;
		}
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Actor | Look At
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Actor/Look At")]
public class LookAtEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public class LookAtEventNode : EventNodeBase {
		LookAtEvent I => target as LookAtEvent;

		public LookAtEventNode() : base() {
			mainContainer.style.minWidth = mainContainer.style.maxWidth = Node1U;
		}

		public override void ConstructData() {
			var instance = ObjectField(I.instance, value => I.instance = value);
			var target   = ObjectField(I.target, value => I.target = value);
			mainContainer.Add(instance);
			mainContainer.Add(target);
		}
	}
	#endif



	// Fields

	public GameObject instance;
	public GameObject target;



	// Methods

	public override void End() {
		if (instance && instance.TryGetComponent(out Actor actor)) actor.LookAt(target);
	}

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is LookAtEvent lookAtEvent) {
			instance = lookAtEvent.instance;
			target   = lookAtEvent.target;
		}
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Actor | Calculate Path
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Actor/Calculate Path")]
public class CalculatePathEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public class CalculatePathEventNode : EventNodeBase {
		CalculatePathEvent I => target as CalculatePathEvent;

		public CalculatePathEventNode() : base() {
			mainContainer.style.minWidth = mainContainer.style.maxWidth = Node2U;
		}

		public override void ConstructData() {
			var instance  = ObjectField(I.instance, value => I.instance = value);
			var target    = ObjectField(I.target, value => I.target = value);
			var threshold = FloatField(I.threshold, value => I.threshold = value);
			mainContainer.Add(instance);
			mainContainer.Add(target);
			mainContainer.Add(threshold);
		}
	}
	#endif



	// Fields

	public GameObject instance;
	public GameObject target;
	public float threshold;



	// Methods

	public override void Start() {
		if (instance && instance.TryGetComponent(out Actor actor)) {
			actor.CalculatePath(target, threshold);
		}
	}

	public override bool Update() {
		if (instance && instance.TryGetComponent(out Actor actor)) {
			return actor.PathPoints.Count == 0;
		} else return true;
	}

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is CalculatePathEvent calculatePathEvent) {
			instance  = calculatePathEvent.instance;
			target    = calculatePathEvent.target;
			threshold = calculatePathEvent.threshold;
		}
	}
}
*/