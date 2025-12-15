using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DiaryDetailUI : MonoBehaviour {
    
    [SerializeField] private TMP_InputField inputTitle;
    [SerializeField] private TMP_InputField inputSituation;
    [SerializeField] private TMP_InputField inputFact;
    [SerializeField] private TMP_InputField inputEmotion;
    [SerializeField] private TMP_InputField inputThought;
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private GameObject detailUIParent;
    [SerializeField] private Button btnDelete;

    private DiaryEntry bound;
    private bool dirty;
    private bool suppressEvents;

    public string BoundId => bound?.id;

    void Awake() {
        HookChange(inputTitle);
        HookChange(inputSituation);
        HookChange(inputFact);
        HookChange(inputEmotion);
        HookChange(inputThought);

        if (btnDelete) { btnDelete.onClick.AddListener(DeleteNow); }

        SetDirty(false);
        detailUIParent.SetActive(false);
    }

    void HookChange(TMP_InputField f) {
        if (!f) {
            return;
        }
        f.onValueChanged.AddListener(_ => { if (!suppressEvents) SetDirty(true); });
        f.onEndEdit.AddListener(_ => TryAutoSave());
        f.onDeselect.AddListener(_ => TryAutoSave());
    }

    void SetDirty(bool v) {
        dirty = v;
    }

    public void ClearView() {
        suppressEvents = true;
        bound = null;
        if (inputTitle) { inputTitle.text = ""; }
        if (inputSituation) { inputSituation.text = ""; }
        if (inputFact) { inputFact.text =inputEmotion.text = ""; }
        if (inputThought) { inputThought.text = ""; }
        suppressEvents = false;

        SetDirty(false);
        detailUIParent.SetActive(false);
    }

    public void Bind(DiaryEntry e) {
        bound = e;
        if (e == null) {
            ClearView();
            return;
        }

        suppressEvents = true;
        if (inputTitle) { inputTitle.text = e.title ?? ""; }
        if (inputSituation) { inputSituation.text = e.situation ?? ""; }
        if (inputFact) { inputFact.text = e.fact ?? ""; }
        if (inputEmotion) { inputEmotion.text = e.emotion ?? ""; }
        if (inputThought) { inputThought.text = e.thought ?? ""; }
        if (dateText) { dateText.text = e.dateIso ?? ""; }
        suppressEvents = false;

        SetDirty(false);
        detailUIParent.SetActive(true);
    }

    void TryAutoSave() {
        if (suppressEvents || bound == null) { return; }
        if (dirty) { SaveNow(); }
    }

    void SaveNow() {
        if (bound == null) { return;}
        DiaryManager.Instance.UpdateEntry(bound.id, e => {
            e.title = inputTitle ? inputTitle.text : e.title;
            e.situation = inputSituation? inputSituation.text : e.situation;
            e.fact = inputFact ? inputFact.text : e.fact;
            e.emotion = inputEmotion ? inputEmotion.text : e.emotion;
            e.thought = inputThought ? inputThought.text : e.thought;
        });
        DiaryManager.Instance.SortByDateThenCreatedAsc();
        DiaryManager.Instance.SaveAllNow();
        SetDirty(false);
        // 목록 새로고침 요청
        onRequestRefreshList?.Invoke();
    }

    void DeleteNow() {
        if (bound == null) return;
        if (DiaryManager.Instance.Delete(bound.id)) {
            DiaryManager.Instance.SaveAllNow();
            ClearView();
            onRequestRefreshList?.Invoke();
        }
    }

    static string FormatLocal(string iso, string fallback = "") {
        if (string.IsNullOrEmpty(iso)) { return fallback; }
        if (DateTime.TryParse(iso, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var utc)) {
            var local = utc.ToLocalTime();
            return local.ToString("yyyy.MM.dd HH:mm");
        }
        return fallback;
    }

    private Action onRequestRefreshList;
    public void SetRefreshCallback(Action a) => onRequestRefreshList = a;
}
