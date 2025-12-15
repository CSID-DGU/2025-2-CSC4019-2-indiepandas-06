using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using System.Collections;

using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Dialogue Screen
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("UI/Dialogue Screen")]
public class DialogueScreen : ScreenBase {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(DialogueScreen))]
	class DialogueScreenEditor : EditorExtensions {
		DialogueScreen I => target as DialogueScreen;
		public override void OnInspectorGUI() {
			Begin("Dialogue Screen");

			LabelField("Selected", EditorStyles.boldLabel);
			I.DefaultSelected = ObjectField("Default Selected", I.DefaultSelected);
			Space();

			LabelField("Speaker Name", EditorStyles.boldLabel);
			I.NameTransform = ObjectField("Name Transform", I.NameTransform);
			I.NameTextUGUI  = ObjectField("Name Text UGUI", I.NameTextUGUI);
			if (I.NameTextUGUI) I.NameText = TextField("Name Text", I.NameText);
			Space();
			LabelField("Dialogue Text", EditorStyles.boldLabel);
			I.TextTransform = ObjectField("Text Transform", I.TextTransform);
			I.TextTextUGUI  = ObjectField("Text Text UGUI", I.TextTextUGUI);
			if (I.TextTextUGUI) I.TextText = TextArea("Text Text", I.TextText);
			Space();
			LabelField("Background", EditorStyles.boldLabel);
			I.m_BackgroundImageFront = ObjectField("Background Image Front", I.m_BackgroundImageFront);
			I.m_BackgroundImageBack = ObjectField("Background Image Back", I.m_BackgroundImageBack);
			I.m_BackgroundGroup = ObjectField("Background Group", I.m_BackgroundGroup);
			Space();
			LabelField("Player Input", EditorStyles.boldLabel);
			I.InputField = ObjectField("Input Field", I.InputField);
			if (I.InputField) I.InputText = TextField("Input Text", I.InputText);
			if (I.InputField) I.EnableInput = Toggle("Enable Input", I.EnableInput);
			Space();
			LabelField("Choice Input", EditorStyles.boldLabel);
			I.ChoiceContainer = ObjectField("Choice Container", I.ChoiceContainer);
			I.ChoiceItemPrefab = ObjectField("Choice Item Prefab", I.ChoiceItemPrefab);
			Space();
			LabelField("Settings", EditorStyles.boldLabel);
			I.TextDisplayDelay = Slider("Text Display Delay", I.TextDisplayDelay, 0.01f, 0.10f);
			I.AutoPlay = Toggle("Auto Play", I.AutoPlay);
			if (I.AutoPlay) I.AutoPlayDelay = Slider("Auto Play Delay", I.AutoPlayDelay, 1f, 10f);
			Space();
			I.MicRecorder = ObjectField("Mic Recorder", I.MicRecorder);
			End();
		}
	}
	#endif



	// Fields

	[SerializeField] RectTransform m_NameTransform;
	[SerializeField] TextMeshProUGUI m_NameTextUGUI;
	[SerializeField] RectTransform m_TextTransform;
	[SerializeField] TextMeshProUGUI m_TextTextUGUI;
	[SerializeField] TMP_InputField m_InputField;
	[SerializeField] Image m_BackgroundImageBack;
	[SerializeField] Image m_BackgroundImageFront;
	[SerializeField] CanvasGroup m_BackgroundGroup;
	private const float m_BackgroundFadeDuration = 0.5f;
	private string m_CurrentBGKey = ""; 
	private Coroutine m_BGGroupCo = null;
	private Coroutine m_BGCrossCo = null;
	float GroupAlpha => m_BackgroundGroup ? m_BackgroundGroup.alpha : 0f;
	[SerializeField] MicRecorder m_MicRecorder;
	UnityEvent OnDialogueInputEnd = new();

	[SerializeField] float m_TextDisplayDelay = 0.04f;
	float m_TextDisplayTimer;
	int m_TextIndex;
	[SerializeField] bool m_AutoPlay;
	[SerializeField] float m_AutoPlayDelay = 2.0f;

	// 선택지 Ref
	[SerializeField] private RectTransform m_ChoiceContainer;
	[SerializeField] private Button m_ChoiceItemPrefab;

	// 버튼 풀링
	private readonly List<Button> choicePool = new();
	private readonly List<Button> activeChoices = new();

	private bool choiceVisible = false;
	private bool hasPendingChoices = false;
	private bool blockCloseThisFrame = false;
	uint audioID;


	Queue<(string name, string text, Action onEnd)> m_DialogueQueue = new();
	// 배경 큐
	private readonly Queue<string> m_DialogueBGQueue = new();

	// Properties

	public override bool IsPrimary => false;
	//public override bool IsOverlay => false;
	//public override BackgroundMode BackgroundMode => BackgroundMode.Scene;

	public int DialogueQueueCount => m_DialogueQueue.Count;
	public bool IsChoiceOpen => choiceVisible || hasPendingChoices;
	public bool IsInputActive => EnableInput;

	private bool overlayTransparentMode = false;

    // 외부에서 켜고 끄기
    public void SetOverlayTransparentMode(bool on) {
        overlayTransparentMode = on;
    }

    public override bool IsOverlay => overlayTransparentMode ? true : base.IsOverlay;
    public override BackgroundMode BackgroundMode => overlayTransparentMode ? BackgroundMode.Preserve : BackgroundMode.Scene;

	RectTransform NameTransform {
		get => m_NameTransform;
		set => m_NameTransform = value;
	}
	TextMeshProUGUI NameTextUGUI {
		get => m_NameTextUGUI;
		set => m_NameTextUGUI = value;
	}
	string NameText {
		get => NameTextUGUI.text;
		set {
			NameTextUGUI.text = value;
			if (NameTransform) {
				float width = Mathf.Max(80f, NameTextUGUI.GetPreferredValues().x);
				NameTransform.sizeDelta = new Vector2(width, NameTransform.sizeDelta.y);
			}
		}
	}

	RectTransform TextTransform {
		get => m_TextTransform;
		set => m_TextTransform = value;
	}
	TextMeshProUGUI TextTextUGUI {
		get => m_TextTextUGUI;
		set => m_TextTextUGUI = value;
	}
	string TextText {
		get => TextTextUGUI.text;
		set => TextTextUGUI.text = value;
	}

	TMP_InputField InputField {
		get => m_InputField;
		set => m_InputField = value;
	}
	string InputText {
		get => InputField.text;
		set => InputField.text = value;
	}
	Image BackgroundImageBack {
		get => m_BackgroundImageBack;
		set => m_BackgroundImageBack = value;
	}
	Image BackgroundImageFront {
		get => m_BackgroundImageFront;
		set => m_BackgroundImageFront = value;
	}
	CanvasGroup BackgroundGroup {
		get => m_BackgroundGroup;
		set => m_BackgroundGroup = value;
	}

	bool EnableInput {
		get => InputField.gameObject.activeSelf;
		set {
			InputField.gameObject.SetActive(value);
			if (TextTransform) {
				TextTransform.anchoredPosition = new Vector2(0f, EnableInput ? 0f : -12f);
			}
		}
	}



	float TextDisplayDelay {
		get => m_TextDisplayDelay;
		set => m_TextDisplayDelay = value;
	}
	float TextDisplayTimer {
		get => m_TextDisplayTimer;
		set => m_TextDisplayTimer = value;
	}
	int TextIndex {
		get => m_TextIndex;
		set => m_TextIndex = value;
	}

	bool AutoPlay {
		get => m_AutoPlay;
		set => m_AutoPlay = value;
	}
	float AutoPlayDelay {
		get => m_AutoPlayDelay;
		set => m_AutoPlayDelay = value;
	}

	// 선택지
	RectTransform ChoiceContainer {
		get => m_ChoiceContainer;
		set => m_ChoiceContainer = value;
	}
	Button ChoiceItemPrefab {
		get => m_ChoiceItemPrefab;
		set => m_ChoiceItemPrefab = value;
	}

	MicRecorder MicRecorder {
		get => m_MicRecorder;
		set => m_MicRecorder = value;
	}



	Queue<(string name, string text, Action onEnd)> DialogueQueue => m_DialogueQueue;



	// Methods

	public void EnqueueDialogue(string name, string text, Action onEnd = null) {
		//DialogueQueue.Enqueue((name, text, onEnd));
		EnqueueDialogue(name, text, bgSpriteNameOrNull: null, onEnd: onEnd);
	}
	public void EnqueueDialogue(string name, string text, string bgSpriteNameOrNull, Action onEnd = null) {
		m_DialogueQueue.Enqueue((name, text, onEnd));
		m_DialogueBGQueue.Enqueue(bgSpriteNameOrNull); 
	}

	public void BeginDialogueInput(Action<MultimodalData> onEnd = null) {
		InputText = "";
		EnableInput = true;
		OnDialogueInputEnd.RemoveAllListeners();
		OnDialogueInputEnd.AddListener(() => {
			EnableInput = false;
			onEnd?.Invoke(new MultimodalData {
				text = InputText,
				voice = MicRecorder.Clip,
			});
		});
		UIManager.Selected = InputField;
	}



	/*
	Dialogue Method Manual

	Use like this:
	"Text...text{Delay(50)}text";
	{MethodName(Argument0, Argument1, ...)}

	Method List:
	- Delay(float seconds)
	- IntValue(string key, string format = "N0")
	- FloatValue(string key, string format = "F2")
	- StringValue(string key)
	*/

	bool TryGetFunction(string text, int start, out int end, out string func, out string[] args) {
		const StringSplitOptions RemoveEntries = StringSplitOptions.RemoveEmptyEntries;
		static bool IsValid(int a, int b) => (0 <= a) && (a < b);
		int a = text.IndexOf('{', start);
		int b = text.IndexOf('}', a + 1);
		if (IsValid(a, b)) {
			var fullCommand = text[(a + 1)..b];
			int c = fullCommand.IndexOf('(');
			int d = fullCommand.LastIndexOf(')');
			bool isValid = IsValid(c, d);
			end = b + 1;
			func = isValid ? fullCommand[..c] : fullCommand;
			args = isValid ? fullCommand[(c + 1)..d].Split(',', RemoveEntries) : null;
			return true;
		} else {
			end = -1;
			func = null;
			args = null;
			return false;
		}
	}

	void UpdateDialogue() {
		TextDisplayTimer -= Time.deltaTime;

		bool match = false;
		match |= InputManager.GetKeyDown(KeyAction.Submit);
		match |= InputManager.GetKeyDown(KeyAction.Cancel);
		if (UIManager.Selected && UIManager.Selected.gameObject == MicRecorder.gameObject) {
			match = false;
		}
		if (EnableInput && match) {
			OnDialogueInputEnd.Invoke();
			return;
		}

		if (DialogueQueue.TryPeek(out var value)) {
			var (name, text, onEnd) = value;

			if (TextIndex > text.Length) { TextIndex = 0; }

			bool initialize = TextIndex == 0;
			if (initialize) {
				NameText = name;
				if (TextText != "") {
					TextText = "";
				}

				// 배경 적용
				if (m_DialogueBGQueue.TryPeek(out var bgCmd) && bgCmd != null) {
					SetBackground(bgCmd); // ""면 클리어, 문자열이면 교체
				}
			}
			// 즉시 타이핑 조건에 선택지 조건 추가
			bool wasAlreadyFinished = TextIndex >= text.Length;
			bool allowAdvance = !choiceVisible && !hasPendingChoices;

			// 입력을 미리 저장
			bool inputSubmit = allowAdvance && InputManager.GetKeyDown(KeyAction.Submit);
            bool inputCancel = allowAdvance && InputManager.GetKeyDown(KeyAction.Cancel);

			bool displayInstantly = false;
			//displayInstantly |= allowAdvance && InputManager.GetKeyDown(KeyAction.Submit);
			//displayInstantly |= allowAdvance && InputManager.GetKeyDown(KeyAction.Cancel);
			displayInstantly |= inputSubmit;
            displayInstantly |= inputCancel;

			while (TextIndex < text.Length && (TextDisplayTimer <= 0f || displayInstantly)) {
				if (audioID == default) {
					audioID = AudioManager.PlaySoundFX(Audio.Dialogue, 0.2f);
				}
				char next = text[TextIndex];
				bool flag = next == '{';
				if (flag && TryGetFunction(text, TextIndex, out int end, out var func, out var args)) {
					bool isArgsValid = args != null && 0 < args.Length;
					switch (func) {
						case "Delay":
							if (isArgsValid && float.TryParse(args[0], out float delay)) {
								TextDisplayTimer = delay;
							}
							break;
						case "IntValue":
							if (isArgsValid) TextText += (1 < args.Length) ?
								GameManager.IntValue[args[0]].ToString(args[1]) :
								GameManager.IntValue[args[0]].ToString("N0");
							break;
						case "FloatValue":
							if (isArgsValid) TextText += (1 < args.Length) ?
								GameManager.FloatValue[args[0]].ToString(args[1]) :
								GameManager.FloatValue[args[0]].ToString("F2");
							break;
						case "StringValue":
							if (isArgsValid) TextText +=
								GameManager.StringValue[args[0]];
							break;
						case "BG":
							// {BG(SpriteName)}
							// {BG()} 로 숨기기
							string key = (isArgsValid ? args[0].Trim() : "");
							SetBackground(key);
							break;
					}
					TextIndex = end;
				} else {
					TextText += next;
					TextDisplayTimer = TextDisplayDelay;
					TextIndex++;
				}
				if (TextIndex == text.Length) {
					TextDisplayTimer = AutoPlayDelay;
				}
			}
			
			bool dequeueDialogue = false;
			dequeueDialogue |= inputSubmit && wasAlreadyFinished;
			//dequeueDialogue |= allowAdvance && TextIndex == text.Length && InputManager.GetKeyDown(KeyAction.Submit);
			dequeueDialogue |= allowAdvance && TextIndex == text.Length && AutoPlay && (TextDisplayTimer <= 0f);

			if (dequeueDialogue) {
				TextDisplayTimer = 0f;
				TextIndex = 0;
				// 디큐 (배경 이미지도 함께)
				DialogueQueue.Dequeue();
				if (m_DialogueBGQueue.Count > 0) {
					m_DialogueBGQueue.Dequeue();
				}
				onEnd?.Invoke();

				blockCloseThisFrame = true;
				return;
			}
		} else if (EnableInput || choiceVisible || hasPendingChoices || blockCloseThisFrame || UIManager.IsDialogueHeld) { // 닫기 조건 추가 
			TextDisplayTimer = 0f;
			blockCloseThisFrame = false;

			TextIndex = 0;
		} else {
			UIManager.Back();
			TextIndex = 0;
		}
	}

	public void BeginChoices() {
		hasPendingChoices = true;
		blockCloseThisFrame = true;
		// 선택지 초기화
		ClearChoices();
		m_ChoiceContainer.gameObject.SetActive(true);
		choiceVisible = true;
	}

	public void AddChoice(string text, Action onChosen = null) {
		var choice = GetChoiceItem();
		// 텍스트 설정
		var choiceText = choice.GetComponentInChildren<TextMeshProUGUI>();
		if (choiceText != null) {
			choiceText.text = text ?? string.Empty;
		}
		// 클릭 액션 등록
		choice.onClick.RemoveAllListeners();
		choice.onClick.AddListener(() => {
			HideChoices();
			onChosen?.Invoke();
		});
		// 리스트에 추가
		activeChoices.Add(choice);
	}

	public void ShowChoices() {
		if (!choiceVisible) { return; }
		if (activeChoices.Count <= 0) { return; }
		m_ChoiceContainer.gameObject.SetActive(true);

		blockCloseThisFrame = true;

		//EventSystem.current?SetSelectedGameObject(activeChoices[0].gameObject);
		
		StartCoroutine(LayoutRebuild());
	}
	private IEnumerator LayoutRebuild() {
		yield return new WaitForEndOfFrame();
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_ChoiceContainer);
	}

	public void HideChoices() {
		if (!choiceVisible) { return; }
		choiceVisible = false;
		hasPendingChoices = false;
		blockCloseThisFrame = true;
		m_ChoiceContainer.gameObject.SetActive(false);

		foreach (var button in activeChoices) {
			button.onClick.RemoveAllListeners();
			button.gameObject.SetActive(false);
			// 풀로 릴리즈
			choicePool.Add(button);
		}
		activeChoices.Clear();
	}

	// 선택지 풀링
	private Button GetChoiceItem() {
		for (int i = choicePool.Count - 1; i >= 0; --i) {
			var button = choicePool[i];
			choicePool.RemoveAt(i);
			button.transform.SetParent(m_ChoiceContainer, false);
			button.gameObject.SetActive(true);
			return button;
		}

		var inst = Instantiate(m_ChoiceItemPrefab, m_ChoiceContainer);
		inst.gameObject.SetActive(true);
		return inst;
	}

	private void ClearChoices() {
		for (int i = m_ChoiceContainer.childCount - 1; i >= 0; --i) {
			var obj = m_ChoiceContainer.GetChild(i);
			var button = obj.GetComponent<Button>();
			if (button != null) {
				// 리스너 날리기
				button.onClick.RemoveAllListeners();
				// 풀로 릴리즈
				button.gameObject.SetActive(false);
				choicePool.Add(button);
			}
		}
		activeChoices.Clear();
	}

	// 배경 이미지 관련
	public void SetBackground(string spriteName) {
		if (!m_BackgroundImageBack || !m_BackgroundImageFront || !m_BackgroundGroup) return;

		// 유지
		if (spriteName == null) {
			return;
		}

		// 클리어
		if (spriteName.Length == 0) {
			StopBGCrossAnim();
			NormalizeBGState();
			StopBGGroupAnim();
			m_BGGroupCo = StartCoroutine(BGGroupFadeTo(0f, m_BackgroundFadeDuration, () => {
				m_BackgroundImageBack.sprite = null;
				m_BackgroundImageFront.sprite = null;
				SetImageAlpha(m_BackgroundImageFront, 0f);
				m_CurrentBGKey = "";
			}));
			return;
		}

		// 스프라이트 로드
		var next = AtlasManager.Instance ? AtlasManager.Instance.Get("Dialogue", spriteName) : null;
		if (!next) {
			// 못 찾으면 클리어
			SetBackground("");
			return;
		}

		bool hasCurrent = !string.IsNullOrEmpty(m_CurrentBGKey) && m_BackgroundImageBack.sprite != null && GroupAlpha > 0.001f;

		// 없다가 생김
		if (!hasCurrent) {
			StopBGCrossAnim();
			NormalizeBGState();
			m_BackgroundImageBack.sprite = next;
			m_CurrentBGKey = spriteName;

			StopBGGroupAnim();
			m_BGGroupCo = StartCoroutine(BGGroupFadeTo(1f, m_BackgroundFadeDuration));
			return;
		}

		// 같은 배경이면 유지
		if (m_CurrentBGKey == spriteName && m_BackgroundImageBack.sprite == next) {
			return;
		}

		// 다른 배경이면 교차 페이드
		StopBGCrossAnim();
		m_BGCrossCo = StartCoroutine(BGCrossfadeCo(next, spriteName));
	}

	IEnumerator BGGroupFadeTo(float target, float duration, Action onComplete = null) {
		if (!m_BackgroundGroup) {
			yield break;
		}

		float start = m_BackgroundGroup.alpha;

		if (Mathf.Approximately(start, target) || duration <= 0f) {
			m_BackgroundGroup.alpha = target;
			onComplete?.Invoke();
			yield break;
		}

		float t = 0f;

		while (t < duration) {
			t += Time.unscaledDeltaTime;
			float k = Mathf.Clamp01(t / duration);
			m_BackgroundGroup.alpha = Mathf.Lerp(start, target, k);
			yield return null;
		}
		m_BackgroundGroup.alpha = target;
		onComplete?.Invoke();
	}

	IEnumerator BGCrossfadeCo(Sprite nextSprite, string nextKey) {
		// Front를 0으로
		SetImageAlpha(m_BackgroundImageFront, 0f);

		// Front 스프라이트 교체
		m_BackgroundImageFront.sprite = nextSprite;

		// Front 0 -> 1
		float t = 0f, dur = Mathf.Max(0.01f, m_BackgroundFadeDuration * 2f);
		while (t < dur) {
			t += Time.unscaledDeltaTime;
			float k = Mathf.Clamp01(t / dur);
			SetImageAlpha(m_BackgroundImageFront, k);

			// 0.995 지점에서 Back 동기화
			//if (k >= 0.995f && m_BackgroundImageBack.sprite != m_BackgroundImageFront.sprite) {
			//	m_BackgroundImageBack.sprite = m_BackgroundImageFront.sprite;
			//}
			yield return null;
		}
		SetImageAlpha(m_BackgroundImageFront, 1f);
		m_BackgroundImageBack.sprite = m_BackgroundImageFront.sprite;

		// 다음 교체 대비
		SetImageAlpha(m_BackgroundImageFront, 0f);

		// 현재 키 갱신
		m_CurrentBGKey = nextKey;
		m_BGCrossCo = null;
	}

	private void ResetBackgroundHidden() {
        // 코루틴 중지
        StopBGGroupAnim();
        StopBGCrossAnim();

        // Back/Front 동기화 강제
        NormalizeBGState();

        // 알파 0으로 즉시 세팅 및 스프라이트 제거
        if (m_BackgroundGroup) m_BackgroundGroup.alpha = 0f;
        if (m_BackgroundImageFront) {
            SetImageAlpha(m_BackgroundImageFront, 0f);
            m_BackgroundImageFront.sprite = null;
        }
        if (m_BackgroundImageBack) {
            m_BackgroundImageBack.sprite = null;
        }
        m_CurrentBGKey = "";
    }

	// 배경 관련 유틸
	void SetImageAlpha(Image img, float a) {
		if (!img) { return; }
		var c = img.color;
		c.a = a;
		img.color = c;
	}

	void StopBGGroupAnim() {
		if (m_BGGroupCo != null) {
			StopCoroutine(m_BGGroupCo);
			m_BGGroupCo = null;
		}
	}
	void StopBGCrossAnim() {
		if (m_BGCrossCo != null) {
			StopCoroutine(m_BGCrossCo);
			m_BGCrossCo = null;
		}
	}

	// 강제 정리: 중간에 끊겨도 Back/Front 일치 + Front 투명
	void NormalizeBGState() {
		if (!m_BackgroundImageBack || !m_BackgroundImageFront) { return; }
		if (m_BackgroundImageFront.sprite != null) {
			m_BackgroundImageBack.sprite = m_BackgroundImageFront.sprite;
		}
		SetImageAlpha(m_BackgroundImageFront, 0f);
	}

	public void ClearBackground() => SetBackground(null);

	// Lifecycle

	void Start() {
		EnableInput = false;	
	}

	void LateUpdate() {
		UpdateDialogue();
		audioID = default;
	}

	public override void Show() {
        base.Show();
        ResetBackgroundHidden();
    }

    public override void Hide() {
        ResetBackgroundHidden();
        base.Hide();
    }

    public override void Back() {
        ResetBackgroundHidden();
        base.Back();
    }
}
