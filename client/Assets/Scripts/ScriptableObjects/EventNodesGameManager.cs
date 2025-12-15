using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using static ElementEditorExtensions;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager | Set Game State
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Manager/Set Game State")]
public sealed class SetGameStateEvent : EventBase {

	// Editor

	#if UNITY_EDITOR
	public sealed class SetGameStateEventNode : EventNodeBase {
		SetGameStateEvent I => target as SetGameStateEvent;

		public SetGameStateEventNode() : base() {
			mainContainer.style.width = Node1U;
			var cyan = new Color(180f, 0.75f, 0.60f).ToRGB();
			titleContainer.style.backgroundColor = cyan;
		}

		public override void ConstructData() {
			var gameState = EnumField(I.GameState, value => I.GameState = value);
			mainContainer.Add(gameState);
		}
	}
	#endif



	// Fields

	#if UNITY_EDITOR
	[SerializeField] string m_GameStateName;
	#endif

	[SerializeField] GameState m_GameState;



	// Properties

	#if UNITY_EDITOR
	public GameState GameState {
		get => !Enum.TryParse(m_GameStateName, out GameState gameState) ?
			Enum.Parse<GameState>(m_GameStateName = m_GameState.ToString()) :
			m_GameState = gameState;
		set => m_GameStateName = (m_GameState = value).ToString();
	}
	#else
	public GameState GameState {
		get => m_GameState;
		set => m_GameState = value;
	}
	#endif



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is SetGameStateEvent setGameStateEvent) {
			GameState = setGameStateEvent.GameState;
		}
	}

	public override void End() {
		GameManager.GameState = GameState;
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager | Set Time Scale
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Manager/Set Time Scale")]
public sealed class SetTimeScaleEvent : EventBase {

	// Editor

	#if UNITY_EDITOR
	public sealed class SetTimeScaleEventNode : EventNodeBase {
		SetTimeScaleEvent I => target as SetTimeScaleEvent;

		public SetTimeScaleEventNode() : base() {
			mainContainer.style.width = Node1U;
			var cyan = new Color(180f, 0.75f, 0.60f).ToRGB();
			titleContainer.style.backgroundColor = cyan;
		}

		public override void ConstructData() {
			var timeScale = Slider(I.TimeScale, 0f, 10f, value => I.TimeScale = value);
			mainContainer.Add(timeScale);
		}
	}
	#endif



	// Fields

	[SerializeField] float m_TimeScale = 1f;



	// Properties

	public float TimeScale {
		get => m_TimeScale;
		set => m_TimeScale = Mathf.Clamp(value, 0f, 10f);
	}



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is SetTimeScaleEvent setTimeScaleEvent) {
			TimeScale = setTimeScaleEvent.TimeScale;
		}
	}

	public override void End() {
		GameManager.TimeScale = TimeScale;
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager | Play Event
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Manager/Play Event")]
public sealed class PlayEventEvent : EventBase {

	// Editor

	#if UNITY_EDITOR
	public sealed class PlayEventEventNode : EventNodeBase {
		PlayEventEvent I => target as PlayEventEvent;

		public PlayEventEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var eventGraph = ObjectField(I.EventGraph, value => I.EventGraph = value);
			mainContainer.Add(eventGraph);
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
			CreatePort(Direction.Output);
			CreatePort(Direction.Output, PortType.MultimodalData);
			RefreshExpandedState();
			RefreshPorts();
		}
	}
	#endif



	// Fields

	[SerializeField] EventGraph m_EventGraph;

	uint m_EventID;



	// Properties

	public EventGraph EventGraph {
		get => m_EventGraph;
		set => m_EventGraph = value;
	}

	ref uint EventID {
		get => ref m_EventID;
	}



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is PlayEventEvent playEventEvent) {
			EventGraph = playEventEvent.EventGraph;
		}
	}

	public override void Start() {
		EventID = default;
	}

	public override void End() {
		if (EventID == default) {
			EventID = GameManager.PlayEvent(EventGraph);
		}
	}

	protected override void GetDataID(ref uint eventID) {
		End();
		if (eventID != default) eventID = EventID;
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager | Is Event Playing
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Manager/Is Event Playing")]
public sealed class IsEventPlayingEvent : EventBase {

	// Editor

	#if UNITY_EDITOR
	public sealed class IsEventPlayingEventNode : EventNodeBase {
		IsEventPlayingEvent I => target as IsEventPlayingEvent;

		public IsEventPlayingEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
			CreatePort(Direction.Input, PortType.MultimodalData);
			CreatePort(Direction.Output).portName = "True";
			CreatePort(Direction.Output).portName = "False";
			CreatePort(Direction.Output, PortType.MultimodalData);
			RefreshExpandedState();
			RefreshPorts();
		}
	}
	#endif



	// Fields

	uint m_EventID;



	// Properties

	ref uint EventID {
		get => ref m_EventID;
	}



	// Methods

	public override void Start() {
		EventID = default;
	}

	public override void GetNexts(List<EventBase> list) {
		if (EventID == default) base.GetDataID(ref EventID);
		int index = GameManager.IsEventPlaying(EventID) ? 0 : 1;
		foreach (var next in Nexts) if (next.oPortType == PortType.Default) {
			if (next.oPort == index) list.Add(next.eventBase);
		}
	}

	protected override void GetDataID(ref uint eventID) {
		if (eventID == default) base.GetDataID(ref eventID);
		if (eventID != default) eventID = EventID;
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager | Stop Event
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Manager/Stop Event")]
public sealed class StopEventEvent : EventBase {

	// Editor

	#if UNITY_EDITOR
	public sealed class StopEventEventNode : EventNodeBase {
		StopEventEvent I => target as StopEventEvent;

		public StopEventEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
			CreatePort(Direction.Input, PortType.MultimodalData);
			CreatePort(Direction.Output);
			RefreshExpandedState();
			RefreshPorts();
		}
	}
	#endif



	// Methods

	public override void End() {
		uint eventID = default;
		base.GetDataID(ref eventID);
		if (eventID != default) GameManager.StopEvent(eventID);
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager | Quit Game
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Manager/Quit Game")]
public sealed class QuitGameEvent : EventBase {

	// Editor

	#if UNITY_EDITOR
	public sealed class QuitGameEventNode : EventNodeBase {
		QuitGameEvent I => target as QuitGameEvent;

		public QuitGameEventNode() : base() {
			mainContainer.style.width = Node1U;
			var cyan = new Color(180f, 0.75f, 0.60f).ToRGB();
			titleContainer.style.backgroundColor = cyan;
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
			RefreshExpandedState();
			RefreshPorts();
		}
	}
	#endif



	// Methods

	public override void End() {
		GameManager.QuitGame();
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager | Get Int Value
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Manager/Get Int Value")]
public class GetIntValueEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public class GetIntValueEventNode : EventNodeBase {
		GetIntValueEvent I => target as GetIntValueEvent;

		public GetIntValueEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var key = TextField(I.key, value => I.key = value);
			var compare = EnumField(I.compare, value => I.compare = value);
			var value = IntField(I.value, value => I.value = value);
			key.textEdition.placeholder = "Key";
			mainContainer.Add(key);
			mainContainer.Add(compare);
			mainContainer.Add(value);
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

	public string key;
	public Compare compare;
	public int value;



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is GetIntValueEvent getIntValueEvent) {
			key = getIntValueEvent.key;
			compare = getIntValueEvent.compare;
			value = getIntValueEvent.value;
		}
	}

	public override void GetNexts(List<EventBase> list) {
		int index = compare switch {
			Compare.Equal              => GameManager.IntValue[key] == value,
			Compare.NotEqual           => GameManager.IntValue[key] != value,
			Compare.LessThan           => GameManager.IntValue[key] <  value,
			Compare.LessThanOrEqual    => GameManager.IntValue[key] <= value,
			Compare.GreaterThan        => GameManager.IntValue[key] >  value,
			Compare.GreaterThanOrEqual => GameManager.IntValue[key] >= value,
			_ => default,
		} ? 0 : 1;
		foreach (var next in Nexts) if (next.oPortType == PortType.Default) {
			if (next.oPort == index) list.Add(next.eventBase);
		}
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager | Set Int Value
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Manager/Set Int Value")]
public class SetIntValueEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public class SetIntValueEventNode : EventNodeBase {
		SetIntValueEvent I => target as SetIntValueEvent;

		public SetIntValueEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var key = TextField(I.key, value => I.key = value);
			var value = IntField(I.value, value => I.value = value);
			key.textEdition.placeholder = "Key";
			mainContainer.Add(key);
			mainContainer.Add(value);
		}
	}
	#endif



	// Fields

	public string key;
	public int value;



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is SetIntValueEvent setIntValueEvent) {
			key = setIntValueEvent.key;
			value = setIntValueEvent.value;
		}
	}

	public override void End() {
		GameManager.IntValue[key] = value;
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager | Add Int Value
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Manager/Add Int Value")]
public class AddIntValueEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public class AddIntValueEventNode : EventNodeBase {
		AddIntValueEvent I => target as AddIntValueEvent;

		public AddIntValueEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var key = TextField(I.key, value => I.key = value);
			var value = IntField(I.value, value => I.value = value);
			key.textEdition.placeholder = "Key";
			mainContainer.Add(key);
			mainContainer.Add(value);
		}
	}
	#endif



	// Fields

	public string key;
	public int value;



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is AddIntValueEvent addIntValueEvent) {
			key = addIntValueEvent.key;
			value = addIntValueEvent.value;
		}
	}

	public override void End() {
		GameManager.IntValue[key] += value;
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager | Get Float Value
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Manager/Get Float Value")]
public class GetFloatValueEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public class GetFloatValueEventNode : EventNodeBase {
		GetFloatValueEvent I => target as GetFloatValueEvent;

		public GetFloatValueEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var key = TextField(I.key, value => I.key = value);
			var compare = EnumField(I.compare, value => I.compare = value);
			var value = FloatField(I.value, value => I.value = value);
			key.textEdition.placeholder = "Key";
			mainContainer.Add(key);
			mainContainer.Add(compare);
			mainContainer.Add(value);
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

	public string key;
	public Compare compare;
	public float value;



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is GetFloatValueEvent getFloatValueEvent) {
			key     = getFloatValueEvent.key;
			compare = getFloatValueEvent.compare;
			value   = getFloatValueEvent.value;
		}
	}

	public override void GetNexts(List<EventBase> list) {
		int index = compare switch {
			Compare.Equal              => GameManager.FloatValue[key] == value,
			Compare.NotEqual           => GameManager.FloatValue[key] != value,
			Compare.LessThan           => GameManager.FloatValue[key] <  value,
			Compare.LessThanOrEqual    => GameManager.FloatValue[key] <= value,
			Compare.GreaterThan        => GameManager.FloatValue[key] >  value,
			Compare.GreaterThanOrEqual => GameManager.FloatValue[key] >= value,
			_ => default,
		} ? 0 : 1;
		foreach (var next in Nexts) if (next.oPortType == PortType.Default) {
			if (next.oPort == index) list.Add(next.eventBase);
		}
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager | Set Float Value
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Manager/Set Float Value")]
public class SetFloatValueEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public class SetFloatValueEventNode : EventNodeBase {
		SetFloatValueEvent I => target as SetFloatValueEvent;

		public SetFloatValueEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var key = TextField(I.key, value => I.key = value);
			var value = FloatField(I.value, value => I.value = value);
			key.textEdition.placeholder = "Key";
			mainContainer.Add(key);
			mainContainer.Add(value);
		}
	}
	#endif



	// Fields

	public string key;
	public float value;



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is SetFloatValueEvent setFloatValueEvent) {
			key   = setFloatValueEvent.key;
			value = setFloatValueEvent.value;
		}
	}

	public override void End() {
		GameManager.FloatValue[key] = value;
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager | Add Float Value
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Manager/Add Float Value")]
public class AddFloatValueEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public class AddFloatValueEventNode : EventNodeBase {
		AddFloatValueEvent I => target as AddFloatValueEvent;

		public AddFloatValueEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var key = TextField(I.key, value => I.key = value);
			var value = FloatField(I.value, value => I.value = value);
			key.textEdition.placeholder = "Key";
			mainContainer.Add(key);
			mainContainer.Add(value);
		}
	}
	#endif



	// Fields

	public string key;
	public float value;



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is AddFloatValueEvent addFloatValueEvent) {
			key = addFloatValueEvent.key;
			value = addFloatValueEvent.value;
		}
	}

	public override void End() {
		GameManager.FloatValue[key] += value;
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager | Get String Value
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Manager/Get String Value")]
public class GetStringValueEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public class GetStringValueEventNode : EventNodeBase {
		GetStringValueEvent I => target as GetStringValueEvent;

		public GetStringValueEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var key = TextField(I.key, value => I.key = value);
			var compare = EnumField(I.compare, value => I.compare = value);
			var value = TextField(I.value, value => I.value = value);
			key.textEdition.placeholder = "Key";
			value.textEdition.placeholder = "Value";
			mainContainer.Add(key);
			mainContainer.Add(compare);
			mainContainer.Add(value);
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

	public string key;
	public Compare compare;
	public string value;



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is GetStringValueEvent getStringValueEvent) {
			key     = getStringValueEvent.key;
			compare = getStringValueEvent.compare;
			value   = getStringValueEvent.value;
		}
	}

	public override void GetNexts(List<EventBase> list) {
		int index = compare switch {
			Compare.Equal    => GameManager.StringValue[key] == value,
			Compare.NotEqual => GameManager.StringValue[key] != value,
			_ => default,
		} ? 0 : 1;
		foreach (var next in Nexts) if (next.oPortType == PortType.Default) {
			if (next.oPort == index) list.Add(next.eventBase);
		}
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager | Set String Value
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Manager/Set String Value")]
public class SetStringValueEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public class SetStringValueEventNode : EventNodeBase {
		SetStringValueEvent I => target as SetStringValueEvent;

		public SetStringValueEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var key = TextField(I.key, value => I.key = value);
			var value = TextField(I.value, value => I.value = value);
			key.textEdition.placeholder = "Key";
			value.textEdition.placeholder = "Value";
			mainContainer.Add(key);
			mainContainer.Add(value);
		}
	}
	#endif



	// Fields

	public string key;
	public string value;



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is SetStringValueEvent setStringValueEvent) {
			key   = setStringValueEvent.key;
			value = setStringValueEvent.value;
		}
	}

	public override void End() {
		GameManager.StringValue[key] = value;
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager | Add String Value
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Manager/Add String Value")]
public class AddStringValueEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public class AddStringValueEventNode : EventNodeBase {
		AddStringValueEvent I => target as AddStringValueEvent;

		public AddStringValueEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var key = TextField(I.key, value => I.key = value);
			var value = TextField(I.value, value => I.value = value);
			key.textEdition.placeholder = "Key";
			value.textEdition.placeholder = "Value";
			mainContainer.Add(key);
			mainContainer.Add(value);
		}
	}
	#endif



	// Fields

	public string key;
	public string value;



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is AddStringValueEvent addStringValueEvent) {
			key = addStringValueEvent.key;
			value = addStringValueEvent.value;
		}
	}

	public override void End() {
		GameManager.StringValue[key] += value;
	}
}
