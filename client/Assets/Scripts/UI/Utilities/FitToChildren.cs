using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class FitToChildren : MonoBehaviour
{
    [Header("Target (empty = this)")]
    public RectTransform target;

    [Header("Axes")]
    public bool fitHorizontal = true;
    public bool fitVertical   = true;

    [Header("Options")]
    public bool includeInactive = false;
    public Vector2 padding = Vector2.zero;      // x=좌우, y=상하 여유치
    public bool everyFrame = false;             // 매 프레임 갱신(동적 UI면 켜기)
    public bool rebuildLayoutBeforeMeasure = true; // 레이아웃 사용 시 측정 전 강제 리빌드

    RectTransform T => target ? target : (RectTransform)transform;

    void OnEnable()  { ResizeNow(); }
    void OnTransformChildrenChanged() { ResizeNow(); }
#if UNITY_EDITOR
    void OnValidate() { ResizeNow(); }
#endif
    void LateUpdate() { if (everyFrame) ResizeNow(); }

    public void ResizeNow()
    {
        var t = T;
        if (!t) return;

        if (rebuildLayoutBeforeMeasure)
            LayoutRebuilder.ForceRebuildLayoutImmediate(t);

        // 자식 없으면 종료
        int count = t.childCount;
        if (count == 0) return;

        bool hasAny = false;
        Bounds bounds = default;

        for (int i = 0; i < count; i++)
        {
            if (!(t.GetChild(i) is RectTransform child)) continue;
            if (!includeInactive && !child.gameObject.activeInHierarchy) continue;

            // 부모(t) 좌표계 기준으로 child의 경계 계산
            var cb = RectTransformUtility.CalculateRelativeRectTransformBounds(t, child);
            if (!hasAny) { bounds = cb; hasAny = true; }
            else bounds.Encapsulate(cb);
        }

        if (!hasAny) return;

        // sizeDelta 갱신 (pivot 영향 없이 크기만 조정)
        var size = t.sizeDelta;
        if (fitHorizontal) size.x = bounds.size.x + padding.x;
        if (fitVertical)   size.y = bounds.size.y + padding.y;
        t.sizeDelta = size;
    }
}