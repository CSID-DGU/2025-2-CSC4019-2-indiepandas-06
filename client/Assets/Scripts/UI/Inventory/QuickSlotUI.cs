using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class QuickSlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backImage;
    [SerializeField] private Sprite[] backSprites;
    [SerializeField] private Sprite focusSprite;

    private int index;

    public void Init(int slotIndex) {
        index = slotIndex;
        UpdateUI();
    }

    public void UpdateUI() {
        var item = QuickSlotManager.Instance.GetItemAt(index);
        iconImage.sprite = item?.ItemData?.ItemIconSprite;
        iconImage.enabled = item != null;

        if (index == QuickSlotManager.Instance.CurrentSlotIndex) {
            backImage.sprite = focusSprite;
        }
        else {
            if (index == 0) {
                backImage.sprite = backSprites[0];
            }
            else if (index == QuickSlotManager.Instance.SlotCount - 1) {
                backImage.sprite = backSprites[2];
            }
            else {
                backImage.sprite = backSprites[1];
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        QuickSlotManager.Instance.SelectSlot(index);
    }
}
