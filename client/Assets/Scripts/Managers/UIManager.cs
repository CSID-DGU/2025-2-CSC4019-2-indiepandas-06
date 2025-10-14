using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif



public enum Screen {
	Alert,
	Confirmation,
	Debug,
	Dialogue,
	Fade,
	Game,
	MainMenu,
	MapEditor,
	Menu,
	Options,
	Shop,
}

public static class ScreenExtensions {
	public static Type ToType(this Screen screen) => screen switch {
		Screen.Alert        => typeof(AlertScreen),
		Screen.Confirmation => typeof(ConfirmationScreen),
		Screen.Debug        => typeof(DebugScreen),
		Screen.Dialogue     => typeof(DialogueScreen),
		Screen.Fade         => typeof(FadeScreen),
		Screen.Game         => typeof(GameScreen),
		Screen.MainMenu     => typeof(MainMenuScreen),
		Screen.MapEditor    => typeof(MapEditorScreen),
		Screen.Menu         => typeof(MenuScreen),
		Screen.Options      => typeof(OptionsScreen),
		Screen.Shop         => typeof(ShopScreen),
		_ => default,
	};

	public static Screen ToScreen(this ScreenBase screenBase) => screenBase switch {
		_ when screenBase is AlertScreen        => Screen.Alert,
		_ when screenBase is ConfirmationScreen => Screen.Confirmation,
		_ when screenBase is DebugScreen        => Screen.Debug,
		_ when screenBase is DialogueScreen     => Screen.Dialogue,
		_ when screenBase is FadeScreen         => Screen.Fade,
		_ when screenBase is GameScreen         => Screen.Game,
		_ when screenBase is MainMenuScreen     => Screen.MainMenu,
		_ when screenBase is MapEditorScreen    => Screen.MapEditor,
		_ when screenBase is MenuScreen         => Screen.Menu,
		_ when screenBase is OptionsScreen      => Screen.Options,
		_ when screenBase is ShopScreen         => Screen.Shop,
		_ => default,
	};
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// UI Manager
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("Manager/UI Manager")]
public class UIManager : MonoSingleton<UIManager> {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(UIManager))]
	class UIManagerEditor : EditorExtensions {
		UIManager I => target as UIManager;
		public override void OnInspectorGUI() {
			Begin();

			LabelField("Background", EditorStyles.boldLabel);
			SceneTexture     = ObjectField("Scene Texture", SceneTexture);
			StaticBackground = ObjectField("UI Background", StaticBackground);
			Space();

			LabelField("Screen", EditorStyles.boldLabel);
			if (ScreenBases?.Length != ScreenCount) ScreenBases = LoadScreenBases();
			foreach (var screenBase in ScreenBases) {
				BeginHorizontal();
				if (screenBase == null) {
					var screen = (Screen)Array.IndexOf(ScreenBases, screenBase);
					PrefixLabel($"{screen} Screen");
					bool value = EditorGUILayout.Toggle(false, GUILayout.Width(14));
					if (value) ScreenBases = LoadScreenBases();
					ObjectField(null as GameObject);
					EndHorizontal();
					var message = string.Empty;
					message += $"{screen} Screen is missing.\n";
					message += $"Please add {screen} Screen to child of this object ";
					HelpBox(message, MessageType.Error);
					BeginHorizontal();
				} else {
					var screen = screenBase.ToScreen();
					PrefixLabel($"{screen} Screen");
					bool match = screenBase.gameObject.activeSelf;
					bool value = EditorGUILayout.Toggle(match, GUILayout.Width(14));
					ObjectField(screenBase.gameObject);
					if (match != value) {
						foreach (var screen8ase in ScreenBases) if (screen8ase) {
							var active = value && (screen8ase == screenBase);
							if (active) {
								switch (screen8ase.BackgroundMode) {
									case BackgroundMode.Scene:
									case BackgroundMode.SceneBlur:
										SceneTexture.gameObject.SetActive(true);
										StaticBackground.gameObject.SetActive(false);
										break;
									case BackgroundMode.StaticBackground:
										SceneTexture.gameObject.SetActive(false);
										StaticBackground.gameObject.SetActive(true);
										break;
								}
								ScreenBlur = screen8ase.BackgroundMode switch {
									BackgroundMode.SceneBlur => true,
									BackgroundMode.PreserveWithBlur => true,
									_ => false,
								};
							}
							screen8ase.gameObject.SetActive(active);
						}
					}
				}
				EndHorizontal();
			}
			Space();

			End();
		}
	}
	#endif



	// Constants

	static readonly int ScreenCount = Enum.GetValues(typeof(Screen)).Length;

	public static readonly Vector2Int ReferenceResolution = new(480, 270);
	public static readonly Vector2Int[] ResolutionPresets = new Vector2Int[] {
		new(0480, 0270),
		new(0960, 0540),
		new(1440, 0810),
		new(1920, 1080),
		new(2560, 1440),
		new(3840, 2160),
	};

	static readonly int BlurOffset = Shader.PropertyToID("_Blur_Offset");



	// Fields

	[SerializeField] RawImage m_SceneTexture;
	[SerializeField] Image m_StaticBackground;

	CanvasScaler m_ScreenScaler;
	Vector2Int m_ScreenResolution;
	ScreenBase[] m_ScreenBases;
	Stack<ScreenBase> m_ScreenStack = new();



	// Properties

	static RawImage SceneTexture {
		get => Instance.m_SceneTexture;
		set => Instance.m_SceneTexture = value;
	}
	static Image StaticBackground {
		get => Instance.m_StaticBackground;
		set => Instance.m_StaticBackground = value;
	}
	static bool ScreenBlur {
		set => SceneTexture.material.SetFloat(BlurOffset, value ? CanvasScale : 0f);
	}



	static CanvasScaler CanvasScaler => !Instance.m_ScreenScaler ?
		Instance.m_ScreenScaler = Instance.GetOwnComponent<CanvasScaler>() :
		Instance.m_ScreenScaler;

	static float CanvasScale {
		get => CanvasScaler.scaleFactor;
		set => CanvasScaler.scaleFactor = value;
	}
	public static Vector2Int ScreenResolution {
		get         => Instance.m_ScreenResolution;
		private set => Instance.m_ScreenResolution = value;
	}

	static ScreenBase[] ScreenBases {
		get => Instance.m_ScreenBases;
		set => Instance.m_ScreenBases = value;
	}
	static Stack<ScreenBase> ScreenStack {
		get => Instance.m_ScreenStack;
	}
	public static Screen? CurrentScreen {
		get => ScreenStack.TryPeek(out var overlay) ? overlay.ToScreen() : null;
	}



	static GameObject SelectedGameObject {
		get => EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
		set => EventSystem.current.SetSelectedGameObject(value);
	}
	public static Selectable Selected {
		get {
			var gameObject = SelectedGameObject;
			if (gameObject && gameObject.TryGetComponent(out Selectable selectable)) {
				return selectable;
			} else return null;
		}
		set => SelectedGameObject = !value ? null : value.gameObject;
	}



	// Initialization Methods

	static ScreenBase[] LoadScreenBases() {
		var screenBases = new ScreenBase[ScreenCount];
		for (int i = 0; i < ScreenCount; i++) {
			var screen = (Screen)i;
			var screenBase = GetChildComponentRecursive(Instance.transform, screen.ToType());
			screenBases[i] = (ScreenBase)screenBase;
		}
		return screenBases;
	}

	static Component GetChildComponentRecursive(Transform parent, Type type) {
		if (parent.TryGetComponent(type, out var component)) return component;
		for (int i = 0; i < parent.childCount; i++) {
			var child = parent.GetChild(i);
			component = GetChildComponentRecursive(child, type);
			if (component) return component;
		}
		return null;
	}



	// Screen Methods

	static void UpdateScreenResolution() {
		bool match = false;
		match = match || ScreenResolution.x != UnityEngine.Screen.width;
		match = match || ScreenResolution.y != UnityEngine.Screen.height;
		if (match) {
			ScreenResolution = new(UnityEngine.Screen.width, UnityEngine.Screen.height);
			float xRatio = UnityEngine.Screen.width / ReferenceResolution.x;
			float yRatio = UnityEngine.Screen.height / ReferenceResolution.y;
			float multiplier = Mathf.Max(1, (int)Mathf.Min(xRatio, yRatio));
			CanvasScale = multiplier;
		}
	}

	static void UpdateScreenBackground() {
		if (ScreenStack.TryPeek(out var screenBase)) {
			switch (screenBase.BackgroundMode) {
				case BackgroundMode.Scene:
				case BackgroundMode.SceneBlur:
					if (SceneTexture.gameObject.activeSelf != true) {
						SceneTexture.gameObject.SetActive(true);
						SceneTexture.transform.SetAsLastSibling();
					}
					StaticBackground.gameObject.SetActive(false);
					break;
				case BackgroundMode.StaticBackground:
					if (StaticBackground.gameObject.activeSelf != true) {
						StaticBackground.gameObject.SetActive(true);
						StaticBackground.transform.SetAsLastSibling();
					}
					SceneTexture.gameObject.SetActive(false);
					break;
			}
			ScreenBlur = screenBase.BackgroundMode switch {
				BackgroundMode.SceneBlur => true,
				BackgroundMode.PreserveWithBlur => true,
				_ => false,
			};
		}
	}



	public static void OpenScreen(Screen screen) {
		OpenScreen(ScreenBases[(int)screen]);
	}

	public static void OpenScreen(ScreenBase screenBase) {
		if (screenBase.IsPrimary) {
			while (ScreenStack.TryPop(out var screen8ase)) {
				screen8ase.Hide();
			}
		} else if (ScreenStack.TryPeek(out var screen8ase)) {
			if (screen8ase.IsOverlay) screen8ase.Hide();
		}
		ScreenStack.Push(screenBase);
		UpdateScreenBackground();
		screenBase.Show();
	}

	public static void CloseScreen(ScreenBase screenBase) {
		if (ScreenStack.TryPop(out var screen8ase)) {
			if (screenBase == screen8ase) screen8ase.Hide();
		}
		if (ScreenStack.TryPeek(out screen8ase)) {
			UpdateScreenBackground();
			screen8ase.Show();
		}
	}

	public static void Back() {
		if (ScreenStack.TryPeek(out var screenBase)) {
			screenBase.Back();
		}
	}



	// Alert Screen Methods

	static AlertScreen AlertScreen {
		get => (AlertScreen)ScreenBases[(int)Screen.Alert];
	}

	public static string AlertContent {
		get => AlertScreen.ContentTextValue;
		set => AlertScreen.ContentTextValue = value;
	}
	public static string AlertClose {
		get => AlertScreen.CloseTextValue;
		set => AlertScreen.CloseTextValue = value;
	}

	public static Action OnAlertClosed {
		get => AlertScreen.OnClosed;
		set => AlertScreen.OnClosed = value;
	}



	// Confirmation Screen Methods

	static ConfirmationScreen ConfirmationScreen {
		get => (ConfirmationScreen)ScreenBases[(int)Screen.Confirmation];
	}

	public static string ConfirmationHeader {
		get => ConfirmationScreen.HeaderTextValue;
		set => ConfirmationScreen.HeaderTextValue = value;
	}
	public static string ConfirmationContent {
		get => ConfirmationScreen.ContentTextValue;
		set => ConfirmationScreen.ContentTextValue = value;
	}
	public static string ConfirmationConfirm {
		get => ConfirmationScreen.ConfirmTextValue;
		set => ConfirmationScreen.ConfirmTextValue = value;
	}
	public static string ConfirmationCancel {
		get => ConfirmationScreen.CancelTextValue;
		set => ConfirmationScreen.CancelTextValue = value;
	}

	public static Action OnConfirmationConfirmed {
		get => ConfirmationScreen.OnConfirmed;
		set => ConfirmationScreen.OnConfirmed = value;
	}
	public static Action OnConfirmationCancelled {
		get => ConfirmationScreen.OnCancelled;
		set => ConfirmationScreen.OnCancelled = value;
	}



	// Dialogue Screen Methods

	static DialogueScreen DialogueScreen {
		get => (DialogueScreen)ScreenBases[(int)Screen.Dialogue];
	}

	public static void EnqueueDialogue(string name, string text, Action onEnd = null) {
		if (!DialogueScreen.gameObject.activeSelf) OpenScreen(Screen.Dialogue);
		DialogueScreen.EnqueueDialogue(name, text, onEnd);
	}

	public static void BeginDialogueInput(Action<MultimodalData> onEnd = null) {
		if (!DialogueScreen.gameObject.activeSelf) OpenScreen(Screen.Dialogue);
		DialogueScreen.BeginDialogueInput(onEnd);
	}



	// Game Screen Methods

	static GameScreen GameScreen {
		get => (GameScreen)ScreenBases[(int)Screen.Game];
	}

	public static void ShowGemCollectMessage(string message) {
		GameScreen.ShowGemCollectMessage(message);
	}



	// Shop Screen Methods

	static ShopScreen ShopScreen {
		get => (ShopScreen)ScreenBases[(int)Screen.Shop];
	}

	public static List<ItemEntry> ShopItemList {
		get => ShopScreen.ItemList;
		set => ShopScreen.ItemList = value;
	}
	public static List<FeatureEntry> ShopFeatureList {
		get => ShopScreen.FeatureList;
		set => ShopScreen.FeatureList = value;
	}



	// Lifecycle

	protected override void Awake() {
		base.Awake();
		ScreenBases = LoadScreenBases();
		foreach (var screenBase in ScreenBases) {
			screenBase.gameObject.SetActive(false);
		}
	}

	void LateUpdate() {
		UpdateScreenResolution();
	}

	protected override void OnDestroy() {
		ScreenBlur = false;
		base.OnDestroy();
	}
}
