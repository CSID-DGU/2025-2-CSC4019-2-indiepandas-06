using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class ResponsivePPU : MonoBehaviour {
    [SerializeField]
    [Header("기준이 되는 Rect")]
    private RectTransform targetRect;
    [Header("기준이 되는 Rect의 너비")]
    [SerializeField] private float referenceWidth = 100f;
    [Header("그 너비일 때 이상적인 PPUM")]
    [SerializeField] private float referencePPUM = 100f;

    private Image image;

    private void Awake() {
        image = GetComponent<Image>();
    }

    private void Update() {
        if (!targetRect || !image) { return; }

        float currentWidth = Mathf.Abs(targetRect.rect.width);
        if (referenceWidth <= 0f) { return; }

        // 비율 계산
        float ratio = currentWidth / referenceWidth;

        // PPUM 조정
        image.pixelsPerUnitMultiplier = referencePPUM / ratio;
    }
}