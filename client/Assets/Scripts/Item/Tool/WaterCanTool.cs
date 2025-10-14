using UnityEngine;

public class WaterCanTool : ToolBase {
    [SerializeField] private GameObject waterAreaPrefab;
    protected override void Awake() {
        base.Awake();
        waterAreaPrefab = Resources.Load<GameObject>("Prefabs/WaterArea");
    }

    public override void Use() {
        toolVisual.PlaySwingAnimation(ToolVisual.SwingMode.Water);
        Debug.Log("물주기!");

        var waterArea = GameObject.Instantiate(waterAreaPrefab, transform.position, Quaternion.identity);
    }
}
