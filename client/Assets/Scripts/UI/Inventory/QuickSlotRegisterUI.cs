using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class QuickSlotRegisterUI : MonoBehaviour, IDropHandler, ILongPressable {
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI countText;
    private int index = 1;

    public void Init(int slotIndex) {
        index = slotIndex;
        UpdateUI();
    }

    private void OnEnable() {
        UpdateUI();
    }

    public void UpdateUI() {
        var item = QuickSlotManager.Instance.GetItemAt(index);
        iconImage.enabled = item != null;
        countText.enabled = item != null;

        if (item != null) {
            iconImage.sprite = item.ItemData?.ItemIconSprite;
            countText.text = item.Count.ToString();
            countText.enabled = (item.ItemData.IsStackable);
        }
    }

    public void OnDrop(PointerEventData eventData) {
        if (!InventoryManager.Instance.IsDragging) {
            return;
        }
        
        var dragItemEntry = ItemDragManager.CurrentDraggingItem;
        QuickSlotManager.Instance.SetItem(index, dragItemEntry.itemType, dragItemEntry.indexInInventory);
        UpdateUI();
    }

    public void OnLongPressed() {
        QuickSlotManager.Instance.ClearSlot(index);
        UpdateUI();
    }
}
