using UnityEngine;
using System.Collections.Generic;

public class MapObjectDatabase : MonoSingleton<MapObjectDatabase>, IRegisterable<MapObjectData>
{
    // 인스펙터에서 관리 용
    public List<MapObjectData> mapObjectDatas = new List<MapObjectData>();
    // 런타임 용 (해시맵)
    private Dictionary<string, MapObjectData> mapObjectDict = new Dictionary<string, MapObjectData>();
    private Dictionary<ItemData, MapObjectData> itemToMapDict = new Dictionary<ItemData, MapObjectData>();

    protected override void Awake() {
        base.Awake();
        // 인스펙터에 등록한 리스트 기반으로 Dictionary 제작
        mapObjectDict = new Dictionary<string, MapObjectData>();
        foreach (var mapObject in mapObjectDatas) {
            mapObjectDict[mapObject.ObjectId] = mapObject;
            if (mapObject.LinkedItem != null) {
                itemToMapDict[mapObject.LinkedItem] = mapObject;
            }
        }
    }

    public MapObjectData GetMapObjectData(string objectId) {
        return mapObjectDict.ContainsKey(objectId) ? mapObjectDict[objectId] : null;
    }

    public MapObjectData GetMapObjectData(ItemData item) {
        return itemToMapDict.ContainsKey(item) ? itemToMapDict[item] : null;
    }

    public void AddItem(MapObjectData data) {
        if (data == null || string.IsNullOrEmpty(data.ObjectId)) { return; }
        if (mapObjectDict.ContainsKey(data.ObjectId)) {
            Debug.LogWarning($"이미 존재하는 오브젝트: {data.ObjectId}");
            return;
        }
        mapObjectDatas.Add(data);
        mapObjectDict[data.ObjectId] = data;
    }

    public void ClearItems() {
        mapObjectDatas.Clear();
        mapObjectDict.Clear();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        
    }
}
