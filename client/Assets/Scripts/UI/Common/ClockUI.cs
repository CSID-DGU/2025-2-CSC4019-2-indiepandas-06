using UnityEngine;
using TMPro;

public class ClockUI : MonoBehaviour {
    [SerializeField] private RectTransform clockHand;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI gemText;

    private void Update() {
        if (EnvironmentManager.Instance == null) { return; }
        UpdateClockUI();

        if (GameManager.Instance == null) { return; }
        UpdateGemUI();
    }

    private void UpdateClockUI() {
        float rawTime = EnvironmentManager.TimeOfDay;
        
        // 소숫점 추출
        float timeFraction = Mathf.Repeat(rawTime, 1.0f);

        // 바늘 회전
        if (clockHand != null) {
            float zRotation = Mathf.Lerp(90f, -90f, timeFraction);
            clockHand.localRotation = Quaternion.Euler(0f, 0f, zRotation);
        }

        // 텍스트 업데이트
        if (timeText != null) {
            UpdateTimeText(timeFraction);
        }
    }

    private void UpdateTimeText(float timeFraction) {
        // 24시간 (=1440분) 으로 변환
        float totalMinutes = timeFraction * 1440f; 
        
        int hours = Mathf.FloorToInt(totalMinutes / 60f);
        int minutes = Mathf.FloorToInt(totalMinutes % 60f);

        // 오전/오후 구분
        string ampm = hours < 12 ? "오전" : "오후";

        int displayHour = hours % 12;
        if (displayHour == 0) {
            displayHour = 12;
        }

        timeText.text = $"{ampm} {displayHour:D2}시 {minutes:D2}분";
    }

    private void UpdateGemUI() {
        if (gemText != null) {
            gemText.text = $"{GameManager.IntValue["Gem"]}";
        }
    }
}