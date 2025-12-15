using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections;

public class Teleporter : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform targetPosition;
    [SerializeField] private bool isEntrance = true;
    [SerializeField] private bool fadeTransition = true;
    [SerializeField] private bool isLocked = false;
    [SerializeField] private string lockComment = "";

    private bool isInRange;

    // interactable
    public InteractionType InteractionType => isEntrance? InteractionType.BuildingEntry : InteractionType.BuildingExit;
    public bool IsInteractable { get { return true; } }


    public void Interact(GameObject interactor) {
        if (isLocked) {
            // 잠김 문구 출력

            UIManager.EnqueueDialogue("알림", lockComment);
            return;
        }
        if (fadeTransition) { FadeAndTeleport(interactor).Forget(); }
        else { Teleport(interactor); }
    }

    private void Teleport(GameObject interactor) {
        interactor.transform.position = targetPosition.position;
    }

    

    private async UniTask FadeAndTeleport(GameObject interactor) {
        // Fade 화면 열기
        FadeLoadingScreen fade = (FadeLoadingScreen)UIManager.OpenScreen(Screen.FadeLoading);

        // Fade 재생 중간에 텔레포트 실행
        await fade.Play(Color.black, async () =>
        {
            Teleport(interactor);
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
        });
    }


	
	void Update() {
		float time = EnvironmentManager.TimeOfDay % 1f;
		isLocked = (20f / 24f) <= time || time < (6f / 24f);
	}
}
