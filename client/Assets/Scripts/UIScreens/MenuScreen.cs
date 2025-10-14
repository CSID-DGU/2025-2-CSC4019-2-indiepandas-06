using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Menu Screen
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("UI Screen/Menu Screen")]
public sealed class MenuScreen : ScreenBase {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(MenuScreen))]
	class MenuScreenEditor : EditorExtensions {
		MenuScreen I => target as MenuScreen;
		public override void OnInspectorGUI() {
			Begin();

			LabelField("Selected", EditorStyles.boldLabel);
			I.DefaultSelected = ObjectField("Default Selected", I.DefaultSelected);
			Space();

			End();
		}
	}
	#endif



	// Properties

	public override bool IsPrimary => false;
	public override bool IsOverlay => true;
	public override BackgroundMode BackgroundMode => BackgroundMode.PreserveWithBlur;



	// Methods

	public void ResumeGame() {
		UIManager.Back();
	}

	public void Options() {
		UIManager.OpenScreen(Screen.Options);
	}

	public void BackToMainMenu() {
		UIManager.OpenScreen(Screen.Confirmation);
		UIManager.OnConfirmationConfirmed += () => {
			UIManager.OpenScreen(Screen.MainMenu);
		};
	}
}
