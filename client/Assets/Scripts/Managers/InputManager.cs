using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif



// Input Actions

public enum ActionMap : byte {
	Player,
	UI,
}

public enum KeyAction : byte {
	Move,
	Jump,
	Interact,
	Menu,

	Point,
	Click,
	MiddleClick,
	RightClick,
	Navigate,
	ScrollWheel,
	Submit,
	Cancel,
	TrackedDevicePosition,
	TrackedDeviceOrientation,
	QuickSlot,
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Input Manager
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("Manager/Input Manager")]
[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoSingleton<InputManager> {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(InputManager))]
	class InputManagerEditor : EditorExtensions {
		InputManager I => target as InputManager;
		public override void OnInspectorGUI() {
			Begin();

			LabelField("Player Input", EditorStyles.boldLabel);
			ObjectField("Player Input", PlayerInput);
			InputActionAsset = ObjectField("Input Action Asset", InputActionAsset);
			if (InputActionAsset == null) {
				var message = string.Empty;
				message += $"Input Action Asset is missing.\n";
				message += $"Please assign a Input Action Asset to here.";
				HelpBox(message, MessageType.Info);
			}
			Space();

			LabelField("Web Cam", EditorStyles.boldLabel);
			CaptureWebCam = Toggle("Capture Web Cam", CaptureWebCam);
			if (CaptureWebCam) RawImage = ObjectField("Web Cam Image", RawImage);
			Space();

			End();
		}
	}
	#endif



	// Constants

	const float WebCamUpdateInterval = 10f;



	// Fields

	PlayerInput m_PlayerInput;

	bool m_IsPointerMode;
	uint m_KeyNext;
	uint m_KeyPrev;
	Vector2 m_MoveDirection;
	Vector2 m_PointPosition;
	Vector2 m_ScrollWheel;
	Vector2 m_Navigate;
	float m_QuickSlot;

	[SerializeField] bool m_CaptureWebCam = true;
	WebCamTexture m_WebCamTexture;
	Texture2D m_CachedWebCamTexture;
	[SerializeField] RawImage m_RawImage;



	// Properties

	static PlayerInput PlayerInput => !Instance.m_PlayerInput ?
		Instance.m_PlayerInput = Instance.GetOwnComponent<PlayerInput>() :
		Instance.m_PlayerInput;

	static InputActionAsset InputActionAsset {
		get => PlayerInput.actions;
		set => PlayerInput.actions = value;
	}



	public static bool IsPointerMode {
		get         => Instance.m_IsPointerMode;
		private set => Instance.m_IsPointerMode = value;
	}

	public static uint KeyNext {
		get         => Instance.m_KeyNext;
		private set => Instance.m_KeyNext = value;
	}
	public static uint KeyPrev {
		get         => Instance.m_KeyPrev;
		private set => Instance.m_KeyPrev = value;
	}
	public static Vector2 MoveDirection {
		get         => Instance.m_MoveDirection;
		private set => Instance.m_MoveDirection = value;
	}
	public static Vector2 PointPosition {
		get         => Instance.m_PointPosition;
		private set => Instance.m_PointPosition = value;
	}
	public static Vector2 ScrollWheel {
		get         => Instance.m_ScrollWheel;
		private set => Instance.m_ScrollWheel = value;
	}
	public static Vector2 Navigate {
		get         => Instance.m_Navigate;
		private set => Instance.m_Navigate = value;
	}
	public static float QuickSlot {
		get         => Instance.m_QuickSlot;
		private set => Instance.m_QuickSlot = value;
	}


	public static bool CaptureWebCam {
		get => Instance.m_CaptureWebCam;
		set => Instance.m_CaptureWebCam = value;
	}
	static WebCamTexture WebCamTexture {
		get => Instance.m_WebCamTexture;
		set => Instance.m_WebCamTexture = value;
	}
	public static Texture2D CachedWebCamTexture {
		get => Instance.m_CachedWebCamTexture;
		set => Instance.m_CachedWebCamTexture = value;
	}
	static RawImage RawImage {
		get => Instance.m_RawImage;
		set => Instance.m_RawImage = value;
	}



	// Key State Methods

	static void RegisterActionMap() {
		if (InputActionAsset == null) return;
		foreach (var inputActionMap in InputActionAsset.actionMaps) {
			
			inputActionMap.Enable();

			if (!Enum.TryParse(inputActionMap.name, out ActionMap actionMap)) continue;
			foreach (var inputAction in inputActionMap.actions) {
				if (!Enum.TryParse(inputAction.name, out KeyAction keyAction)) continue;

				int index = (int)keyAction;
				inputAction.started += action => KeyNext |= 1u << index;
				inputAction.performed += keyAction switch {
					KeyAction.Move        => action => MoveDirection = action.ReadValue<Vector2>(),
					KeyAction.Point       => action => PointPosition = action.ReadValue<Vector2>(),
					KeyAction.ScrollWheel => action => ScrollWheel   = action.ReadValue<Vector2>(),
					KeyAction.Navigate    => action => Navigate      = action.ReadValue<Vector2>(),
					_ => action => _ = action.action.IsPressed() switch {
						true  => KeyNext |=  (1u << index),
						false => KeyNext &= ~(1u << index),
					},
				};
				inputAction.canceled += keyAction switch {
					KeyAction.Move        => action => MoveDirection = Vector2.zero,
					KeyAction.Point       => action => PointPosition = Vector2.zero,
					KeyAction.ScrollWheel => action => ScrollWheel   = Vector2.zero,
					KeyAction.Navigate    => action => Navigate      = Vector2.zero,
					_ => action => KeyNext &= ~(1u << index),
				};
			}
		}
		InputSystem.onBeforeUpdate += () => KeyPrev = KeyNext;
		InputSystem.onActionChange += (obj, change) => {
			if (change != InputActionChange.ActionPerformed) return;
			var inputAction = obj as InputAction;
			if (inputAction?.activeControl == null) return;
			var device = inputAction.activeControl.device;
			IsPointerMode = device is Pointer;
		};
	}

	public static void SwitchActionMap(ActionMap actionMap) {
		if (InputActionAsset == null) return;
		PlayerInput.currentActionMap = InputActionAsset.FindActionMap(actionMap.ToString());
		MoveDirection = PointPosition = ScrollWheel = Navigate = default;
		KeyNext = KeyPrev = default;
		MoveDirection = PointPosition = ScrollWheel = Navigate = default;
	}

	static bool GetKeyNext(KeyAction key) => (KeyNext & (1u << (int)key)) != 0u;
	static bool GetKeyPrev(KeyAction key) => (KeyPrev & (1u << (int)key)) != 0u;

	public static bool GetKey(KeyAction key) => GetKeyNext(key);
	public static bool GetKeyDown(KeyAction key) => GetKeyNext(key) && !GetKeyPrev(key);
	public static bool GetKeyUp(KeyAction key) => !GetKeyNext(key) && GetKeyPrev(key);



	// Lifecycle

	void Start() {
		RegisterActionMap();
	}

	/*void Update() {
		if (!CaptureWebCam) return;
		float time = Time.realtimeSinceStartup;
		if ((time + Time.unscaledDeltaTime) % WebCamUpdateInterval < time % WebCamUpdateInterval) {
			if (!WebCamTexture && 0 < WebCamTexture.devices.Length) {
				WebCamTexture = new WebCamTexture(WebCamTexture.devices[0].name);
				WebCamTexture.Play();
			}
			if (WebCamTexture) {
				CachedWebCamTexture ??= new Texture2D(WebCamTexture.width, WebCamTexture.height);
				CachedWebCamTexture.SetPixels(WebCamTexture.GetPixels());
				CachedWebCamTexture.Apply();
				if (RawImage) RawImage.material.mainTexture = CachedWebCamTexture;
				
				ServerRequestManager.Instance.RequestImageAnalysis(CachedWebCamTexture, (result) => {
					if (result != null) {
						Debug.Log("분석 감정: " + result.emotion_result.emotion);
						if (result.emotion_result.emotion == "sadness" || result.emotion_result.emotion == "anger") {
							GameManager.Instance.m_Negative = true;
						} else {
							GameManager.Instance.m_Negative = false;
						}
					} else {
						Debug.LogWarning("이미지 분석 실패!");
					}
            	});
			}
		}
	}*/
}
