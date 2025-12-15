using UnityEngine;

public class ToolItem : ItemBase {
    public ToolItem(ItemData data, int count = 1) : base(data, count) { }
    public override void Use() {
        if (ItemData.IsTool) {
            ToolManager toolManager = GameManager.Player.GetComponent<ToolManager>();
            toolManager.EquipTool(ItemData);
        }
    }
}
