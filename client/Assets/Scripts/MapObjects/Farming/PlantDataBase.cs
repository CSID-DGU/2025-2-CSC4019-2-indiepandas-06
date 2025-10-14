using UnityEngine;
using System.Collections.Generic;

public class PlantDatabase : MonoSingleton<PlantDatabase>, IRegisterable<PlantData>
{
    // 인스펙터에서 관리 용
    public List<PlantData> plantDatas = new List<PlantData>();
    // 런타임 용 (해시맵)
    private Dictionary<string, PlantData> plantDict = new Dictionary<string, PlantData>();

    protected override void Awake() {
        base.Awake();
        // 인스펙터에 등록한 리스트 기반으로 Dictionary 제작
        plantDict = new Dictionary<string, PlantData>();
        foreach (var plant in plantDatas) {
            plantDict[plant.PlantId] = plant;
        }
    }

    public PlantData GetPlantData(string plantId) {
        return plantDict.ContainsKey(plantId) ? plantDict[plantId] : null;
    }

    public void AddItem(PlantData data) {
        if (data == null || string.IsNullOrEmpty(data.PlantId)) { return; }
        if (plantDict.ContainsKey(data.PlantId)) {
            Debug.LogWarning($"이미 존재하는 식물: {data.PlantId}");
            return;
        }
        plantDatas.Add(data);
        plantDict[data.PlantId] = data;
    }

    public void ClearItems() {
        plantDatas.Clear();
        plantDict.Clear();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        
    }
}
