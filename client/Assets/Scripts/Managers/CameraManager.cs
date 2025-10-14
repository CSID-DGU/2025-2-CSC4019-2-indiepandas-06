using UnityEngine;

using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Camera Manager
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("Manager/Camera Manager")]
public class CameraManager : MonoSingleton<CameraManager> {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(CameraManager))]
	class CameraManagerEditor : EditorExtensions {
		CameraManager I => target as CameraManager;
		public override void OnInspectorGUI() {
			Begin();

			LabelField("Camera", EditorStyles.boldLabel);
			Camera = ObjectField("Camera", Camera);
			Space();

			if (Camera == null) {
				var message = string.Empty;
				message += $"Camera is missing.\n";
				message += $"Please add Camera to child of this object ";
				message += $"and assign here.";
				HelpBox(message, MessageType.Error);
				Space();
			} else {
				LabelField("Camera Properties", EditorStyles.boldLabel);
				OrthographicSize = FloatField("Orthographic Size", OrthographicSize);
				BeginDisabledGroup(true);
				TextField("Reference Size", $"{ReferenceResolution.y / 16f * 0.5f}");
				EndDisabledGroup();
				Space();
			}

			End();
		}
	}
	#endif



	// Constants

	static readonly Vector2Int ReferenceResolution = new(480, 270);



	// Fields

	[SerializeField] Camera m_Camera;

	[SerializeField] float m_OrthoMultiplier = 1f;
	Vector2Int m_ScreenResolution = default;

	float m_ShakeStrength;
	float m_ShakeDuration;
	bool2 m_ShakeDirection;



	// Properties

	static Camera Camera {
		get => Instance.m_Camera;
		set => Instance.m_Camera = value;
	}
	static RenderTexture RenderTexture {
		get => Camera.targetTexture;
		set => Camera.targetTexture = value;
	}



	public static Vector3 Position {
		get => Instance.transform.position;
		set => Instance.transform.position = value;
	}

	public static float OrthographicSize {
		get => Camera.orthographicSize / OrthoMultiplier;
		set {
			value = Mathf.Max(0.01f, value);
			Camera.orthographicSize = value * OrthoMultiplier;
		}
	}
	static float OrthoMultiplier {
		get => Instance.m_OrthoMultiplier;
		set {
			float orthographicSize = OrthographicSize;
			Instance.m_OrthoMultiplier = value = Mathf.Max(0.01f, value);
			Camera.orthographicSize = orthographicSize * value;
		}
	}
	static Vector2Int ScreenResolution {
		get => Instance.m_ScreenResolution;
		set => Instance.m_ScreenResolution = value;
	}



	static float ShakeStrength {
		get => Instance.m_ShakeStrength;
		set => Instance.m_ShakeStrength = value;
	}
	static float ShakeDuration {
		get => Instance.m_ShakeDuration;
		set => Instance.m_ShakeDuration = value;
	}
	static bool2 ShakeDirection {
		get => Instance.m_ShakeDirection;
		set => Instance.m_ShakeDirection = value;
	}



	// Methods

	public static Vector3 WorldToScreenPoint(Vector3 position) {
		return Camera.WorldToScreenPoint(position);
	}

	public static Vector3 WorldToViewportPoint(Vector3 position) {
		return Camera.WorldToViewportPoint(position);
	}

	public static Vector3 ScreenToWorldPoint(Vector3 position) {
		return Camera.ScreenToWorldPoint(position);
	}

	public static Vector3 ScreenToViewportPoint(Vector3 position) {
		return Camera.ScreenToViewportPoint(position);
	}

	public static Vector3 ViewportToWorldPoint(Vector3 position) {
		return Camera.ViewportToWorldPoint(position);
	}

	public static Vector3 ViewportToScreenPoint(Vector3 position) {
		return Camera.ViewportToScreenPoint(position);
	}



	static void UpdateScreenResolution() {
		bool match = false;
		match = match || ScreenResolution.x != UnityEngine.Screen.width;
		match = match || ScreenResolution.y != UnityEngine.Screen.height;
		if (match) {
			ScreenResolution = new(UnityEngine.Screen.width, UnityEngine.Screen.height);
			float aspect = (float)UnityEngine.Screen.width / UnityEngine.Screen.height;
			Camera.aspect = aspect;
			float xRatio = (float)UnityEngine.Screen.width / ReferenceResolution.x;
			float yRatio = (float)UnityEngine.Screen.height / ReferenceResolution.y;
			float multiplier = Mathf.Max(1, (int)Mathf.Min(xRatio, yRatio));
			OrthoMultiplier = yRatio / multiplier;

			RenderTexture.Release();
			RenderTexture.width  = UnityEngine.Screen.width;
			RenderTexture.height = UnityEngine.Screen.height;
			RenderTexture.Create();
		}
	}



	public static void ShakeCamera(float strength, float duration, bool2 direction = default) {
		ShakeStrength = Mathf.Max(0f, strength);
		ShakeDuration = Mathf.Max(0f, duration);
		ShakeDirection = direction.Equals(default) ? new bool2(true, true) : direction;
	}

	public static void StopShaking() {
		ShakeDuration = 0f;
	}

	static void UpdateCameraShake() {

	}



	// Lifecycle

	void LateUpdate() {
		UpdateScreenResolution();
		UpdateCameraShake();
	}
}
