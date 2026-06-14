using UnityEngine;

public class InventoryFollow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private Camera playerCamera;

    [Header("Position")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private Vector2 screenOffset = new Vector2(180f, 0f);

    [Header("Follow")]
    [SerializeField] private bool followContinuouslyWhileOpen = true;

    [Header("Smoothing")]
    [SerializeField] private bool smoothPosition = true;
    [SerializeField] private float smoothTime = 0.1f;

    [Header("Pixel Stability")]
    [SerializeField] private bool snapToPixel = true;

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
        if (rectTransform != null)
            rectTransform.pivot = new Vector2(0f, 0.5f);
    }

    private void LateUpdate()
    {
        bool inventoryOpen =
            InventoryMenuController.IsInventoryOpen;

        if (!inventoryOpen)
        {
            wasInventoryOpen = false;
            smoothVelocity = Vector2.zero;
            return;
        }

        bool justOpened =
            !wasInventoryOpen;

        if (justOpened)
        {
            UpdatePosition(true);
            wasInventoryOpen = true;
            return;
        }

        if (!followContinuouslyWhileOpen)
            return;

        UpdatePosition(false);
    }

    private void UpdatePosition(bool snapImmediately)
    {
        if (target == null ||
            playerCamera == null ||
            rootCanvas == null ||
            canvasRect == null ||
            rectTransform == null)
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

        if (snapImmediately || !smoothPosition)
        {
            rectTransform.anchoredPosition =
                ApplyPixelSnap(targetPosition);

            smoothVelocity = Vector2.zero;
            return;
        }

        Vector2 smoothedPosition =
            Vector2.SmoothDamp(
                rectTransform.anchoredPosition,
                targetPosition,
                ref smoothVelocity,
                smoothTime,
                Mathf.Infinity,
                Time.unscaledDeltaTime
            );

        rectTransform.anchoredPosition =
            ApplyPixelSnap(smoothedPosition);
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

    private Vector2 ApplyPixelSnap(Vector2 position)
    {
        if (!snapToPixel)
            return position;

        float scaleFactor = 1f;

        if (rootCanvas != null)
            scaleFactor = Mathf.Max(1f, rootCanvas.scaleFactor);

        return new Vector2(
            Mathf.Round(position.x * scaleFactor) / scaleFactor,
            Mathf.Round(position.y * scaleFactor) / scaleFactor
        );
    }
}