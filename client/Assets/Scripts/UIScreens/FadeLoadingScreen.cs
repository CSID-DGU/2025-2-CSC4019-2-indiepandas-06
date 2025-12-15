using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Game Screen")]
public class FadeLoadingScreen : ScreenBase 
{
    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float stayDuration = 0.2f;
    [SerializeField] private Color fadeColor = Color.black;

    private Color currentColor;

    public override bool IsPrimary => false;
    public override bool IsOverlay => true;
    public override BackgroundMode BackgroundMode => BackgroundMode.Preserve;

    protected void Awake() {
        if (fadeImage != null) {
            currentColor = fadeColor;
            currentColor.a = 0f;
            fadeImage.color = currentColor;
        }
    }

    public void SetFadeColor(Color color) {
        fadeColor = color;
        if (fadeImage != null) {
            currentColor = color;
            currentColor.a = fadeImage.color.a;
            fadeImage.color = currentColor;
        }
    }

    public async UniTask Play(Color color, Func<UniTask> midAction = null) {
        SetFadeColor(color);

        // 페이드 인
        await Fade(0f, 1f);

        // 잠깐 유지
        await UniTask.Delay(TimeSpan.FromSeconds(stayDuration), ignoreTimeScale: true);

        // 중간 액션 실행
        if (midAction != null)
            await midAction();

        // 페이드 아웃
        await Fade(1f, 0f);

        // 닫기
        UIManager.CloseScreen(this);
    }

    private async UniTask Fade(float from, float to) {
        if (fadeImage == null) return;

        float t = 0f;
        Color c = fadeColor;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = Mathf.Clamp01(t / fadeDuration);
            c.a = Mathf.Lerp(from, to, lerp);
            fadeImage.color = c;
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        c.a = to;
        fadeImage.color = c;
    }
}
