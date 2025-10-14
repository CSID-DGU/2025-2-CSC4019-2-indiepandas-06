using UnityEngine;

public abstract class ToolBase : MonoBehaviour
{
    public ItemData ItemData { get; private set; }
    protected ToolVisual toolVisual;

    protected virtual void Awake() {
        toolVisual = GetComponent<ToolVisual>();
    }

    // 각 ToolBase 구현 클래스에서 구현
    public abstract void Use();

    public void SetToolData(ItemData newData) {
        ItemData = newData;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        
    }
}
