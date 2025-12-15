using UnityEngine;
using System;

[CreateAssetMenu(fileName = "ItemData", menuName = "Items/ItemData")]
public class ItemData : ScriptableObject
{
    [SerializeField] private string itemId;
    public string ItemId => itemId;
    [SerializeField] private string itemName;
    public string ItemName => itemName;
    [SerializeField] private string itemDescription;
    public string ItemDescription => itemDescription;
    [SerializeField] private ItemType itemType;
    public ItemType ItemType => itemType;
    [SerializeField] private bool isStackable;
    public bool IsStackable => isStackable;
    [SerializeField] private Sprite itemIconSprite;
    public Sprite ItemIconSprite => itemIconSprite;
    [SerializeField] private bool isTool;
    public bool IsTool => isTool;
    [SerializeField] private string itemClassName;
    public string ItemClassName => itemClassName;
    [SerializeField] private string toolClassName;
    public string ToolClassName => toolClassName;
    [SerializeField] private Sprite holdingSprite;
    public Sprite HoldingSprite => ((holdingSprite == null) ? itemIconSprite : holdingSprite);

    public ToolBase AttachToolComponent(GameObject host) {
        if (!isTool) { return null; }

        var type = Type.GetType(toolClassName);
        if (type == null) {
            Debug.LogError($"'{toolClassName}' 이름의 도구 클래스 없음");
            return null;
        }
        ToolBase toolBase = (ToolBase)host.AddComponent(type);
        toolBase.SetToolData(this);
        return toolBase;
    }
}
