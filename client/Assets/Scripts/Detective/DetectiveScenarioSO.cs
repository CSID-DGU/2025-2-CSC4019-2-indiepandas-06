using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(menuName="Detective/Scenario", fileName="NewDetectiveScenario")]
public class DetectiveScenarioSO : ScriptableObject {
    [Header("Meta")]
    public string scenarioId;
    public string clientName;                 // 사연자 이름 (엔딩 멘트에서 사용)

    [Header("도입부(탐정의 대사 등)")]
    [TextArea(1, 2)] public List<string> introLines = new();   // 예: ["자, 무슨 일이 있었는지 차근차근 살펴보자."]

    [Header("사연 씬들(이미지+대사 묶음)")]
    public List<StoryScene> scenes = new();   // 각 씬 = 배경 스프라이트 1 + 대사들(여러 줄)

    [Header("자동적 사고 '생각 맞추기' 선택지")]
    public List<ThoughtOption> thoughtOptions = new(); // 2~4개 정도
    public string choosePrompt = "자, 사연자는 어떤 생각이 들어서 우울했을까?"; // 선택지 직전에 보여줄 프롬프트 대사

    [Tooltip("증거를 몇 % 이상 찾으면 결론 단계로 넘어갈지(0~1)")]
    [Range(0.1f, 1f)] public float completeRatio = 0.5f; // 최소 절반 이상 등

    [Header("엔딩(종합 정리)")]
    public List<string> outroLines = new(); // 마지막 고정 멘트들 (ex: “그럼 난 이 사실을 알리러 {clientName}에게 가볼게! 고마워!”)
}

[Serializable]
public class StoryScene {
    [Tooltip("AtlasManager에서 \"Dialogue\" 키로 가져올 스프라이트 이름")]
    public string spriteName;

    [Tooltip("이 장면을 설명하는 대사들(스피커, 텍스트). 한 장면에 여러 줄 가능")]
    public List<DialogueLine> lines = new();

    [Header("증거 여부")]
    public bool isEvidence = false;
    public EvidenceData evidence; // isEvidence=false면 무시
}

[Serializable]
public class DialogueLine {
    public string speaker;
    [TextArea(2, 4)] public string text;
}

[Serializable]
public class EvidenceData {
    [Tooltip("이미지 기준 정규화 좌표(0~1). 예: (0.20, 0.40)")]
    public Vector2 normalizedPoint = new Vector2(0.5f, 0.5f);

    [Tooltip("탭 허용 반경(정규화)")]
    [Range(0.01f, 0.2f)] public float radius = 0.06f;

    [Header("증거 발견 시 UI 하단에 뜰 문구")]
    [TextArea] public string uiHint;

    [Header("증거 발견 시 즉시 출력할 대사(여러 줄 가능)")]
    public List<DialogueLine> successDialogue = new();

    [Header("사건의 전말(리캡) 단계에서 이 증거를 배경으로 보여주며 칠 멘트들")]
    public List<DialogueLine> recapLines = new();
}

[Serializable]
public class ThoughtOption {
    public string text;                 // 선택지 문구
    public bool correct;                // 정답 여부
    [TextArea] public string feedback;  // 정답이면 정답 피드백, 오답이면 힌트 멘트
}
