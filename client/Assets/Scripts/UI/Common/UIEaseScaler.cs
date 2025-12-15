using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class UIEaseScaler : MonoBehaviour {
    [Header("Target")]
    [SerializeField] private RectTransform target;

    [Header("Scale")]
    [SerializeField] private float fromScale = 0f; 
    [SerializeField] private float toScale = 1f;

    [Header("Timing")]
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private float delay = 0f;
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Curve")]
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Options")]
    [SerializeField] private bool resetOnDisable = true;
    [SerializeField] private bool playOnEnable = true;

    private Coroutine co;

    void Awake() {
        if (!target) {
            target = transform as RectTransform;
        }

        if (!target) {
            enabled = false;
        }
    }

    void OnEnable() {
        if (!playOnEnable) { return; }

        if (co != null) { StopCoroutine(co); }

        SetScale(fromScale);
        co = StartCoroutine(CoPlay(fromScale, toScale, duration));
    }

    void OnDisable() {
        if (co != null) { StopCoroutine(co); co = null; }
        if (resetOnDisable) {
            SetScale(fromScale);
        }
    }

    public void Play() {
        if (!gameObject.activeInHierarchy) { return; }

        if (co != null) { StopCoroutine(co); }

        SetScale(fromScale);
        co = StartCoroutine(CoPlay(fromScale, toScale, duration));
    }

    public void PlayReverse() {
        if (!gameObject.activeInHierarchy) { return; }

        if (co != null) { StopCoroutine(co); }

        SetScale(toScale);
        co = StartCoroutine(CoPlay(toScale, fromScale, duration));
    }

    private IEnumerator CoPlay(float from, float to, float dur) {
        float t = 0f;
        while (t < dur) {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float n = dur > 0f ? Mathf.Clamp01(t / dur) : 1f;
            float e = ease != null ? ease.Evaluate(n) : n;
            float s = Mathf.LerpUnclamped(from, to, e);
            SetScale(s);
            yield return null;
        }
        SetScale(to);
        co = null;
    }

    private void SetScale(float s) {
        if (!target) { return; }
        target.localScale = new Vector3(s, s, 1f);
    }
}
