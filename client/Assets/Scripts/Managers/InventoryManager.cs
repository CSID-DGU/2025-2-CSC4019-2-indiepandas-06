using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

public class InventoryManager : MonoSingleton<InventoryManager> {
    private Dictionary<ItemType, Inventory> inventories = new();
    private int currentSlotIndex;
    public int CurrentSlotIndex => currentSlotIndex;
    private ItemType currnentType = ItemType.Tool;
    
    public ItemType CurrentType => currnentType;
    public Inventory CurrentInventory => inventories[currnentType];

    
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image dragImage;
    private bool isDragging = false;
    public bool IsDragging => isDragging;
    public event Action OnInventoryChanged;

    protected override void Awake() {
        base.Awake();
        foreach (ItemType type in Enum.GetValues(typeof(ItemType))) {
            inventories[type] = new Inventory();
        }
    }

    public ItemBase GetItemAt(int index, ItemType itemType) {
        return inventories[itemType].GetItemAt(index);
    }
    public ItemBase GetItemAt(int index) {
        return GetItemAt(index, currnentType);
    }

    public bool AddItem(ItemBase item) {
        bool result = inventories[item.ItemData.ItemType].AddItem(item);
        if (result) { OnInventoryChanged?.Invoke(); }
        return result;
    }

    public bool RemoveItem(ItemBase item) {
        bool result = inventories[item.ItemData.ItemType].RemoveItem(item);
        if (result) { OnInventoryChanged?.Invoke(); }
        return result;
    }
    public void SelectSlot(int index) {
        currentSlotIndex = index;
        OnInventoryChanged?.Invoke();
    }

    public void ChangeTab(ItemType newType) {
        currnentType = newType;
        OnInventoryChanged?.Invoke();
    }

    public Inventory GetInventory(ItemType type) {
        return inventories[type];
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        // TODO 테스트 코드
        InventoryManager.Instance.AddItem(new ToolItem(ItemDatabase.Instance.GetItemData("CarrotSeed")));
        InventoryManager.Instance.AddItem(new ToolItem(ItemDatabase.Instance.GetItemData("CarrotSeed")));
        InventoryManager.Instance.AddItem(new ToolItem(ItemDatabase.Instance.GetItemData("CarrotSeed")));
        InventoryManager.Instance.AddItem(new ToolItem(ItemDatabase.Instance.GetItemData("CarrotSeed")));
        InventoryManager.Instance.AddItem(new ToolItem(ItemDatabase.Instance.GetItemData("CarrotSeed")));
        InventoryManager.Instance.AddItem(new ToolItem(ItemDatabase.Instance.GetItemData("WaterCan")));
        InventoryManager.Instance.AddItem(new ToolItem(ItemDatabase.Instance.GetItemData("WaterCan")));
        InventoryManager.Instance.AddItem(new ToolItem(ItemDatabase.Instance.GetItemData("WaterCan")));
        InventoryManager.Instance.AddItem(new MapObjectItem(ItemDatabase.Instance.GetItemData("VendingMachineRedItem"), 5));
        InventoryManager.Instance.AddItem(new MapObjectItem(ItemDatabase.Instance.GetItemData("VendingMachineRedItem"), 5));
        InventoryManager.Instance.AddItem(new MapObjectItem(ItemDatabase.Instance.GetItemData("VendingMachineRedItem"), 5));
        InventoryManager.Instance.AddItem(new MapObjectItem(ItemDatabase.Instance.GetItemData("WoodenRoundTableItem"), 2));
        InventoryManager.Instance.AddItem(new MapObjectItem(ItemDatabase.Instance.GetItemData("WoodenRoundTableItem"), 2));
        InventoryManager.Instance.AddItem(new MapObjectItem(ItemDatabase.Instance.GetItemData("VendingMachineRedItem"), 5));
        InventoryManager.Instance.AddItem(new MapObjectItem(ItemDatabase.Instance.GetItemData("WoodenRoundTableItem"), 2));
        InventoryManager.Instance.AddItem(new MapObjectItem(ItemDatabase.Instance.GetItemData("VendingMachineRedItem"), 5));
        InventoryManager.Instance.AddItem(new MapObjectItem(ItemDatabase.Instance.GetItemData("FountainItem"), 2));
    }

    // Update is called once per frame
    void Update() {
        // 아이템 드래그 처리
        ItemDrag();
    }

    private void ItemDrag() {
        if (dragImage.gameObject.activeSelf) {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Mouse.current.position.ReadValue(),
                canvas.worldCamera,
                out Vector2 localPos);
            dragImage.rectTransform.localPosition = localPos;

            if (Mouse.current.leftButton.wasReleasedThisFrame) {
                EndItemDrag();
            }
        }
    }

    public void BeginItemDrag(Sprite icon) {
        isDragging = true;
        dragImage.sprite = icon;
        dragImage.color = new Color(1, 1, 1, 0.5f);
        dragImage.gameObject.SetActive(true);
    }

    public void EndItemDrag() {
        isDragging = false;
        dragImage.gameObject.SetActive(false);
    }
}
