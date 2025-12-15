using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;


public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, ILongPressable
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backImage;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Sprite backSprite;
    [SerializeField] private Sprite focusSprite;

    private int index;

    private void Start() {
        
    }

    public void Init(int slotIndex) {
        index = slotIndex;
        UpdateUI();
    }

    public void UpdateUI() {
        var item = InventoryManager.Instance.GetItemAt(index);
        
        iconImage.enabled = item != null;
        countText.enabled = item != null;

        if (item != null) {
            iconImage.sprite = item.ItemData?.ItemIconSprite;
            countText.text = item.Count.ToString();
            countText.enabled = (item.ItemData.IsStackable);
        }

        if (index == InventoryManager.Instance.CurrentSlotIndex) {
            backImage.sprite = focusSprite;
        }
        else {
            backImage.sprite = backSprite;
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        InventoryManager.Instance.SelectSlot(index);
    }

    public void OnLongPressed() {
        var item = InventoryManager.Instance.GetItemAt(index);
        if (item != null) {
            ItemDragManager.CurrentDraggingItem = new QuickSlotEntry(InventoryManager.Instance.CurrentType, index, item.InstanceId.ToString());
            InventoryManager.Instance.BeginItemDrag(item.ItemData.ItemIconSprite);
        }
    }
}
