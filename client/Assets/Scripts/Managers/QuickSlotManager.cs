using UnityEngine;

[System.Serializable]
public struct QuickSlotEntry {
    public ItemType itemType;
    public int indexInInventory;

    public QuickSlotEntry(ItemType type, int index) {
        itemType = type;
        indexInInventory = index;
    }
}

public class QuickSlotManager : MonoSingleton<QuickSlotManager>
{
    [SerializeField] private int slotCount = 10;
    private QuickSlotEntry[] slots;
    public int CurrentSlotIndex => currentSlotIndex;
    private int currentSlotIndex = 0;

    public int SlotCount => slotCount;
    public QuickSlotEntry CurrentItem => slots[currentSlotIndex];

    protected override void Awake() {
        base.Awake();
        slots = new QuickSlotEntry[slotCount];
        for (int i = 0; i < slotCount; i++) {
            slots[i] = new QuickSlotEntry();
        }
    }

    public void SetItem(int index, ItemType itemType, int inventoryIndex) {
        if (index < 0 || index >= slotCount) { return; }
        slots[index] = new QuickSlotEntry(itemType, inventoryIndex);
    }

    public void ClearSlot(int index) {
        if (index < 0 || index >= slotCount) return;
        slots[index] = new QuickSlotEntry();
}

    public ItemBase GetItemAt(int index) {
        if (index < 0 || index >= slotCount) { return null; }
        var slot = slots[index];
        return InventoryManager.Instance.GetItemAt(slot.indexInInventory, slot.itemType);
    }

    public void SelectSlot(int index) {
        if (index < 0 || index >= slotCount) { return; }
        currentSlotIndex = index;
        
        var item = GetItemAt(index);
        if (item is ToolItem toolItem) {
            toolItem.Use();
        }
    }

    public void UseCurrentItem() {
        var item = GetItemAt(currentSlotIndex);
        if (item != null) {
            item.Use();
        }
        else {
            Debug.Log("선택된 슬롯에 아이템이 없습니다");
        }
    }

    private void Start() {
        
    }

    private void Update() {
            //int slotIndex = Mathf.Clamp((int)InputManager.QuickSlot - 1, 0, 9);
            //SelectSlot(slotIndex);
    }
}
