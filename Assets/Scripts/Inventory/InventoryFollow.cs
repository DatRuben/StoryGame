using UnityEngine;

public class InventoryFollow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private Camera playerCamera;

    [Header("Position")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private Vector2 screenOffset = new Vector2(180f, 0f);

    [Header("Smoothing")]
    [SerializeField] private bool smoothPosition = true;
    [SerializeField] private float smoothTime = 0.05f;

    [Header("Screen Clamp")]
    [SerializeField] private bool clampToScreen = true;
    [SerializeField] private Vector2 screenPadding = new Vector2(24f, 24f);

    private RectTransform rectTransform;
    private Canvas rootCanvas;
    private RectTransform canvasRect;

    private Vector2 smoothVelocity;
    private bool wasInventoryOpen;

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
        bool inventoryOpen =
            InventoryMenuController.IsInventoryOpen;

        if (!inventoryOpen)
        {
            wasInventoryOpen = false;
            return;
        }

        UpdatePosition(!wasInventoryOpen);

        wasInventoryOpen = true;
    }

    private void UpdatePosition(bool snap)
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

        Vector2 targetPosition =
            localPoint + screenOffset;

        if (clampToScreen)
        {
            targetPosition =
                ClampToCanvas(targetPosition);
        }

        if (snap || !smoothPosition)
        {
            rectTransform.anchoredPosition = targetPosition;
            smoothVelocity = Vector2.zero;
            return;
        }

        rectTransform.anchoredPosition =
            Vector2.SmoothDamp(
                rectTransform.anchoredPosition,
                targetPosition,
                ref smoothVelocity,
                smoothTime,
                Mathf.Infinity,
                Time.unscaledDeltaTime
            );
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

        Vector2 pivot =
            rectTransform.pivot;

        float minX =
            -canvasHalfWidth +
            screenPadding.x +
            panelWidth * pivot.x;

        float maxX =
            canvasHalfWidth -
            screenPadding.x -
            panelWidth * (1f - pivot.x);

        float minY =
            -canvasHalfHeight +
            screenPadding.y +
            panelHeight * pivot.y;

        float maxY =
            canvasHalfHeight -
            screenPadding.y -
            panelHeight * (1f - pivot.y);

        position.x =
            Mathf.Clamp(position.x, minX, maxX);

        position.y =
            Mathf.Clamp(position.y, minY, maxY);

        return position;
    }
}