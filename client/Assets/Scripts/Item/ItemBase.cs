using UnityEngine;

public abstract class ItemBase {
    public ItemData ItemData { get; protected set; }
    public int Count { get; private set; } = 1;

    public ItemBase(ItemData data, int count = 1) {
        ItemData = data;
        Count = count;
    }

    public void AddCount(int amount) {
        if (ItemData.IsStackable) {
            Count += amount;
        }
    }

    public void SubtractCount(int amount) {
        Count = Mathf.Max(0, Count - amount);
    }

    public abstract void Use();
}
