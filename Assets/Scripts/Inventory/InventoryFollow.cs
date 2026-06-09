using UnityEngine;
using UnityEngine.UI;

public class InventoryFollow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private Camera playerCamera;

    [Header("Position")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private Vector2 screenOffset = new Vector2(180f, 0f);

    [Header("Screen Clamp")]
    [SerializeField] private bool clampToScreen = true;
    [SerializeField] private Vector2 screenPadding = new Vector2(24f, 24f);

    private RectTransform rectTransform;
    private Canvas rootCanvas;
    private RectTransform canvasRect;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();

        if (rootCanvas != null)
            canvasRect = rootCanvas.transform as RectTransform;

        if (playerCamera == null)
            playerCamera = Camera.main;

        // Left-middle pivot means the panel grows to the right of the target.
        rectTransform.pivot = new Vector2(0f, 0.5f);
    }

    private void LateUpdate()
    {
        if (!InventoryMenuController.IsInventoryOpen)
            return;

        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (target == null ||
            playerCamera == null ||
            rootCanvas == null ||
            canvasRect == null)
        {
            return;
        }

        Vector3 worldPosition =
            target.position + worldOffset;

        Vector3 screenPosition =
            playerCamera.WorldToScreenPoint(worldPosition);

        if (screenPosition.z < 0f)
            return;

        Camera canvasCamera =
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : rootCanvas.worldCamera;

        bool hasPoint =
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPosition,
                canvasCamera,
                out Vector2 localPoint
            );

        if (!hasPoint)
            return;

        Vector2 finalPosition =
            localPoint + screenOffset;

        if (clampToScreen)
        {
            finalPosition =
                ClampToCanvas(finalPosition);
        }

        rectTransform.anchoredPosition =
            finalPosition;
    }

    private Vector2 ClampToCanvas(Vector2 position)
    {
        float panelWidth =
            rectTransform.rect.width;

        float panelHeight =
            rectTransform.rect.height;

        float canvasHalfWidth =
            canvasRect.rect.width * 0.5f;

        float canvasHalfHeight =
            canvasRect.rect.height * 0.5f;

        float minX =
            -canvasHalfWidth + screenPadding.x;

        float maxX =
            canvasHalfWidth - panelWidth - screenPadding.x;

        float minY =
            -canvasHalfHeight + panelHeight * 0.5f + screenPadding.y;

        float maxY =
            canvasHalfHeight - panelHeight * 0.5f - screenPadding.y;

        position.x =
            Mathf.Clamp(position.x, minX, maxX);

        position.y =
            Mathf.Clamp(position.y, minY, maxY);

        return position;
    }
}