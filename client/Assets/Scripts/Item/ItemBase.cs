using UnityEngine;
using System;

public abstract class ItemBase {
    public readonly Guid InstanceId = Guid.NewGuid();

    public ItemData ItemData { get; protected set; }
    public int Count { get; private set; } = 1;

    public event Action<ItemBase> OnCountChanged;

    public ItemBase(ItemData data, int count = 1) {
        ItemData = data;
        Count = count;
    }

    public void AddCount(int amount = 1) {
        if (ItemData.IsStackable) {
            Count += amount;
            OnCountChanged?.Invoke(this);
        }
    }

    public void SubtractCount(int amount = 1) {
        Count = Mathf.Max(0, Count - amount);
        OnCountChanged?.Invoke(this);
    }

    public void SetCount(int amount) {
        Count = amount;
    }

    public abstract void Use();
}
