using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Screen
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("UI/Game Screen")]
public class GameScreen : ScreenBase {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(GameScreen))]
	class GameScreenEditor : EditorExtensions {
		GameScreen I => target as GameScreen;
		static bool foldout = false;
		public override void OnInspectorGUI() {
			Begin();

			LabelField("Selected", EditorStyles.boldLabel);
			I.DefaultSelected = ObjectField("Default Selected", I.DefaultSelected);
			Space();

			if (foldout = Foldout("Sprite", foldout)) {
				PropertyField("NumPSprite");
				PropertyField("NumMSprite");
				PropertyField("Num0Sprite");
				PropertyField("Num1Sprite");
				PropertyField("Num2Sprite");
				PropertyField("Num3Sprite");
				PropertyField("Num4Sprite");
				PropertyField("Num5Sprite");
				PropertyField("Num6Sprite");
				PropertyField("Num7Sprite");
				PropertyField("Num8Sprite");
				PropertyField("Num9Sprite");
			}
			Space();
			LabelField("Interactable Text", EditorStyles.boldLabel);
			I.InteractableTransform = ObjectField("Interactable Transform", I.InteractableTransform);
			I.InteractableNameUGUI  = ObjectField("Interactable Name UGUI", I.InteractableNameUGUI);
			I.InteractableTypeUGUI  = ObjectField("Interactable Type UGUI", I.InteractableTypeUGUI);
			Space();
			LabelField("Gem Collect Message", EditorStyles.boldLabel);
			I.MessageTransform      = ObjectField("Message Transform",       I.MessageTransform);
			I.MessageIconImage      = ObjectField("Message Icon Image",      I.MessageIconImage);
			I.MessageValueTransform = ObjectField("Message Value Transform", I.MessageValueTransform);
			I.MessageTextUGUI       = ObjectField("Message Text UGUI",       I.MessageTextUGUI);
			if (I.MessageTextUGUI) {
				I.MessageText = TextField("Message Text", I.MessageText);
				LabelField(" ", "{N} = Gem Amount");
			}
			I.MessageLifetime = FloatField("Message Lifetime", I.MessageLifetime);
			Space();

			End();
		}
	}
	#endif



	// Constants

	[SerializeField] Sprite NumPSprite;
	[SerializeField] Sprite NumMSprite;
	[SerializeField] Sprite Num0Sprite;
	[SerializeField] Sprite Num1Sprite;
	[SerializeField] Sprite Num2Sprite;
	[SerializeField] Sprite Num3Sprite;
	[SerializeField] Sprite Num4Sprite;
	[SerializeField] Sprite Num5Sprite;
	[SerializeField] Sprite Num6Sprite;
	[SerializeField] Sprite Num7Sprite;
	[SerializeField] Sprite Num8Sprite;
	[SerializeField] Sprite Num9Sprite;



	// Fields

	[SerializeField] RectTransform m_InteractableTransform;
	[SerializeField] TextMeshProUGUI m_InteractableNameUGUI;
	[SerializeField] TextMeshProUGUI m_InteractableTypeUGUI;

	[SerializeField] RectTransform m_MessageTransform;
	[SerializeField] Image m_MessageIconImage;
	[SerializeField] RectTransform m_MessageValueTransform;
	[SerializeField] TextMeshProUGUI m_MessageTextUGUI;
	[SerializeField] string m_MessageText;
	[SerializeField] float m_MessageLifetime = 5f;
	float m_MessageTimer;



	// Properties

	public override bool IsPrimary => true;
	public override bool IsOverlay => false;
	public override BackgroundMode BackgroundMode => BackgroundMode.Scene;



	RectTransform InteractableTransform {
		get => m_InteractableTransform;
		set => m_InteractableTransform = value;
	}
	TextMeshProUGUI InteractableNameUGUI {
		get => m_InteractableNameUGUI;
		set => m_InteractableNameUGUI = value;
	}
	TextMeshProUGUI InteractableTypeUGUI {
		get => m_InteractableTypeUGUI;
		set => m_InteractableTypeUGUI = value;
	}



	RectTransform MessageTransform {
		get => m_MessageTransform;
		set => m_MessageTransform = value;
	}
	Image MessageIconImage {
		get => m_MessageIconImage;
		set => m_MessageIconImage = value;
	}
	RectTransform MessageValueTransform {
		get => m_MessageValueTransform;
		set => m_MessageValueTransform = value;
	}
	TextMeshProUGUI MessageTextUGUI {
		get => m_MessageTextUGUI;
		set => m_MessageTextUGUI = value;
	}
	string MessageText {
		get => m_MessageText;
		set {
			if (m_MessageText != value) {
				m_MessageText = value;
				int gemGained = 0;
				var match = Regex.Match(value, @"\{(-?\d+)\}");
				if (match.Success) {
					if (!int.TryParse(match.Groups[1].Value, out gemGained)) gemGained = 0;
					value = value.Replace(match.Value, match.Groups[1].Value);
				}
				MessageTextUGUI.text = value;
				UpdateMessage(gemGained);
			}
		}
	}
	float MessageLifetime {
		get => m_MessageLifetime;
		set => m_MessageLifetime = value;
	}
	float MessageTimer {
		get => m_MessageTimer;
		set => m_MessageTimer = value;
	}



	// Methods

	public void ShowGemCollectMessage(string text) {
		MessageTransform.gameObject.SetActive(true);
		MessageText = text;
		MessageTimer = MessageLifetime;
	}

	void UpdateMessage(int amount = 0) {
		if (MessageIconImage) {
			MessageIconImage.gameObject.SetActive(amount != 0);
		}
		float valueWidth = 0f;
		if (MessageValueTransform) {
			if (amount == 0) for (int i = 0; i < MessageValueTransform.childCount; i++) {
				MessageValueTransform.GetChild(i).gameObject.SetActive(false);
			} else {
				MessageValueTransform.GetChild(0).gameObject.SetActive(true);
				valueWidth += 28f;
				int numCount = 1 + (int)Mathf.Log10(Mathf.Abs(amount)) + 1;
				for (int i = 0; i < numCount; i++) {
					int num = Mathf.Abs(amount) / (int)Mathf.Pow(10, numCount - i - 1) % 10;
					var sprite = (i == 0) ? (amount < 0 ? NumMSprite : NumPSprite) : (num switch {
						0 => Num0Sprite,
						1 => Num1Sprite,
						2 => Num2Sprite,
						3 => Num3Sprite,
						4 => Num4Sprite,
						5 => Num5Sprite,
						6 => Num6Sprite,
						7 => Num7Sprite,
						8 => Num8Sprite,
						9 => Num9Sprite,
						_ => null,
					});
					if (1 + i < MessageValueTransform.childCount) {
						var item = MessageValueTransform.GetChild(1 + i);
						if (item.TryGetComponent(out Image image)) image.sprite = sprite;
						item.gameObject.SetActive(true);
						valueWidth += 7f;
					}
				}
				if (0 < numCount) valueWidth += 1f;
				for (int i = numCount + 1; i < MessageValueTransform.childCount; i++) {
					MessageValueTransform.GetChild(i).gameObject.SetActive(false);
				}
			}
		}
		float textWidth = 0f;
		if (MessageTextUGUI) {
			var messageTextTransform = MessageTextUGUI.rectTransform;
			textWidth = MessageTextUGUI.GetPreferredValues().x;
			messageTextTransform.sizeDelta = new(textWidth, messageTextTransform.sizeDelta.y);
		}
		if (MessageTransform) {
			MessageTransform.sizeDelta = new(valueWidth + textWidth, MessageTransform.sizeDelta.y);
		}
	}



	public override void Show() {
		base.Show();
		if (GameManager.GameState == GameState.Paused) {
			GameManager.GameState = GameState.Gameplay;
		}
	}

	public override void Hide() {
		base.Hide();
		if (GameManager.GameState == GameState.Gameplay) {
			GameManager.GameState = GameState.Paused;
		}
	}

	public override void Back() {
		UIManager.OpenScreen(Screen.Menu);
	}



	// Lifecycle

	void Start() {
		InteractableTransform.gameObject.SetActive(false);
		MessageTransform.gameObject.SetActive(false);
	}

	protected override void Update() {
		switch (UIManager.CurrentScreen) {
			case Screen.Debug:
			case Screen.Game: {
				if (InputManager.GetKeyUp(KeyAction.Menu)) Back();
				if (GameManager.GameState == GameState.Paused) {
					GameManager.GameState = GameState.Gameplay;
				}
			} break;
			default: {
				if (GameManager.GameState == GameState.Gameplay) {
					GameManager.GameState = GameState.Paused;
				}
			} break;
		}
	}

	void LateUpdate() {
		var (gameObject, interactable) = GameManager.Player.GetNearestInteractable();
		bool match = true;
		match &= interactable != null && interactable.IsInteractable;
		match &= GameManager.GameState == GameState.Gameplay;
		if (match) {
			InteractableTransform.gameObject.SetActive(true);
			var aWorldPos = GameManager.Player.transform.position;
			var bWorldPos = gameObject.transform.position;
			var aScreenPos = CameraManager.WorldToScreenPoint(aWorldPos);
			var bScreenPos = CameraManager.WorldToScreenPoint(bWorldPos);
			var namePos = bScreenPos + new Vector3(0f, 150f, 0f);
			InteractableNameUGUI.rectTransform.position = namePos;
			InteractableNameUGUI.text = gameObject.name switch {
				"Yejin"  => "예진",
				"Minsu"  => "민수",
				"People" => "사람들",
				_ => gameObject.name,
			};
			var typePos = Vector3.Lerp(aScreenPos, bScreenPos, 0.5f) + new Vector3(0f, -50f, 0f);
			InteractableTypeUGUI.rectTransform.position = typePos;
			InteractableTypeUGUI.text = interactable.InteractionType switch {
				InteractionType.Talk  => "말하기",
				InteractionType.Trade => "거래하기",
				_ => "상호작용",
			};
		} else {
			InteractableTransform.gameObject.SetActive(false);
		}

		if (0f < MessageTimer) {
			MessageTimer = Mathf.Max(0f, MessageTimer - Time.deltaTime);
			if (MessageTimer == 0f) MessageTransform.gameObject.SetActive(false);
		}
	}
}
