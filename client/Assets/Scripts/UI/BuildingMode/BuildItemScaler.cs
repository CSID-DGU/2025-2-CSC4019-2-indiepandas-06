using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
[ExecuteAlways]
public class BuildItemScaler : MonoBehaviour {
    [Min(1)] public int columns = 4;
    public float aspect = 1f;

    private GridLayoutGroup grid;
    private RectTransform rect;

    void OnEnable() {
        grid = GetComponent<GridLayoutGroup>();
        rect = GetComponent<RectTransform>();
        ItemSizeApply();
    }

    void OnRectTransformDimensionsChange() {
        if (!isActiveAndEnabled) { return; }
        ItemSizeApply();
    }

    void ItemSizeApply() {
        if (grid == null || rect == null || columns <= 0) { return; }

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;

        float totalWidth = rect.rect.width;
        if (totalWidth <= 0f) { return; }

        float usedByPadding = grid.padding.left + grid.padding.right;
        float usedBySpacing = grid.spacing.x * (columns - 1);
        float available = totalWidth - usedByPadding - usedBySpacing;
        if (available <= 0f) { return; }

        float cellW = available / columns;
        cellW = Mathf.Floor(cellW);

        float cellH = Mathf.Max(1f, cellW * Mathf.Max(0.01f, aspect));
        grid.cellSize = new Vector2(cellW, cellH);
    }
}