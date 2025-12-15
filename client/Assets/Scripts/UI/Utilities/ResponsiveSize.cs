using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class ResponsiveSize : MonoBehaviour {
    [SerializeField]
    [Header("기준이 되는 Rect")]
    private RectTransform targetRect;
    [Header("기준이 되는 Rect의 너비")]
    [SerializeField] private float referenceWidthOfTarget = 100f;
    [Header("그 너비일 때 이상적인 크기")]
    [SerializeField] private float referenceMyWidth = 100f;
    [SerializeField] private float referenceMyHeight = 100f;

    private RectTransform rect;

    private void Awake() {
        rect = GetComponent<RectTransform>();
    }

    private void Update() {
        if (!targetRect || !rect) { return; }

        float currentWidth = Mathf.Abs(targetRect.rect.width);
        if (referenceWidthOfTarget <= 0f) { return; }

        // 비율 계산
        float ratio = currentWidth / referenceWidthOfTarget;

        // 크기 조정
        rect.sizeDelta = new Vector2(referenceMyWidth * ratio, referenceMyHeight * ratio);
    }
}