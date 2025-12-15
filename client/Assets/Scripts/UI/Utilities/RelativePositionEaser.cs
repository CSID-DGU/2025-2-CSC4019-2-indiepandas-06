using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RelativePositionEaser : MonoBehaviour {
    public enum SpaceMode { RectAnchored2D, Local3D }
    public enum Axis2D { Both, XOnly, YOnly }

    [Header("Target")]
    [SerializeField] private SpaceMode spaceMode = SpaceMode.RectAnchored2D;
    [SerializeField] private Axis2D axis2D = Axis2D.Both;
    [SerializeField] private RectTransform rect;
    [SerializeField] private Transform target;

    [Header("Animation")]
    [SerializeField] private Vector2 from2D = Vector2.zero;
    [SerializeField] private Vector2 to2D   = Vector2.one;
    [SerializeField] private Vector3 from3D = Vector3.zero;
    [SerializeField] private Vector3 to3D   = Vector3.one;
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private float delay = 0f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Options")]
    [Tooltip("OnEnable 시 from 값으로 세팅 후 애니메이션 시작")]
    [SerializeField] private bool initializeToFromOnEnable = true;
    [Tooltip("Disable 시 현재 값을 to로 스냅")]
    [SerializeField] private bool snapToEndOnDisable = false;
    [Tooltip("OnEnable에서 자동 재생")]
    [SerializeField] private bool playOnEnable = true;

    Coroutine animCo;

    void Reset() {
        if (!rect) {
            rect = GetComponent<RectTransform>();
        }
        if (!target) {
            target = transform;
        }
    }

    void Awake() {
        if (!rect) {
            rect = GetComponent<RectTransform>();
        }
        if (!target) {
            target = transform;
        }
    }

    void OnEnable() {
        if (initializeToFromOnEnable) {
            SetPos(0f);
        }

        if (playOnEnable) {
            if (animCo != null) {
                StopCoroutine(animCo);
            }
            animCo = StartCoroutine(CoAnimate(true));
        }
        MarkLayoutDirtyIfNeeded();
    }

    void OnDisable() {
        if (animCo != null) {
            StopCoroutine(animCo);
            animCo = null;
        }
        if (snapToEndOnDisable) {
            SetPos(1f);
        }

        MarkLayoutDirtyIfNeeded();
    }

    IEnumerator CoAnimate(bool forward) {
        if (delay > 0f) {
            float t = 0f;
            while (t < delay) { t += DeltaTime(); yield return null; }
        }

        float time = 0f;
        while (time < duration) {
            time += DeltaTime();
            float p = duration > 0f ? Mathf.Clamp01(time / duration) : 1f;
            float eased = ease != null ? ease.Evaluate(p) : p;
            SetPos(forward ? eased : 1f - eased);
            MarkLayoutDirtyIfNeeded();
            yield return null;
        }

        SetPos(forward ? 1f : 0f);
        MarkLayoutDirtyIfNeeded();
        animCo = null;
    }

    float DeltaTime() => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

    // 시간 비율 기반
    void SetPos(float t) {
        if (spaceMode == SpaceMode.RectAnchored2D) {
            if (!rect) rect = GetComponent<RectTransform>();
            Vector2 cur = Vector2.LerpUnclamped(from2D, to2D, t);

            switch (axis2D) {
                case Axis2D.XOnly:
                    rect.anchoredPosition = new Vector2(cur.x, rect.anchoredPosition.y);
                    break;
                case Axis2D.YOnly:
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, cur.y);
                    break;
                default:
                    rect.anchoredPosition = cur;
                    break;
            }
        }
        else {
            if (!target) target = transform;
            Vector3 cur = Vector3.LerpUnclamped(from3D, to3D, t);
            target.localPosition = cur;
        }
    }

    void MarkLayoutDirtyIfNeeded() {
        if (spaceMode != SpaceMode.RectAnchored2D) { return; }
        if (!rect) {
            rect = GetComponent<RectTransform>();
        }
        if (rect && rect.parent) {
            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)rect.parent);
        }
    }

    public void PlayForward() {
        if (animCo != null) {
            StopCoroutine(animCo);
        }
        animCo = StartCoroutine(CoAnimate(true));
    }

    public void PlayReverse() {
        if (animCo != null) {
            StopCoroutine(animCo);
        }
        animCo = StartCoroutine(CoAnimate(false));
    }

    public void Stop() {
        if (animCo != null) {
            StopCoroutine(animCo);
            animCo = null;
        }
    }
}