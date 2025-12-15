using UnityEngine;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class DetectiveScenarioTestBoot : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private DetectiveScenarioRunner runner;
    [SerializeField] private DetectiveScenarioSO scenario;   // 비워두면 런타임 생성
    [SerializeField] private bool startOnPlay = true;

    void Start()
    {
        if (startOnPlay) Task();
    }
    async UniTask Task()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(2f));
        Run();
    }

    void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("[TestBoot] T pressed → Run demo.");
            Run();
        }
        */
    }

    public void Run()
    {
        if (!runner)
        {
            runner = FindObjectOfType<DetectiveScenarioRunner>();
            if (!runner)
            {
                Debug.LogError("[TestBoot] DetectiveScenarioRunner가 씬에 없음!");
                return;
            }
        }

        runner.Play(scenario);
    }
}