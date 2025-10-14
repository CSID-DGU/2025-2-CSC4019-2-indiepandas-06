using UnityEngine;
using System.Text;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif



public static class StringBuilderExtensions {
	static readonly float[] pow = new float[] {
		Mathf.Pow(10f, 0),
		Mathf.Pow(10f, 1),
		Mathf.Pow(10f, 2),
		Mathf.Pow(10f, 3),
		Mathf.Pow(10f, 4),
		Mathf.Pow(10f, 5),
		Mathf.Pow(10f, 6),
	};

	public static StringBuilder Append(this StringBuilder builder, float value, int precision) {
		if (value < 0f) builder.Append('-');
		value = Mathf.Abs(Mathf.Round(value * pow[precision]) / pow[precision]);
		builder.Append((int)value);
		if (0 < precision) builder.Append('.');
		for (int i = 0; i < precision; i++) {
			builder.Append((int)(value * pow[i + 1]) % 10);
		}
		return builder;
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Debug Screen
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("UI Screen/Debug Screen")]
public sealed class DebugScreen : ScreenBase {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(DebugScreen))]
	class DebugScreenEditor : EditorExtensions {
		DebugScreen I => target as DebugScreen;
		public override void OnInspectorGUI() {
			Begin();

			LabelField("Selected", EditorStyles.boldLabel);
			I.DefaultSelected = ObjectField("Default Selected", I.DefaultSelected);
			Space();

			LabelField("Debug", EditorStyles.boldLabel);
			I.DebugText = ObjectField("Debug Text", I.DebugText);
			Space();

			End();
		}
	}
	#endif



	// Fields

	[SerializeField] TextMeshProUGUI m_DebugText;
	StringBuilder m_StringBuilder = new();

	float m_DeltaTime;



	// Properties

	public override bool IsPrimary => false;
	public override bool IsOverlay => true;
	public override BackgroundMode BackgroundMode => BackgroundMode.Preserve;



	TextMeshProUGUI DebugText {
		get => m_DebugText;
		set => m_DebugText = value;
	}
	StringBuilder StringBuilder {
		get => m_StringBuilder;
		set => m_StringBuilder = value;
	}

	float DeltaTime {
		get => m_DeltaTime;
		set => m_DeltaTime = value;
	}



	// Methods

	void LateUpdate() {
		StringBuilder.Append("FPS: ");
		DeltaTime += (Time.unscaledDeltaTime - DeltaTime) * 0.01f;
		StringBuilder.Append(Mathf.RoundToInt(1f / DeltaTime)).AppendLine();
		StringBuilder.AppendLine();

		StringBuilder.Append("XY: ");
		var position = CameraManager.Position;
		StringBuilder.Append(position.x, 2).Append(" / ");
		StringBuilder.Append(position.y, 2).AppendLine();
		StringBuilder.Append("Yaw: ");
		StringBuilder.Append("Time of Day: ");
		float timeOfDay = EnvironmentManager.TimeOfDay;
		StringBuilder.Append(timeOfDay, 2).AppendLine();
		StringBuilder.AppendLine();

		DebugText.SetText(StringBuilder);
		StringBuilder.Clear();
	}
}
