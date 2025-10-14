using UnityEngine;

public class SeedTool : ToolBase
{
    // private PlantData plantData;
    public override void Use() {
        // TODO 경작지가 있으면 plantData 식물의 씨앗을 심는 내용 구현
        Debug.Log("씨앗 심기!");
    }
    public string GetPlantId() {
        string seedItemId = ItemData.ItemId;
        return seedItemId.EndsWith("Seed") ? seedItemId.Substring(0, seedItemId.Length - 4) : seedItemId;
    }
}
