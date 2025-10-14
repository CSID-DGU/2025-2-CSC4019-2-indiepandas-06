using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ServerRequestManager : MonoBehaviour
{
    public static ServerRequestManager Instance;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    // 텍스트 분석 요청 함수
    public void RequestTextAnalysis(string inputText, System.Action<AnalysisResponse> callback) {
        StartCoroutine(PostText(inputText, callback));
    }
    
    // 음성 분석 요청 함수
    public void RequestVoiceAnalysis(byte[] audioData, string filename, System.Action<AnalysisResponse> callback) {
        StartCoroutine(PostVoice(audioData, filename, callback));
    }

    // 이미지 분석 요청 함수
    public void RequestImageAnalysis(Texture2D image, System.Action<AnalysisResponse> callback) {
        byte[] imageBytes = image.EncodeToPNG();
        PostImage(imageBytes, "webcam.png", callback);
    }


    // 서버로 텍스트를 POST하고 응답 로그 출력
    private IEnumerator PostText(string input, System.Action<AnalysisResponse> callback)
    {
        // 로컬 테스트 서버 활용 -> 클라우드에 올린 후 수정 필요
        string url = "https://[test_server]:8000/analyze/text";

        // json 형식으로 요청 형식 변환
        string json = JsonUtility.ToJson(new TextPayload { text = input });
        // byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);
        byte[] jsonBytes = new System.Text.UTF8Encoding(false).GetBytes(json);

        Debug.Log("보낼 JSON: " + json);
        // POST 요청 생성
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log("[서버 요청] 텍스트 분석 요청 중...");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("[서버 응답 수신 완료]");

            string responseText = request.downloadHandler.text;
            Debug.Log($"전체 응답 내용: {responseText}");

            AnalysisResponse parsed = JsonUtility.FromJson<AnalysisResponse>(responseText);
            Debug.Log("감정: " + parsed.emotion_result.emotion);
            Debug.Log("GPT: " + parsed.openai_output.content);

            // 콜백으로 전달
            callback?.Invoke(parsed);
        }
        else
        {
            Debug.LogError($"서버 요청 실패: {request.error}");
        }
    }

    private IEnumerator PostVoice(byte[] audioData, string filename, System.Action<AnalysisResponse> callback) {
        string url = "https://[test_server]:8000/analyze/audio";

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", audioData, filename, "audio/wav");

        UnityWebRequest request = UnityWebRequest.Post(url, form);

        Debug.Log("[서버 요청] 음성 분석 요청 중...");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("[서버 응답 수신 완료]");
            string responseText = request.downloadHandler.text;
            Debug.Log($"전체 응답 내용: {responseText}");

            AnalysisResponse parsed = JsonUtility.FromJson<AnalysisResponse>(responseText);
            Debug.Log("감정: " + parsed.emotion_result.emotion);
            Debug.Log("GPT: " + parsed.openai_output.content);
            
            // 콜백으로 전달
            callback?.Invoke(parsed);
        }
        else
        {
            Debug.LogError($"서버 요청 실패: {request.error}");
        }
    }

    private IEnumerator PostImage(byte[] imageData, string filename, System.Action<AnalysisResponse> callback) {
        string url = "https://[test_server]:8000/analyze/image";

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", imageData, filename, "image/png");

        UnityWebRequest request = UnityWebRequest.Post(url, form);

        Debug.Log("[서버 요청] 이미지 분석 요청 중...");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("[서버 응답 수신 완료]");
            string responseText = request.downloadHandler.text;
            Debug.Log($"전체 응답 내용: {responseText}");

            AnalysisResponse parsed = JsonUtility.FromJson<AnalysisResponse>(responseText);
            Debug.Log("감정: " + parsed.emotion_result.emotion);
            Debug.Log("GPT: " + parsed.openai_output.content);
            
            // 콜백으로 전달
            callback?.Invoke(parsed);
        }
        else
        {
            Debug.LogError($"서버 요청 실패: {request.error}");
        }
    }

    [System.Serializable]
    private class TextPayload {
        public string text;
    }
}

[System.Serializable]
public class EmotionResult {
    public string text;
    public string emotion;
}

[System.Serializable]
public class OpenAIOutput {
    public string role;
    public string content;
}

[System.Serializable]
public class AnalysisResponse {
    public EmotionResult emotion_result;
    public OpenAIOutput openai_output;
}