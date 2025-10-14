using UnityEngine;
using System;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
//  Confirmation Screen
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("UI Screen/Confirmation Screen")]
public sealed class ConfirmationScreen : ScreenBase {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(ConfirmationScreen))]
	class ConfirmationScreenEditor : EditorExtensions {
		ConfirmationScreen I => target as ConfirmationScreen;
		public override void OnInspectorGUI() {
			Begin();

			LabelField("Selected", EditorStyles.boldLabel);
			I.DefaultSelected = ObjectField("Default Selected", I.DefaultSelected);
			Space();

			LabelField("Confirmation", EditorStyles.boldLabel);
			I.HeaderText  = ObjectField("Header Text",  I.HeaderText);
			I.ContentText = ObjectField("Content Text", I.ContentText);
			I.ConfirmText = ObjectField("Confirm Text", I.ConfirmText);
			I.CancelText  = ObjectField("Cancel Text",  I.CancelText);
			Space();

			End();
		}
	}
	#endif



	// Fields

	[SerializeField] TextMeshProUGUI m_HeaderText;
	[SerializeField] TextMeshProUGUI m_ContentText;
	[SerializeField] TextMeshProUGUI m_ConfirmText;
	[SerializeField] TextMeshProUGUI m_CancelText;

	Action m_OnConfirmed;
	Action m_OnCancelled;



	// Properties

	public override bool IsPrimary => false;
	public override bool IsOverlay => true;
	public override BackgroundMode BackgroundMode => BackgroundMode.PreserveWithBlur;



	TextMeshProUGUI HeaderText {
		get => m_HeaderText;
		set => m_HeaderText = value;
	}
	TextMeshProUGUI ContentText {
		get => m_ContentText;
		set => m_ContentText = value;
	}
	TextMeshProUGUI ConfirmText {
		get => m_ConfirmText;
		set => m_ConfirmText = value;
	}
	TextMeshProUGUI CancelText {
		get => m_CancelText;
		set => m_CancelText = value;
	}

	public string HeaderTextValue {
		get => HeaderText.text;
		set => HeaderText.text = value;
	}
	public string ContentTextValue {
		get => ContentText.text;
		set => ContentText.text = value;
	}
	public string ConfirmTextValue {
		get => ConfirmText.text;
		set => ConfirmText.text = value;
	}
	public string CancelTextValue {
		get => CancelText.text;
		set => CancelText.text = value;
	}



	public Action OnConfirmed {
		get => m_OnConfirmed;
		set => m_OnConfirmed = value;
	}
	public Action OnCancelled {
		get => m_OnCancelled;
		set => m_OnCancelled = value;
	}



	// Methods

	public void Confirm() {
		UIManager.CloseScreen(this);
		OnConfirmed?.Invoke();
		OnConfirmed = null;
		OnCancelled = null;
	}

	public void Cancel() {
		UIManager.CloseScreen(this);
		OnCancelled?.Invoke();
		OnConfirmed = null;
		OnCancelled = null;
	}

	public override void Show() {
		CurrentSelected = DefaultSelected;
		base.Show();
	}

	public override void Back() {
		Cancel();
	}
}
