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
    public event Action<string> OnItemChanged;

    protected override void Awake() {
        base.Awake();
        foreach (ItemType type in Enum.GetValues(typeof(ItemType))) {
            inventories[type] = new Inventory();
        }
        // 아이템 변경 시 게임 전역으로 바로 브로드캐스트
        OnItemChanged += (id) => GameplayEvents.RaiseItemChanged(id);
    }

    public ItemBase GetItemAt(int index, ItemType itemType) {
        return inventories[itemType].GetItemAt(index);
    }
    public ItemBase GetItemAt(int index) {
        return GetItemAt(index, currnentType);
    }

    public bool AddItem(ItemBase item) {
        bool result = inventories[item.ItemData.ItemType].AddItem(item);
        if (result) { 
            item.OnCountChanged += HandleItemCountChanged;
            OnItemChanged?.Invoke(item.ItemData.ItemId);
            OnInventoryChanged?.Invoke();
        }
        return result;
    }

    public bool AddItem(ItemData itemData) {
        return InventoryManager.Instance.AddItem(ItemDatabase.Instance.CreateItem(itemData));
    }

    public bool AddItem(String itemId) {
        return InventoryManager.Instance.AddItem(ItemDatabase.Instance.CreateItem(itemId));
    }

    public bool RemoveItem(ItemBase item) {
        bool result = inventories[item.ItemData.ItemType].RemoveItem(item);
        if (result) {
            item.OnCountChanged -= HandleItemCountChanged;
            OnItemChanged?.Invoke(item.ItemData.ItemId);
            OnInventoryChanged?.Invoke();
        }
        return result;
    }

    private void HandleItemCountChanged(ItemBase item) {
        if (item.Count > 0) {
            OnInventoryChanged?.Invoke();
            OnItemChanged?.Invoke(item.ItemData.ItemId);
        } else {
            RemoveItem(item);
        }
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
        /*
        InventoryManager.Instance.AddItem(new ToolItem(ItemDatabase.Instance.GetItemData("CarrotSeed")));
        InventoryManager.Instance.AddItem(new ToolItem(ItemDatabase.Instance.GetItemData("CarrotSeed")));
        InventoryManager.Instance.AddItem(new ToolItem(ItemDatabase.Instance.GetItemData("CarrotSeed")));
        InventoryManager.Instance.AddItem(new ToolItem(ItemDatabase.Instance.GetItemData("CarrotSeed")));
        InventoryManager.Instance.AddItem(new ToolItem(ItemDatabase.Instance.GetItemData("CarrotSeed")));
        InventoryManager.Instance.AddItem(new ToolItem(ItemDatabase.Instance.GetItemData("WaterCan")));
        InventoryManager.Instance.AddItem(new ToolItem(ItemDatabase.Instance.GetItemData("WaterCan")));
        InventoryManager.Instance.AddItem(new ToolItem(ItemDatabase.Instance.GetItemData("WaterCan")));
        InventoryManager.Instance.AddItem(new MapObjectItem(ItemDatabase.Instance.GetItemData("VendingMachineRedItem"), 5));
        InventoryManager.Instance.AddItem(new MapObjectItem(ItemDatabase.Instance.GetItemData("SimpleBedItem"), 5));
        InventoryManager.Instance.AddItem(new MapObjectItem(ItemDatabase.Instance.GetItemData("GlassSquareTableItem"), 5));
        InventoryManager.Instance.AddItem(new MapObjectItem(ItemDatabase.Instance.GetItemData("WoodenSquareTableItem"), 2));
        InventoryManager.Instance.AddItem(new MapObjectItem(ItemDatabase.Instance.GetItemData("WoodenRoundTableItem"), 2));
        InventoryManager.Instance.AddItem(new MapObjectItem(ItemDatabase.Instance.GetItemData("VendingMachineRedItem"), 5));
        InventoryManager.Instance.AddItem(new MapObjectItem(ItemDatabase.Instance.GetItemData("WoodenRoundTableItem"), 2));
        InventoryManager.Instance.AddItem(new MapObjectItem(ItemDatabase.Instance.GetItemData("VendingMachineRedItem"), 5));
        InventoryManager.Instance.AddItem(new MapObjectItem(ItemDatabase.Instance.GetItemData("FountainItem"), 2));
        */
		/*
        InventoryManager.Instance.AddItem(ItemDatabase.Instance.CreateItem("CarrotSeed"));
        InventoryManager.Instance.AddItem(ItemDatabase.Instance.CreateItem("WaterCan"));
        InventoryManager.Instance.AddItem(ItemDatabase.Instance.CreateItem("VendingMachineRedItem"));
        InventoryManager.Instance.AddItem(ItemDatabase.Instance.CreateItem("FountainItem"));
        InventoryManager.Instance.AddItem(ItemDatabase.Instance.CreateItem("SimpleBedItem", 5));
		*/
    }

    private void OnEnable() {
        isDragging = false;
        dragImage.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update() {
        // 아이템 드래그 처리
        ItemDrag();
    }

    private void ItemDrag() {
        if (!isDragging || dragImage == null || !dragImage.gameObject.activeSelf) {
            return;
        }
        Vector2 screenPos = InputManager.PointPositionSafe;
        if (screenPos == Vector2.zero) {
            return;
        }
        //dragImage.rectTransform.anchoredPosition = screenPos;
        
        var rectCanvas = canvas.transform as RectTransform;
        Camera cam = null;
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace)
            cam = canvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectCanvas, screenPos, cam, out Vector2 localPos)) {
            dragImage.rectTransform.anchoredPosition = localPos;
        }
        
        if (InputManager.GetKeyUp(KeyAction.Click)) {
            EndItemDrag();
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
