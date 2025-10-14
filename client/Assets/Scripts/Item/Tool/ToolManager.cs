using UnityEngine;

public class ToolManager : MonoBehaviour
{
    private ToolVisual toolVisual;
    [SerializeField] private GameObject toolObject;
    private ToolBase equippedTool;
    public ToolBase EquippedTool => equippedTool;

    public void EquipTool(ItemData data) {
        if (!data.IsTool || string.IsNullOrEmpty(data.ToolClassName)) { return; }

        // 기존 ToolBase 제거
        if (equippedTool != null) {
            foreach (var oldTool in toolObject.GetComponents<ToolBase>()) {
                Destroy(oldTool);
            }
        }

        equippedTool = data.AttachToolComponent(toolObject);
        toolVisual?.SetSprite(data);
    }

    public void UseTool() {
        if (equippedTool != null) {
            equippedTool.Use();
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        toolVisual = toolObject?.GetComponent<ToolVisual>();

        // TODO 테스트 코드 삭제
        ItemBase testItem = new ToolItem(ItemDatabase.Instance.GetItemData("CarrotSeed"));
        testItem.Use();
        //EquipTool(ItemDataBase.Instance.GetItemData("CarrotSeed"));
        //equippedTool.Use();
    }

    // Update is called once per frame
    void Update() {
        
    }
}
