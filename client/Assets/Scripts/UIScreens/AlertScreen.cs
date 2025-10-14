using UnityEngine;
using System;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Alert Screen
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("UI Screen/Alert Screen")]
public sealed class AlertScreen : ScreenBase {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(AlertScreen))]
	class AlertScreenEditor : EditorExtensions {
		AlertScreen I => target as AlertScreen;
		public override void OnInspectorGUI() {
			Begin();

			LabelField("Selected", EditorStyles.boldLabel);
			I.DefaultSelected = ObjectField("Default Selected", I.DefaultSelected);
			Space();

			LabelField("Alert", EditorStyles.boldLabel);
			I.ContentText = ObjectField("Content Text", I.ContentText);
			I.CloseText   = ObjectField("Close Text",   I.CloseText);
			Space();

			End();
		}
	}
	#endif



	// Fields

	[SerializeField] TextMeshProUGUI m_ContentText;
	[SerializeField] TextMeshProUGUI m_CloseText;

	Action m_OnClosed;



	// Properties

	public override bool IsPrimary => false;
	public override bool IsOverlay => true;
	public override BackgroundMode BackgroundMode => BackgroundMode.PreserveWithBlur;



	TextMeshProUGUI ContentText {
		get => m_ContentText;
		set => m_ContentText = value;
	}
	TextMeshProUGUI CloseText {
		get => m_CloseText;
		set => m_CloseText = value;
	}

	public string ContentTextValue {
		get => ContentText.text;
		set => ContentText.text = value;
	}
	public string CloseTextValue {
		get => CloseText.text;
		set => CloseText.text = value;
	}



	public Action OnClosed {
		get => m_OnClosed;
		set => m_OnClosed = value;
	}



	// Methods

	public void Close() {
		UIManager.CloseScreen(this);
		OnClosed?.Invoke();
		OnClosed = null;
	}

	public override void Back() {
		Close();
	}
}
