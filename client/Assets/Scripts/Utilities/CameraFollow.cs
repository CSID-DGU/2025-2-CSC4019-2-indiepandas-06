using UnityEngine;

public class CameraFollow : MonoBehaviour {
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float snapThreshold = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);

    private void LateUpdate() {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        // 거리가 너무 멀어지면 스냅
        float distance = Vector3.Distance(transform.position, desiredPosition);
        if (distance > snapThreshold) {
            SnapToTarget();
            return;
        }

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );
    }

    public void SnapToTarget() {
        transform.position = target.position + offset;
    }
}