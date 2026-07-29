using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterCreatorStageCamera : MonoBehaviour
{
    [Header("Stage")]
    [SerializeField] private Transform cameraPoint;
    [SerializeField] private Transform lookTarget;

    [Header("Input Area")]
    [SerializeField] private RectTransform inputArea;

    [Header("Orbit")]
    [SerializeField] private float orbitSpeed = 0.15f;
    [SerializeField] private float minPitch = -10f;
    [SerializeField] private float maxPitch = 35f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 0.25f;
    [SerializeField] private float minDistance = 0.75f;
    [SerializeField] private float maxDistance = 5f;

    [Header("Automatic Target")]
    [SerializeField, Range(0f, 1f)]
    private float targetHeightPercent = 0.55f;

    private float yaw;
    private float pitch;
    private float distance;

    private bool dragging;

    private void OnEnable()
    {
        ReadCamera();
        ApplyCamera();
    }

    private void OnDisable()
    {
        dragging = false;
    }

    private void Update()
    {
        if (cameraPoint == null ||
            lookTarget == null ||
            Mouse.current == null)
        {
            return;
        }

        Mouse mouse = Mouse.current;

        Vector2 mousePosition =
            mouse.position.ReadValue();

        bool insideInputArea =
            inputArea == null ||
            RectTransformUtility
                .RectangleContainsScreenPoint(
                    inputArea,
                    mousePosition
                );

        if (mouse.leftButton.wasPressedThisFrame)
            dragging = insideInputArea;

        if (mouse.leftButton.wasReleasedThisFrame)
            dragging = false;

        if (dragging)
        {
            Orbit(
                mouse.delta.ReadValue()
            );
        }

        if (insideInputArea)
        {
            Zoom(
                mouse.scroll.ReadValue().y
            );
        }
    }

    public void SetTarget(Bounds bounds)
    {
        if (cameraPoint == null ||
            lookTarget == null)
        {
            return;
        }

        Vector3 targetPosition =
            new Vector3(
                bounds.center.x,
                Mathf.Lerp(
                    bounds.min.y,
                    bounds.max.y,
                    targetHeightPercent
                ),
                bounds.center.z
            );

        Vector3 targetOffset =
            targetPosition -
            lookTarget.position;

        lookTarget.position =
            targetPosition;

        cameraPoint.position +=
            targetOffset;
    }

    private void ReadCamera()
    {
        if (cameraPoint == null ||
            lookTarget == null)
        {
            return;
        }

        Vector3 offset =
            cameraPoint.position -
            lookTarget.position;

        distance =
            Mathf.Clamp(
                offset.magnitude,
                minDistance,
                maxDistance
            );

        if (offset.sqrMagnitude <= 0.0001f)
        {
            yaw = 0f;
            pitch = 10f;
            return;
        }

        Quaternion rotation =
            Quaternion.LookRotation(
                -offset.normalized,
                Vector3.up
            );

        Vector3 eulerAngles =
            rotation.eulerAngles;

        yaw = eulerAngles.y;
        pitch = NormalizeAngle(
            eulerAngles.x
        );
    }

    private void Orbit(
        Vector2 mouseDelta)
    {
        if (mouseDelta.sqrMagnitude <= 0f)
            return;

        yaw +=
            mouseDelta.x *
            orbitSpeed;

        pitch =
            Mathf.Clamp(
                pitch -
                mouseDelta.y *
                orbitSpeed,
                minPitch,
                maxPitch
            );

        ApplyCamera();
    }

    private void Zoom(
        float scrollAmount)
    {
        if (Mathf.Abs(scrollAmount) <= 0.01f)
            return;

        float zoomInput =
            scrollAmount / 120f;

        distance =
            Mathf.Clamp(
                distance -
                zoomInput *
                zoomSpeed,
                minDistance,
                maxDistance
            );

        ApplyCamera();
    }

    private void ApplyCamera()
    {
        if (cameraPoint == null ||
            lookTarget == null)
        {
            return;
        }

        Quaternion rotation =
            Quaternion.Euler(
                pitch,
                yaw,
                0f
            );

        cameraPoint.position =
            lookTarget.position +
            rotation *
            Vector3.back *
            distance;

        cameraPoint.rotation =
            rotation;
    }

    private float NormalizeAngle(
        float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }
}