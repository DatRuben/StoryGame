using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryGridUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private Transform cellParent;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private RectTransform itemOutline;

    [Header("Drag Detection")]
    [SerializeField] private float dragStartDistance = 12f;

    [Header("Colors")]
    [SerializeField] private Color emptyColor = new Color(0f, 0f, 0f, 0.35f);
    [SerializeField] private Color occupiedColor = new Color(1f, 1f, 1f, 0.85f);
    [SerializeField] private Color validPlacementColor = new Color(0.2f, 1f, 0.2f, 0.85f);
    [SerializeField] private Color invalidPlacementColor = new Color(1f, 0.2f, 0.2f, 0.85f);
    [SerializeField] private Color heldPreviewColor = new Color(1f, 1f, 1f, 0.65f);
    [SerializeField] private Color dragOriginalGhostColor = new Color(0.45f, 0.45f, 0.45f, 0.35f);

    [Header("Item Outlines")]
    [SerializeField] private Color itemOutlineColor = new Color(1f, 1f, 1f, 0.95f);
    [SerializeField] private Color dragOriginalOutlineColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
    [SerializeField] private float itemOutlineThickness = 3f;
    [SerializeField] private bool fillPaddingBetweenCells = true;

    [Header("Held Item Mouse Preview")]
    [SerializeField] private Vector2 heldPreviewOffset = Vector2.zero;

    private GridLayoutGroup gridLayoutGroup;
    private Canvas rootCanvas;
    private PlayerInputActions playerInput;

    private RectTransform heldPreviewRoot;
    private GridLayoutGroup heldPreviewLayoutGroup;
    private CanvasGroup heldPreviewCanvasGroup;

    private readonly List<InventoryCellUI> cells = new List<InventoryCellUI>();
    private readonly List<Vector2Int> cellCoordinates = new List<Vector2Int>();

    private Vector2Int hoveredCoordinate = new Vector2Int(-1, -1);
    private Vector2Int heldGrabOffset = Vector2Int.zero;

    private bool pointerIsDown;
    private bool pendingDragPickup;
    private bool isDraggingItem;
    private bool suppressNextClick;

    private Vector2 pointerDownScreenPosition;
    private Vector2Int pointerDownCoordinate;

    private Vector2Int dragOriginalPosition;
    private int dragOriginalRotationSteps;

    private PlacedInventoryItem HeldItem
    {
        get
        {
            if (playerInventory == null)
                return null;

            return playerInventory.HeldItem;
        }
    }

    private void Awake()
    {
        if (cellParent != null)
            gridLayoutGroup = cellParent.GetComponent<GridLayoutGroup>();

        if (gridLayoutGroup == null)
            gridLayoutGroup = GetComponent<GridLayoutGroup>();

        rootCanvas = GetComponentInParent<Canvas>();
        playerInput = new PlayerInputActions();
    }

    private void OnEnable()
    {
        if (playerInput == null)
            playerInput = new PlayerInputActions();

        playerInput.Player.RotateItem.started += OnRotateItem;
        playerInput.Player.Enable();
    }

    private void OnDisable()
    {
        if (playerInput != null)
        {
            playerInput.Player.RotateItem.started -= OnRotateItem;
            playerInput.Player.Disable();
        }
    }

    private void Start()
    {
        if (playerInventory == null)
            return;

        playerInventory.OnInventoryChanged += Refresh;
        playerInventory.OnHeldItemChanged += HandleHeldItemChanged;

        CreateHeldPreviewRoot();
        BuildGrid();
        HandleHeldItemChanged();
        Refresh();
    }

    private void Update()
    {
        HandleDragDetection();
        HandleDragRelease();
        UpdateHeldPreviewVisibility();
        UpdateHeldPreviewPosition();
    }

    private void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= Refresh;
            playerInventory.OnHeldItemChanged -= HandleHeldItemChanged;
        }
    }

    private void OnRotateItem(InputAction.CallbackContext context)
    {
        if (!InventoryMenuController.IsInventoryOpen)
            return;

        if (playerInventory == null ||
            !playerInventory.IsHoldingItem)
        {
            return;
        }

        bool rotated =
            playerInventory.RotateHeldItemCounterClockwise();

        if (!rotated)
            return;

        ClampHeldGrabOffsetToHeldItem();
        BuildHeldItemPreview();
        Refresh();
    }

    private void HandleDragDetection()
    {
        if (!pointerIsDown ||
            !pendingDragPickup ||
            isDraggingItem ||
            Mouse.current == null)
        {
            return;
        }

        if (!Mouse.current.leftButton.isPressed)
        {
            pointerIsDown = false;
            pendingDragPickup = false;
            return;
        }

        Vector2 currentMousePosition =
            Mouse.current.position.ReadValue();

        float movedDistance =
            Vector2.Distance(
                pointerDownScreenPosition,
                currentMousePosition
            );

        if (movedDistance < dragStartDistance)
            return;

        StartDragPickup(pointerDownCoordinate);
    }

    private void HandleDragRelease()
    {
        if (!isDraggingItem)
            return;

        if (!InventoryMenuController.IsInventoryOpen)
        {
            ReturnDraggedItemToOriginalPosition();
            return;
        }

        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            CompleteDragDrop(hoveredCoordinate);
        }
    }

    private void StartDragPickup(Vector2Int coordinate)
    {
        if (playerInventory == null ||
            playerInventory.Grid == null)
        {
            return;
        }

        if (playerInventory.IsHoldingItem)
            return;

        PlacedInventoryItem itemAtCell =
            playerInventory.Grid.GetPlacedItem(
                coordinate.x,
                coordinate.y
            );

        if (itemAtCell == null)
            return;

        dragOriginalPosition = itemAtCell.Position;
        dragOriginalRotationSteps = itemAtCell.RotationSteps;

        PlacedInventoryItem pickedItem =
            playerInventory.TryPickUpItemAt(
                coordinate.x,
                coordinate.y,
                false
            );

        if (pickedItem == null)
            return;

        heldGrabOffset =
            coordinate - pickedItem.Position;

        ClampHeldGrabOffsetToHeldItem();

        isDraggingItem = true;
        pendingDragPickup = false;
        suppressNextClick = true;

        Refresh();
    }

    private void CompleteDragDrop(Vector2Int releaseCoordinate)
    {
        if (!isDraggingItem)
            return;

        isDraggingItem = false;
        pointerIsDown = false;
        pendingDragPickup = false;
        suppressNextClick = true;

        bool releaseIsValid =
            releaseCoordinate.x >= 0 &&
            releaseCoordinate.y >= 0;

        if (releaseIsValid)
        {
            Vector2Int placementOrigin =
                GetHeldPlacementOrigin(releaseCoordinate);

            bool placed =
                playerInventory.TryPlaceHeldItem(
                    placementOrigin.x,
                    placementOrigin.y
                );

            if (placed)
            {
                heldGrabOffset = Vector2Int.zero;
                Refresh();
                return;
            }
        }

        ReturnDraggedItemToOriginalPosition();
    }

    private void ReturnDraggedItemToOriginalPosition()
    {
        isDraggingItem = false;
        pointerIsDown = false;
        pendingDragPickup = false;
        suppressNextClick = true;

        if (playerInventory == null ||
            !playerInventory.IsHoldingItem ||
            playerInventory.HeldItem == null)
        {
            heldGrabOffset = Vector2Int.zero;
            Refresh();
            return;
        }

        playerInventory.HeldItem.SetRotationSteps(
            dragOriginalRotationSteps
        );

        bool returned =
            playerInventory.TryPlaceHeldItem(
                dragOriginalPosition.x,
                dragOriginalPosition.y
            );

        if (!returned)
        {
            Debug.LogError(
                "Could not return dragged item to its original inventory position."
            );
        }

        heldGrabOffset = Vector2Int.zero;
        Refresh();
    }

    private void ClampHeldGrabOffsetToHeldItem()
    {
        PlacedInventoryItem heldItem =
            HeldItem;

        if (heldItem == null)
        {
            heldGrabOffset = Vector2Int.zero;
            return;
        }

        heldGrabOffset.x =
            Mathf.Clamp(
                heldGrabOffset.x,
                0,
                Mathf.Max(0, heldItem.Width - 1)
            );

        heldGrabOffset.y =
            Mathf.Clamp(
                heldGrabOffset.y,
                0,
                Mathf.Max(0, heldItem.Height - 1)
            );
    }

    private void BuildGrid()
    {
        if (playerInventory.Grid == null ||
            cellParent == null ||
            cellPrefab == null)
        {
            return;
        }

        foreach (Transform child in cellParent)
        {
            Destroy(child.gameObject);
        }

        cells.Clear();
        cellCoordinates.Clear();

        InventoryGrid grid = playerInventory.Grid;

        if (gridLayoutGroup != null)
        {
            gridLayoutGroup.constraint =
                GridLayoutGroup.Constraint.FixedColumnCount;

            gridLayoutGroup.constraintCount =
                grid.Width;
        }

        for (int y = grid.Height - 1; y >= 0; y--)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                GameObject cellObject =
                    Instantiate(cellPrefab, cellParent);

                InventoryCellUI cellUI =
                    cellObject.GetComponent<InventoryCellUI>();

                if (cellUI == null)
                    cellUI = cellObject.AddComponent<InventoryCellUI>();

                Vector2Int coordinate =
                    new Vector2Int(x, y);

                cellUI.Initialize(
                    coordinate,
                    emptyColor,
                    OnCellClicked,
                    OnCellPointerEntered,
                    OnCellPointerExited,
                    OnCellPointerDown,
                    OnCellPointerUp
                );

                cells.Add(cellUI);
                cellCoordinates.Add(coordinate);
            }
        }
    }

    private void Refresh()
    {
        if (playerInventory == null ||
            playerInventory.Grid == null)
        {
            return;
        }

        InventoryGrid grid =
            playerInventory.Grid;

        PlacedInventoryItem heldItem =
            HeldItem;

        bool hasHeldItem =
            heldItem != null &&
            heldItem.ItemData != null;

        bool hoverIsValid =
            hoveredCoordinate.x >= 0 &&
            hoveredCoordinate.y >= 0;

        bool inventoryOpen =
            InventoryMenuController.IsInventoryOpen;

        Vector2Int placementOrigin =
            hoverIsValid
            ? GetHeldPlacementOrigin(hoveredCoordinate)
            : new Vector2Int(-999, -999);

        bool canPlaceHeldItem =
            hasHeldItem &&
            hoverIsValid &&
            inventoryOpen &&
            playerInventory.CanPlaceHeldItem(
                placementOrigin.x,
                placementOrigin.y
            );

        for (int i = 0; i < cells.Count; i++)
        {
            InventoryCellUI cell =
                cells[i];

            if (cell == null)
                continue;

            Vector2Int coordinate =
                cellCoordinates[i];

            if (hasHeldItem &&
                hoverIsValid &&
                inventoryOpen &&
                IsHeldItemPreview(coordinate, placementOrigin))
            {
                cell.SetColor(
                    canPlaceHeldItem
                    ? validPlacementColor
                    : invalidPlacementColor
                );

                continue;
            }

            if (isDraggingItem && IsOriginalFootprint(coordinate))
            {
                cell.SetColor(dragOriginalGhostColor);
                continue;
            }

            ItemData item =
                grid.GetCell(
                    coordinate.x,
                    coordinate.y
                );

            cell.SetColor(
                item == null
                ? emptyColor
                : occupiedColor
            );
        }

        BuildItemOutlines();
    }

    private void OnCellClicked(Vector2Int coordinate)
    {
        if (suppressNextClick)
        {
            suppressNextClick = false;
            return;
        }

        if (playerInventory == null ||
            playerInventory.Grid == null)
        {
            return;
        }

        if (playerInventory.IsHoldingItem)
        {
            Vector2Int placementOrigin =
                GetHeldPlacementOrigin(coordinate);

            bool placed =
                playerInventory.TryPlaceHeldItem(
                    placementOrigin.x,
                    placementOrigin.y
                );

            if (!placed)
            {
                Debug.Log(
                    "Cannot place held item at: " +
                    placementOrigin
                );
            }
            else
            {
                heldGrabOffset = Vector2Int.zero;
            }

            Refresh();
            return;
        }

        TryPickUpItem(coordinate);
    }

    private void OnCellPointerDown(Vector2Int coordinate)
    {
        if (!InventoryMenuController.IsInventoryOpen)
            return;

        if (Mouse.current == null)
            return;

        pointerIsDown = true;
        pendingDragPickup = false;
        isDraggingItem = false;

        pointerDownScreenPosition =
            Mouse.current.position.ReadValue();

        pointerDownCoordinate = coordinate;

        if (playerInventory == null ||
            playerInventory.Grid == null ||
            playerInventory.IsHoldingItem)
        {
            return;
        }

        PlacedInventoryItem itemAtCell =
            playerInventory.Grid.GetPlacedItem(
                coordinate.x,
                coordinate.y
            );

        pendingDragPickup =
            itemAtCell != null;
    }

    private void OnCellPointerUp(Vector2Int coordinate)
    {
        if (isDraggingItem)
        {
            CompleteDragDrop(hoveredCoordinate);
            return;
        }

        pointerIsDown = false;
        pendingDragPickup = false;
    }

    private void TryPickUpItem(Vector2Int coordinate)
    {
        PlacedInventoryItem pickedItem =
            playerInventory.TryPickUpItemAt(
                coordinate.x,
                coordinate.y
            );

        if (pickedItem == null)
        {
            Debug.Log(
                "Clicked empty inventory cell: " +
                coordinate
            );

            return;
        }

        heldGrabOffset =
            coordinate - pickedItem.Position;

        ClampHeldGrabOffsetToHeldItem();

        Debug.Log(
            "Picked up item: " +
            pickedItem.ItemData.itemName +
            " with grab offset: " +
            heldGrabOffset
        );

        Refresh();
    }

    private void OnCellPointerEntered(Vector2Int coordinate)
    {
        hoveredCoordinate = coordinate;
        Refresh();
    }

    private void OnCellPointerExited(Vector2Int coordinate)
    {
        if (hoveredCoordinate == coordinate)
        {
            hoveredCoordinate =
                new Vector2Int(-1, -1);

            Refresh();
        }
    }

    private Vector2Int GetHeldPlacementOrigin(Vector2Int hoveredCell)
    {
        return hoveredCell - heldGrabOffset;
    }

    private bool IsHeldItemPreview(
        Vector2Int coordinate,
        Vector2Int origin)
    {
        PlacedInventoryItem heldItem =
            HeldItem;

        if (heldItem == null ||
            heldItem.ItemData == null)
        {
            return false;
        }

        int localX =
            coordinate.x - origin.x;

        int localY =
            coordinate.y - origin.y;

        if (localX < 0 ||
            localY < 0 ||
            localX >= heldItem.Width ||
            localY >= heldItem.Height)
        {
            return false;
        }

        return heldItem.ItemData.IsCellOccupied(
            localX,
            localY,
            heldItem.RotationSteps
        );
    }

    private bool IsOriginalFootprint(Vector2Int coordinate)
    {
        if (!isDraggingItem)
            return false;

        PlacedInventoryItem heldItem =
            HeldItem;

        if (heldItem == null ||
            heldItem.ItemData == null)
        {
            return false;
        }

        ItemData itemData =
            heldItem.ItemData;

        int localX =
            coordinate.x - dragOriginalPosition.x;

        int localY =
            coordinate.y - dragOriginalPosition.y;

        int originalWidth =
            itemData.GetWidth(dragOriginalRotationSteps);

        int originalHeight =
            itemData.GetHeight(dragOriginalRotationSteps);

        if (localX < 0 ||
            localY < 0 ||
            localX >= originalWidth ||
            localY >= originalHeight)
        {
            return false;
        }

        return itemData.IsCellOccupied(
            localX,
            localY,
            dragOriginalRotationSteps
        );
    }

    private void HandleHeldItemChanged()
    {
        BuildHeldItemPreview();
        Refresh();
    }

    private void CreateHeldPreviewRoot()
    {
        if (rootCanvas == null)
            return;

        GameObject previewObject =
            new GameObject(
                "InventoryMouseHeldItemPreview",
                typeof(RectTransform),
                typeof(CanvasGroup),
                typeof(GridLayoutGroup)
            );

        previewObject.transform.SetParent(
            rootCanvas.transform,
            false
        );

        heldPreviewRoot =
            previewObject.GetComponent<RectTransform>();

        heldPreviewRoot.anchorMin =
            new Vector2(0.5f, 0.5f);

        heldPreviewRoot.anchorMax =
            new Vector2(0.5f, 0.5f);

        heldPreviewRoot.pivot =
            new Vector2(0f, 1f);

        heldPreviewCanvasGroup =
            previewObject.GetComponent<CanvasGroup>();

        heldPreviewCanvasGroup.blocksRaycasts = false;
        heldPreviewCanvasGroup.interactable = false;

        heldPreviewLayoutGroup =
            previewObject.GetComponent<GridLayoutGroup>();

        heldPreviewLayoutGroup.startCorner =
            GridLayoutGroup.Corner.UpperLeft;

        heldPreviewLayoutGroup.startAxis =
            GridLayoutGroup.Axis.Horizontal;

        heldPreviewLayoutGroup.childAlignment =
            TextAnchor.UpperLeft;

        heldPreviewLayoutGroup.constraint =
            GridLayoutGroup.Constraint.FixedColumnCount;

        if (gridLayoutGroup != null)
        {
            heldPreviewLayoutGroup.cellSize =
                gridLayoutGroup.cellSize;

            heldPreviewLayoutGroup.spacing =
                gridLayoutGroup.spacing;
        }
        else
        {
            heldPreviewLayoutGroup.cellSize =
                new Vector2(40f, 40f);

            heldPreviewLayoutGroup.spacing =
                new Vector2(2f, 2f);
        }

        previewObject.SetActive(false);
    }

    private void BuildHeldItemPreview()
    {
        if (heldPreviewRoot == null ||
            cellPrefab == null)
        {
            return;
        }

        foreach (Transform child in heldPreviewRoot)
        {
            Destroy(child.gameObject);
        }

        PlacedInventoryItem heldItem =
            HeldItem;

        if (heldItem == null ||
            heldItem.ItemData == null)
        {
            heldPreviewRoot.gameObject.SetActive(false);
            return;
        }

        heldPreviewLayoutGroup.constraintCount =
            heldItem.Width;

        ItemData itemData =
            heldItem.ItemData;

        for (int y = heldItem.Height - 1; y >= 0; y--)
        {
            for (int x = 0; x < heldItem.Width; x++)
            {
                GameObject cellObject =
                    Instantiate(
                        cellPrefab,
                        heldPreviewRoot
                    );

                InventoryCellUI cellUI =
                    cellObject.GetComponent<InventoryCellUI>();

                if (cellUI != null)
                    cellUI.enabled = false;

                Button button =
                    cellObject.GetComponent<Button>();

                if (button != null)
                    button.interactable = false;

                Image image =
                    cellObject.GetComponent<Image>();

                if (image != null)
                {
                    bool occupied =
                        itemData.IsCellOccupied(
                            x,
                            y,
                            heldItem.RotationSteps
                        );

                    image.raycastTarget = false;

                    image.color =
                        occupied
                        ? heldPreviewColor
                        : new Color(0f, 0f, 0f, 0f);
                }
            }
        }

        UpdateHeldPreviewVisibility();
        UpdateHeldPreviewPosition();
    }

    private void UpdateHeldPreviewVisibility()
    {
        if (heldPreviewRoot == null)
            return;

        bool shouldShow =
            HeldItem != null &&
            InventoryMenuController.IsInventoryOpen;

        if (heldPreviewRoot.gameObject.activeSelf != shouldShow)
        {
            heldPreviewRoot.gameObject.SetActive(shouldShow);
        }
    }

    private void UpdateHeldPreviewPosition()
    {
        if (heldPreviewRoot == null ||
            !heldPreviewRoot.gameObject.activeSelf ||
            rootCanvas == null ||
            Mouse.current == null)
        {
            return;
        }

        Vector2 screenPosition =
            Mouse.current.position.ReadValue();

        RectTransform canvasRect =
            rootCanvas.transform as RectTransform;

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

        heldPreviewRoot.anchoredPosition =
            localPoint -
            GetHeldPreviewGrabPoint() +
            heldPreviewOffset;
    }

    private Vector2 GetHeldPreviewGrabPoint()
    {
        PlacedInventoryItem heldItem =
            HeldItem;

        if (heldItem == null ||
            heldPreviewLayoutGroup == null)
        {
            return Vector2.zero;
        }

        Vector2 cellSize =
            heldPreviewLayoutGroup.cellSize;

        Vector2 spacing =
            heldPreviewLayoutGroup.spacing;

        int visualRowFromTop =
            heldItem.Height - 1 - heldGrabOffset.y;

        float x =
            heldGrabOffset.x * (cellSize.x + spacing.x) +
            cellSize.x * 0.5f;

        float y =
            -visualRowFromTop * (cellSize.y + spacing.y) -
            cellSize.y * 0.5f;

        return new Vector2(x, y);
    }

    private void BuildItemOutlines()
    {
        if (itemOutline == null ||
            gridLayoutGroup == null ||
            playerInventory == null ||
            playerInventory.Grid == null)
        {
            return;
        }

        for (int i = itemOutline.childCount - 1; i >= 0; i--)
        {
            Destroy(itemOutline.GetChild(i).gameObject);
        }

        HashSet<PlacedInventoryItem> outlinedItems =
            new HashSet<PlacedInventoryItem>();

        InventoryGrid grid =
            playerInventory.Grid;

        for (int y = 0; y < grid.Height; y++)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                PlacedInventoryItem placedItem =
                    grid.GetPlacedItem(x, y);

                if (placedItem == null)
                    continue;

                if (outlinedItems.Contains(placedItem))
                    continue;

                outlinedItems.Add(placedItem);

                DrawItemOutline(
                    placedItem.ItemData,
                    placedItem.Position,
                    placedItem.RotationSteps,
                    itemOutlineColor
                );
            }
        }

        if (isDraggingItem &&
            HeldItem != null &&
            HeldItem.ItemData != null)
        {
            DrawItemOutline(
                HeldItem.ItemData,
                dragOriginalPosition,
                dragOriginalRotationSteps,
                dragOriginalOutlineColor
            );
        }
    }

    private void DrawItemOutline(
        ItemData itemData,
        Vector2Int origin,
        int rotationSteps,
        Color color)
    {
        if (itemData == null)
            return;

        int width =
            itemData.GetWidth(rotationSteps);

        int height =
            itemData.GetHeight(rotationSteps);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (!itemData.IsCellOccupied(x, y, rotationSteps))
                    continue;

                bool topOpen =
                    !IsOccupiedInShape(itemData, x, y + 1, rotationSteps);

                bool bottomOpen =
                    !IsOccupiedInShape(itemData, x, y - 1, rotationSteps);

                bool leftOpen =
                    !IsOccupiedInShape(itemData, x - 1, y, rotationSteps);

                bool rightOpen =
                    !IsOccupiedInShape(itemData, x + 1, y, rotationSteps);

                if (topOpen)
                    DrawOutlineEdge(origin.x + x, origin.y + y, OutlineSide.Top, color);

                if (bottomOpen)
                    DrawOutlineEdge(origin.x + x, origin.y + y, OutlineSide.Bottom, color);

                if (leftOpen)
                    DrawOutlineEdge(origin.x + x, origin.y + y, OutlineSide.Left, color);

                if (rightOpen)
                    DrawOutlineEdge(origin.x + x, origin.y + y, OutlineSide.Right, color);

                if (fillPaddingBetweenCells)
                {
                    bool rightFilled =
                        IsOccupiedInShape(itemData, x + 1, y, rotationSteps);

                    bool downFilled =
                        IsOccupiedInShape(itemData, x, y - 1, rotationSteps);

                    if (topOpen &&
                        rightFilled &&
                        !IsOccupiedInShape(itemData, x + 1, y + 1, rotationSteps))
                    {
                        DrawBridge(origin.x + x, origin.y + y, OutlineSide.Top, color);
                    }

                    if (bottomOpen &&
                        rightFilled &&
                        !IsOccupiedInShape(itemData, x + 1, y - 1, rotationSteps))
                    {
                        DrawBridge(origin.x + x, origin.y + y, OutlineSide.Bottom, color);
                    }

                    if (leftOpen &&
                        downFilled &&
                        !IsOccupiedInShape(itemData, x - 1, y - 1, rotationSteps))
                    {
                        DrawBridge(origin.x + x, origin.y + y, OutlineSide.Left, color);
                    }

                    if (rightOpen &&
                        downFilled &&
                        !IsOccupiedInShape(itemData, x + 1, y - 1, rotationSteps))
                    {
                        DrawBridge(origin.x + x, origin.y + y, OutlineSide.Right, color);
                    }
                }

                if (topOpen && leftOpen)
                    DrawCorner(origin.x + x, origin.y + y, OutlineCorner.TopLeft, color);

                if (topOpen && rightOpen)
                    DrawCorner(origin.x + x, origin.y + y, OutlineCorner.TopRight, color);

                if (bottomOpen && leftOpen)
                    DrawCorner(origin.x + x, origin.y + y, OutlineCorner.BottomLeft, color);

                if (bottomOpen && rightOpen)
                    DrawCorner(origin.x + x, origin.y + y, OutlineCorner.BottomRight, color);
            }
        }

        if (fillPaddingBetweenCells)
        {
            DrawInnerCorners(
                itemData,
                origin,
                rotationSteps,
                color
            );
        }
    }

    private bool IsOccupiedInShape(
        ItemData itemData,
        int x,
        int y,
        int rotationSteps)
    {
        int width =
            itemData.GetWidth(rotationSteps);

        int height =
            itemData.GetHeight(rotationSteps);

        if (x < 0 ||
            y < 0 ||
            x >= width ||
            y >= height)
        {
            return false;
        }

        return itemData.IsCellOccupied(x, y, rotationSteps);
    }

    private enum OutlineSide
    {
        Top,
        Bottom,
        Left,
        Right
    }

    private enum OutlineCorner
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    private void DrawOutlineEdge(
        int gridX,
        int gridY,
        OutlineSide side,
        Color color)
    {
        Vector2 cellSize = gridLayoutGroup.cellSize;
        Vector2 spacing = gridLayoutGroup.spacing;
        RectOffset padding = gridLayoutGroup.padding;

        float halfSpacingX = fillPaddingBetweenCells ? spacing.x * 0.5f : 0f;
        float halfSpacingY = fillPaddingBetweenCells ? spacing.y * 0.5f : 0f;

        int rowFromTop =
            playerInventory.Grid.Height - 1 - gridY;

        float cellLeft =
            padding.left +
            gridX * (cellSize.x + spacing.x);

        float cellTop =
            -padding.top -
            rowFromTop * (cellSize.y + spacing.y);

        Vector2 position;
        Vector2 size;

        switch (side)
        {
            case OutlineSide.Top:
                position = new Vector2(
                    cellLeft + cellSize.x * 0.5f,
                    cellTop + halfSpacingY
                );

                size = new Vector2(
                    cellSize.x,
                    itemOutlineThickness
                );
                break;

            case OutlineSide.Bottom:
                position = new Vector2(
                    cellLeft + cellSize.x * 0.5f,
                    cellTop - cellSize.y - halfSpacingY
                );

                size = new Vector2(
                    cellSize.x,
                    itemOutlineThickness
                );
                break;

            case OutlineSide.Left:
                position = new Vector2(
                    cellLeft - halfSpacingX,
                    cellTop - cellSize.y * 0.5f
                );

                size = new Vector2(
                    itemOutlineThickness,
                    cellSize.y
                );
                break;

            default:
                position = new Vector2(
                    cellLeft + cellSize.x + halfSpacingX,
                    cellTop - cellSize.y * 0.5f
                );

                size = new Vector2(
                    itemOutlineThickness,
                    cellSize.y
                );
                break;
        }

        CreateOutlineRect(position, size, color);
    }

    private void DrawBridge(
        int gridX,
        int gridY,
        OutlineSide side,
        Color color)
    {
        Vector2 cellSize = gridLayoutGroup.cellSize;
        Vector2 spacing = gridLayoutGroup.spacing;
        RectOffset padding = gridLayoutGroup.padding;

        if (!fillPaddingBetweenCells)
            return;

        int rowFromTop =
            playerInventory.Grid.Height - 1 - gridY;

        float cellLeft =
            padding.left +
            gridX * (cellSize.x + spacing.x);

        float cellTop =
            -padding.top -
            rowFromTop * (cellSize.y + spacing.y);

        Vector2 position;
        Vector2 size;

        switch (side)
        {
            case OutlineSide.Top:
                if (spacing.x <= 0f)
                    return;

                position = new Vector2(
                    cellLeft + cellSize.x + spacing.x * 0.5f,
                    cellTop + spacing.y * 0.5f
                );

                size = new Vector2(
                    spacing.x,
                    itemOutlineThickness
                );
                break;

            case OutlineSide.Bottom:
                if (spacing.x <= 0f)
                    return;

                position = new Vector2(
                    cellLeft + cellSize.x + spacing.x * 0.5f,
                    cellTop - cellSize.y - spacing.y * 0.5f
                );

                size = new Vector2(
                    spacing.x,
                    itemOutlineThickness
                );
                break;

            case OutlineSide.Left:
                if (spacing.y <= 0f)
                    return;

                position = new Vector2(
                    cellLeft - spacing.x * 0.5f,
                    cellTop - cellSize.y - spacing.y * 0.5f
                );

                size = new Vector2(
                    itemOutlineThickness,
                    spacing.y
                );
                break;

            default:
                if (spacing.y <= 0f)
                    return;

                position = new Vector2(
                    cellLeft + cellSize.x + spacing.x * 0.5f,
                    cellTop - cellSize.y - spacing.y * 0.5f
                );

                size = new Vector2(
                    itemOutlineThickness,
                    spacing.y
                );
                break;
        }

        CreateOutlineRect(position, size, color);
    }

    private void DrawCorner(
        int gridX,
        int gridY,
        OutlineCorner corner,
        Color color)
    {
        Vector2 cellSize = gridLayoutGroup.cellSize;
        Vector2 spacing = gridLayoutGroup.spacing;
        RectOffset padding = gridLayoutGroup.padding;

        float halfSpacingX = fillPaddingBetweenCells ? spacing.x * 0.5f : 0f;
        float halfSpacingY = fillPaddingBetweenCells ? spacing.y * 0.5f : 0f;

        int rowFromTop =
            playerInventory.Grid.Height - 1 - gridY;

        float cellLeft =
            padding.left +
            gridX * (cellSize.x + spacing.x);

        float cellTop =
            -padding.top -
            rowFromTop * (cellSize.y + spacing.y);

        Vector2 position;

        switch (corner)
        {
            case OutlineCorner.TopLeft:
                position = new Vector2(
                    cellLeft - halfSpacingX,
                    cellTop + halfSpacingY
                );
                break;

            case OutlineCorner.TopRight:
                position = new Vector2(
                    cellLeft + cellSize.x + halfSpacingX,
                    cellTop + halfSpacingY
                );
                break;

            case OutlineCorner.BottomLeft:
                position = new Vector2(
                    cellLeft - halfSpacingX,
                    cellTop - cellSize.y - halfSpacingY
                );
                break;

            default:
                position = new Vector2(
                    cellLeft + cellSize.x + halfSpacingX,
                    cellTop - cellSize.y - halfSpacingY
                );
                break;
        }

        CreateOutlineRect(
            position,
            new Vector2(itemOutlineThickness, itemOutlineThickness),
            color
        );
    }

    private void DrawInnerCorners(
        ItemData itemData,
        Vector2Int origin,
        int rotationSteps,
        Color color)
    {
        if (itemData == null)
            return;

        int width =
            itemData.GetWidth(rotationSteps);

        int height =
            itemData.GetHeight(rotationSteps);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (IsOccupiedInShape(itemData, x, y, rotationSteps))
                    continue;

                bool leftFilled =
                    IsOccupiedInShape(itemData, x - 1, y, rotationSteps);

                bool rightFilled =
                    IsOccupiedInShape(itemData, x + 1, y, rotationSteps);

                bool upFilled =
                    IsOccupiedInShape(itemData, x, y + 1, rotationSteps);

                bool downFilled =
                    IsOccupiedInShape(itemData, x, y - 1, rotationSteps);

                if (rightFilled && downFilled)
                {
                    DrawGapCorner(
                        origin.x + x,
                        origin.y + y,
                        OutlineCorner.BottomRight,
                        color
                    );
                }

                if (leftFilled && downFilled)
                {
                    DrawGapCorner(
                        origin.x + x,
                        origin.y + y,
                        OutlineCorner.BottomLeft,
                        color
                    );
                }

                if (rightFilled && upFilled)
                {
                    DrawGapCorner(
                        origin.x + x,
                        origin.y + y,
                        OutlineCorner.TopRight,
                        color
                    );
                }

                if (leftFilled && upFilled)
                {
                    DrawGapCorner(
                        origin.x + x,
                        origin.y + y,
                        OutlineCorner.TopLeft,
                        color
                    );
                }
            }
        }
    }

    private void DrawGapCorner(
        int gridX,
        int gridY,
        OutlineCorner corner,
        Color color)
    {
        Vector2 cellSize = gridLayoutGroup.cellSize;
        Vector2 spacing = gridLayoutGroup.spacing;
        RectOffset padding = gridLayoutGroup.padding;

        int rowFromTop =
            playerInventory.Grid.Height - 1 - gridY;

        float cellLeft =
            padding.left +
            gridX * (cellSize.x + spacing.x);

        float cellTop =
            -padding.top -
            rowFromTop * (cellSize.y + spacing.y);

        Vector2 position;

        switch (corner)
        {
            case OutlineCorner.TopLeft:
                position = new Vector2(
                    cellLeft - spacing.x * 0.5f,
                    cellTop + spacing.y * 0.5f
                );
                break;

            case OutlineCorner.TopRight:
                position = new Vector2(
                    cellLeft + cellSize.x + spacing.x * 0.5f,
                    cellTop + spacing.y * 0.5f
                );
                break;

            case OutlineCorner.BottomLeft:
                position = new Vector2(
                    cellLeft - spacing.x * 0.5f,
                    cellTop - cellSize.y - spacing.y * 0.5f
                );
                break;

            default:
                position = new Vector2(
                    cellLeft + cellSize.x + spacing.x * 0.5f,
                    cellTop - cellSize.y - spacing.y * 0.5f
                );
                break;
        }

        CreateOutlineRect(
            position,
            new Vector2(itemOutlineThickness, itemOutlineThickness),
            color
        );
    }

    private void CreateOutlineRect(
        Vector2 position,
        Vector2 size,
        Color color)
    {
        GameObject edgeObject =
            new GameObject(
                "ItemOutlinePiece",
                typeof(RectTransform),
                typeof(Image)
            );

        edgeObject.transform.SetParent(itemOutline, false);

        RectTransform rect =
            edgeObject.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image =
            edgeObject.GetComponent<Image>();

        image.color = color;
        image.raycastTarget = false;
    }
}