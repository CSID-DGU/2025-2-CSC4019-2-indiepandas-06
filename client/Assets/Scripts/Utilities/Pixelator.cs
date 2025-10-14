using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Pixelator
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("Utility/Pixelator")]
[ExecuteInEditMode, RequireComponent(typeof(Camera))]
public class Pixelator : MonoBehaviour {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(Pixelator))]
	public class PixelatorEditor : EditorExtensions {
		Pixelator I => target as Pixelator;
		public override void OnInspectorGUI() {
			Begin();

			LabelField("Camera", EditorStyles.boldLabel);
			I.DollyDistance = Slider("Dolly Distance", I.DollyDistance, 0f, 64f);
			I.FieldOfView = FloatField("Field Of View", I.FieldOfView);
			I.OrthographicSize = FloatField("Orthographic Size", I.OrthographicSize);
			I.Projection = Slider("Projection", I.Projection, 0f, 1f);
			BeginHorizontal();
			PrefixLabel(" ");
			var l = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft };
			var r = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight };
			var s = new GUIStyle(GUI.skin.label) { fixedWidth = 50 };
			GUILayout.Label("< Perspective ", l);
			GUILayout.Label("Orthographic >", r);
			GUILayout.Label(" ", s);
			EndHorizontal();
			Space();

			BeginDisabledGroup(I.SourceObject);
			I.CullingMask = LayerField("Culling Mask", I.CullingMask);
			EndDisabledGroup();
			Space();

			LabelField("Pixelator", EditorStyles.boldLabel);
			I.SourceObject = ObjectField("Source Object", I.SourceObject);
			if (I.SourceObject) {
				var text = string.Empty;
				var prev = LayerMask.LayerToName(I.SourceObjectLayer);
				var next = LayerMask.LayerToName(I.SourceObject.layer);
				if (string.IsNullOrEmpty(prev)) prev = "Unlayered";
				if (string.IsNullOrEmpty(next)) next = "Unlayered";
				text += $"{I.SourceObject.name} layer changed ";
				text += $"{prev}({I.SourceObjectLayer}) to {next}({I.SourceObject.layer}).";
				HelpBox(text);
				BeginVertical(EditorStyles.helpBox);
				if (true) {
					LabelField("Transform", EditorStyles.boldLabel);
					var position = I.SourceObject.transform.localPosition;
					var rotation = I.SourceObject.transform.localRotation;
					I.SourceObject.transform.localPosition = Vector3Field("Position", position);
					I.SourceObject.transform.localRotation = Vector3Field("Rotation", rotation);
					Space();
				}
				if (I.SourceObject.TryGetComponent(out Animator animator)) {
					LabelField("Animator", EditorStyles.boldLabel);
					var controller = animator.runtimeAnimatorController;
					animator.runtimeAnimatorController = ObjectField("Controller", controller);
					if (controller) {
						var clip = I.SourceObjectAnimationClip;
						var time = I.SourceObjectAnimationTime;
						var clips = controller.animationClips;
						var options = new string[clips.Length];
						for (int i = 0; i < options.Length; i++) options[i] = clips[i].name;
						clip = EditorGUILayout.Popup("Animation Clip", clip, options);
						time = Slider("Animation Time", time, 0f, clips[clip].length);
						if (I.SourceObjectAnimationClip != clip) time = 0f;
						bool match = false;
						match = match || I.SourceObjectAnimationClip != clip;
						match = match || I.SourceObjectAnimationTime != time;
						if (match) {
							var position = I.SourceObject.transform.localPosition;
							var rotation = I.SourceObject.transform.localRotation;
							clips[clip].SampleAnimation(I.SourceObject, time);
							I.SourceObject.transform.localPosition = position;
							I.SourceObject.transform.localRotation = rotation;
						}
						I.SourceObjectAnimationClip = clip;
						I.SourceObjectAnimationTime = time;
					}
					Space();
				}
				EndVertical();
			}
			I.TargetPath = TextField("Target Path", I.TargetPath);
			I.TextureSize = IntSlider("Texture Size", I.TextureSize, 16, 2048);
			BeginHorizontal();
			PrefixLabel("Save Pixelated");
			if (Button("Save")) I.SavePixelated(I.TargetPath);
			if (Button("Save As")) {
				var name = "Save Pixelated";
				var path = EditorUtility.SaveFilePanel(name, "Assets", "Pixelated.png", "png");
				if (!string.IsNullOrEmpty(path)) I.SavePixelated(path);
			}
			EndHorizontal();
			Space();

			LabelField("Preview", EditorStyles.boldLabel);
			var rect = GUILayoutUtility.GetAspectRect(1.0f);
			EditorGUI.DrawPreviewTexture(rect, I.RenderTexture);
			Space();

			End();
		}

		void OnSceneGUI() {
			if (I.SourceObject) {
				EditorGUI.BeginChangeCheck();
				var position = I.SourceObject.transform.localPosition;
				var rotation = I.SourceObject.transform.localRotation;
				var positionHandle = Handles.PositionHandle(position, rotation);
				var rotationHandle = Handles.RotationHandle(rotation, position);
				I.SourceObject.transform.localPosition = positionHandle;
				I.SourceObject.transform.localRotation = rotationHandle;
				if (EditorGUI.EndChangeCheck()) Repaint();
			}
		}
	}
	#endif



	// Constants

	const int CullingLayer = 31;
	const TextureFormat Format = TextureFormat.ARGB32;
	const RenderTextureFormat RenderFormat = RenderTextureFormat.ARGB32;



	// Fields

	Camera m_Camera;
	[SerializeField] float m_DollyDistance = 0f;
	[SerializeField] float m_Projection = 1f;

	RenderTexture m_RenderTexture;
	[SerializeField] GameObject m_SourceObject;
	[SerializeField] int m_SourceObjectLayer;
	[SerializeField] int m_SourceObjectAnimationClip;
	[SerializeField] float m_SourceObjectAnimationTime;
	[SerializeField] string m_TargetPath = "Assets/Pixelated.png";
	[SerializeField] int m_TextureSize = 64;



	// Properties

	Camera Camera => !m_Camera ?
		m_Camera = TryGetComponent(out Camera camera) ? camera : null :
		m_Camera;

	public float DollyDistance {
		get => m_DollyDistance;
		set {
			float delta = value - m_DollyDistance;
			if (m_DollyDistance != value) {
				m_DollyDistance = value;
				transform.localPosition -= delta * transform.forward;
			}
		}
	}

	public float FieldOfView {
		get => Camera.fieldOfView;
		set => Camera.fieldOfView = value;
	}
	public float OrthographicSize {
		get => Camera.orthographicSize;
		set => Camera.orthographicSize = value;
	}
	public float Projection {
		get => m_Projection;
		set {
			value = Mathf.Clamp(value, 0f, 1f);
			if (m_Projection != value) {
				float fov    = Camera.fieldOfView;
				float aspect = Camera.aspect;
				float zNear  = Camera.nearClipPlane;
				float zFar   = Camera.farClipPlane;
				float wHalf  = Camera.orthographicSize * aspect;
				float hHalf  = Camera.orthographicSize;

				m_Projection = value;
				var matrix = Camera.projectionMatrix;
				var a = Matrix4x4.Perspective(fov, aspect, zNear, zFar);
				var b = Matrix4x4.Ortho(-wHalf, wHalf, -hHalf, hHalf, zNear, zFar);
				float t = Mathf.Pow(Mathf.Max(0.01f, value), 0.03f);
				for (int i = 0; i < 16; i++) matrix[i] = Mathf.Lerp(a[i], b[i], t);
				Camera.projectionMatrix = matrix;
			}
		}
	}

	public int CullingMask {
		get => Camera.cullingMask;
		set => Camera.cullingMask = value;
	}



	RenderTexture RenderTexture {
		get => m_RenderTexture;
		set => m_RenderTexture = value;
	}

	public GameObject SourceObject {
		get => m_SourceObject;
		set {
			if (m_SourceObject != value) {
				if (m_SourceObject != null) {
					SwitchLayerRecursive(m_SourceObject, SourceObjectLayer);
				}
				if (value != null) {
					SourceObjectLayer = value.layer;
					SwitchLayerRecursive(value, CullingLayer);
					CullingMask = 1 << CullingLayer;
				} else {
					CullingMask = -1;
				}
				m_SourceObject = value;
			}
		}
	}
	int SourceObjectLayer {
		get => m_SourceObjectLayer;
		set => m_SourceObjectLayer = value;
	}
	public int SourceObjectAnimationClip {
		get => m_SourceObjectAnimationClip;
		set => m_SourceObjectAnimationClip = value;
	}
	public float SourceObjectAnimationTime {
		get => m_SourceObjectAnimationTime;
		set => m_SourceObjectAnimationTime = value;
	}
	public string TargetPath {
		get => m_TargetPath;
		set => m_TargetPath = value;
	}

	public int TextureSize {
		get => m_TextureSize;
		set {
			value = value / 2 * 2;
			if (m_TextureSize != value) {
				m_TextureSize = value;
				RenderTexture.Release();
				RenderTexture.width = value;
				RenderTexture.height = value;
				RenderTexture.Create();
			}
		}
	}



	// Methods

	void SwitchLayerRecursive(GameObject gameObject, int layer) {
		gameObject.layer = layer;
		foreach (Transform child in gameObject.transform) {
			SwitchLayerRecursive(child.gameObject, layer);
		}
	}

	#if UNITY_EDITOR
	public void SavePixelated(string path) {
		var color = Camera.backgroundColor;
		Camera.backgroundColor = Color.clear;
		Camera.Render();
		Camera.backgroundColor = color;

		var active = RenderTexture.active;
		RenderTexture.active = RenderTexture;
		var texture = new Texture2D(TextureSize, TextureSize, Format, false);
		texture.ReadPixels(new Rect(0, 0, TextureSize, TextureSize), 0, 0);
		texture.Apply();
		RenderTexture.active = active;
		try {
			var bytes = texture.EncodeToPNG();
			File.WriteAllBytes(path, bytes);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		} finally {
			DestroyImmediate(texture);
		}
	}
	#endif



	// Lifecycle

	void OnEnable() {
		Camera.orthographic = true;
		Camera.clearFlags = CameraClearFlags.SolidColor;
		Camera.backgroundColor = new(0.16f, 0.16f, 0.16f, 1f);
		Camera.targetTexture = new(TextureSize, TextureSize, 16, RenderFormat) {
			name = "Render Texture",
			antiAliasing = 1,
			useMipMap = false,
			wrapMode = TextureWrapMode.Clamp,
			filterMode = FilterMode.Point,
		};
		RenderTexture = Camera.targetTexture;
		RenderTexture.Create();
	}

	void OnDisable() {
		if (RenderTexture != null) {
			Camera.targetTexture = null;
			if (RenderTexture.IsCreated()) RenderTexture.Release();
			DestroyImmediate(RenderTexture);
			RenderTexture = null;
		}
		if (SourceObject != null) {
			SourceObject.layer = SourceObjectLayer;
		}
	}
}
