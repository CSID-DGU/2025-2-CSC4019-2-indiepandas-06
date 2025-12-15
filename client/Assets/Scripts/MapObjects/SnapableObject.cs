using UnityEngine;

public interface ISnappable {
    void SnapToGrid();
}

public class SnapableObject : MonoBehaviour, ISnappable {
    public void SnapToGrid() {
        MapObject mapObject = GetComponent<MapObject>();
        int size = 1;
        if (mapObject != null && mapObject.Data != null) {
            size = mapObject.Data.Size;
        }
        transform.position = GridUtility.SnapToIsometricGrid(transform.position, size);
    }

    private void OnEnable() {
        //SnapToGrid();
    }
}