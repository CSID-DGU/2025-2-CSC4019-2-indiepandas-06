using UnityEngine;

public class FarmPlotInteractable : MonoBehaviour, IInteractable
{
    private FarmPlot parentPlot;
    private ToolManager playerToolManager;
    
    // interactable
    public InteractionType InteractionType => InteractionType.Interact;
    public bool IsInteractable {
        get {
            if (parentPlot.CurrentPlant == null) {
                return playerToolManager?.EquippedTool is SeedTool;
            }
            return parentPlot.CurrentPlant.IsHarvestable();
        }
    }

    public void Interact(GameObject interactor) {
		AudioManager.PlaySoundFX(Audio.Farming, 0.8f);
        Debug.Log("interact!");
        if (parentPlot.CurrentPlant == null) {
            SeedTool seed = (SeedTool)playerToolManager?.EquippedTool;
            parentPlot.PlantSeed(PlantDatabase.Instance.GetPlantData(seed.GetPlantId()));
        }
        else if (parentPlot.CurrentPlant.IsHarvestable()) {
            Debug.Log("수확!");
            parentPlot.HarvestPlant();
        }
        // TODO 시든 식물이 추가되면 처리 필요
    }

    private void Awake() {
        parentPlot = GetComponentInParent<FarmPlot>();
    }

    private void Start() {
        playerToolManager = GameManager.Player.GetComponent<ToolManager>();
    }
}
