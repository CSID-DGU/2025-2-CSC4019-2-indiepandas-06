using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class EvidenceScreen : ScreenBase, IPointerClickHandler
{
    // ScreenBase 정책
    public override bool IsPrimary => false;
    public override bool IsOverlay => false; // 최상단으로 단독 노출
    public override BackgroundMode BackgroundMode => BackgroundMode.PreserveWithBlur;
    public override InputPolicy InputPolicy => InputPolicy.UIOnly;

    [Header("Refs")]
    [SerializeField] private RectTransform listContainer;
    [SerializeField] private Button listItemPrefab;
    [SerializeField] private Image largeImage;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject listItemSelectedBgPrefab; // 선택 하이라이트(선택)

    [Header("Found Marker")]
    [SerializeField] private RectTransform marker;

    private readonly Dictionary<int, RectTransform> markers = new();

    private DetectiveScenarioSO so;
    private readonly List<int> evidenceIndices = new();
    private readonly HashSet<int> found = new();
    private int currentLocalIdx = -1;

    private int lastHintIdx = -1;

    private Func<bool> shouldAutoClose;
    private Action<int> onFound;
    private Action onClosed;

    // 리스트 아이템(체크/선택 표시) 캐시
    private struct ListItemCtx { public Button btn; public Image img; public GameObject sel; public int local; }
    private readonly List<ListItemCtx> items = new();

    public bool IsClosed { get; private set; }

    private bool autoCloseRunning = false;

    void Awake() {
        if (closeButton) {
            closeButton.onClick.AddListener(CloseSelf);
        }
    }

    // UIManager에서 이걸 호출하도록 설계
    public void Open(
        DetectiveScenarioSO so,
        Action<int> onFound,
        Action onClosed,
        Func<bool> shouldAutoClose)
    {
        //base.Show();

        // 초기화
        this.so = so;
        this.onFound = onFound;
        this.onClosed = onClosed;
        this.shouldAutoClose = shouldAutoClose;
        this.IsClosed = false;

        BuildEvidenceIndexList();
        BuildListUI();
        if (evidenceIndices.Count > 0) {
            SelectLocal(0);
        }
    }

    private void BuildEvidenceIndexList() {
        evidenceIndices.Clear();
        for (int i = 0; i < so.scenes.Count; i++) {
            if (true) { // 증거 있는 이미지만 나오게 하고싶으면 여기 조건을 so.scenes[i].isEvidence로 변경
                evidenceIndices.Add(i);
            }
        }
    }

    private void BuildListUI() {
        items.Clear();
        foreach (Transform c in listContainer) Destroy(c.gameObject);

        for (int local = 0; local < evidenceIndices.Count; local++) {
            int sceneIdx = evidenceIndices[local];
            var btn = Instantiate(listItemPrefab, listContainer);
            var img = btn.GetComponentInChildren<Image>(true);

            if (img) {
                img.sprite = AtlasManager.Instance ? AtlasManager.Instance.Get("Dialogue", so.scenes[sceneIdx].spriteName) : null;
            }

            GameObject sel = null;
            if (listItemSelectedBgPrefab)
            {
                sel = Instantiate(listItemSelectedBgPrefab, btn.transform);
                sel.SetActive(false);
            }

            var ctx = new ListItemCtx { btn = btn, img = img, sel = sel, local = local };
            items.Add(ctx);

            btn.onClick.RemoveAllListeners();

            int localCopy = local;
            btn.onClick.AddListener(() => SelectLocal(localCopy));
        }
        RefreshListVisuals();
    }

    private void RefreshListVisuals() {
        for (int i = 0; i < items.Count; i++) {
            bool isFound = found.Contains(items[i].local);
            bool isSel = (i == currentLocalIdx);

            // 체크/틴트 표시는 자유롭게
            if (items[i].img) {
                items[i].img.color = isFound ? new Color(0.9f, 1f, 0.9f, 1f) : Color.white;
            }

            if (items[i].sel) {
                items[i].sel.SetActive(isSel);
            }
        }
    }

    private void SelectLocal(int localIdx) {
        if (localIdx < 0 || localIdx >= evidenceIndices.Count) {
            return;
        }

        currentLocalIdx = localIdx;
        RefreshListVisuals();

        int sceneIdx = evidenceIndices[localIdx];
        if (largeImage) {
            largeImage.sprite = AtlasManager.Instance ? AtlasManager.Instance.Get("Dialogue", so.scenes[sceneIdx].spriteName) : null;
            bool isFound = found.Contains(items[localIdx].local);
            marker.gameObject.SetActive(isFound);
            Debug.Log($"Found? {isFound}");

            if (isFound) {
                var sc = so.scenes[sceneIdx];
                var point = sc.evidence.normalizedPoint;
                // 마커 위치 설정
                var amin = marker.anchorMin;
                var amax = marker.anchorMax;
                amin.x = point.x;
                amax.x = point.x;
                amin.y = point.y;
                amax.y = point.y;
                marker.anchorMin = amin;
                marker.anchorMax = amax;
            }
            // largeImage.SetNativeSize(); // 필요 시
        }

        if (found.Contains(localIdx)) {
            hintText.text = so.scenes[sceneIdx].evidence.uiHint;
        }
        else {
            hintText.text = "장면을 살펴보고 증거를 찾아보자!";
        }
    }

    // IPointerClickHandler로 안전한 좌표계
    public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData) {
        if (!largeImage || currentLocalIdx < 0) {
            return;
        }

        int sceneIdx = evidenceIndices[currentLocalIdx];
        var sc = so.scenes[sceneIdx];
        if (!sc.isEvidence) {
            ShowMissText();
            return;
        }

        var rt = largeImage.rectTransform;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.position, eventData.pressEventCamera, out var local)) {
            return;
        }

        // 유효 영역(AspectFit 반영) 기반 정규화
        // 이미지 실제 표시영역 계산
        Rect rect = rt.rect;
        Vector2 spriteSize = largeImage.sprite ? (Vector2)largeImage.sprite.rect.size : Vector2.one;
        if (spriteSize.x <= 0 || spriteSize.y <= 0) {
            return;
        }

        float rectAspect = rect.width / rect.height;
        float spriteAspect = spriteSize.x / spriteSize.y;

        Rect drawn; // 실제 스프라이트가 그려지는 로컬 영역
        if (rectAspect > spriteAspect) {
            // 가로 여백 생김
            float h = rect.height;
            float w = h * spriteAspect;
            drawn = new Rect(rect.xMin + (rect.width - w) * 0.5f, rect.yMin, w, h);
        }
        else {
            // 세로 여백 생김
            float w = rect.width;
            float h = w / spriteAspect;
            drawn = new Rect(rect.xMin, rect.yMin + (rect.height - h) * 0.5f, w, h);
        }

        float nx = Mathf.InverseLerp(drawn.xMin, drawn.xMax, local.x);
        float ny = Mathf.InverseLerp(drawn.yMin, drawn.yMax, local.y);
        var p = new Vector2(nx, ny);

        float dist = Vector2.Distance(p, sc.evidence.normalizedPoint);
        if (dist <= sc.evidence.radius) {
            if (!found.Contains(currentLocalIdx)) {
                found.Add(currentLocalIdx);
                hintText.text = sc.evidence.uiHint;
                onFound?.Invoke(currentLocalIdx);
                RefreshListVisuals();
                // for UI Refesh
                SelectLocal(currentLocalIdx);
            }

            if (shouldAutoClose != null) {
                StartCoroutine(CoAutoCloseWhenReady());
            }
        }
        else {
            ShowMissText();
        }
    }

    private void ShowMissText() {
        string[] missHints = { "음… 좀 더 자세히 살펴볼까?", "다른 곳도 유심히 보자!", "음… 거긴 아닌 것 같아" };
        int idx;
        do { idx = UnityEngine.Random.Range(0, missHints.Length); }
        while (missHints.Length > 1 && idx == lastHintIdx);

        lastHintIdx = idx;
        hintText.text = missHints[idx];
    }

    private IEnumerator CoAutoCloseWhenReady() {
        if (autoCloseRunning) { yield break; }
        autoCloseRunning = true;

        // 1) found가 onEnd에서 증가하므로, 그때까지 기다림
        yield return new WaitUntil(() => shouldAutoClose != null && shouldAutoClose());

        // 2) 성공 대사 큐가 모두 빠질 때까지 기다림
        yield return new WaitUntil(() => !UIManager.HasPendingDialogue);

        // (선택) 혹시 선택지/입력/홀드 중이면 그것도 해제될 때까지
        yield return new WaitUntil(() =>
            !UIManager.IsChoiceOpen &&
            !UIManager.IsDialogueInput &&
            !UIManager.IsDialogueHeld
        );

        CloseSelf();
        autoCloseRunning = false;
    }

    public void CloseSelf() {
        if (IsClosed) return;
        IsClosed = true;
        onClosed?.Invoke();
        UIManager.CloseScreen(this);
    }
}
