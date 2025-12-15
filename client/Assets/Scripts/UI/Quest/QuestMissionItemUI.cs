using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestMissionItemUI : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private GameObject completeUI;
    [SerializeField] private TextMeshProUGUI completeText;
    [SerializeField] private Image completeImage;
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Sprite completeSprite;

    public void Setup(string title, string description, string completeComment, MissionState state, bool isCurrent) {
        if (titleText) {
            titleText.text = string.IsNullOrEmpty(title) ? "(미션)" : title;
        }

        if (completeImage) {
            completeImage.sprite = (state == MissionState.Completed) ? completeSprite : activeSprite;
        }

        if (completeUI) {
            completeUI.SetActive(state == MissionState.Completed);
        }

        if (descriptionText) {
            descriptionText.text = string.IsNullOrEmpty(description) ? "" : description;
        }
        if (completeText) {
            completeText.text = string.IsNullOrEmpty(completeComment) ? "" : completeComment;
        }
    }
}
