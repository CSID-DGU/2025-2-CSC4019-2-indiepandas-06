using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Plant : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public PlantData PlantData { get; private set; }
    public FarmPlot ParentPlot { get; private set; }

    // Fields
    [SerializeField] private float elapsedGrowthTime;
    public float ElapsedGrowthTime => elapsedGrowthTime;

    [SerializeField] private int growthLevel;
    public int GrowthLevel => growthLevel;

    // 시간 업데이트 간격(최적화용)
    [SerializeField] private float updateInterval = 1f;
    private float updateTimer;


    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void init() {
        // 식물 상태 초기화
        spriteRenderer.sprite = PlantData.GrowthSprites[0];
        elapsedGrowthTime = 0f;
        updateTimer = 0f;
    }

    public void SetPlantData(PlantData data) {
        PlantData = data;
        init();
    }

    public void SetFarmPlot(FarmPlot plot) {
        ParentPlot = plot;
    }

    public void ApplyElapsedTime(float passedTime) {
        elapsedGrowthTime = Mathf.Min(PlantData.GrowthTime, elapsedGrowthTime + passedTime * ParentPlot.GetGrowthSpeedMultiplier());
        UpdateGrowthLevel();
    }

    private void UpdateGrowthLevel() {
        // 추후 PlantData에서 각 단계 시간을 지정할 수도 있음
        float growthPerLevel = PlantData.GrowthTime / PlantData.GrowthSprites.Length;
        growthLevel = Mathf.Min((int)(elapsedGrowthTime / growthPerLevel), PlantData.GrowthSprites.Length - 1);

        spriteRenderer.sprite = PlantData.GrowthSprites[growthLevel];
    }

    public bool IsHarvestable() {
        return growthLevel == PlantData.GrowthSprites.Length - 1;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval) {
            ApplyElapsedTime(updateTimer);
            updateTimer = 0f;
        }
    }
}
