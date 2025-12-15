using UnityEngine;
using UnityEngine.UI;
using System;

public class BuildingModeScreen : ScreenBase {
    [SerializeField] private MapObjectItemContainer container;
    [SerializeField] private Button backButton;

    public override bool IsPrimary => false;
    public override bool IsOverlay => false;
    public override BackgroundMode BackgroundMode => BackgroundMode.Preserve;
    public override InputPolicy InputPolicy => InputPolicy.Both;

    public override void Show() {
        base.Show();
    }

    public override void Hide() {
        base.Hide();
        // 빌딩 모드 종료시 정리 필요하면 여기서
    }

    public override void Back() {
        // 뒤로가기 시 빌딩 모드 취소 처리 등
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
