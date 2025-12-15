using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class QuestListItemUI : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button button;
    [SerializeField] private Image backImage;
    [SerializeField] private Image completeImage;

    private string questId;
    private Action<string> onClicked;
    public string QuestId => questId;

    public void Setup(string questId, string title, Action<string> onClicked, string currentlySelectedId, bool isCompleted) {
        this.questId = questId;
        this.onClicked = onClicked;

        if (titleText) {
            titleText.text = string.IsNullOrEmpty(title) ? "(제목 없음)" : title;
        }

        // 버튼 리스너 중복 방지
        if (button) {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => this.onClicked?.Invoke(this.questId));
        }

        if (completeImage) {
            completeImage.enabled = isCompleted;
        }

        RefreshSelection(currentlySelectedId);
    }

    public void RefreshSelection(string selectedId) {
        if (backImage) {
            backImage.enabled = (questId == selectedId && !string.IsNullOrEmpty(selectedId));
        }
    }
}