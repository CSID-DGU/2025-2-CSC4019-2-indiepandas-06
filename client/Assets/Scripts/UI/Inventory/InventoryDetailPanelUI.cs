using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class InventoryDetailPanelUI : MonoBehaviour {
    [Header("참조")]
    [SerializeField] private GameObject InventoryScreen;
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private Image iconImage;

    private Coroutine hideCo;
    private GroupAlphaEaser alphaEaser;
    private RelativePositionEaser positionEaser;

    private void Reset() {
        if (!root) {
            root = gameObject;
        }
        if (!nameText) {
            nameText = GetComponentInChildren<TextMeshProUGUI>();
        }
        if (!iconImage) {
            iconImage = GetComponentInChildren<Image>();
        }
    }

    private void Awake() {
        if (root) {
            alphaEaser = root.GetComponent<GroupAlphaEaser>();
            positionEaser = root.GetComponent<RelativePositionEaser>();
        }
    }

    private void OnEnable() {
        if (InventoryManager.Instance != null) {
            InventoryManager.Instance.OnInventoryChanged += Refresh;
        }
        Refresh();
    }

    private void OnDisable() {
        if (InventoryManager.Instance != null) {
            InventoryManager.Instance.OnInventoryChanged -= Refresh;
        }
    }

    public void Refresh() {
        if (InventoryScreen == null || !InventoryScreen.activeSelf) {
            return;
        }
        if (InventoryManager.Instance == null) {
            SetEmpty();
            return;
        }

        var inv = InventoryManager.Instance.CurrentInventory;
        int idx = InventoryManager.Instance.CurrentSlotIndex;

        ItemBase item = inv?.GetItemAt(idx);
        bool hasItem = item != null && item.ItemData != null && item.Count > 0;

        if (!hasItem) {
            PlayEmpty();
            return;
        }

        // UI 채우기
        if (nameText) {
            nameText.text = item.ItemData.ItemName ?? "";
        }
        if (descText) {
            descText.text = item.ItemData.ItemDescription ?? "";
        }
        if (iconImage) {
            iconImage.sprite = item.ItemData.HoldingSprite;
            iconImage.enabled = (iconImage.sprite != null);
            iconImage.preserveAspect = true;
        }

        if (root) {
            root.SetActive(true);
        }
    }

    private void PlayEmpty() {
        if (hideCo != null) {
            StopCoroutine(hideCo);
        }
        hideCo = StartCoroutine(CoDelayedHide());
        if (root.activeSelf) {
            alphaEaser.PlayReverse();
            positionEaser.PlayReverse();
        }
    }

    private IEnumerator CoDelayedHide() {
        // 대기
        float t = 0f;
        while (t < 0.3f) {
            t += Time.unscaledDeltaTime; // UI라면 언스케일 권장
            yield return null;
        }

        // 최종 숨김
        SetEmpty();
        hideCo = null;
    }

    private void SetEmpty() {
        if (nameText) {
            nameText.text = "";
        }
        if (descText) {
            descText.text = "";
        }
        if (iconImage) {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
        if (root) {
            root.SetActive(false);
        }
    }

}
