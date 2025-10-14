using UnityEngine;
using System.Collections.Generic;

public class FarmPlot : MonoBehaviour
{
    public enum PlotState { Dry, Normal, Wet, GemFertilized }
    private static readonly Dictionary<PlotState, float> StateDurations = new() {
        { PlotState.Dry, 0f },
        { PlotState.Normal, 200f },
        { PlotState.Wet, 220f },
        { PlotState.GemFertilized, 230f }
    };

    private SpriteRenderer spriteRenderer;

    [SerializeField] private PlotState currentPlotState;
    public PlotState CurrentPlotState => currentPlotState;
    [SerializeField] private float stateTimer;
    public float StateTimer => stateTimer;

    // 시간 업데이트 간격(최적화용)
    [SerializeField] private float updateInterval = 1f;
    private float updateTimer;

    [SerializeField] private Sprite[] stateSprites;

    // Fields
    [SerializeField] private GameObject plantPrefab;

    [SerializeField] private Plant currentPlant;
    public Plant CurrentPlant => currentPlant;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public bool PlantSeed(PlantData plantData) {
        if (currentPlant != null) {
            Debug.Log("이미 식물이 심어져 있음");
            return false;
        }
        // Plant 오브젝트 생성
        GameObject plantObject = Instantiate(plantPrefab, transform.position, Quaternion.identity);
        plantObject.transform.SetParent(transform);
        currentPlant = plantObject.GetComponent<Plant>();
        currentPlant.SetPlantData(plantData);
        currentPlant.SetFarmPlot(this);

        return true;
    }

    public float GetGrowthSpeedMultiplier() {
        return currentPlotState switch {
            PlotState.Dry => 0f,
            PlotState.Normal => 0.5f,
            PlotState.Wet => 1f,
            PlotState.GemFertilized => 2f,
            _ => 1f
        };
    }

    private void SetPlotState(PlotState newState) {
        if (currentPlotState == newState) { return; }
        currentPlotState = newState;

        // 상태 스프라이트 적용
        spriteRenderer.sprite = stateSprites[(int)newState];
    }

    public void ChangePlotState(PlotState newState) {
        if (currentPlotState == newState) { return; }
        stateTimer = StateDurations[newState];
        UpdatePlotStateTimer();
    }

    public void UpdatePlotStateTimer(float passedTime = 0f) {
        stateTimer = Mathf.Max(0f, stateTimer - passedTime);

        if (stateTimer == StateDurations[PlotState.Dry]) {
            SetPlotState(PlotState.Dry);
        } else if (stateTimer <= StateDurations[PlotState.Normal]) {
            SetPlotState(PlotState.Normal);
        } else if (stateTimer <= StateDurations[PlotState.Wet]) {
            SetPlotState(PlotState.Wet);
        } else {
            SetPlotState(PlotState.GemFertilized);
        }
    }

    private void Init() {
        SetPlotState(PlotState.Normal);
    }

    void Start() {
        Init();
        // TODO 테스트용 당근 심기 삭제 필요
        // PlantSeed(PlantDataBase.Instance.GetPlantData("Carrot"));
    }

    void Update() {
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval) {
            UpdatePlotStateTimer(updateTimer);
            updateTimer = 0f;
        }
    }

    private void RemovePlant() {
        if (currentPlant != null) {
            Destroy(currentPlant.gameObject);
            currentPlant = null;
        }
    }

    public void HarvestPlant() {
        if(currentPlant != null && currentPlant.IsHarvestable()) {
            // TODO 아이템 획득 로직 추가 필요
            RemovePlant();
        }
    }
}
