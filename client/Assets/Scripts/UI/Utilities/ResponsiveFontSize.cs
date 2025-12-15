using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResponsiveFontByWidth : MonoBehaviour {
    [Header("Target (둘 중 하나만 채우면 됨)")]
    [SerializeField] private TextMeshProUGUI tmpLabel;
    [SerializeField] private Text uGUIText;

    [Header("Base")]
    [Tooltip("디자인/프리뷰에서의 기준 너비(px)")]
    [SerializeField] private float baseWidth = 864f;
    [Tooltip("디자인/프리뷰에서의 기준 폰트 크기")]
    [SerializeField] private float baseFontSize = 88f;

    [Header("Measure")]
    [Tooltip("라벨 Rect의 현재 너비를 기준으로 할지, 화면 너비를 기준으로 할지")]
    [SerializeField] private bool useRectWidth = true;

    [Tooltip("폰트 크기 최소/최대 클램프 (0이면 무시)")]
    [SerializeField] private int minFontSize = 12;
    [SerializeField] private int maxFontSize = 200;

    [Header("Update")]
    [Tooltip("Rect 크기 변경 시 자동 갱신")]
    [SerializeField] private bool updateOnRectChange = true;
    [Tooltip("매 프레임 갱신(동적 리사이즈가 잦을 때만)")]
    [SerializeField] private bool updateEveryFrame = false;

    RectTransform rt;
    Vector2 lastSize;
    int lastAppliedSize = -1;

    private void Reset() {
        rt = GetComponent<RectTransform>();
        if (!tmpLabel) {
            tmpLabel = GetComponent<TextMeshProUGUI>();
        }
        if (!uGUIText) {
            uGUIText = GetComponent<Text>();
        }
    }

    private void Awake() {
        if (!rt) {
            rt = GetComponent<RectTransform>();
        }
    }

    private void OnEnable() {
        RefreshLabelFont();
        lastSize = GetCurrentRectSize();
    }

    void Update() {
        if (updateEveryFrame) {
            RefreshLabelFont();
            return;
        }

        if (updateOnRectChange) {
            var s = GetCurrentRectSize();
            if (!Mathf.Approximately(s.x, lastSize.x) || !Mathf.Approximately(s.y, lastSize.y)) {
                lastSize = s;
                RefreshLabelFont();
            }
        }
    }

    Vector2 GetCurrentRectSize() {
        if (rt) return rt.rect.size;
        return Vector2.zero;
    }

    public void RefreshLabelFont() {
        // 대상 체크
        if (tmpLabel == null && uGUIText == null) return;

        // 측정 너비 결정
        float currentWidth = useRectWidth
            ? (rt ? rt.rect.width : 0f)
            : (float)UnityEngine.Screen.width;

        if (currentWidth <= 0f || baseWidth <= 0f) return;

        // 비율 계산
        float ratio = currentWidth / baseWidth;
        int targetSize = Mathf.RoundToInt(baseFontSize * ratio);

        // 클램프
        if (minFontSize > 0) targetSize = Mathf.Max(targetSize, minFontSize);
        if (maxFontSize > 0) targetSize = Mathf.Min(targetSize, maxFontSize);

        // 같은 값이면 스킵
        if (targetSize == lastAppliedSize) return;

        // 적용
        if (tmpLabel) {
            tmpLabel.fontSize = targetSize;
        }
        if (uGUIText) {
            uGUIText.fontSize = targetSize;
        }
        lastAppliedSize = targetSize;
    }
}