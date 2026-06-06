using UnityEngine;
using UnityEngine.UI;

public class PlayerCrosshair : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Transform character;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private RectTransform crosshair;
    [SerializeField] private Transform aimTarget;

    [Header("Aim")]
    [SerializeField] private float aimHeight = 1.2f;
    [SerializeField] private float aimDistance = 50f;
    [SerializeField] private LayerMask aimLayers;

    [Header("Unlocked Mode")]
    [SerializeField] private bool useCameraPitchWhenUnlocked = true;

    private Canvas canvas;
    private RectTransform canvasRect;
    private Graphic crosshairGraphic;

    private void Awake()
    {
        if (crosshair == null)
            return;

        canvas = crosshair.GetComponentInParent<Canvas>();

        if (canvas != null)
            canvasRect = canvas.GetComponent<RectTransform>();

        crosshairGraphic = crosshair.GetComponent<Graphic>();
    }

    private void LateUpdate()
    {
        if (playerInput == null ||
            character == null ||
            playerCamera == null ||
            crosshair == null ||
            aimTarget == null)
        {
            return;
        }

        if (playerInput.CameraLocked)
        {
            UpdateLockedAim();
        }
        else
        {
            UpdateUnlockedHiddenAim();
        }
    }

    private void UpdateLockedAim()
    {
        // Locked mode:
        // Crosshair is visible and centered.
        if (crosshairGraphic != null)
            crosshairGraphic.enabled = true;

        Vector2 screenPoint =
            new Vector2(
                Screen.width * 0.5f,
                Screen.height * 0.5f
            );

        Ray ray =
            playerCamera.ScreenPointToRay(screenPoint);

        Vector3 targetPoint;

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            aimDistance,
            aimLayers,
            QueryTriggerInteraction.Ignore))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint =
                ray.origin + ray.direction * aimDistance;
        }

        aimTarget.position = targetPoint;

        SetCrosshairScreenPosition(screenPoint);
    }

    private void UpdateUnlockedHiddenAim()
    {
        // Unlocked mode:
        // Crosshair is hidden, but AimTarget still updates
        // as if the old character-facing crosshair existed.
        if (crosshairGraphic != null)
            crosshairGraphic.enabled = false;

        Vector3 rayStart =
            character.position + Vector3.up * aimHeight;

        Vector3 aimDirection =
            GetUnlockedAimDirection();

        Vector3 targetPoint;

        if (Physics.Raycast(
            rayStart,
            aimDirection,
            out RaycastHit hit,
            aimDistance,
            aimLayers,
            QueryTriggerInteraction.Ignore))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint =
                rayStart + aimDirection * aimDistance;
        }

        aimTarget.position = targetPoint;
    }

    private Vector3 GetUnlockedAimDirection()
    {
        Vector3 flatForward = character.forward;
        flatForward.y = 0f;

        if (flatForward.sqrMagnitude < 0.01f)
            flatForward = character.forward;

        flatForward.Normalize();

        if (!useCameraPitchWhenUnlocked)
            return flatForward;

        float cameraPitch =
            playerCamera.transform.forward.y;

        Vector3 aimDirection =
            flatForward + Vector3.up * cameraPitch;

        return aimDirection.normalized;
    }

    private void SetCrosshairScreenPosition(Vector2 screenPosition)
    {
        if (canvas == null || canvasRect == null)
        {
            crosshair.position = screenPosition;
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : playerCamera,
            out Vector2 localPoint
        );

        crosshair.anchoredPosition = localPoint;
    }
}