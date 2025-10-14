using UnityEngine;

public class ToolItem : ItemBase {
    public ToolItem(ItemData data) : base(data) { }
    public override void Use() {
        if (ItemData.IsTool) {
            ToolManager toolManager = GameManager.Player.GetComponent<ToolManager>();
            toolManager.EquipTool(ItemData);
        }
    }
}
