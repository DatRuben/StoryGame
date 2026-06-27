using UnityEngine;

[DefaultExecutionOrder(10000)]
public class CameraCollision : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraPivot;

    [Header("Collision")]
    [SerializeField] private LayerMask collisionLayers;
    [SerializeField] private float cameraRadius = 0.15f;
    [SerializeField] private float collisionBuffer = 0.05f;
    [SerializeField] private float minimumDistance = 0.25f;

    [Header("Smoothing")]
    [SerializeField] private float blockedSmoothSpeed = 40f;
    [SerializeField] private float returnSmoothSpeed = 15f;

    private Vector3 currentPosition;
    private bool hasPosition;

    public void SetCameraPivot(Transform pivot)
    {
        cameraPivot = pivot;
        hasPosition = false;
    }

    private void LateUpdate()
    {
        if (cameraPivot == null)
            return;

        Vector3 pivotPosition = cameraPivot.position;
        Vector3 desiredPosition = transform.position;

        Vector3 pivotToCamera = desiredPosition - pivotPosition;
        float desiredDistance = pivotToCamera.magnitude;

        if (desiredDistance < 0.01f)
            return;

        Vector3 direction = pivotToCamera / desiredDistance;

        float correctedDistance = desiredDistance;

        if (Physics.SphereCast(
            pivotPosition,
            cameraRadius,
            direction,
            out RaycastHit hit,
            desiredDistance,
            collisionLayers,
            QueryTriggerInteraction.Ignore))
        {
            correctedDistance =
                Mathf.Max(
                    hit.distance - collisionBuffer,
                    minimumDistance
                );
        }

        Vector3 targetPosition =
            pivotPosition + direction * correctedDistance;

        if (!hasPosition)
        {
            currentPosition = targetPosition;
            hasPosition = true;
        }

        bool isBlocked =
            correctedDistance < desiredDistance;

        float smoothSpeed =
            isBlocked ? blockedSmoothSpeed : returnSmoothSpeed;

        float t =
            1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);

        currentPosition =
            Vector3.Lerp(
                currentPosition,
                targetPosition,
                t
            );

        transform.position = currentPosition;
    }
}