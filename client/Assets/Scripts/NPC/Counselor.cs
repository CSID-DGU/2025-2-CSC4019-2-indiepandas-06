using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.Networking;

public class Counseler : MonoBehaviour, IInteractable {
    public InteractionType InteractionType => InteractionType.Talk;
    public bool IsInteractable => true;

    private string apiUrl = "http://SERVER_IP/v1/dialog/generate";
    private string apiKey = "API_KEY";

    private string lastUserInput = "";

   [SerializeField] MicRecorder micRecorder;

    // 서버 요청/응답 DTO
    [Serializable]
    private class ApiRequest {
        public string player_id = AuthenticationService.Instance.PlayerId;
        public string session_id = "session_01";
        public string dialog_text;
        public string locale = "ko-KR";
        public string npc_persona = "따뜻한 상담사";
        public object game_state = new object();
    }

    [Serializable]
    private class ApiResponse {
        public string emotion;
        public string npc_line;
    }

    public void Interact(GameObject interactor) {
        StartCoroutine(RunInteraction());
    }

    private IEnumerator RunInteraction() {
        // 인사
        UIManager.EnqueueDialogue("수정", "안녕~ 반가워요", null);
        yield return WaitDialogueDrain();

        // 선택지
        int selected = -1;
        UIManager.BeginChoices();
        UIManager.AddChoice("얘기 좀 들어줄 수 있어요?", () => { selected = 0; });
        UIManager.AddChoice("안녕!", () => { selected = 1; });
        UIManager.ShowChoices();

        // 선택될 때까지 대기
        yield return new WaitUntil(() => selected >= 0);

        if (selected == 0) {
            // 상담 시작 멘트
            UIManager.EnqueueDialogue("수정", "그럼요 편하게 말해보세요. 다 들어드릴게요!", null);
            yield return WaitDialogueDrain();

            // 사용자 입력
            bool inputDone = false;
            AudioClip userVoiceClip = null;

            UIManager.BeginDialogueInput(data => {
                lastUserInput = data.text ?? "";
                inputDone = true;
                Debug.Log($"[Counseler] 사용자가 입력한 내용: {lastUserInput}");
                userVoiceClip = data.voice;
            });
            yield return new WaitUntil(() => inputDone);

            // 응답 받아서 NPC 대사 출력
            string npcLineFromServer = null;
            bool replyReady = false;
            var prompt = BuildUserInput(lastUserInput);

            if (userVoiceClip != null) {
                StartCoroutine(GenerateDialog(prompt, userVoiceClip, line => {
                    npcLineFromServer = line;
                    replyReady = true;
                }));
            }
            else {
                StartCoroutine(GenerateDialog(prompt, line => {
                    npcLineFromServer = line;
                    replyReady = true;
                }));
            }

            UIManager.EnqueueDialogue("수정", "잠깐만요… 생각을 정리해볼게요.", null);
            yield return WaitDialogueDrain();

            // 아직 안 왔으면 조금 더 기다리며 짧은 멘트 한 번 더
            float waitStart = Time.time;
            float hardTimeout = 12f;
            if (!replyReady) {
                UIManager.EnqueueDialogue("수정", "음… 말씀해주신 부분을 정리 중이에요.", null);
                while (!replyReady && (Time.time - waitStart) < hardTimeout) {
                    yield return null;
                }
            }

            if (string.IsNullOrEmpty(npcLineFromServer)) {
                npcLineFromServer = "응답을 받지 못했어요. 다시 한 번 이야기해줄래요?";
            }

            // 말풍선 분할
            var bubbles = SplitIntoBubblesEnsureRange(npcLineFromServer, 1, 5);
            if (bubbles.Count == 0) { bubbles.Add(npcLineFromServer); }

            // 말풍선 순차 출력
            yield return StartCoroutine(EnqueueBubbles(bubbles, speaker: "수정"));
        }
        else {
            UIManager.EnqueueDialogue("수정", "안녕! 오늘 하루 어땠나요?", null);
            yield return WaitDialogueDrain();
        }
    }

    private string BuildUserInput(string userInput) {
        return $@"{userInput}";
    }

    // 서버 호출 코루틴
    private IEnumerator GenerateDialog(string inputText, Action<string> callback) {
        // 요청 바디 구성
        var requestData = new ApiRequest { dialog_text = inputText };
        string jsonRequest = JsonUtility.ToJson(requestData);
        byte[] jsonBytes = new UTF8Encoding().GetBytes(jsonRequest);

        using (var webRequest = new UnityWebRequest(apiUrl, "POST")) {
            webRequest.uploadHandler = new UploadHandlerRaw(jsonBytes);
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("X-Api-Key", apiKey);

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError) {
                Debug.LogError($"[Counseler] API Error: {webRequest.error}");
                callback?.Invoke("미안해요, 지금은 인터넷 연결 상태가 좋지 않아서 다음에 다시 시도해볼까요?");
            } else {
                string responseText = webRequest.downloadHandler.text;
                Debug.Log($"[Counseler] Raw Response: {responseText}");
                try {
                    var result = JsonUtility.FromJson<ApiResponse>(responseText);
                    callback?.Invoke(result?.npc_line ?? "조금 더 자세히 말씀해주시면 더 잘 도와드릴 수 있어요.");
                } catch (Exception e) {
                    Debug.LogError($"[Counseler] JSON 파싱 에러: {e.Message}");
                    callback?.Invoke("죄송해요. 몸이 좋지 않아서 다음에 꼭 다시 얘기해 줄 수 있을까요?");
                }
            }
        }
    }

    private IEnumerator GenerateDialog(string inputText, AudioClip voiceClip, Action<string> callback) {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();

        formData.Add(new MultipartFormDataSection("player_id", AuthenticationService.Instance.PlayerId));
        formData.Add(new MultipartFormDataSection("session_id", "session_01"));
        formData.Add(new MultipartFormDataSection("dialog_text", inputText));
        formData.Add(new MultipartFormDataSection("locale", "ko-KR"));
        formData.Add(new MultipartFormDataSection("npc_persona", "따뜻한 상담사"));

        if (voiceClip != null) {
            byte[] wavData = ConvertClipToWav(voiceClip);
            formData.Add(new MultipartFormFileSection("audio_file", wavData, "voice.wav", "audio/wav"));
        }

        using (var webRequest = UnityWebRequest.Post(apiUrl, formData)) {
            webRequest.SetRequestHeader("X-Api-Key", apiKey);

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError) {
                Debug.LogError($"[Counseler] API Error: {webRequest.error}");
                callback?.Invoke("미안해요, 지금은 인터넷 연결 상태가 좋지 않아서 다음에 다시 시도해볼까요?");
            } else {
                string responseText = webRequest.downloadHandler.text;
                Debug.Log($"[Counseler] Raw Response: {responseText}");
                try {
                    var result = JsonUtility.FromJson<ApiResponse>(responseText);
                    callback?.Invoke(result?.npc_line ?? "조금 더 자세히 말씀해주시면 더 잘 도와드릴 수 있어요.");
                } catch (Exception e) {
                    Debug.LogError($"[Counseler] JSON 파싱 에러: {e.Message}");
                    callback?.Invoke("죄송해요. 몸이 좋지 않아서 다음에 꼭 다시 얘기해 줄 수 있을까요?");
                }
            }
        }
    }

    // AudioClip을 WAV 포맷 바이트 배열로 변환
    private byte[] ConvertClipToWav(AudioClip clip) {
        using (var memoryStream = new MemoryStream()) {
            using (var writer = new BinaryWriter(memoryStream)) {
                var samples = new float[clip.samples * clip.channels];
                clip.GetData(samples, 0);

                int sampleRate = clip.frequency;
                int channels = clip.channels;
                int sampleCount = samples.Length;

                // WAV Header 작성
                writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + sampleCount * 2);
                writer.Write(new char[4] { 'W', 'A', 'V', 'E' });
                writer.Write(new char[4] { 'f', 'm', 't', ' ' });
                writer.Write(16);
                writer.Write((ushort)1);
                writer.Write((ushort)channels);
                writer.Write(sampleRate);
                writer.Write(sampleRate * channels * 2);
                writer.Write((ushort)(channels * 2));
                writer.Write((ushort)16);
                writer.Write(new char[4] { 'd', 'a', 't', 'a' });
                writer.Write(sampleCount * 2);

                // Data 작성
                foreach (var sample in samples) {
                    short intSample = (short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue);
                    writer.Write(intSample);
                }
            }
            return memoryStream.ToArray();
        }
    }

    // 응답을 분할
    private List<string> SplitIntoBubblesEnsureRange(string text, int minBubbles = 1, int maxBubbles = 5) {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(text)) { return result; }

        string src = text.Replace("...", "…").Trim();

        // 문장부호 기준 1차 분할
        var sentences = Regex.Split(src, @"(?<=[\.!?…。！？])\s+")
                            .Select(s => s.Trim())
                            .Where(s => s.Length > 0)
                            .ToList();

        result.AddRange(sentences);

        if (result.Count < minBubbles) {
            var expanded = new List<string>();
            foreach (var s in result) {
                if (expanded.Count >= maxBubbles) { expanded.Add(s); continue; }

                var clauses = s.Split(',').Select(c => c.Trim()).Where(c => c.Length > 0).ToList();

                if (clauses.Count == 1) {
                    expanded.Add(s);
                }
                else {
                    for (int i = 0; i < clauses.Count; i++) {
                        var piece = clauses[i];
                        bool isLastPiece = (i == clauses.Count - 1);

                        // 마지막 조각이 아니면 이어짐 표시
                        if (!isLastPiece && !Regex.IsMatch(piece, @"[\.!?…]$")) {
                            piece += "…";
                        }

                        expanded.Add(piece);

                        if (expanded.Count >= maxBubbles) { break; }
                    }
                }
                if (expanded.Count >= maxBubbles) { break; }
            }

            if (expanded.Count > 0) {
                result = expanded;
            }
        }

        int guard = 0;
        while (result.Count < minBubbles && result.Count > 0 && guard++ < 8) {
            int idx = 0, maxLen = 0;
            for (int i = 0; i < result.Count; i++)
                if (result[i].Length > maxLen) { maxLen = result[i].Length; idx = i; }

            var target = result[idx];
            if (target.Length < 20) break;

            int mid = target.Length / 2;

            int cut = -1;
            var matches = Regex.Matches(target, @"\s+").Cast<Match>()
                            .Select(m => m.Index + m.Length)
                            .OrderBy(i => Mathf.Abs(i - mid))
                            .ToList();
            if (matches.Count > 0) cut = matches[0];
            if (cut < 0) cut = mid;

            var left  = target[..cut].Trim().TrimEnd(',');
            var right = target[cut..].Trim().TrimStart(',');

            if (left.Length >= 6 && right.Length >= 6)
            {
                if (!Regex.IsMatch(left, @"[\.!?…]$")) left += "…";
                result[idx] = left;
                result.Insert(idx + 1, right);
            }
            else break;
        }

        // 너무 많으면 뒤에서부터 합치기
        while (result.Count > maxBubbles) {
            result[result.Count - 2] = (result[result.Count - 2] + " " + result[^1]).Trim();
            result.RemoveAt(result.Count - 1);
        }

        if (result.Count == 0) { result.Add(src); }
        return result;
    }
    // 말풍선 순차 출력
    private IEnumerator EnqueueBubbles(List<string> bubbles, string speaker = "수정") {
        for (int i = 0; i < bubbles.Count; i++) {
            UIManager.EnqueueDialogue(speaker, bubbles[i], onEnd: null);
            //yield return WaitDialogueDrain();
            yield return null;
        }
    }

    // 대화/선택/입력 등이 완전히 끝날 때까지 대기
    private IEnumerator WaitDialogueDrain() {
        yield return null;
        while (UIManager.HasPendingDialogue
            || UIManager.IsDialogueHeld
            || UIManager.IsChoiceOpen
            || UIManager.IsDialogueInput) {
            yield return null;
        }
        yield return null;
    }
}
