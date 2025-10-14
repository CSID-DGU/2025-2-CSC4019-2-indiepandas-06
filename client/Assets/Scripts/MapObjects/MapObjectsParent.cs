using UnityEngine;

public class MapObjectsParent : MonoBehaviour {

    public void SnapAllChildrenToGrid() {
        foreach (Transform child in transform) {
            if (child.TryGetComponent(out SnapableObject snapable)) {
                snapable.SnapToGrid();
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        SnapAllChildrenToGrid();
    }

    // Update is called once per frame
    void Update() {
        
    }
}