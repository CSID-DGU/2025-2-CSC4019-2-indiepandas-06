using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
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
	BuildingMode,
	Menu,
	Options,
	Shop,
	FadeLoading,
	Inventory,
	Toast,
	Quest,
	Diary,
	Evidence,
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
		Screen.BuildingMode => typeof(BuildingModeScreen),
		Screen.Menu         => typeof(MenuScreen),
		Screen.Options      => typeof(OptionsScreen),
		Screen.Shop         => typeof(ShopScreen),
		Screen.FadeLoading  => typeof(FadeLoadingScreen),
		Screen.Inventory    => typeof(InventoryUICanvas),
		Screen.Toast        => typeof(ToastScreen),
		Screen.Quest        => typeof(QuestScreen),
		Screen.Diary        => typeof(DiaryScreen),
		Screen.Evidence     => typeof(EvidenceScreen),
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
		_ when screenBase is BuildingModeScreen => Screen.BuildingMode,
		_ when screenBase is MenuScreen         => Screen.Menu,
		_ when screenBase is OptionsScreen      => Screen.Options,
		_ when screenBase is ShopScreen         => Screen.Shop,
		_ when screenBase is FadeLoadingScreen  => Screen.FadeLoading,
		_ when screenBase is InventoryUICanvas  => Screen.Inventory,
		_ when screenBase is ToastScreen        => Screen.Toast,
		_ when screenBase is QuestScreen        => Screen.Quest,
		_ when screenBase is DiaryScreen        => Screen.Diary,
		_ when screenBase is EvidenceScreen     => Screen.Evidence,
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

	public static ScreenBase CurrentScreenBase {
		get => ScreenStack.TryPeek(out var overlay) ? overlay : null;
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



	public static ScreenBase OpenScreen(Screen screen) {
		return OpenScreen(ScreenBases[(int)screen]);
	}

	public static ScreenBase OpenScreen(ScreenBase screenBase) {
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

		//ApplyInputPolicyFromStack();
		UIManager.Instance.DeferApplyInputPolicy();
		return screenBase;
	}

	public static void OpenScreenAlt(ScreenBase screenBase) {
		OpenScreen(screenBase);
	}

	public static void CloseScreen(Screen screen) {
		CloseScreen(ScreenBases[(int)screen]);
	}

	public static void CloseScreen(ScreenBase screenBase) {
		/*
		if (ScreenStack.TryPop(out var screen8ase)) {
			if (screenBase == screen8ase) screen8ase.Hide();
		}
		if (ScreenStack.TryPeek(out screen8ase)) {
			UpdateScreenBackground();
			screen8ase.Show();
		}
		UIManager.Instance.DeferApplyInputPolicy();
		*/
		if (screenBase == null) return;

    	// Top이 대상이면 정상 Pop + Hide
		if (ScreenStack.TryPeek(out var top) && ReferenceEquals(top, screenBase)) {
			ScreenStack.Pop();
			top.Hide();

			if (ScreenStack.TryPeek(out var below)) {
				UpdateScreenBackground();
				below.Show();
			} else {
				UpdateScreenBackground();
			}

			UIManager.Instance.DeferApplyInputPolicy();
			return;
		}

		// Top이 아니면, 스택 중간에서 제거
		var temp = new Stack<ScreenBase>();
		bool found = false;

		while (ScreenStack.Count > 0) {
			var s = ScreenStack.Pop();
			if (!found && ReferenceEquals(s, screenBase)) {
				found = true;
				s.Hide();
				break;
			}
			temp.Push(s);
		}
		while (temp.Count > 0) ScreenStack.Push(temp.Pop());

		if (found) {
			UpdateScreenBackground();
			UIManager.Instance.DeferApplyInputPolicy();
		}
	}

	public void DeferApplyInputPolicy() {
		StartCoroutine(ApplyInputPolicyEndOfFrame());
	}

	private IEnumerator ApplyInputPolicyEndOfFrame() {
		yield return new WaitForEndOfFrame();
		ApplyInputPolicyFromStack();
		// RestoreFocusIfNeeded();
	}


	public static void Back() {
		if (ScreenStack.TryPeek(out var screenBase)) {
			screenBase.Back();
		}
	}

    public static InputPolicy GetEffectiveInputPolicy() {
        // UIOnly가 하나라도 있으면 UIOnly
		// Both가 하나라도 있으면 Both
		// 아니면 PlayerOnly
        var policy = InputPolicy.PlayerOnly;

        foreach (var s in ScreenStack) {
            if (s == null) { continue; }
            if (s.InputPolicy == InputPolicy.UIOnly) { return InputPolicy.UIOnly; }
            if (s.InputPolicy == InputPolicy.Both) { policy = InputPolicy.Both; }
        }
        return policy;
    }

    public static void ApplyInputPolicyFromStack() {
        var policy = GetEffectiveInputPolicy();
        InputManager.ApplyInputPolicy(policy);
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
	public static string AlertTitle {
		get => AlertScreen.TitleTextValue;
		set => AlertScreen.TitleTextValue = value;
	}

	public static Action OnAlertClosed {
		get => AlertScreen.OnClosed;
		set => AlertScreen.OnClosed = value;
	}

	public static void ShowAlert(string content, string closeText = "확인", string titleText = "알림", Action onClosed = null) {
		// 내용/버튼 텍스트 설정
		AlertContent = content ?? string.Empty;
		if (!string.IsNullOrEmpty(closeText)) { 
			AlertClose = closeText;
		}
		if (!string.IsNullOrEmpty(titleText)) { 
			AlertTitle = titleText;
		}

		// 이전 핸들러 덮어쓰기
		OnAlertClosed = null;
		if (onClosed != null) {
			OnAlertClosed += onClosed;
		}

		// 화면 열기
		OpenScreen(Screen.Alert);
	}

	public static void CloseAlert() {
		// 열린 상태라면 닫기
		var alert = (AlertScreen)ScreenBases[(int)Screen.Alert];
		if (alert && alert.gameObject.activeSelf) {
			CloseScreen(alert);
		}
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

	// Toast Screen

	[Serializable]
	struct ToastRequest {
		public ToastIconType icon;
		public string title;
		public string body;
		public ToastRequest(ToastIconType i, string t, string b) {
			icon = i; title = t; body = b;
		}
	}

	private Queue<ToastRequest> toastQueue = new();
	private bool toastLoopRunning = false;
	private bool toastClosedFlag = false;

	static ToastScreen ToastScreenSafe => (ToastScreen)ScreenBases[(int)Screen.Toast];
	
	public static void ShowToast(ToastIconType icon, string title, string body) {
		Instance.EnqueueToast(new ToastRequest(icon, title, body));
		Instance.TryStartToastLoop();
	}

	private void EnqueueToast(ToastRequest req) {
		toastQueue.Enqueue(req);
	}

	void TryStartToastLoop() {
		if (toastLoopRunning) {
			return;
		}
		StartCoroutine(ToastLoop());
	}

	private IEnumerator ToastLoop() {
		toastLoopRunning = true;

		// 스크린 하나만 열어서 재사용
		var screen = ToastScreenSafe;
		if (screen == null || !screen.gameObject.activeSelf) {
			screen = OpenScreen(Screen.Toast) as ToastScreen;
		}

		// 중복 연결 방지
		screen.OnClosed -= OnToastClosedSignal;
		screen.OnClosed += OnToastClosedSignal;

		while (toastQueue.Count > 0) {
			var req = toastQueue.Dequeue();

			// 컨텐츠 세팅하고 활성화
			if (!screen.gameObject.activeSelf) { screen.gameObject.SetActive(true); }
			screen.SetContent(req.icon, req.title, req.body);
			// InputPolicy 업데이트 (OpenScreen이 아니므로)
			ApplyInputPolicyFromStack();

			// 닫힘 신호를 기다림
			toastClosedFlag = false;
			screen.PlayAnim();
			yield return new WaitUntil(() => toastClosedFlag);

			// 한 토스트 종료
			// 다음 토스트가 있으면 계속
		}

		// 모두 끝났으면 닫기
		if (screen) {
			//screen.gameObject.SetActive(false);
			Debug.Log("토스트 닫기");
			CloseScreen(Screen.Toast);
		}

		toastLoopRunning = false;
	}

	private void OnToastClosedSignal() {
		toastClosedFlag = true;
	}


	// Dialogue Screen Methods

	static DialogueScreen DialogueScreen {
		get => (DialogueScreen)ScreenBases[(int)Screen.Dialogue];
	}

	public static void EnqueueDialogue(string name, string text, Action onEnd = null) {
		if (!DialogueScreen.gameObject.activeSelf) OpenScreen(Screen.Dialogue);
		DialogueScreen.EnqueueDialogue(name, text, onEnd);
	}

	public static void EnqueueDialogue(string name, string text, string bgSpriteNameOrNull, Action onEnd = null) {
		if (!DialogueScreen.gameObject.activeSelf) OpenScreen(Screen.Dialogue);
		DialogueScreen.EnqueueDialogue(name, text, bgSpriteNameOrNull, onEnd);
	}

	public static void BeginDialogueInput(Action<MultimodalData> onEnd = null) {
		if (!DialogueScreen.gameObject.activeSelf) OpenScreen(Screen.Dialogue);
		DialogueScreen.BeginDialogueInput(onEnd);
	}

	public static void SetDialogueOverlayTransparentMode(bool on) {
        if (DialogueScreen) DialogueScreen.SetOverlayTransparentMode(on);
    }

	private int dialogueHoldCount = 0;
    public static bool IsDialogueHeld => Instance && Instance.dialogueHoldCount > 0;
	public static bool HasPendingDialogue => DialogueScreen && DialogueScreen.DialogueQueueCount > 0;
	public static bool IsChoiceOpen => DialogueScreen && DialogueScreen.IsChoiceOpen;
	public static bool IsDialogueInput => DialogueScreen && DialogueScreen.IsInputActive;

	public static void HoldDialogue() {
		Debug.Log("Hold!! ");
        if (Instance) { Instance.dialogueHoldCount++; }
    }

    public static void ReleaseDialogue() {
		Debug.Log("ReleaseDialogue");
        if (Instance && Instance.dialogueHoldCount > 0) { Instance.dialogueHoldCount--; }
    }

    // 프레임 대기 후에 풀어주기 (깜빡임 방지)
    public static void ReleaseDialogueDeferred() {
		Debug.Log("Try ReleaseDialogueDeferred");
        if (!Instance) { return; }
        Instance.StartCoroutine(ReleaseDialogueCo());
    }
    private static IEnumerator ReleaseDialogueCo() {
		Debug.Log("Try ReleaseDialogueCo");
        yield return null;
        ReleaseDialogue();
    }

	// 선택지 관련
	public static void BeginChoices() {
		if (!DialogueScreen.gameObject.activeSelf) { OpenScreen(Screen.Dialogue); }
		DialogueScreen.BeginChoices();
	}

	public static void AddChoice(string text, Action onChosen = null) {
		if (!DialogueScreen.gameObject.activeSelf) { return; }
		DialogueScreen.AddChoice(text, onChosen);
	}

	public static void ShowChoices() {
		if (!DialogueScreen.gameObject.activeSelf) { return; }
		DialogueScreen.ShowChoices();
	}

	// 감정탐정 관련
	static EvidenceScreen EvidenceScreen {
		get => (EvidenceScreen)ScreenBases[(int)Screen.Evidence];
	}

	public static EvidenceScreen OpenEvidence(DetectiveScenarioSO so,
                                    Action<int> onFound,
                                    Action onClosed,
                                    Func<bool> shouldAutoClose)
    {
        var ev = (EvidenceScreen)UIManager.OpenScreen(Screen.Evidence);
        ev.Open(so, onFound, onClosed, shouldAutoClose);
		return ev;
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

	Selectable prevSelected;

	void LateUpdate() {
		UpdateScreenResolution();
		if (prevSelected != Selected && Selected != null) {
			prevSelected = Selected;
			AudioManager.PlaySoundFX(Audio.Click, 0.8f);
		}
	}

	protected override void OnDestroy() {
		ScreenBlur = false;
		base.OnDestroy();
	}
}
