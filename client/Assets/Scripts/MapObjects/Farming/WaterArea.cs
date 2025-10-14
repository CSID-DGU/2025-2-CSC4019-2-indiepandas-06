using UnityEngine;

public class WaterArea : MonoBehaviour {
    private const float Duration = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        Destroy(gameObject, Duration);
    }

    // Update is called once per frame
    void Update() {
        
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.TryGetComponent<FarmPlot>(out var plot)) {
            if (plot.CurrentPlotState != FarmPlot.PlotState.GemFertilized) {
                plot.ChangePlotState(FarmPlot.PlotState.Wet);
            }
        }    
    }
}
