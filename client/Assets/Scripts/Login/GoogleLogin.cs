using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using UnityEngine;
using UnityEngine.EventSystems;

public enum SignInFailure {
    UserCanceled,
    Timeout,
    Network,
    ProviderMisconfigured,
    InvalidCredentials,
    AlreadyLinked,
    Unknown
}

public class GoogleLogin : MonoBehaviour, IPointerClickHandler {
    public event Action OnLoginSucceeded;
    public event Action<SignInFailure, string> OnLoginFailed;

    private int timeoutSeconds = 90;
    private bool isBusy;

    async void Awake() {
        await UnityServices.InitializeAsync();

        // UPA 브라우저 로그인 성공 시 콜백 등록
        PlayerAccountService.Instance.SignedIn += OnUpaSignedIn;

        OnLoginFailed += ShowErrorLog;
    }

    private void Start() {
        // StartUpaSignIn();
    }

    public void OnPointerClick(PointerEventData eventData) {
        StartUpaSignIn();
    }

    // UPA 로그인 시작(구글/애플/이메일 등 선택 UI는 UPA가 제공)
    public async void StartUpaSignIn() {
        if (isBusy) { return; }
        isBusy = true;

        try {
            if (PlayerAccountService.Instance.IsSignedIn) {
                await SignInWithUnityAuth();      // 이미 UPA 인증됨 → UGS 마무리
                return;
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            var upaTask = PlayerAccountService.Instance.StartSignInAsync();
            var completed = await Task.WhenAny(upaTask, Task.Delay(Timeout.Infinite, cts.Token)) == upaTask;

            if (!completed) {
                OnLoginFailed?.Invoke(SignInFailure.Timeout, "로그인 대기 시간이 초과되었어요. 다시 시도해 주세요.");
                return;
            }

            await upaTask;
        }
        catch (Exception e) {
            HandleUpaStartSignInError(e);
        }
        finally {
            isBusy = false;
        }
    }

    
    private async void OnUpaSignedIn() {
        await SignInWithUnityAuth();
    }

    private async Task SignInWithUnityAuth() {
        try {
            var accessToken = PlayerAccountService.Instance.AccessToken;
            await AuthenticationService.Instance.SignInWithUnityAsync(accessToken);
            // AuthenticationService.Instance.PlayerId에 플레이어 아이디 들어가있음
            Debug.Log("UGS 인증 성공. PlayerID = " + AuthenticationService.Instance.PlayerId);
        }
        catch (System.Exception e) {
            Debug.LogException(e);
        }
    }

    private void HandleUpaStartSignInError(Exception e) {
        var message = e.Message ?? string.Empty;

        if (message.IndexOf("Cancel", StringComparison.OrdinalIgnoreCase) >= 0) {
            OnLoginFailed?.Invoke(SignInFailure.UserCanceled, "로그인이 취소되었어요.");
        }
        else if (message.IndexOf("network", StringComparison.OrdinalIgnoreCase) >= 0 || message.IndexOf("transport", StringComparison.OrdinalIgnoreCase) >= 0) {
            OnLoginFailed?.Invoke(SignInFailure.Network, "네트워크 연결을 확인하고 다시 시도해 주세요.");
        }
        else {
            OnLoginFailed?.Invoke(SignInFailure.Unknown, "로그인 창을 열지 못했어요. 다시 시도해 주세요.");
        }

        Debug.LogException(e);
    }

    private (SignInFailure reason, string msg) MapAuthException(AuthenticationException ex) {
        // 메시지 기반
        var m = ex.Message?.ToLowerInvariant() ?? string.Empty;

        if (m.Contains("invalid") && m.Contains("token")) {
            return (SignInFailure.InvalidCredentials, "로그인 토큰이 유효하지 않아요. 다시 로그인해 주세요.");
        }
        if (m.Contains("link") && m.Contains("already")) {
            return (SignInFailure.AlreadyLinked, "이미 다른 계정에 연결된 로그인 방식이에요.");
        }
        if (m.Contains("configuration") || m.Contains("redirect") || m.Contains("client id") || m.Contains("401")) {
            return (SignInFailure.ProviderMisconfigured, "로그인 제공자 설정에 문제가 있어요(클라이언트ID/리디렉트).");
        }

        return (SignInFailure.Unknown, "인증 서버에서 요청을 처리하지 못했어요. 잠시 후 다시 시도해 주세요.");
    }

    private (SignInFailure reason, string msg) MapRequestFailed(RequestFailedException ex) {
        var m = ex.Message?.ToLowerInvariant() ?? string.Empty;

        if (m.Contains("cancel")) {
            return (SignInFailure.UserCanceled, "로그인이 취소되었어요.");
        }
        if (m.Contains("network") || m.Contains("transport") || m.Contains("timeout")) {
            return (SignInFailure.Network, "네트워크 연결을 확인하고 다시 시도해 주세요.");
        }
        if (m.Contains("401") || m.Contains("unauthorized") || m.Contains("redirect") || m.Contains("client id")) {
            return (SignInFailure.ProviderMisconfigured, "로그인 제공자 설정에 문제가 있어요(클라이언트ID/리디렉트).");
        }

        return (SignInFailure.Unknown, "요청을 처리하지 못했어요. 잠시 후 다시 시도해 주세요.");
    }

    // 익명 로그인에서 UPA로 계정 승격 (링크)
    public async Task LinkUpa() {
        try {
            await AuthenticationService.Instance.LinkWithUnityAsync(
                PlayerAccountService.Instance.AccessToken);
            Debug.Log("UPA 계정 링크 성공");
        }
        catch (System.Exception e) {
            Debug.LogException(e);
            OnLoginFailed?.Invoke(SignInFailure.Unknown, "계정 링크에 실패했어요. 다시 시도해 주세요.");
        }
    }

    public void ShowErrorLog(SignInFailure reason, string msg) {
        Debug.Log(msg);
    }
}
