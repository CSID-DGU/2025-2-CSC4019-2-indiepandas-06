using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using static ElementEditorExtensions;
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



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager | Get Behavior Name
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Actor/Get Behavior Name")]
public class GetBehaviorNameEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public class GetBehaviorNameEventNode : EventNodeBase {
		GetBehaviorNameEvent I => target as GetBehaviorNameEvent;

		public GetBehaviorNameEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var instance = ObjectField(I.instance, value => I.instance = value);
			var behavior = TextField(I.behaviorName, value => I.behaviorName = value);
			behavior.textEdition.placeholder = "Behavior Name";
			mainContainer.Add(instance);
			mainContainer.Add(behavior);
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
			CreatePort(Direction.Output).portName = "True";
			CreatePort(Direction.Output).portName = "False";
			RefreshExpandedState();
			RefreshPorts();
		}
	}
	#endif



	// Fields

	public GameObject instance;
	public string behaviorName;



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is GetBehaviorNameEvent getBehaviorNameEvent) {
			instance     = getBehaviorNameEvent.instance;
			behaviorName = getBehaviorNameEvent.behaviorName;
		}
	}

	public override void GetNexts(List<EventBase> list) {
		int index = 1;
		if (instance != null && instance.TryGetComponent(out Actor actor)) {
			if (actor.BehaviorName == behaviorName) index = 0;
		}
		foreach (var next in Nexts) if (next.oPortType == PortType.Default) {
			if (next.oPort == index) list.Add(next.eventBase);
		}
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Actor | Open Shop
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Actor/Open Shop")]
public class OpenShopEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public class OpenShopEventNode : EventNodeBase {
		OpenShopEvent I => target as OpenShopEvent;

		public OpenShopEventNode() : base() {
			mainContainer.style.minWidth = mainContainer.style.maxWidth = Node1U;
		}

		public override void ConstructData() {
			var instance = ObjectField(I.instance, value => I.instance = value);
			mainContainer.Add(instance);
		}
	}
	#endif



	// Fields

	public GameObject instance;



	// Methods

	public override void End() {
		if (instance && instance.TryGetComponent(out ShopOwner shopOwner)) {
			shopOwner.OpenShop();
		}
	}

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is OpenShopEvent openShopEvent) {
			instance = openShopEvent.instance;
		}
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Actor | Set State
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Actor/Set State")]
public class SetStateEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public class SetStateEventNode : EventNodeBase {
		SetStateEvent I => target as SetStateEvent;

		public SetStateEventNode() : base() {
			mainContainer.style.minWidth = mainContainer.style.maxWidth = Node1U;
		}

		public override void ConstructData() {
			var instance = ObjectField(I.instance, value => I.instance = value);
			var state  = EnumField(I.state, value => I.state = value);
			mainContainer.Add(instance);
			mainContainer.Add(state);
		}
	}
	#endif



	// Fields

	public GameObject instance;
	public State state;



	// Methods

	public override void End() {
		if (instance && instance.TryGetComponent(out Actor actor)) actor.State = state;
	}

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is SetStateEvent setStateEvent) {
			instance = setStateEvent.instance;
			state  = setStateEvent.state;
		}
	}
}
