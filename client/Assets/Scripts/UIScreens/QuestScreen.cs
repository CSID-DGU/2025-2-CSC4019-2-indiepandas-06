using UnityEngine;
using UnityEngine.UI;
using System;

public class QuestScreen : ScreenBase {
    [SerializeField] private Button backButton;

    public override bool IsPrimary => false;
    public override bool IsOverlay => false;
    public override BackgroundMode BackgroundMode => BackgroundMode.Preserve;
    public override InputPolicy InputPolicy => InputPolicy.UIOnly;

    public override void Show() {
        base.Show();
    }

    public override void Hide() {
        base.Hide();
    }

    public override void Back() {
        base.Back();
    }

    private void OnEnable() {
        backButton.onClick.AddListener(OnClickBack);
    }
    
    private void OnDisable() {
        backButton.onClick.RemoveListener(OnClickBack);
    }

    private void OnClickBack() {
        UIManager.CloseScreen(this);
    }

}
