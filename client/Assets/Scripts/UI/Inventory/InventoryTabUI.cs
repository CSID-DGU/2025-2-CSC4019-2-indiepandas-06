using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryTabUI : MonoBehaviour, IPointerClickHandler {
    private ItemType itemType;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image focusImage;
    

    public void Init(ItemType newType, Sprite iconSprite) {
        itemType = newType;
        iconImage.sprite = iconSprite;
    }

    public void UpdateUI() {
        Color iconColor = iconImage.color;
        Color focusColor = focusImage.color;
        if (itemType == InventoryManager.Instance.CurrentType) {
            iconColor.a = 1f;
            focusColor.a = 1f;
        }
        else {
            iconColor.a = 0.5f;
            focusColor.a = 0f;
        }
        iconImage.color = iconColor;
        focusImage.color = focusColor;
    }


    public void OnPointerClick(PointerEventData eventData) {
        InventoryManager.Instance.ChangeTab(itemType);
        Debug.Log($"탭 전환 {itemType.ToString()}");
    }
}
