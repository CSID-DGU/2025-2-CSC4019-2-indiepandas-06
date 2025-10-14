using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Inventory : MonoBehaviour {
    [SerializeField] private int capacity = 40;
    public int Capacity => capacity;
    private List<ItemBase> items = new List<ItemBase>();

    public IReadOnlyList<ItemBase> Items => items;

    public bool AddItem(ItemBase item) {
        if (items.Count >= capacity) { return false; }

        if (item.ItemData.IsStackable) {
            var existingItem = items.FirstOrDefault(i => i.ItemData == item.ItemData);
            if (existingItem != null) {
                existingItem.AddCount(item.Count);
                return true;
            }
        }

        items.Add(item);
        return true;
    }

    public bool RemoveItem(ItemBase item) {
        return items.Remove(item);
    }

    public ItemBase GetItemAt(int index) {
        if (index < 0 || index >= items.Count) { return null; }
        return items[index];
    }
}
