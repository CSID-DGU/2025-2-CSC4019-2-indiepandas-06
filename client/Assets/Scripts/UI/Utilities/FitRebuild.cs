using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FitRebuild : MonoBehaviour {
    public RectTransform target;
    public float delay = 0f;

    void OnEnable() {
        StartCoroutine(DelayUpdate());
    }

    private IEnumerator DelayUpdate() {
        yield return new WaitForSeconds(delay);
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(target);
    }
}