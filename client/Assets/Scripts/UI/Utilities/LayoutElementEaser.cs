using UnityEngine;

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LayoutElementEaser : MonoBehaviour {
    public enum Axis { FlexibleWidth, FlexibleHeight }

    [Header("Target")]
    [SerializeField] private Axis axis = Axis.FlexibleHeight;
    [SerializeField] private LayoutElement layout;  // 자동 할당됨

    [Header("Animation")]
    [SerializeField] private float from = 0f;
    [SerializeField] private float to = 1f;
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private float delay = 0f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Options")]
    [Tooltip("OnEnable에서 from 값으로 초기화 후 애니메이션 시작")]
    [SerializeField] private bool initializeToFromOnEnable = true;
    [Tooltip("Disable 시 현재 값을 to로 스냅(중간에 꺼져도 깔끔히 마무리)")]
    [SerializeField] private bool snapToEndOnDisable = false;

    Coroutine animCo;
    RectTransform rt;

    void Reset() {
        layout = GetComponent<LayoutElement>();
    }

    void Awake() {
        if (!layout) { layout = GetComponent<LayoutElement>(); }
        rt = GetComponent<RectTransform>();
    }

    void OnEnable() {
        if (!layout) layout = GetComponent<LayoutElement>();

        if (initializeToFromOnEnable) {
            SetFlexible(from);
        }

        if (animCo != null) {
            StopCoroutine(animCo);
        }
        animCo = StartCoroutine(CoAnimate());
    }

    void OnDisable() {
        if (animCo != null) {
            StopCoroutine(animCo);
            animCo = null;
        }
        if (snapToEndOnDisable) {
            SetFlexible(to);
        }
        MarkLayoutDirty();
    }

    IEnumerator CoAnimate() {
        if (delay > 0f) {
            float t = 0f;
            while (t < delay) {
                t += DeltaTime();
                yield return null;
            }
        }

        float time = 0f;
        while (time < duration) {
            time += DeltaTime();
            float p = duration > 0f ? Mathf.Clamp01(time / duration) : 1f;
            float eased = ease != null ? ease.Evaluate(p) : p;
            SetFlexible(Mathf.LerpUnclamped(from, to, eased));
            MarkLayoutDirty();
            yield return null;
        }

        // 보정
        SetFlexible(to);
        MarkLayoutDirty();
        animCo = null;
    }

    float DeltaTime() => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

    void SetFlexible(float value) {
        switch (axis) {
            case Axis.FlexibleWidth:
                layout.flexibleWidth = value;
                break;
            case Axis.FlexibleHeight:
                layout.flexibleHeight = value;
                break;
        }
    }

    void MarkLayoutDirty() {
        if (!rt) {
            rt = GetComponent<RectTransform>();
        }
        if (rt && rt.parent) {
            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)rt.parent);
        }
    }

}