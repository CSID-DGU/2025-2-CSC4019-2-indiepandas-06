using UnityEngine;

public class ToolManager : MonoBehaviour
{
    private ToolVisual toolVisual;
    [SerializeField] private GameObject toolObject;
    private ToolBase equippedTool;
    public ToolBase EquippedTool => equippedTool;

    public void EquipTool(ItemData data) {
        // 기존 툴 컴포넌트 모두 제거
        RemoveAllToolComponents();

        // null 이면 맨손 전환
        if (data == null) {
            equippedTool = null;
            toolVisual?.SetSprite(null);
            // 툴 오브젝트 숨김
            // if (toolObject) { toolObject.SetActive(false); }
            return;
        }

        // 툴이 아닌 데이터면 실패
        if (!data.IsTool || string.IsNullOrEmpty(data.ToolClassName)) { 
            equippedTool = null;
            toolVisual?.SetSprite(null);
            return;
        }

        equippedTool = data.AttachToolComponent(toolObject);
        toolVisual?.SetSprite(data);
    }

    public void EquipNone() => EquipTool(null);
    
    void RemoveAllToolComponents() {
        if (!toolObject) {
            return;
        }
        var olds = toolObject.GetComponents<ToolBase>();
        for (int i = 0; i < olds.Length; i++) {
            Destroy(olds[i]);
        }
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
       // ItemBase testItem = new ToolItem(ItemDatabase.Instance.GetItemData("CarrotSeed"));
        //testItem.Use();
        //EquipTool(ItemDataBase.Instance.GetItemData("CarrotSeed"));
        //equippedTool.Use();
    }

    // Update is called once per frame
    void Update() {
        
    }
}
