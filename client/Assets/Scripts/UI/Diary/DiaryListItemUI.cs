using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DiaryListItemUI : MonoBehaviour {
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private Image selectedBackground;

    private string entryId;
    private Action<string> onClick;

    public string EntryId => entryId;

    public void Bind(DiaryEntry e, Action<string> onClick) {
        entryId = e.id;
        this.onClick = onClick;
        if (titleText) { titleText.text = string.IsNullOrEmpty(e.title) ? "(제목 없음)" : e.title; }
        if (dateText) { dateText.text  = string.IsNullOrEmpty(e.dateIso) ? "-" : e.dateIso; }
        if (button) {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => this.onClick?.Invoke(entryId));
        }

        SetSelected(false);
    }

    public void SetSelected(bool selected) {
        if (selectedBackground) {
            selectedBackground.enabled = selected;
        }
    }
}
