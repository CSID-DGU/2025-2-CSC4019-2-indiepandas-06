using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Main Menu Screen
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("UI Screen/Main Menu Screen")]
public sealed class MainMenuScreen : ScreenBase {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(MainMenuScreen))]
	class TitleScreenEditor : EditorExtensions {
		MainMenuScreen I => target as MainMenuScreen;
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

	public override bool IsPrimary => true;
	public override bool IsOverlay => false;
	public override BackgroundMode BackgroundMode => BackgroundMode.StaticBackground;



	// Methods

	public void StartGame() {
		UIManager.OpenScreen(Screen.Game);
	}

	public void Options() {
		UIManager.OpenScreen(Screen.Options);
	}

	public void QuitGame() {
		UIManager.OpenScreen(Screen.Confirmation);
		UIManager.ConfirmationHeader  = "게임 종료";
		UIManager.ConfirmationContent = "게임을 종료하시겠습니까?";
		UIManager.ConfirmationConfirm = "종료";
		UIManager.ConfirmationCancel  = "취소";
		UIManager.OnConfirmationConfirmed += () => {
			GameManager.QuitGame();
		};
	}

	public override void Back() {
		QuitGame();
	}
}
