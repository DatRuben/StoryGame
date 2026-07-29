using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

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

    [Serializable]
    private class RaceSizeZoomLimit
    {
        public RaceSize raceSize;

        [Min(0f)]
        public float maxDistance = 8f;
    }

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 0.25f;
    [SerializeField] private float minDistance = 0.75f;
    [SerializeField] private float maxDistance = 5f;

    [SerializeField]
    private List<RaceSizeZoomLimit> raceSizeZoomLimits = new();

    [Header("Automatic Target")]
    [SerializeField, Range(0f, 1f)]
    private float targetHeightPercent = 0.55f;

    [SerializeField] private float distanceMultiplier = 2.75f;
    [SerializeField] private float distancePadding = 1f;

    private float yaw;
    private float pitch;
    private float distance;
    private float currentMaxDistance;
    private float automaticDistance;
    private float zoomOffset;

    private bool dragging;

    private void OnEnable()
    {
        currentMaxDistance =
            Mathf.Max(
                minDistance,
                maxDistance
            );

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

    public void SetTarget(
        Bounds bounds,
        RaceSize raceSize)
    {
        if (cameraPoint == null ||
            lookTarget == null)
        {
            return;
        }

        currentMaxDistance =
            GetMaxDistance(raceSize);

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

        lookTarget.position =
            targetPosition;

        float framedSize =
            Mathf.Max(
                bounds.size.x,
                bounds.size.y,
                bounds.size.z
            );

        automaticDistance =
            framedSize *
            distanceMultiplier +
            distancePadding;

        distance =
            Mathf.Clamp(
                automaticDistance +
                zoomOffset,
                minDistance,
                currentMaxDistance
            );

        ApplyCamera();
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

        automaticDistance = distance;
        zoomOffset = 0f;

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

        zoomOffset -=
            zoomInput *
            zoomSpeed;

        zoomOffset =
            Mathf.Clamp(
                zoomOffset,
                minDistance -
                automaticDistance,
                maxDistance -
                automaticDistance
            );

        distance =
            Mathf.Clamp(
                automaticDistance +
                zoomOffset,
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