using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using static ElementEditorExtensions;
#endif



public enum PortType : byte {
	Default,
	Object,
	MultimodalData,
	DataID,
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class NodeMenuAttribute : Attribute {
	public string Path { get; }
	public NodeMenuAttribute(string path) => Path = path;
}

public static class ListExtensions {
	public static void CopyFrom<T>(this List<T> target, List<T> origin) {
		target.Clear();
		target.AddRange(origin);
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Event Base
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[Serializable]
public abstract class EventBase {

	// Editor

	#if UNITY_EDITOR
	public abstract class EventNodeBase : Node {

		public static List<string> Dropdown {
			get {
				var assembly = System.Reflection.Assembly.GetExecutingAssembly();
				var dropdown = assembly.GetTypes().Where(type => {
					return typeof(EventBase).IsAssignableFrom(type);
				}).Select(type => {
					var attributes = type.GetCustomAttributes(typeof(NodeMenuAttribute), false);
					var attribute = attributes.Cast<NodeMenuAttribute>().FirstOrDefault();
					return new { Type = type, Attribute = attribute };
				}).Where(x => x.Attribute != null).Select(x => x.Attribute.Path).ToList();
				return dropdown;
			}
		}



		public EventBase target;

		public EventNodeBase() {
			var name = ToString().Split(" ")[0][..^4];
			var type = Type.GetType(name);
			title = Regex.Replace(name[..^5], "(?<=[a-z])(?=[A-Z])", " ");
			target = Activator.CreateInstance(type) as EventBase;
			target.Node = this;
			var separator = new VisualElement();
			separator.style.height = 1f;
			separator.style.backgroundColor = new Color(0.14f, 0.14f, 0.14f);
			mainContainer.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f);
			mainContainer.ElementAt(1).Insert(2, separator);
		}



		public virtual void ConstructData() { }
		public virtual void ConstructPort() {
			CreatePort(Direction.Input);
			CreatePort(Direction.Output);
			RefreshExpandedState();
			RefreshPorts();
		}

		protected Port CreatePort(Direction direction, PortType type = PortType.Default) {
			var isInput = direction == Direction.Input;
			var port = default(Port);
			switch (type) {
				case PortType.Default: {
					var orientation = Orientation.Horizontal;
					var capacity = Port.Capacity.Multi;
					port = InstantiatePort(orientation, direction, capacity, null);
					port.portColor = new Color(1.0f, 1.0f, 1.0f);
					port.portName = isInput ? "Prev" : "Next";
				} break;
				case PortType.Object: {
					var orientation = Orientation.Horizontal;
					var capacity = Port.Capacity.Multi;
					port = InstantiatePort(orientation, direction, capacity, typeof(GameObject));
					port.portColor = new Color(0.0f, 0.8f, 1.0f);
					port.portName = isInput ? "In" : "Out";
				} break;
				case PortType.MultimodalData: {
					var orientation = Orientation.Horizontal;
					var capacity = isInput ? Port.Capacity.Single : Port.Capacity.Multi;
					port = InstantiatePort(orientation, direction, capacity, typeof(MultimodalData));
					port.portColor = new Color(0.6f, 0.3f, 0.9f);
					port.portName = isInput ? "In" : "Out";
				} break;
				case PortType.DataID: {
					var orientation = Orientation.Horizontal;
					var capacity = isInput ? Port.Capacity.Single : Port.Capacity.Multi;
					port = InstantiatePort(orientation, direction, capacity, typeof(uint));
					port.portColor = new Color(0.3f, 0.3f, 0.6f);
					port.portName = isInput ? "In" : "Out";
				} break;
			}
			port.userData = type;
			switch (isInput) {
				case true:  inputContainer.Add(port); break;
				case false: outputContainer.Add(port); break;
			}
			return port;
		}
	}
	#endif



	// Constants

	[Serializable]
	public struct Connection {
		[SerializeReference] public EventBase eventBase;
		public byte iPort;
		public byte oPort;
		public PortType iPortType;
		public PortType oPortType;
	}



	// Fields

	#if UNITY_EDITOR
	[SerializeField] string m_Guid;
	[SerializeField] EventNodeBase m_Node;
	[SerializeField] Vector2 m_Position;
	#endif

	[SerializeField] List<Connection> m_Prevs = new();
	[SerializeField] List<Connection> m_Nexts = new();



	// Properties

	#if UNITY_EDITOR
	public string Guid {
		get => m_Guid;
		set => m_Guid = value;
	}
	public EventNodeBase Node {
		get => m_Node;
		set => m_Node = value;
	}
	public Vector2 Position {
		get => m_Position;
		set => m_Position = value;
	}
	#endif

	public List<Connection> Prevs {
		get => m_Prevs;
		set => m_Prevs = value;
	}
	public List<Connection> Nexts {
		get => m_Nexts;
		set => m_Nexts = value;
	}



	// Methods

	#if UNITY_EDITOR
	public EventBase() {
		Guid = System.Guid.NewGuid().ToString();
	}

	public virtual void CopyFrom(EventBase eventBase) {
		Guid = eventBase.Guid;
		Position = eventBase.Position;
		Prevs.CopyFrom(eventBase.Prevs);
		Nexts.CopyFrom(eventBase.Nexts);
	}
	#else
	public virtual void CopyFrom(EventBase eventBase) {
		Prevs.CopyFrom(eventBase.Prevs);
		Nexts.CopyFrom(eventBase.Nexts);
	}
	#endif

	public virtual void Start() { }
	public virtual bool Update() => true;
	public virtual void End() { }

	public virtual void GetNexts(List<EventBase> list) {
		foreach (var next in Nexts) if (next.oPortType == PortType.Default) {
			if (next.oPort == 0) list.Add(next.eventBase);
		}
	}

	protected virtual void GetObjects(List<GameObject> list) {
		foreach (var prev in Prevs) if (prev.oPortType == PortType.Object) {
			prev.eventBase.GetObjects(list);
		}
	}

	protected virtual void GetMultimodalData(List<MultimodalData> list) {
		foreach (var prev in Prevs) if (prev.oPortType == PortType.MultimodalData) {
			prev.eventBase.GetMultimodalData(list);
		}
	}

	protected virtual void GetDataID(ref uint dataID) {
		foreach (var prev in Prevs) if (prev.oPortType == PortType.DataID) {
			prev.eventBase.GetDataID(ref dataID);
		}
	}



	#if UNITY_EDITOR
	public List<EventBase> GetEvents() {
		var queue = new Queue<EventBase>();
		var stack = new Stack<EventBase>();
		stack.Push(this);
		while (stack.TryPop(out var eventBase)) {
			if (queue.Contains(eventBase)) continue;
			queue.Enqueue(eventBase);
			foreach (var prev in eventBase.Prevs) stack.Push(prev.eventBase);
			foreach (var next in eventBase.Nexts) stack.Push(next.eventBase);
		}
		return queue.ToList();
	}

	public virtual void DrawGizmos() { }
	public virtual void DrawHandles() { }
	#endif
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Entry
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public sealed class EntryEvent : EventBase {

	// Editor

	#if UNITY_EDITOR
	public sealed class EntryEventNode : EventNodeBase {
		EntryEvent I => target as EntryEvent;

		public EntryEventNode() : base() {
			capabilities &= ~Capabilities.Deletable;
			var bluegreen = new Color(160f, 0.75f, 0.60f).ToRGB();
			titleContainer.style.backgroundColor = bluegreen;
		}

		public override void ConstructPort() {
			CreatePort(Direction.Output);
			RefreshExpandedState();
			RefreshPorts();
		}
	}
	#endif
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Logic | Delay
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Logic/Delay")]
public sealed class DelayEvent : EventBase {

	// Editor

	#if UNITY_EDITOR
	public sealed class DelayEventNode : EventNodeBase {
		DelayEvent I => target as DelayEvent;

		public DelayEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var delay = FloatField(I.Delay, value => I.Delay = value);
			mainContainer.Add(delay);
		}
	}
	#endif



	// Fields

	[SerializeField] float m_Delay = 0.1f;

	float m_Time = 0f;



	// Properties

	public float Delay {
		get => m_Delay;
		set => m_Delay = Mathf.Max(0f, value);
	}

	float Time {
		get => m_Time;
		set => m_Time = value;
	}



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is DelayEvent delayEvent) {
			Delay = delayEvent.Delay;
		}
	}

	public override void Start() {
		Time = UnityEngine.Time.time;
	}

	public override bool Update() {
		return Delay <= (UnityEngine.Time.time - Time);
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Logic | Once Then
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Logic/Once Then")]
public sealed class OnceThenEvent : EventBase {

	// Editor

	#if UNITY_EDITOR
	public sealed class OnceThenEventNode : EventNodeBase {
		OnceThenEvent I => target as OnceThenEvent;

		public OnceThenEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
			CreatePort(Direction.Output).portName = "Once";
			CreatePort(Direction.Output).portName = "Then";
			RefreshExpandedState();
			RefreshPorts();
		}
	}
	#endif



	// Fields

	bool m_Value;



	// Properties

	bool Value {
		get => m_Value;
		set => m_Value = value;
	}



	// Methods

	public override void GetNexts(List<EventBase> list) {
		int index = !Value ? 0 : 1;
		foreach (var next in Nexts) if (next.oPortType == PortType.Default) {
			if (next.oPort == index) list.Add(next.eventBase);
		}
		Value = true;
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Logic | Repeat
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Logic/Repeat")]
public sealed class RepeatEvent : EventBase {

	// Editor

	#if UNITY_EDITOR
	public sealed class RepeatEventNode : EventNodeBase {
		RepeatEvent I => target as RepeatEvent;

		public RepeatEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var count = IntField(I.Count, value => I.Count = value);
			mainContainer.Add(count);
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
			CreatePort(Direction.Output).portName = "While";
			CreatePort(Direction.Output).portName = "Break";
			RefreshExpandedState();
			RefreshPorts();
		}
	}
	#endif



	// Fields

	[SerializeField] int m_Count = 1;

	int m_Value = 0;



	// Properties

	public int Count {
		get => m_Count;
		set => m_Count = Mathf.Max(1, value);
	}

	int Value {
		get => m_Value;
		set => m_Value = value;
	}



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is RepeatEvent repeatEvent) {
			Count = repeatEvent.Count;
		}
	}

	public override void Start() {
		Value = (Value <= Count) ? Value : 0;
	}

	public override void GetNexts(List<EventBase> list) {
		int index = (Value++ < Count) ? 0 : 1;
		foreach (var next in Nexts) if (next.oPortType == PortType.Default) {
			if (next.oPort == index) list.Add(next.eventBase);
		}
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Logic | Randomize
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Logic/Randomize")]
public sealed class RandomizeEvent : EventBase {

	// Editor

	#if UNITY_EDITOR
	public sealed class RandomizeEventNode : EventNodeBase {
		RandomizeEvent I => target as RandomizeEvent;

		public RandomizeEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var weights = new VisualElement();
			for (int i = 0; i < I.Weights.Count; i++) {
				int index = i;
				var element = new VisualElement();
				element.style.flexDirection = FlexDirection.Row;
				var weight = FloatField(I.Weights[index], value => {
					I.Weights[index] = value;
					UpdateProbability();
				});
				weight.style.width = Node1U - 11f - 18f;
				var remove = Button("-", () => {
					I.Weights.RemoveAt(index);
					I.Weights.TrimExcess();
					mainContainer.Remove(weights);
					ConstructData();
					var port = outputContainer.ElementAt(index) as Port;
					var graphView = port.GetFirstAncestorOfType<GraphView>();
					graphView.DeleteElements(port.connections);
					outputContainer.RemoveAt(index);
					UpdateProbability();
				});
				remove.style.marginTop = remove.style.marginBottom = 0f;
				remove.style.marginLeft = remove.style.marginRight = 0f;
				remove.style.width = 18f;
				element.Add(weight);
				element.Add(remove);
				weights.Add(element);
			}
			var add = Button("Add", () => {
				I.Weights.Add(1f);
				I.Weights.TrimExcess();
				mainContainer.Remove(weights);
				ConstructData();
				CreatePort(Direction.Output);
				UpdateProbability();
			});
			weights.Add(add);
			mainContainer.Add(weights);
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
			foreach (var weight in I.Weights) CreatePort(Direction.Output);
			UpdateProbability();
			RefreshExpandedState();
			RefreshPorts();
		}

		void UpdateProbability() {
			float sum = 0f;
			foreach (float weight in I.Weights) sum += weight;
			if (sum == 0f) sum = 1f;
			var ports = outputContainer.Children().OfType<Port>().ToList();
			for (int i = 0; i < ports.Count; i++) {
				ports[i].portName = $"{(100f * I.Weights[i] / sum).ToString("F1")}%";
			}
		}
	}
	#endif



	// Fields

	[SerializeField] List<float> m_Weights = new() { 1f, 1f, };



	// Properties

	public List<float> Weights => m_Weights;



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is RandomizeEvent randomizeEvent) {
			Weights.CopyFrom(randomizeEvent.Weights);
		}
	}

	public override void GetNexts(List<EventBase> list) {
		float sum = 0f;
		foreach (float weight in Weights) sum += weight;
		float random = Random.Range(0f, sum);
		int index = Weights.FindIndex(weight => (random -= weight) <= 0f);
		if (index == -1) index = Weights.Count - 1;
		foreach (var next in Nexts) if (next.oPortType == PortType.Default) {
			if (next.oPort == index) list.Add(next.eventBase);
		}
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Object | Object
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Object/Object")]
public sealed class ObjectEvent : EventBase {

	// Editor

	#if UNITY_EDITOR
	public sealed class ObjectEventNode : EventNodeBase {
		ObjectEvent I => target as ObjectEvent;

		public ObjectEventNode() : base() {
			mainContainer.style.width = Node1U;
			var skyblue = new Color(200f, 0.75f, 0.60f).ToRGB();
			titleContainer.style.backgroundColor = skyblue;
		}

		public override void ConstructData() {
			var instance = ObjectField(I.Instance, value => I.Instance = value);
			mainContainer.Add(instance);
		}

		public override void ConstructPort() {
			CreatePort(Direction.Output, PortType.Object);
			RefreshExpandedState();
			RefreshPorts();
		}
	}
	#endif



	// Fields

	[SerializeField] GameObject m_Instance;



	// Properties

	public GameObject Instance {
		get => m_Instance;
		set => m_Instance = value;
	}



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is ObjectEvent objectEvent) {
			Instance = objectEvent.Instance;
		}
	}

	protected override void GetObjects(List<GameObject> list) {
		if (Instance != null) list.Add(Instance);
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Object | Instantiate Object
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Object/Instantiate Object")]
public sealed class InstantiateObjectEvent : EventBase {

	// Editor

	#if UNITY_EDITOR
	public sealed class InstantiateObjectEventNode : EventNodeBase {
		InstantiateObjectEvent I => target as InstantiateObjectEvent;

		public InstantiateObjectEventNode() : base() {
			mainContainer.style.width = Node2U;
			var skyblue = new Color(200f, 0.75f, 0.60f).ToRGB();
			titleContainer.style.backgroundColor = skyblue;
		}

		public override void ConstructData() {
			var prefab = ObjectField("Prefab", I.Prefab, value => I.Prefab = value);
			var anchor = ObjectField("Anchor", I.Anchor, value => I.Anchor = value);
			var offset = Vector3Field("Offset", I.Offset, value => I.Offset = value);
			mainContainer.Add(prefab);
			mainContainer.Add(anchor);
			mainContainer.Add(offset);
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
			CreatePort(Direction.Output);
			CreatePort(Direction.Output, PortType.Object);
			RefreshExpandedState();
			RefreshPorts();
		}
	}
	#endif



	// Fields

	[SerializeField] GameObject m_Prefab;
	[SerializeField] GameObject m_Anchor;
	[SerializeField] Vector3 m_Offset;

	GameObject m_Instance;



	// Properties

	public GameObject Prefab {
		get => m_Prefab;
		set => m_Prefab = value;
	}
	public GameObject Anchor {
		get => m_Anchor;
		set => m_Anchor = value;
	}
	public Vector3 Offset {
		get => m_Offset;
		set => m_Offset = value;
	}

	GameObject Instance {
		get => m_Instance;
		set => m_Instance = value;
	}



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is InstantiateObjectEvent instantiateObjectEvent) {
			Prefab = instantiateObjectEvent.Prefab;
			Anchor = instantiateObjectEvent.Anchor;
			Offset = instantiateObjectEvent.Offset;
		}
	}

	public override void Start() {
		Instance = null;
	}

	public override void End() {
		if (Instance == null && Prefab) {
			var position = Anchor ? Anchor.transform.TransformPoint(Offset) : Offset;
			var rotation = Anchor ? Anchor.transform.rotation : Quaternion.identity;
			Instance = Object.Instantiate(Prefab, position, rotation);
		}
	}

	protected override void GetObjects(List<GameObject> list) {
		End();
		if (Instance != null) list.Add(Instance);
	}



	#if UNITY_EDITOR
	public override void DrawHandles() {
		var position = Anchor ? Anchor.transform.TransformPoint(Offset) : Offset;
		var handle = Handles.PositionHandle(position, Quaternion.identity);
		Offset = Anchor ? Anchor.transform.InverseTransformPoint(handle) : handle;
		if (Node != null) Node.Q<Vector3Field>().value = Offset;
	}
	#endif
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Object | Destroy Object
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Object/Destroy Object")]
public sealed class DestroyObjectEvent : EventBase {

	// Editor

	#if UNITY_EDITOR
	public sealed class DestroyObjectEventNode : EventNodeBase {
		DestroyObjectEvent I => target as DestroyObjectEvent;

		public DestroyObjectEventNode() : base() {
			mainContainer.style.width = Node1U;
			var skyblue = new Color(200f, 0.75f, 0.60f).ToRGB();
			titleContainer.style.backgroundColor = skyblue;
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
			CreatePort(Direction.Output);
			CreatePort(Direction.Input, PortType.Object);
			RefreshExpandedState();
			RefreshPorts();
		}
	}
	#endif



	// Fields

	List<GameObject> m_List = new();



	// Properties

	List<GameObject> List => m_List;



	// Methods

	public override void End() {
		base.GetObjects(List);
		foreach (var instance in List) Object.Destroy(instance);
		List.Clear();
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Debug | Log
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Debug/Log")]
public sealed class LogEvent : EventBase {

	// Editor

	#if UNITY_EDITOR
	public sealed class LogEventNode : EventNodeBase {
		LogEvent I => target as LogEvent;

		public LogEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var message = TextField(I.Message, value => I.Message = value);
			message.textEdition.placeholder = "Message";
			message.multiline = true;
			mainContainer.Add(message);
		}
	}
	#endif



	// Fields

	[SerializeField] string m_Message = string.Empty;



	// Properties

	public string Message {
		get => m_Message;
		set => m_Message = value;
	}



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is LogEvent logEvent) {
			Message = logEvent.Message;
		}
	}

	public override void End() {
		Debug.Log(Message);
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Multimodal | Validate
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Multimodal/Validate Answer")]
public class ValidateAnswerEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public class ValidateAnswerEventNode : EventNodeBase {
		ValidateAnswerEvent I => target as ValidateAnswerEvent;

		public ValidateAnswerEventNode() : base() {
			var purple = new StyleColor(new Color(114f / 255f, 38f / 255f, 152f / 255f));
			titleContainer.style.backgroundColor = purple;
		}

		public override void ConstructData() {
			var prompt = new TextField() { value = I.prompt, multiline = true };
			prompt.style.minWidth = prompt.style.maxWidth = 204f;
			prompt.style.whiteSpace = WhiteSpace.Normal;
			prompt.textEdition.placeholder = "Prompt";
			var field = prompt.Q<VisualElement>(className: "unity-text-field__input");
			if (field != null) field.style.minHeight = 71f;
			prompt.RegisterValueChangedCallback(evt => I.prompt = evt.newValue);
			mainContainer.Add(prompt);
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
			CreatePort(Direction.Input, PortType.MultimodalData);
			CreatePort(Direction.Output).portName = "True";
			CreatePort(Direction.Output).portName = "False";
			CreatePort(Direction.Output).portName = "Exception";
			RefreshExpandedState();
			RefreshPorts();
		}
	}
	#endif



	// Fields

	public string prompt;

	float timer;
	bool isValid;
	bool isException;



	// Methods

	public override void CopyFrom(EventBase data) {
		base.CopyFrom(data);
		if (data is ValidateAnswerEvent validateAnswer) {
			prompt = validateAnswer.prompt;
		}
	}

	public override void Start() {
		timer = 3f;
		// Send the prompt to the server
	}

	public override bool Update() {
		if (0f < timer) {
			timer -= Time.deltaTime;
			// wait for the server response
			if (timer <= 0f) {
				// timeout
				isValid = true;
				return true;
			}
		}
		isValid = true;
		return false;
	}

	public override void GetNexts(List<EventBase> list) {
		list ??= new();
		list.Clear();
		int index = isException ? 2 : (isValid ? 0 : 1);
		foreach (var next in Nexts) if (next.oPortType == PortType.Default) {
			if (next.oPort == index) list.Add(next.eventBase);
		}
	}
}
