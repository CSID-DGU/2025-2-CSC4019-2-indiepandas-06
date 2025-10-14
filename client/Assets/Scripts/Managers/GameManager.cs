using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif



// Game States

public enum GameState : byte {
	Gameplay,
	Cutscene,
	Paused,
}

public enum Compare : byte {
	Equal,
	NotEqual,
	LessThan,
	LessThanOrEqual,
	GreaterThan,
	GreaterThanOrEqual,
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("Manager/Game Manager")]
public class GameManager : MonoSingleton<GameManager> {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(GameManager))]
	class GameManagerEditor : EditorExtensions {
		GameManager I => target as GameManager;
		public override void OnInspectorGUI() {
			Begin();

			LabelField("Event Instance", EditorStyles.boldLabel);
			LabelField("Event Template", "None (Event Graph SO)");
			int num = EventInstance.Count;
			int den = 0;
			LabelField("Event Pool", $"{num} / {den}");
			Space();

			LabelField("Game Status", EditorStyles.boldLabel);
			LabelField("Game State", $"{GameState}");
			LabelField("Time Scale", $"{TimeScale:F2}");

			LabelField("Game Data", EditorStyles.boldLabel);
			DictionaryField("Int Value", IntValue, (list, index) => {
				var pair = list[index];
				pair.key = TextField(pair.key);
				pair.value = IntField(pair.value);
				list[index] = pair;
			}, ($"New Key {IntValue.Count}", default));
			DictionaryField("Float Value", FloatValue, (list, index) => {
				var pair = list[index];
				pair.key = TextField(pair.key);
				pair.value = FloatField(pair.value);
				list[index] = pair;
			}, ($"New Key {FloatValue.Count}", default));
			DictionaryField("String Value", StringValue, (list, index) => {
				var pair = list[index];
				pair.key = TextField(pair.key);
				pair.value = TextField(pair.value);
				list[index] = pair;
			}, ($"New Key {StringValue.Count}", default));
			Space();
			BeginHorizontal();
			if (Button("Save Data")) SaveData();
			if (Button("Load Data")) LoadData();
			EndHorizontal();
			Space();

			End();
		}
	}
	#endif



	// Constants

	public const float GridXMultiplier = 1.0f;
	public const float GridYMultiplier = 0.5f;
	public static Vector2 GridMultiplier => new(GridXMultiplier, GridYMultiplier);



	// Fields

	GameState m_GameState;
	Player m_Player;

	Dictionary<uint, byte> m_EventInstance = new();
	List<(uint, EventBase, float)> m_EventList = new();
	List<EventBase> m_EventBuffer = new();
	uint m_NextID;

	[SerializeField] HashMap<string, int> m_IntValue = new();
	[SerializeField] HashMap<string, float> m_FloatValue = new();
	[SerializeField] HashMap<string, string> m_StringValue = new();



	// Properties

	public static GameState GameState {
		get => Instance.m_GameState;
		set {
			if (Instance.m_GameState != value) {
				Instance.m_GameState = value;
				InputManager.SwitchActionMap(value switch {
					GameState.Gameplay => ActionMap.Player,
					GameState.Cutscene => ActionMap.UI,
					GameState.Paused   => ActionMap.UI,
					_ => default,
				});
			}
		}
	}
	public static float TimeScale {
		get => Time.timeScale;
		set => Time.timeScale = Mathf.Clamp(value, 0f, 10f);
	}

	public static Player Player => !Instance.m_Player ?
		Instance.m_Player = FindAnyObjectByType<Player>() :
		Instance.m_Player;



	static Dictionary<uint, byte> EventInstance {
		get => Instance.m_EventInstance;
	}
	static List<(uint, EventBase, float)> EventList {
		get => Instance.m_EventList;
	}
	static List<EventBase> EventBuffer {
		get => Instance.m_EventBuffer;
	}
	static uint NextID {
		get => Instance.m_NextID;
		set => Instance.m_NextID = value;
	}



	public static HashMap<string, int> IntValue {
		get => Instance.m_IntValue;
	}
	public static HashMap<string, float> FloatValue {
		get => Instance.m_FloatValue;
	}
	public static HashMap<string, string> StringValue {
		get => Instance.m_StringValue;
	}



	// Instance Methods

	static uint AddInstance(EventBase eventBase) {
		if (eventBase == null) return default;
		while (++NextID == default || EventInstance.ContainsKey(NextID));
		EventInstance.Add(NextID, 1);
		EventList.Add((NextID, eventBase, 0f));
		return NextID;
	}

	static void RemoveInstances(uint id) {
		byte numEvents = EventInstance[id];
		EventInstance.Remove(id);
		for (int i = EventList.Count; 0 < i--;) {
			var (eventID, eventBase, startTime) = EventList[i];
			if (eventID == id) {
				EventList.RemoveAt(i);
				if (--numEvents == 0) break;
			}
		}
	}

	static void UpdateInstances() {
		if (GameState == GameState.Paused) return;
		int i = 0;
		while (i < EventList.Count) {
			var (eventID, eventBase, startTime) = EventList[i];
			if (startTime == 0f) {
				eventBase.Start();
				EventList[i] = (eventID, eventBase, Time.time);
				continue;
			}
			if (eventBase.Update() == false) {
				i++;
				continue;
			}
			eventBase.End();
			eventBase.GetNexts(EventBuffer);
			int numNexts = EventBuffer.Count;
			if (numNexts == 0) {
				if (--EventInstance[eventID] == 0) EventInstance.Remove(eventID);
				EventList.RemoveAt(i);
			} else {
				if (1 < numNexts) EventInstance[eventID] += (byte)(numNexts - 1);
				EventList[i] = (eventID, EventBuffer[0], 0f);
				for (int j = 1; j < numNexts; j++) EventList.Add((eventID, EventBuffer[j], 0f));
				EventBuffer.Clear();
			}
		}
	}



	// Event Methods

	public static uint PlayEvent(EventGraph eventGraph) {
		return eventGraph == null ? default : AddInstance(eventGraph.Entry);
	}

	public static bool IsEventPlaying(uint eventID = default) {
		return eventID == default ? 0 < EventInstance.Count : EventInstance.ContainsKey(eventID);
	}

	public static void StopEvent(uint eventID) {
		if (EventInstance.ContainsKey(eventID)) RemoveInstances(eventID);
	}



	// Methods

	public static void SaveData() {
		foreach (var (key, value) in IntValue) {
			PlayerPrefs.SetInt(key, value);
		}
		foreach (var (key, value) in FloatValue) {
			PlayerPrefs.SetFloat(key, value);
		}
		foreach (var (key, value) in StringValue) {
			PlayerPrefs.SetString(key, value);
		}
	}

	public static void LoadData() {
		foreach (var key in IntValue.Keys) {
			IntValue[key] = PlayerPrefs.GetInt(key);
		}
		foreach (var key in FloatValue.Keys) {
			FloatValue[key] = PlayerPrefs.GetFloat(key);
		}
		foreach (var key in StringValue.Keys) {
			StringValue[key] = PlayerPrefs.GetString(key);
		}
	}

	public static void QuitGame() {
		#if UNITY_EDITOR
		EditorApplication.isPlaying = false;
		#else
		Application.Quit();
		#endif
	}



	// Lifecycle

	void Start() {
		GameState = GameState.Gameplay;
		UIManager.OpenScreen(Screen.Game);
	}

	void Update() {
		UpdateInstances();
	}
}
