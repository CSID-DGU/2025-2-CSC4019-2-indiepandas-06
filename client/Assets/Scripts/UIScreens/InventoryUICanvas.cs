using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class InventoryUICanvas : ScreenBase {
    private int maxSlotNum = 80;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private Transform tabContainer;
    [SerializeField] private Transform quickSlotRegisterContainer;
    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private InventoryTabUI tabPrefab;
    [SerializeField] private QuickSlotRegisterUI quickSlotRegisterPrefab;
    [SerializeField] private QuickSlotContainerUI quickSlotUI;
    [SerializeField] private ScrollRect scrollRect;

    [SerializeField] private List<TabIconEntry> tabIconsList;
    [SerializeField] private Button backButton;
    private Dictionary<ItemType, Sprite> tabIcons;

    [System.Serializable]
    public class TabIconEntry {
        public ItemType itemType;
        public Sprite icon;
    }

    private List<InventorySlotUI> slotUIs = new();
    private List<InventoryTabUI> tabUIs = new();
    private List<QuickSlotRegisterUI> quiclSlotUIs = new();

    public virtual bool IsPrimary => true;
	public virtual bool IsOverlay => false;
	public virtual BackgroundMode BackgroundMode => BackgroundMode.Preserve;


    private void Awake() {
        tabIcons = new Dictionary<ItemType, Sprite>();
        foreach (var entry in tabIconsList) {
            tabIcons[entry.itemType] = entry.icon;
        }
        BuildSlots();
    }

    private void Start() {   
        HandleInventoryChanged();
    }

    private void BuildSlots() {
        for (int i = 0; i < maxSlotNum; i++) {
            var slot = Instantiate(slotPrefab, slotContainer);
            slot.Init(i);
            slotUIs.Add(slot);
        }
        foreach (ItemType type in Enum.GetValues(typeof(ItemType))) {
            var tab = Instantiate(tabPrefab, tabContainer);
            tab.Init(type, tabIcons[type]);
            tabUIs.Add(tab);
        }

        int count = QuickSlotManager.Instance.SlotCount;
        for (int i = 0; i < count; i++) {
            var quickSlot = Instantiate(quickSlotRegisterPrefab, quickSlotRegisterContainer);
            // var slotUI = quickSlot.GetComponent<QuickSlotRegisterUI>();
            quickSlot.Init(i);
            quiclSlotUIs.Add(quickSlot);
        }
    }

    protected override void Update() {
        base.Update();
        if (InventoryManager.Instance.IsDragging) {
            scrollRect.vertical = false;
        } else {
            scrollRect.vertical = true;
        }
    }

    public void RefreshUI(Inventory inventory) {
        for (int i = 0; i < maxSlotNum; i++) {
            if (i < inventory.Capacity) {
                slotUIs[i].gameObject.SetActive(true);
                slotUIs[i].UpdateUI();
            } else {
                slotUIs[i].gameObject.SetActive(false);
            }
        }
        foreach (var tab in tabUIs) {
            tab.UpdateUI();
        }
    }

    public override void Show() {
        base.Show();
    }

    private void OnEnable() {
        backButton.onClick.AddListener(OnClickBack);
        InventoryManager.Instance.OnInventoryChanged += HandleInventoryChanged;
        //quickSlotUI.gameObject.SetActive(false);
        HandleInventoryChanged();
    }

    private void OnDisable() {
        backButton.onClick.RemoveListener(OnClickBack);
        InventoryManager.Instance.OnInventoryChanged -= HandleInventoryChanged;
        //quickSlotUI.gameObject.SetActive(true);
    }

    private void OnClickBack() {
        UIManager.CloseScreen(this);
    }



    private void HandleInventoryChanged() {
        RefreshUI(InventoryManager.Instance.CurrentInventory);
    }
}
