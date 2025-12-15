using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GroupAlphaEaser : MonoBehaviour
{
    public enum FadeDriver { CanvasGroup, GraphicsAlpha }
    public enum AlphaMode   { Absolute, Multiply }

    [Header("Target")]
    [SerializeField] private LayoutGroup layoutGroup;
    [SerializeField] private FadeDriver driver = FadeDriver.CanvasGroup;
    [SerializeField] private bool includeInactiveChildren = false;

    [Header("Animation")]
    [Range(0f, 1f)] [SerializeField] private float from = 0f;
    [Range(0f, 1f)] [SerializeField] private float to   = 1f;
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private float delay    = 0f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Options")]
    [SerializeField] private bool playOnEnable = true;
    [SerializeField] private bool initializeToFromOnEnable = true;
    [SerializeField] private bool snapToEndOnDisable = false;

    [Tooltip("CanvasGroup 사용 시, alpha==0 근처면 raycast 차단/비활성, alpha>0면 활성")]
    [SerializeField] private bool toggleInteractableByAlpha = true;
    [Range(0f, 1f)] [SerializeField] private float interactableThreshold = 0.001f;

    [Header("Graphics Mode")]
    [SerializeField] private AlphaMode graphicsAlphaMode = AlphaMode.Absolute;
    [Tooltip("GraphicsAlpha 모드에서, 원본 알파를 저장해서 곱/복원")]
    [SerializeField] private bool cacheOriginalAlphas = true;

    CanvasGroup canvasGroup;
    RectTransform rt;
    readonly List<Graphic> graphics = new();
    float[] originalAlphas;

    Coroutine animCo;

    void Reset() {
        if (!layoutGroup) {
            layoutGroup = GetComponent<LayoutGroup>();
        }
    }

    void Awake() {
        if (!layoutGroup) {
            layoutGroup = GetComponent<LayoutGroup>();
        }
        rt = (layoutGroup ? layoutGroup.transform as RectTransform : transform as RectTransform);

        if (driver == FadeDriver.CanvasGroup) {
            canvasGroup = GetComponent<CanvasGroup>();
            if (!canvasGroup) {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        else {
            BuildGraphicList();
        }
    }

    void OnEnable() {
        if (initializeToFromOnEnable) {
            ApplyAlpha(from);
        }

        if (playOnEnable) {
            if (animCo != null) {
                StopCoroutine(animCo);
            }
            animCo = StartCoroutine(CoFade(from, to));
        }
        MarkLayoutDirty();
    }

    void OnDisable() {
        if (animCo != null) {
            StopCoroutine(animCo);
            animCo = null;
        }
        if (snapToEndOnDisable) {
            ApplyAlpha(to);
        }

        MarkLayoutDirty();
    }

    IEnumerator CoFade(float a, float b) {
        if (delay > 0f) {
            float t0 = 0f;
            while (t0 < delay) { t0 += DeltaTime(); yield return null; }
        }

        float t = 0f;
        while (t < duration) {
            t += DeltaTime();
            float p = duration > 0f ? Mathf.Clamp01(t / duration) : 1f;
            float eased = (ease != null) ? ease.Evaluate(p) : p;
            ApplyAlpha(Mathf.LerpUnclamped(a, b, eased));
            MarkLayoutDirty();
            yield return null;
        }

        ApplyAlpha(b);
        MarkLayoutDirty();
        animCo = null;
    }

    float DeltaTime() => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

    // 실제 알파 적용
    void ApplyAlpha(float alpha) {
        alpha = Mathf.Clamp01(alpha);

        if (driver == FadeDriver.CanvasGroup) {
            if (!canvasGroup) {
                canvasGroup = GetComponent<CanvasGroup>();
                if (!canvasGroup) {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
            canvasGroup.alpha = alpha;

            if (toggleInteractableByAlpha) {
                bool on = alpha > interactableThreshold;
                canvasGroup.interactable   = on;
                canvasGroup.blocksRaycasts = on;
            }
        }
        else {
            // 그래픽 목록이 동적으로 바뀔 수 있으면 필요 시 빌드
            if (graphics.Count == 0 || (cacheOriginalAlphas && (originalAlphas == null || originalAlphas.Length != graphics.Count))) {
                BuildGraphicList();
            }

            for (int i = 0; i < graphics.Count; i++) {
                var g = graphics[i];
                if (!g) {
                    continue;
                }

                var c = g.color;
                if (graphicsAlphaMode == AlphaMode.Absolute) {
                    // 절대 알파로 덮어쓰기 (원본 알파는 무시)
                    c.a = alpha;
                }
                else {
                    // Multiply
                    float baseA = cacheOriginalAlphas ? originalAlphas[i] : c.a;
                    c.a = baseA * alpha;
                }
                g.color = c;
            }
        }
    }

    void BuildGraphicList() {
        graphics.Clear();
        if (!layoutGroup) {
            return;
        }

        // 레이아웃 그룹 하위의 모든 Graphic 수집
        layoutGroup.GetComponentsInChildren(includeInactiveChildren, graphics);

        if (cacheOriginalAlphas) {
            originalAlphas = new float[graphics.Count];
            for (int i = 0; i < graphics.Count; i++) {
                originalAlphas[i] = graphics[i] ? graphics[i].color.a : 1f;
            }
        }
        else originalAlphas = null;
    }

    void MarkLayoutDirty() {
        if (rt && rt.parent) {
            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)rt.parent);
        }
    }

    // Public controls
    public void PlayForward() {
        if (animCo != null) {
            StopCoroutine(animCo);
        }
        animCo = StartCoroutine(CoFade(from, to));
    }

    public void PlayReverse() {
        if (animCo != null) {
            StopCoroutine(animCo);
        }
        animCo = StartCoroutine(CoFade(to, from));
    }

    public void StopAnimation() {
        if (animCo != null) { 
            StopCoroutine(animCo);
            animCo = null;
        }
    }
}