using UnityEngine;
using UnityEngine.UI;

public class CharacterFacingCrosshair : MonoBehaviour
{
    [SerializeField] private Transform character;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private RectTransform crosshair;

    [SerializeField] private float aimHeight = 1.2f;
    [SerializeField] private float aimDistance = 30f;
    [SerializeField] private LayerMask aimLayers;

    [Header("Vertical Aiming")]
    [SerializeField] private bool useCameraPitch = true;

    private Graphic crosshairGraphic;

    private void Awake()
    {
        if (crosshair != null)
            crosshairGraphic = crosshair.GetComponent<Graphic>();
    }

    private void LateUpdate()
    {
        if (character == null || playerCamera == null || crosshair == null)
            return;

        Vector3 rayStart =
            character.position + Vector3.up * aimHeight;

        Vector3 aimDirection = GetAimDirection();

        Vector3 targetPoint;

        // This makes the crosshair stop on objects instead of passing through them
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
            targetPoint = rayStart + aimDirection * aimDistance;
        }

        Vector3 screenPosition =
            playerCamera.WorldToScreenPoint(targetPoint);

        bool visible =
            screenPosition.z > 0f &&
            screenPosition.x >= 0f &&
            screenPosition.x <= Screen.width &&
            screenPosition.y >= 0f &&
            screenPosition.y <= Screen.height;

        if (crosshairGraphic != null)
            crosshairGraphic.enabled = visible;

        if (!visible)
            return;

        crosshair.position = screenPosition;
    }

    private Vector3 GetAimDirection()
    {
        Vector3 flatForward = character.forward;
        flatForward.y = 0f;
        flatForward.Normalize();

        if (!useCameraPitch)
            return flatForward;

        float cameraPitch =
            playerCamera.transform.forward.y;

        Vector3 aimDirection =
            flatForward + Vector3.up * cameraPitch;

        return aimDirection.normalized;
    }
}