using UnityEngine;

[System.Serializable]
public struct QuickSlotEntry {
    public ItemType itemType;
    public int indexInInventory;
    public string instanceId;
    public bool IsAssigned => !string.IsNullOrEmpty(instanceId);

    public QuickSlotEntry(ItemType type, int index, string id) {
        itemType = type;
        indexInInventory = index;
        instanceId = id;
    }
}

public class QuickSlotManager : MonoSingleton<QuickSlotManager> {
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

        var item = InventoryManager.Instance.GetItemAt(inventoryIndex, itemType);
        if (item == null) { 
            ClearSlot(index);
            return;
        }
        
        slots[index] = new QuickSlotEntry(itemType, inventoryIndex, item.InstanceId.ToString());
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
        else {
            // 툴이 아니면 맨손 전환
            ToolManager toolManager = GameManager.Player.GetComponent<ToolManager>();
            toolManager.EquipNone();
        }
        if (item is MapObjectItem mapObjectItem) {
            // 빌딩모드 돌입
            Debug.Log("MapObject!");
            UIManager.OpenScreen(Screen.BuildingMode);
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

    private void OnEnable() {
        if(InventoryManager.Instance != null) {
            InventoryManager.Instance.OnInventoryChanged += RemapIfNeeded;
        }
    }
    private void OnDisable() {
        if(InventoryManager.Instance != null) {
            InventoryManager.Instance.OnInventoryChanged -= RemapIfNeeded;
        }
    }

    private void RemapIfNeeded() {
        // 각 슬롯 Remap
        for (int i = 0; i < slots.Length; i++) {
            var e = slots[i];

            // 빈 슬롯 무시
            if (!e.IsAssigned) {
                continue;
            }

            // 가리키는 칸의 아이템 가져오기
            var inv = InventoryManager.Instance.GetInventory(e.itemType);
            var curr = inv.GetItemAt(e.indexInInventory);

            // 같은 인스턴스면 유지
            if (curr != null && curr.InstanceId.ToString() == e.instanceId) {
                continue;
            }

            // 다르다면 인벤토리에서 GUID로 재탐색
            int found = -1;
            for (int k = 0; k < inv.Items.Count; k++) {
                var it = inv.GetItemAt(k);
                if (it != null && it.InstanceId.ToString() == e.instanceId) {
                    found = k;
                    break;
                }
            }

            if (found >= 0) {
                // 발견했다면 인덱스 갱신
                e.indexInInventory = found;
                slots[i] = e;
            } else {
                // 사라졌으면 비우기
                ClearSlot(i);
            }
        }
    }
}
