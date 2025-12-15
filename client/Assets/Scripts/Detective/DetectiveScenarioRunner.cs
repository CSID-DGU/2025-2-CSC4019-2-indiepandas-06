using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class DetectiveScenarioRunner : MonoBehaviour
{
    [SerializeField] private string detectiveName = "탐정 고난"; // 도입부/엔딩 등 진행자 이름

    public void Play(DetectiveScenarioSO so)
    {
        StartCoroutine(Run(so));
    }

    private IEnumerator Run(DetectiveScenarioSO so) {
        // 1) 도입부
        EnqueueLinesWithBG(so.introLines, so.clientName, bgSprite: null);

        // 2) 사연 씬들 (이미지 1장에 대사 여러 줄)
        foreach (var scene in so.scenes) {
            // 첫 줄에만 BG 지정 → 이후 줄은 null로 유지
            for (int i = 0; i < scene.lines.Count; i++) {
                var line = scene.lines[i];
                string bg = (i == 0) ? scene.spriteName : null;
                UIManager.EnqueueDialogue(line.speaker, line.text, bg);
            }
        }

        // 3) 생각 맞추기 선택지
        UIManager.EnqueueDialogue(detectiveName, so.choosePrompt, null);
        yield return WaitDialogueDrain();

        // 루프: 정답 고를 때까지
        bool solved = false;
        while (!solved) {
            ChoiceState.Reset();

            UIManager.BeginChoices();
            for (int i = 0; i < so.thoughtOptions.Count; i++) {
                int captured = i;
                UIManager.AddChoice(so.thoughtOptions[i].text, () => {
                    ChoiceState.SelectFromCode(captured);
                });
            }
            UIManager.ShowChoices();

            // 사용자가 하나 고를 때까지 대기
            yield return new WaitUntil(() => ChoiceState.HasSelection);

            UIManager.HoldDialogue();

            var opt = so.thoughtOptions[ChoiceState.SelectedIndex];
            if (opt.correct)
            {
                UIManager.EnqueueDialogue(detectiveName, "맞아, 바로 그거야!", null);
                UIManager.EnqueueDialogue(detectiveName, opt.feedback, null); // 정답 피드백
                UIManager.EnqueueDialogue(detectiveName, 
                    "그런데, 그 생각이 과연 진실이었을까?\n사연을 다시 되짚어보며 생각을 반박할 증거를 모아보자!", null);
                solved = true;
            }
            else
            {
                UIManager.EnqueueDialogue(detectiveName, opt.feedback, null); // 오답 힌트
                UIManager.EnqueueDialogue(detectiveName, "다시 생각해볼까?", null);
            }

            UIManager.ReleaseDialogueDeferred();

            yield return WaitDialogueDrain();
        }

        UIManager.CloseScreen(Screen.Dialogue);

        // 잠시 대화를 투명 오버레이로 전환
        UIManager.SetDialogueOverlayTransparentMode(true);

        // 4) 증거 찾기 UI
        var evidenceScenes = CollectEvidenceScenes(so);
        int total = evidenceScenes.Count;
        int need = Mathf.CeilToInt(total * Mathf.Clamp01(so.completeRatio));
        int found = 0;

        //UIManager.HoldDialogue();

        var ev = UIManager.OpenEvidence(
            so, 
            onFound: (sceneIdx) => {   // 증거 발견 콜백
                var sc = so.scenes[sceneIdx]; // 증거가 있는 씬만 나오게 할 거면 evidenceScenes[sceneIdx]
                var sDialog = sc.evidence.successDialogue;
                // 즉시 대사 출력 (성공 대사)
                for (int i = 0; i < sDialog.Count; i++) {
                    var line = sDialog[i];
                    Action onEnd = (i == sDialog.Count - 1) ? (Action)(() => found++) : null;
                    UIManager.EnqueueDialogue(line.speaker, line.text, onEnd); //sc.spriteName
                }
            },
            onClosed: () => { /* 유저가 닫기 눌렀을 때(선택) */ },
            shouldAutoClose: () => found >= need
        );

        // EvidenceScreen이 "필요개수 달성"으로 종료될 때까지 대기
        yield return new WaitUntil(() => ev == null || ev.IsClosed);

        //UIManager.ReleaseDialogue();

        // 오버레이 모드 해제
        UIManager.SetDialogueOverlayTransparentMode(false);

        // 5) 리캡(사건의 전말)
        UIManager.EnqueueDialogue(detectiveName, "아주 예리했어! 사건의 전말은 이래!", null);
        foreach (var sc in evidenceScenes)
        {
            foreach (var line in sc.evidence.recapLines) {
                UIManager.EnqueueDialogue(line.speaker, line.text, sc.spriteName);
            }
        }

        // 6) 엔딩
        foreach (var line in so.outroLines)
        {
            string cooked = line.Replace("{client}", so.clientName);
            UIManager.EnqueueDialogue(detectiveName, cooked, "");
        }

        yield return WaitDialogueDrain();
    }

    // 사연 씬을 대사 목록만 받아 일괄 Enqueue (BG는 첫 줄만)
    private void EnqueueLinesWithBG(List<string> lines, string clientName, string bgSprite)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            string bg = (i == 0) ? bgSprite : null;
            UIManager.EnqueueDialogue(detectiveName, lines[i], bg);
        }
    }

    private List<StoryScene> CollectEvidenceScenes(DetectiveScenarioSO so)
    {
        var list = new List<StoryScene>();
        foreach (var sc in so.scenes) if (sc.isEvidence) list.Add(sc);
        return list;
    }

    // “대화 큐가 비었을 때(=화면이 다음 단계로 넘어가도 되는 시점)”까지 대기
    private IEnumerator WaitDialogueDrain()
    {
        // 프레임 하나 양보(막 Enqueue된 걸 Start에서 잡을 시간)
        yield return null;

        while (UIManager.HasPendingDialogue 
            || UIManager.IsDialogueHeld 
            || UIManager.IsChoiceOpen 
            || UIManager.IsDialogueInput)
        {
            yield return null; // 완전히 Idle될 때까지 대기
        }

        // 상태 수렴을 위해 1프레임 더
        yield return null;
    }
}
