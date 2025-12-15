using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

public enum ToastIconType {
    None = 0,
    QuestStart,
    MissionComplete,
    MissionStart,
    QuestComplete,
}

[Serializable]
public struct IconPair {
    public ToastIconType type;
    public Sprite sprite;
}

public class ToastScreen : ScreenBase {
    [Header("Refs")]
    [SerializeField] private CanvasGroup toastGroup;
    [SerializeField] private GameObject iconObject;
    [SerializeField] private RectTransform backImageTransform;
    [SerializeField] private RectTransform bodyTransform;
    [SerializeField] private CanvasGroup bodyGroup;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Easing")]
    [SerializeField] private AnimationCurve iconScaleEase = AnimationCurve.EaseInOut(0,0,1,1);
    [SerializeField] private AnimationCurve bodyScaleEase = AnimationCurve.EaseInOut(0,0,1,1);
    [SerializeField] private AnimationCurve bodyAlphaEase = AnimationCurve.EaseInOut(0,0,1,1);

    [Header("Icon Library")]
    [SerializeField] private List<IconPair> iconPairs = new();
    [SerializeField] private Sprite fallbackSprite;
    private Dictionary<ToastIconType, Sprite> iconDict;

    public event Action OnClosed;
    private bool isPlaying;
    public bool IsPlaying => isPlaying;
    private bool closedNotified;


    // 고정 파라미터
    private const float ICON_DURATION = 0.4f;
    private const float BODY_DURATION = 0.4f;
    private const float DISPLAY_DURATION = 3f;
    private const float FADEOUT_DURATION = 0.5f;

    private static readonly Vector3 ICON_SCALE_START = Vector3.zero;
    private static readonly Vector3 ICON_SCALE_END = Vector3.one;

    private Coroutine playCo;

    public override bool IsPrimary => false;
    public override bool IsOverlay => true;
    public override BackgroundMode BackgroundMode => BackgroundMode.Scene;
    public override InputPolicy InputPolicy => InputPolicy.Both;

    protected void Awake() {
        //base.Awake();
        // 딕셔너리 생성
        iconDict = new(iconPairs.Count);
        foreach (var p in iconPairs) {
            if (!iconDict.ContainsKey(p.type) && p.sprite != null) {
                iconDict.Add(p.type, p.sprite);
            }
        }

        // 초기 상태
        if (iconObject) {
            iconObject.transform.localScale = Vector3.zero;
        }
        if (bodyGroup) {
            bodyGroup.alpha = 0f;
            bodyGroup.transform.localScale = new Vector3(0f, 1f, 1f);
        }
        if (toastGroup) {
            toastGroup.alpha = 1f;
        }
    }

    public void SetContent(ToastIconType iconType, string title, string body) {
        Sprite icon = null;
        if (iconDict != null && iconDict.TryGetValue(iconType, out var found)) {
            icon = found;
        } else {
            icon = fallbackSprite; // 없으면 폴백 쓰거나 null 허용
        }
        ApplyIcon(icon);
        ApplyTexts(title, body);
    }

    private void ApplyIcon(Sprite icon) {
        if (!iconImage || !iconObject) return;
        if (icon != null) {
            iconImage.sprite = icon;
            iconObject.SetActive(true);
        } else {
            // 아이콘 없는 토스트
            iconObject.SetActive(false);
        }
    }

    private void ApplyTexts(string title, string body) {
        if (titleText) { titleText.text = title ?? string.Empty; }
        if (messageText) { messageText.text = body ?? string.Empty; }
    }

    public override void Show() {
        base.Show();
        closedNotified = false;
        isPlaying = false;
    }

    public void PlayAnim() {
        closedNotified = false;
        PrepareInitialState();
        if (playCo != null) {
            StopCoroutine(playCo);
        }
        playCo = StartCoroutine(PlaySequence());
    }

    public override void Hide() {
        Debug.Log("Toast Hide!");
        if (playCo != null) {
            StopCoroutine(playCo);
            playCo = null;
        }
        isPlaying = false;
        SafeNotifyClose();
        base.Hide();
    }

    void OnEnable() {
        closedNotified = false;
        isPlaying = false;
    }

    /*
    private void OnDisable() {
        isPlaying = false;
        SafeNotifyClose();
    }

    private void OnDestroy() {
        isPlaying = false;
        SafeNotifyClose();
    }
    */

    private void SafeNotifyClose() {
        if (closedNotified) { return; }
        closedNotified = true;
        try { OnClosed?.Invoke(); } catch {}
    }

    private void PrepareInitialState() {
        if (toastGroup) {
            toastGroup.alpha = 1f;
            toastGroup.interactable = false;
            toastGroup.blocksRaycasts = false;
        }

        if (iconObject) {
            var rt = iconObject.transform as RectTransform;
            if (rt) rt.localScale = ICON_SCALE_START;
        }

        if (backImageTransform) { backImageTransform.localScale = new Vector3(0f, 1f, 1f); }
        if (bodyTransform) { bodyTransform.localScale = new Vector3(0f, 1f, 1f); }
        if (bodyGroup) { bodyGroup.alpha = 0f; }
    }

    private IEnumerator PlaySequence() {
        isPlaying = true;

        // 아이콘 스케일 애니메이션
        if (iconObject) {
            float t = 0f;
            var rt = iconObject.transform as RectTransform;
            while (t < ICON_DURATION) {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / ICON_DURATION);
                float e = iconScaleEase.Evaluate(u);
                if (rt) { rt.localScale = Vector3.LerpUnclamped(ICON_SCALE_START, ICON_SCALE_END, e); }
                yield return null;
            }
            if (rt) { rt.localScale = ICON_SCALE_END; }
        }

        // 본문 X 스케일 + 알파 애니메이션
        if (bodyTransform || bodyGroup) {
            float t = 0f;
            while (t < BODY_DURATION) {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / BODY_DURATION);

                float sx = bodyScaleEase.Evaluate(u);
                float a = bodyAlphaEase.Evaluate(u);

                if (backImageTransform) { backImageTransform.localScale = new Vector3(sx, 1f, 1f); }
                if (bodyTransform) { bodyTransform.localScale = new Vector3(sx, 1f, 1f); }
                if (bodyGroup) { bodyGroup.alpha = a; }

                yield return null;
            }
            if (backImageTransform) { backImageTransform.localScale = Vector3.one; }
            if (bodyTransform) { bodyTransform.localScale = Vector3.one; }
            if (bodyGroup) { bodyGroup.alpha = 1f; }
        }

        // 표시 유지
        yield return new WaitForSecondsRealtime(DISPLAY_DURATION);

        // 전체 페이드아웃
        if (toastGroup) {
            float t = 0f;
            float startA = toastGroup.alpha;
            while (t < FADEOUT_DURATION) {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / FADEOUT_DURATION);
                toastGroup.alpha = Mathf.Lerp(startA, 0f, u);
                yield return null;
            }
            toastGroup.alpha = 0f;
        }

        // 토스트 종료
        isPlaying = false;
        SafeNotifyClose();
        playCo = null;
        //UIManager.CloseScreen(this);
        //gameObject.SetActive(false);
    }
}