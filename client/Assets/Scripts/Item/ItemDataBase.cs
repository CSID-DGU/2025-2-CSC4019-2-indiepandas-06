using UnityEngine;
using System;
using System.Collections.Generic;

public enum ItemType {
    Special,
    Tool,
    Seed,
    Food,
    Equipment,
    MapObject,
    ETC
}

public class ItemDatabase : MonoSingleton<ItemDatabase>, IRegisterable<ItemData> {
    // 인스펙터에서 관리 용
    public List<ItemData> itemDatas = new List<ItemData>();
    // 런타임 용 (해시맵)
    private Dictionary<string, ItemData> itemDict = new Dictionary<string, ItemData>();

    protected override void Awake() {
        base.Awake();
        // 인스펙터에 등록한 리스트 기반으로 Dictionary 제작
        itemDict = new Dictionary<string, ItemData>();
        foreach (var item in itemDatas) {
            itemDict[item.ItemId] = item;
        }
    }

    public ItemData GetItemData(string itemId) {
        return itemDict.ContainsKey(itemId) ? itemDict[itemId] : null;
    }

    public void AddItem(ItemData data) {
        if (data == null || string.IsNullOrEmpty(data.ItemId)) { return; }
        if (itemDict.ContainsKey(data.ItemId)) {
            Debug.LogWarning($"이미 존재하는 아이템: {data.ItemId}");
            return;
        }
        itemDatas.Add(data);
        itemDict[data.ItemId] = data;
    }

    public void ClearItems() {
        itemDatas.Clear();
    }

    // 아이템 팩토리
    public ItemBase CreateItem(string itemId, int amount = 1) {
        var data = GetItemData(itemId);
        if (data == null) {
            Debug.LogWarning($"아이템 ID '{itemId}' 를 찾을 수 없음");
            return null;
        }
        return CreateItem(data, amount);
    }

    public ItemBase CreateItem(ItemData data, int amount = 1) {
        if (data == null) {
            Debug.LogWarning("ItemData가 null입니다.");
            return null;
        }

        Type itemType = Type.GetType(data.ItemClassName);
        if (itemType == null) {
            Debug.LogWarning($"'{data.ItemClassName}' 클래스를 찾을 수 없습니다.");
            return null;
        }

        try {
            // Activator를 사용해 동적 생성
            var instance = Activator.CreateInstance(itemType, data, amount) as ItemBase;
            if (instance == null) {
                Debug.LogWarning($"'{data.ItemClassName}'은 ItemBase를 상속하지 않았습니다.");
                return null;
            }
            return instance;
        }
        catch (Exception e) {
            Debug.LogError($"'{data.ItemClassName}' 생성 실패 - {e.Message}");
            return null;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
