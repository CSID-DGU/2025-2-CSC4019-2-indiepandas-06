using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapObjectSlotUI : MonoBehaviour, ILongPressable {
    [SerializeField] private Image objectImage;
    [SerializeField] private TextMeshProUGUI countText;

    private ItemBase item;
    private MapObjectData mapObjData;

    public void Init(ItemBase newItem) {
        item = newItem;
        UpdateUI();
    }

    public void UpdateUI() {
        if (item == null) {
            objectImage.enabled = false;
            countText.enabled = false;
            return;
        }

        if (item.ItemData.ItemType == ItemType.MapObject) {
            // MapObjectData 가져오기
            mapObjData = MapObjectDatabase.Instance.GetMapObjectData(item.ItemData);

            if (mapObjData != null && mapObjData.Sprites.Count > 0) {
                objectImage.sprite = mapObjData.Sprites[0];
                objectImage.enabled = true;
            } else {
                objectImage.enabled = false;
            }
        } else {
            objectImage.sprite = item.ItemData.ItemIconSprite;
            objectImage.enabled = true;
        }

        countText.text = item.Count.ToString();
        countText.enabled = item.ItemData.IsStackable;
    }

    public void OnLongPressed() {
        BuildingModeManager.Instance.ShowGhost(mapObjData);
        item.SubtractCount();
        UpdateUI();
    }
}