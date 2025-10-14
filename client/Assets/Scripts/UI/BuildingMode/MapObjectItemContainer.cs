using UnityEngine;
using System.Collections.Generic;

public class MapObjectItemContainer : MonoBehaviour {
    [SerializeField] private Transform slotContainer;
    [SerializeField] private MapObjectSlotUI slotPrefab;

    private List<MapObjectSlotUI> slotPool = new();

    private void OnEnable() {
        InventoryManager.Instance.OnInventoryChanged += RefreshUI;
        RefreshUI();
    }

    private void OnDisable() {
        InventoryManager.Instance.OnInventoryChanged -= RefreshUI;
    }

    private void EnsurePoolSize(int size) {
        while (slotPool.Count < size) {
            var slot = Instantiate(slotPrefab, slotContainer);
            slotPool.Add(slot);
        }
    }

    private void RefreshUI() {
        // MapObject 인벤토리 가져옴
        var inventory = InventoryManager.Instance.GetInventory(ItemType.MapObject);
        int itemCount = inventory.Items.Count;

        EnsurePoolSize(itemCount);

        for (int i = 0; i < slotPool.Count; i++) {
            if (i < itemCount) {
                slotPool[i].gameObject.SetActive(true);
                slotPool[i].Init(inventory.GetItemAt(i));
            } else {
                slotPool[i].gameObject.SetActive(false);
            }
        }
    }
}