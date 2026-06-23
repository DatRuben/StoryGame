using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryGridUI : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerStorageContainerInteract playerStorageContainerInteract;
    [SerializeField] private Transform cellParent;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private RectTransform itemOutline;

    [Header("Drag Detection")]
    [SerializeField] private float dragStartDistance = 12f;

    [Header("Colors")]
    [SerializeField] private Color emptyColor = new Color(0f, 0f, 0f, 0.35f);
    [SerializeField] private Color occupiedColor = new Color(1f, 1f, 1f, 0.85f);
    [SerializeField] private Color validPlacementColor = new Color(0.2f, 1f, 0.2f, 0.85f);
    [SerializeField] private Color partialStackPlacementColor = new Color(1f, 0.85f, 0.15f, 0.85f);
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
    private RectTransform heldPreviewOutlineRoot;

    private readonly List<InventoryCellUI> cells = new List<InventoryCellUI>();
    private readonly List<Vector2Int> cellCoordinates = new List<Vector2Int>();

    private Vector2Int hoveredCoordinate = new Vector2Int(-1, -1);
    private Vector2Int heldGrabOffset = Vector2Int.zero;
    private bool centerHeldPreviewOnMouse = false;

    private bool pointerIsDown;
    private bool pendingDragPickup;
    private bool isDraggingItem;
    private bool suppressNextClick;
    private bool wasShowingHeldPreview;
    private Vector2 lastHeldPreviewMousePosition;

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

        if (playerStorageContainerInteract == null &&
            playerInventory != null)
        {
            playerStorageContainerInteract =
                playerInventory.GetComponent<PlayerStorageContainerInteract>();
        }
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
        RefreshHeldPreviewIfNeeded();
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

        if (centerHeldPreviewOnMouse)
        {
            CenterHeldItemOnMouse();
        }
        else
        {
            ClampHeldGrabOffsetToHeldItem();
        }

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
            Vector2Int releaseCoordinate =
                hoveredCoordinate;

            if (!IsValidGridCoordinate(releaseCoordinate))
            {
                TryGetGridCoordinateFromScreenPoint(
                    Mouse.current.position.ReadValue(),
                    out releaseCoordinate
                );
            }

            CompleteDragDrop(releaseCoordinate);
        }
    }

    private void StartDragPickup(Vector2Int coordinate)
    {
        if (playerInventory == null ||
            playerInventory.Grid == null)
        {
            return;
        }

        if (IsQuickTransferHeld() &&
            !playerInventory.IsHoldingItem)
        {
            if (playerStorageContainerInteract != null)
            {
                playerStorageContainerInteract.TryQuickTransferPlayerItemToOpenContainer(
                    playerInventory,
                    coordinate
                );
            }

            heldGrabOffset = Vector2Int.zero;
            centerHeldPreviewOnMouse = false;

            Refresh();
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

        CenterHeldItemOnMouse();

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

        if (!StorageContainerGridUI.HasPendingStorageDrag &&
            Mouse.current != null &&
            playerStorageContainerInteract != null)
        {
            bool placedInStorage =
                playerStorageContainerInteract
                    .TryPlaceHeldPlayerItemInOpenContainerAtScreenPosition(
                        Mouse.current.position.ReadValue()
                    );

            if (placedInStorage)
            {
                heldGrabOffset = Vector2Int.zero;
                centerHeldPreviewOnMouse = false;
                Refresh();
                return;
            }
        }

        if (releaseIsValid)
        {
            bool merged =
                playerInventory.TryMergeHeldItemIntoStackAt(
                    releaseCoordinate.x,
                    releaseCoordinate.y
                );

            if (merged)
            {
                if (StorageContainerGridUI.HasPendingStorageDrag)
                    StorageContainerGridUI.CommitPendingStorageDrag();

                if (!playerInventory.IsHoldingItem)
                {
                    heldGrabOffset = Vector2Int.zero;
                    centerHeldPreviewOnMouse = false;
                }
                else
                {
                    CenterHeldItemOnMouse();
                }

                Refresh();
                return;
            }

            Vector2Int placementOrigin =
                GetHeldPlacementOrigin(releaseCoordinate);

            bool placed =
                playerInventory.TryPlaceHeldItem(
                    placementOrigin.x,
                    placementOrigin.y
                );

            if (placed)
            {
                if (StorageContainerGridUI.HasPendingStorageDrag)
                    StorageContainerGridUI.CommitPendingStorageDrag();

                heldGrabOffset = Vector2Int.zero;
                centerHeldPreviewOnMouse = false;
                Refresh();
                return;
            }
        }

        if (StorageContainerGridUI.HasPendingStorageDrag)
        {
            StorageContainerGridUI.CancelPendingStorageDrag(
                playerInventory
            );

            heldGrabOffset = Vector2Int.zero;
            centerHeldPreviewOnMouse = false;
            Refresh();
            return;
        }

        ReturnDraggedItemToOriginalPosition();
    }

    private void ReturnDraggedItemToOriginalPosition()
    {
        isDraggingItem = false;
        pointerIsDown = false;
        pendingDragPickup = false;
        suppressNextClick = true;

        if (StorageContainerGridUI.HasPendingStorageDrag)
        {
            StorageContainerGridUI.CancelPendingStorageDrag(
                playerInventory
            );

            heldGrabOffset = Vector2Int.zero;
            centerHeldPreviewOnMouse = false;
            Refresh();
            return;
        }

        if (playerInventory == null ||
            !playerInventory.IsHoldingItem ||
            playerInventory.HeldItem == null)
        {
            heldGrabOffset = Vector2Int.zero;
            centerHeldPreviewOnMouse = false;
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
        centerHeldPreviewOnMouse = false;
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
                    OnCellPointerUp,
                    OnCellRightClicked
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
                TryGetStackPreviewColor(
                    coordinate,
                    out Color stackPreviewColor))
            {
                cell.SetColor(stackPreviewColor);

                PlacedInventoryItem placedItemForQuantity =
                    grid.GetPlacedItem(
                        coordinate.x,
                        coordinate.y
                    );

                cell.SetQuantityText(
                    GetQuantityTextForCell(
                        placedItemForQuantity,
                        coordinate
                    )
                );

                continue;
            }

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

                cell.SetQuantityText("");
                continue;
            }

            if (isDraggingItem && IsOriginalFootprint(coordinate))
            {
                cell.SetColor(dragOriginalGhostColor);
                cell.SetQuantityText("");
                continue;
            }

            PlacedInventoryItem placedItem =
                grid.GetPlacedItem(
                    coordinate.x,
                    coordinate.y
                );

            ItemData item =
                placedItem != null
                    ? placedItem.ItemData
                    : null;

            cell.SetColor(
                item == null
                ? emptyColor
                : occupiedColor
            );

            cell.SetQuantityText(
                GetQuantityTextForCell(
                    placedItem,
                    coordinate
                )
            );
        }

        BuildItemOutlines();
    }


    private string GetQuantityTextForCell(
        PlacedInventoryItem placedItem,
        Vector2Int coordinate)
    {
        return InventoryQuantityTextUtility.GetTextForCell(
            placedItem,
            coordinate
        );
    }

    private bool TryGetStackPreviewColor(
        Vector2Int coordinate,
        out Color previewColor)
    {
        previewColor = invalidPlacementColor;

        if (playerInventory == null ||
            playerInventory.Grid == null)
        {
            return false;
        }

        PlacedInventoryItem heldItem =
            HeldItem;

        if (!IsValidGridCoordinate(hoveredCoordinate))
            return false;

        return InventoryStackPreviewUtility.TryGetPreviewColor(
            playerInventory.Grid,
            heldItem,
            hoveredCoordinate,
            coordinate,
            validPlacementColor,
            partialStackPlacementColor,
            invalidPlacementColor,
            out previewColor
        );
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (suppressNextClick)
            suppressNextClick = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (playerInventory == null ||
            playerInventory.Grid == null)
        {
            return;
        }

        if (!InventoryMenuController.IsInventoryOpen)
            return;

        if (!playerInventory.IsHoldingItem)
            return;

        if (!TryGetGridCoordinateFromScreenPoint(
                eventData.position,
                out Vector2Int coordinate))
        {
            return;
        }

        OnCellClicked(coordinate);
    }


    public bool TryDropHeldItemAtScreenPoint(
        Vector2 screenPosition)
    {
        if (playerInventory == null ||
            playerInventory.Grid == null ||
            !playerInventory.IsHoldingItem)
        {
            return false;
        }

        if (!InventoryMenuController.IsInventoryOpen)
            return false;

        if (!TryGetGridCoordinateFromScreenPoint(
                screenPosition,
                out Vector2Int coordinate))
        {
            return false;
        }

        bool hadPendingStorageDrag =
            StorageContainerGridUI.HasPendingStorageDrag;

        PlacedInventoryItem heldItemBefore =
            playerInventory.HeldItem;

        int quantityBefore =
            heldItemBefore != null
                ? heldItemBefore.Quantity
                : 0;

        OnCellClicked(coordinate);

        if (!hadPendingStorageDrag)
            return true;

        if (!StorageContainerGridUI.HasPendingStorageDrag)
            return true;

        if (!playerInventory.IsHoldingItem)
            return true;

        if (playerInventory.HeldItem != heldItemBefore)
            return true;

        if (playerInventory.HeldItem != null &&
            playerInventory.HeldItem.Quantity != quantityBefore)
        {
            return true;
        }

        return false;
    }

    private bool IsQuickTransferHeld()
    {
        if (Keyboard.current == null)
            return false;

        return Keyboard.current.leftCtrlKey.isPressed ||
               Keyboard.current.rightCtrlKey.isPressed;
    }

    private void OnCellClicked(Vector2Int coordinate)
    {
        if (suppressNextClick)
        {
            if (!StorageContainerGridUI.HasPendingStorageDrag)
            {
                suppressNextClick = false;
                return;
            }

            suppressNextClick = false;
        }

        if (playerInventory == null ||
            playerInventory.Grid == null)
        {
            return;
        }

        if (IsQuickTransferHeld() &&
            !playerInventory.IsHoldingItem)
        {
            if (playerStorageContainerInteract != null)
            {
                bool transferred =
                    playerStorageContainerInteract.TryQuickTransferPlayerItemToOpenContainer(
                        playerInventory,
                        coordinate
                    );

                if (transferred)
                {
                    heldGrabOffset = Vector2Int.zero;
                    centerHeldPreviewOnMouse = false;
                    Refresh();
                    return;
                }
            }
        }

        if (playerInventory.IsHoldingItem)
        {
            bool merged =
                playerInventory.TryMergeHeldItemIntoStackAt(
                    coordinate.x,
                    coordinate.y
                );

            if (merged)
            {
                if (StorageContainerGridUI.HasPendingStorageDrag)
                    StorageContainerGridUI.CommitPendingStorageDrag();

                if (!playerInventory.IsHoldingItem)
                {
                    heldGrabOffset = Vector2Int.zero;
                    centerHeldPreviewOnMouse = false;
                }
                else
                {
                    CenterHeldItemOnMouse();
                }

                Refresh();
                return;
            }

            Vector2Int placementOrigin =
                GetHeldPlacementOrigin(coordinate);

            bool placed =
                playerInventory.TryPlaceHeldItem(
                    placementOrigin.x,
                    placementOrigin.y
                );

            if (!placed)
            {
                if (StorageContainerGridUI.HasPendingStorageDrag)
                {
                    StorageContainerGridUI.CancelPendingStorageDrag(
                        playerInventory
                    );

                    heldGrabOffset = Vector2Int.zero;
                    centerHeldPreviewOnMouse = false;
                    Refresh();
                    return;
                }

                Debug.Log(
                    "Cannot place held item at: " +
                    placementOrigin
                );
            }
            else
            {
                if (StorageContainerGridUI.HasPendingStorageDrag)
                    StorageContainerGridUI.CommitPendingStorageDrag();

                heldGrabOffset = Vector2Int.zero;
                centerHeldPreviewOnMouse = false;
            }

            Refresh();
            return;
        }

        TryPickUpItem(coordinate);
    }

    private void OnCellRightClicked(Vector2Int coordinate)
    {
        if (!InventoryMenuController.IsInventoryOpen)
            return;

        if (playerInventory == null ||
            playerInventory.Grid == null)
        {
            return;
        }

        if (playerInventory.IsHoldingItem)
        {
            bool placedOne =
                playerInventory.TryPlaceOneHeldItem(
                    coordinate.x,
                    coordinate.y
                );

            if (!placedOne)
                return;

            if (!playerInventory.IsHoldingItem)
            {
                heldGrabOffset = Vector2Int.zero;
                centerHeldPreviewOnMouse = false;
            }
            else
            {
                CenterHeldItemOnMouse();
            }

            Refresh();
            return;
        }

        bool split =
            playerInventory.TrySplitStackAt(
                coordinate.x,
                coordinate.y
            );

        if (!split)
            return;

        CenterHeldItemOnMouse();
        Refresh();
    }

    private void OnCellPointerDown(Vector2Int coordinate)
    {
        if (!InventoryMenuController.IsInventoryOpen)
            return;

        if (Mouse.current == null)
            return;

        if (suppressNextClick)
            suppressNextClick = false;

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

        CenterHeldItemOnMouse();

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
        if (hoveredCoordinate != coordinate)
            return;

        if (playerInventory != null &&
            playerInventory.IsHoldingItem &&
            Mouse.current != null &&
            TryGetGridCoordinateFromScreenPoint(
                Mouse.current.position.ReadValue(),
                out Vector2Int mouseCoordinate))
        {
            hoveredCoordinate = mouseCoordinate;
            Refresh();
            return;
        }

        hoveredCoordinate =
            new Vector2Int(-1, -1);

        Refresh();
    }

    private bool TryGetGridCoordinateFromScreenPoint(
        Vector2 screenPosition,
        out Vector2Int coordinate)
    {
        coordinate = new Vector2Int(-1, -1);

        if (playerInventory == null ||
            playerInventory.Grid == null ||
            cellParent == null ||
            gridLayoutGroup == null ||
            rootCanvas == null)
        {
            return false;
        }

        RectTransform cellParentRect =
            cellParent as RectTransform;

        if (cellParentRect == null)
            return false;

        Camera canvasCamera =
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : rootCanvas.worldCamera;

        bool hasPoint =
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                cellParentRect,
                screenPosition,
                canvasCamera,
                out Vector2 localPoint
            );

        if (!hasPoint)
            return false;

        Rect rect =
            cellParentRect.rect;

        float xFromLeft =
            localPoint.x - rect.xMin;

        float yFromTop =
            rect.yMax - localPoint.y;

        xFromLeft -= gridLayoutGroup.padding.left;
        yFromTop -= gridLayoutGroup.padding.top;

        if (xFromLeft < 0f ||
            yFromTop < 0f)
        {
            return false;
        }

        Vector2 cellSize =
            gridLayoutGroup.cellSize;

        Vector2 spacing =
            gridLayoutGroup.spacing;

        float pitchX =
            cellSize.x + spacing.x;

        float pitchY =
            cellSize.y + spacing.y;

        if (pitchX <= 0f ||
            pitchY <= 0f)
        {
            return false;
        }

        int x =
            GetNearestGridIndex(
                xFromLeft,
                cellSize.x,
                spacing.x,
                pitchX,
                playerInventory.Grid.Width
            );

        int rowFromTop =
            GetNearestGridIndex(
                yFromTop,
                cellSize.y,
                spacing.y,
                pitchY,
                playerInventory.Grid.Height
            );

        if (x < 0 ||
            rowFromTop < 0)
        {
            return false;
        }

        int y =
            playerInventory.Grid.Height - 1 - rowFromTop;

        coordinate =
            new Vector2Int(x, y);

        return IsValidGridCoordinate(coordinate);
    }

    private int GetNearestGridIndex(
        float distance,
        float cellSize,
        float spacing,
        float pitch,
        int count)
    {
        if (count <= 0)
            return -1;

        int index =
            Mathf.FloorToInt(distance / pitch);

        if (index < 0 ||
            index >= count)
        {
            return -1;
        }

        float insidePitch =
            distance - index * pitch;

        if (insidePitch > cellSize &&
            spacing > 0f)
        {
            float gapPosition =
                insidePitch - cellSize;

            if (gapPosition > spacing * 0.5f)
                index++;
        }

        if (index < 0 ||
            index >= count)
        {
            return -1;
        }

        return index;
    }

    private bool IsValidGridCoordinate(Vector2Int coordinate)
    {
        if (playerInventory == null ||
            playerInventory.Grid == null)
        {
            return false;
        }

        return coordinate.x >= 0 &&
               coordinate.y >= 0 &&
               coordinate.x < playerInventory.Grid.Width &&
               coordinate.y < playerInventory.Grid.Height;
    }

    private Vector2Int GetHeldPlacementOrigin(Vector2Int hoveredCell)
    {
        return hoveredCell - heldGrabOffset;
    }

    private void CenterHeldItemOnMouse()
    {
        PlacedInventoryItem heldItem =
            HeldItem;

        if (heldItem == null ||
            heldItem.ItemData == null)
        {
            heldGrabOffset = Vector2Int.zero;
            centerHeldPreviewOnMouse = false;
            return;
        }

        int centerX =
            Mathf.Max(
                0,
                heldItem.Width / 2
            );

        int centerY =
            Mathf.Max(
                0,
                heldItem.Height / 2
            );

        heldGrabOffset =
            new Vector2Int(
                centerX,
                centerY
            );

        centerHeldPreviewOnMouse = true;
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

        return InventoryShapeUtility.IsOccupiedInShape(
            heldItem.ItemData,
            coordinate.x - dragOriginalPosition.x,
            coordinate.y - dragOriginalPosition.y,
            dragOriginalRotationSteps
        );
    }

    private void HandleHeldItemChanged()
    {
        if (playerInventory != null &&
            playerInventory.ConsumeCenterHeldItemOnCursorRequest())
        {
            CenterHeldItemOnMouse();
        }

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

        GameObject outlineObject =
            new GameObject(
                "InventoryMouseHeldItemPreviewOutline",
                typeof(RectTransform)
            );

        outlineObject.transform.SetParent(
            rootCanvas.transform,
            false
        );

        heldPreviewOutlineRoot =
            outlineObject.GetComponent<RectTransform>();

        heldPreviewOutlineRoot.anchorMin =
            new Vector2(0.5f, 0.5f);

        heldPreviewOutlineRoot.anchorMax =
            new Vector2(0.5f, 0.5f);

        heldPreviewOutlineRoot.pivot =
            new Vector2(0f, 1f);

        outlineObject.SetActive(false);

        previewObject.SetActive(false);
    }

    private void BuildHeldItemPreview()
    {
        if (heldPreviewRoot == null ||
            cellPrefab == null)
        {
            return;
        }

        InventoryUIUtility.ClearChildren(
            heldPreviewRoot
        );

        PlacedInventoryItem heldItem =
            HeldItem;

        if (heldItem == null ||
            heldItem.ItemData == null)
        {
            heldPreviewRoot.gameObject.SetActive(false);

            if (heldPreviewOutlineRoot != null)
                heldPreviewOutlineRoot.gameObject.SetActive(false);

            centerHeldPreviewOnMouse = false;
            return;
        }

        heldPreviewLayoutGroup.constraintCount =
            heldItem.Width;

        ItemData itemData =
            heldItem.ItemData;

        bool quantityTextAssigned = false;

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

                bool occupied =
                    itemData.IsCellOccupied(
                        x,
                        y,
                        heldItem.RotationSteps
                    );

                Image image =
                    cellObject.GetComponent<Image>();

                if (image != null)
                {
                    image.raycastTarget = false;

                    image.color =
                        occupied
                        ? heldPreviewColor
                        : new Color(0f, 0f, 0f, 0f);
                }

                TextMeshProUGUI quantityText =
                    cellObject.GetComponentInChildren<TextMeshProUGUI>(true);

                bool showQuantity =
                    occupied &&
                    !quantityTextAssigned &&
                    itemData.isStackable &&
                    heldItem.Quantity > 1;

                if (quantityText != null)
                {
                    quantityText.text =
                        showQuantity
                        ? heldItem.Quantity.ToString()
                        : "";

                    quantityText.gameObject.SetActive(showQuantity);
                }

                if (showQuantity)
                    quantityTextAssigned = true;
            }
        }

        BuildHeldPreviewOutline();
        UpdateHeldPreviewVisibility();
        UpdateHeldPreviewPosition();
    }

    private void RefreshHeldPreviewIfNeeded()
    {
        bool shouldTrackHeldPreview =
            InventoryMenuController.IsInventoryOpen &&
            playerInventory != null &&
            playerInventory.Grid != null &&
            playerInventory.IsHoldingItem &&
            Mouse.current != null;

        if (shouldTrackHeldPreview != wasShowingHeldPreview)
        {
            wasShowingHeldPreview = shouldTrackHeldPreview;

            if (shouldTrackHeldPreview)
            {
                lastHeldPreviewMousePosition =
                    Mouse.current.position.ReadValue();

                UpdateHoveredCoordinateFromMouse();
            }
            else
            {
                hoveredCoordinate =
                    new Vector2Int(-1, -1);
            }

            Refresh();
            return;
        }

        if (!shouldTrackHeldPreview)
            return;

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        if ((mousePosition - lastHeldPreviewMousePosition).sqrMagnitude < 0.01f)
            return;

        lastHeldPreviewMousePosition =
            mousePosition;

        UpdateHoveredCoordinateFromMouse();
        Refresh();
    }

    private void UpdateHoveredCoordinateFromMouse()
    {
        if (Mouse.current == null)
            return;

        if (TryGetGridCoordinateFromScreenPoint(
                Mouse.current.position.ReadValue(),
                out Vector2Int coordinate))
        {
            hoveredCoordinate = coordinate;
            return;
        }

        hoveredCoordinate =
            new Vector2Int(-1, -1);
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

        if (heldPreviewOutlineRoot != null &&
            heldPreviewOutlineRoot.gameObject.activeSelf != shouldShow)
        {
            heldPreviewOutlineRoot.gameObject.SetActive(shouldShow);
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

        Vector2 previewPosition =
            localPoint -
            GetHeldPreviewGrabPoint() +
            heldPreviewOffset;

        heldPreviewRoot.anchoredPosition =
            previewPosition;

        if (heldPreviewOutlineRoot != null)
        {
            heldPreviewOutlineRoot.anchoredPosition =
                previewPosition;
        }
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

        if (centerHeldPreviewOnMouse)
        {
            float width =
                heldItem.Width * cellSize.x +
                Mathf.Max(0, heldItem.Width - 1) * spacing.x;

            float height =
                heldItem.Height * cellSize.y +
                Mathf.Max(0, heldItem.Height - 1) * spacing.y;

            return new Vector2(
                width * 0.5f,
                -height * 0.5f
            );
        }

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

    private void BuildHeldPreviewOutline()
    {
        if (heldPreviewOutlineRoot == null)
            return;

        InventoryUIUtility.ClearChildren(
            heldPreviewOutlineRoot
        );

        PlacedInventoryItem heldItem =
            HeldItem;

        if (heldItem == null ||
            heldItem.ItemData == null ||
            heldPreviewLayoutGroup == null)
        {
            return;
        }

        ItemData itemData =
            heldItem.ItemData;

        int width =
            heldItem.Width;

        int height =
            heldItem.Height;

        int rotationSteps =
            heldItem.RotationSteps;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (!InventoryShapeUtility.IsOccupiedInShape(
                        itemData,
                        x,
                        y,
                        rotationSteps))
                {
                    continue;
                }

                bool topOpen =
                    !InventoryShapeUtility.IsOccupiedInShape(
                        itemData,
                        x,
                        y + 1,
                        rotationSteps
                    );

                bool bottomOpen =
                    !InventoryShapeUtility.IsOccupiedInShape(
                        itemData,
                        x,
                        y - 1,
                        rotationSteps
                    );

                bool leftOpen =
                    !InventoryShapeUtility.IsOccupiedInShape(
                        itemData,
                        x - 1,
                        y,
                        rotationSteps
                    );

                bool rightOpen =
                    !InventoryShapeUtility.IsOccupiedInShape(
                        itemData,
                        x + 1,
                        y,
                        rotationSteps
                    );

                if (topOpen)
                    DrawHeldPreviewOutlineEdge(x, y, InventoryOutlineSide.Top);

                if (bottomOpen)
                    DrawHeldPreviewOutlineEdge(x, y, InventoryOutlineSide.Bottom);

                if (leftOpen)
                    DrawHeldPreviewOutlineEdge(x, y, InventoryOutlineSide.Left);

                if (rightOpen)
                    DrawHeldPreviewOutlineEdge(x, y, InventoryOutlineSide.Right);

                if (fillPaddingBetweenCells)
                {
                    bool rightFilled =
                        InventoryShapeUtility.IsOccupiedInShape(
                            itemData,
                            x + 1,
                            y,
                            rotationSteps
                        );

                    bool downFilled =
                        InventoryShapeUtility.IsOccupiedInShape(
                            itemData,
                            x,
                            y - 1,
                            rotationSteps
                        );

                    if (topOpen &&
                        rightFilled &&
                        !InventoryShapeUtility.IsOccupiedInShape(
                            itemData,
                            x + 1,
                            y + 1,
                            rotationSteps))
                    {
                        DrawHeldPreviewBridge(x, y, InventoryOutlineSide.Top);
                    }

                    if (bottomOpen &&
                        rightFilled &&
                        !InventoryShapeUtility.IsOccupiedInShape(
                            itemData,
                            x + 1,
                            y - 1,
                            rotationSteps))
                    {
                        DrawHeldPreviewBridge(x, y, InventoryOutlineSide.Bottom);
                    }

                    if (leftOpen &&
                        downFilled &&
                        !InventoryShapeUtility.IsOccupiedInShape(
                            itemData,
                            x - 1,
                            y - 1,
                            rotationSteps))
                    {
                        DrawHeldPreviewBridge(x, y, InventoryOutlineSide.Left);
                    }

                    if (rightOpen &&
                        downFilled &&
                        !InventoryShapeUtility.IsOccupiedInShape(
                            itemData,
                            x + 1,
                            y - 1,
                            rotationSteps))
                    {
                        DrawHeldPreviewBridge(x, y, InventoryOutlineSide.Right);
                    }
                }

                if (topOpen && leftOpen)
                    DrawHeldPreviewCorner(x, y, InventoryOutlineCorner.TopLeft);

                if (topOpen && rightOpen)
                    DrawHeldPreviewCorner(x, y, InventoryOutlineCorner.TopRight);

                if (bottomOpen && leftOpen)
                    DrawHeldPreviewCorner(x, y, InventoryOutlineCorner.BottomLeft);

                if (bottomOpen && rightOpen)
                    DrawHeldPreviewCorner(x, y, InventoryOutlineCorner.BottomRight);
            }
        }

        if (fillPaddingBetweenCells)
        {
            DrawHeldPreviewInnerCorners(
                itemData,
                rotationSteps,
                width,
                height
            );
        }
    }

    private void DrawHeldPreviewOutlineEdge(
        int localX,
        int localY,
        InventoryOutlineSide side)
    {
        if (heldPreviewLayoutGroup == null)
            return;

        Vector2 cellSize =
            heldPreviewLayoutGroup.cellSize;

        Vector2 spacing =
            heldPreviewLayoutGroup.spacing;

        PlacedInventoryItem heldItem =
            HeldItem;

        if (heldItem == null)
            return;

        int rowFromTop =
            heldItem.Height - 1 - localY;

        float cellLeft =
            localX * (cellSize.x + spacing.x);

        float cellTop =
            -rowFromTop * (cellSize.y + spacing.y);

        float halfSpacingX =
            fillPaddingBetweenCells ? spacing.x * 0.5f : 0f;

        float halfSpacingY =
            fillPaddingBetweenCells ? spacing.y * 0.5f : 0f;

        Vector2 position;
        Vector2 size;

        switch (side)
        {
            case InventoryOutlineSide.Top:
                position =
                    new Vector2(
                        cellLeft + cellSize.x * 0.5f,
                        cellTop + halfSpacingY
                    );

                size =
                    InventoryUIUtility.GetHorizontalEdgeSize(
                        cellSize.x,
                        itemOutlineThickness
                    );
                break;

            case InventoryOutlineSide.Bottom:
                position =
                    new Vector2(
                        cellLeft + cellSize.x * 0.5f,
                        cellTop - cellSize.y - halfSpacingY
                    );

                size =
                    InventoryUIUtility.GetHorizontalEdgeSize(
                        cellSize.x,
                        itemOutlineThickness
                    );
                break;

            case InventoryOutlineSide.Left:
                position =
                    new Vector2(
                        cellLeft - halfSpacingX,
                        cellTop - cellSize.y * 0.5f
                    );

                size =
                    InventoryUIUtility.GetVerticalEdgeSize(
                        itemOutlineThickness,
                        cellSize.y
                    );
                break;

            default:
                position =
                    new Vector2(
                        cellLeft + cellSize.x + halfSpacingX,
                        cellTop - cellSize.y * 0.5f
                    );

                size =
                    InventoryUIUtility.GetVerticalEdgeSize(
                        itemOutlineThickness,
                        cellSize.y
                    );
                break;
        }

        CreateHeldPreviewOutlineRect(position, size);
    }

    private void DrawHeldPreviewBridge(
        int localX,
        int localY,
        InventoryOutlineSide side)
    {
        if (heldPreviewLayoutGroup == null ||
            !fillPaddingBetweenCells)
        {
            return;
        }

        Vector2 cellSize =
            heldPreviewLayoutGroup.cellSize;

        Vector2 spacing =
            heldPreviewLayoutGroup.spacing;

        PlacedInventoryItem heldItem =
            HeldItem;

        if (heldItem == null)
            return;

        int rowFromTop =
            heldItem.Height - 1 - localY;

        float cellLeft =
            localX * (cellSize.x + spacing.x);

        float cellTop =
            -rowFromTop * (cellSize.y + spacing.y);

        Vector2 position;
        Vector2 size;

        switch (side)
        {
            case InventoryOutlineSide.Top:
                if (spacing.x <= 0f)
                    return;

                position =
                    new Vector2(
                        cellLeft + cellSize.x + spacing.x * 0.5f,
                        cellTop + spacing.y * 0.5f
                    );

                size =
                    InventoryUIUtility.GetHorizontalBridgeSize(
                        spacing.x,
                        itemOutlineThickness
                    );
                break;

            case InventoryOutlineSide.Bottom:
                if (spacing.x <= 0f)
                    return;

                position =
                    new Vector2(
                        cellLeft + cellSize.x + spacing.x * 0.5f,
                        cellTop - cellSize.y - spacing.y * 0.5f
                    );

                size =
                    InventoryUIUtility.GetHorizontalBridgeSize(
                        spacing.x,
                        itemOutlineThickness
                    );
                break;

            case InventoryOutlineSide.Left:
                if (spacing.y <= 0f)
                    return;

                position =
                    new Vector2(
                        cellLeft - spacing.x * 0.5f,
                        cellTop - cellSize.y - spacing.y * 0.5f
                    );

                size =
                    InventoryUIUtility.GetVerticalBridgeSize(
                        itemOutlineThickness,
                        spacing.y
                    );
                break;

            default:
                if (spacing.y <= 0f)
                    return;

                position =
                    new Vector2(
                        cellLeft + cellSize.x + spacing.x * 0.5f,
                        cellTop - cellSize.y - spacing.y * 0.5f
                    );

                size =
                    InventoryUIUtility.GetVerticalBridgeSize(
                        itemOutlineThickness,
                        spacing.y
                    );
                break;
        }

        CreateHeldPreviewOutlineRect(position, size);
    }

    private void DrawHeldPreviewCorner(
        int localX,
        int localY,
        InventoryOutlineCorner corner)
    {
        if (heldPreviewLayoutGroup == null)
            return;

        Vector2 cellSize =
            heldPreviewLayoutGroup.cellSize;

        Vector2 spacing =
            heldPreviewLayoutGroup.spacing;

        PlacedInventoryItem heldItem =
            HeldItem;

        if (heldItem == null)
            return;

        int rowFromTop =
            heldItem.Height - 1 - localY;

        float cellLeft =
            localX * (cellSize.x + spacing.x);

        float cellTop =
            -rowFromTop * (cellSize.y + spacing.y);

        float halfSpacingX =
            fillPaddingBetweenCells ? spacing.x * 0.5f : 0f;

        float halfSpacingY =
            fillPaddingBetweenCells ? spacing.y * 0.5f : 0f;

        Vector2 position;

        switch (corner)
        {
            case InventoryOutlineCorner.TopLeft:
                position =
                    new Vector2(
                        cellLeft - halfSpacingX,
                        cellTop + halfSpacingY
                    );
                break;

            case InventoryOutlineCorner.TopRight:
                position =
                    new Vector2(
                        cellLeft + cellSize.x + halfSpacingX,
                        cellTop + halfSpacingY
                    );
                break;

            case InventoryOutlineCorner.BottomLeft:
                position =
                    new Vector2(
                        cellLeft - halfSpacingX,
                        cellTop - cellSize.y - halfSpacingY
                    );
                break;

            default:
                position =
                    new Vector2(
                        cellLeft + cellSize.x + halfSpacingX,
                        cellTop - cellSize.y - halfSpacingY
                    );
                break;
        }

        CreateHeldPreviewOutlineRect(
            position,
            InventoryUIUtility.GetCornerSize(
                itemOutlineThickness
            )
        );
    }

    private void DrawHeldPreviewInnerCorners(
        ItemData itemData,
        int rotationSteps,
        int width,
        int height)
    {
        if (itemData == null)
            return;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (InventoryShapeUtility.IsOccupiedInShape(
                        itemData,
                        x,
                        y,
                        rotationSteps))
                {
                    continue;
                }

                bool leftFilled =
                    InventoryShapeUtility.IsOccupiedInShape(
                        itemData,
                        x - 1,
                        y,
                        rotationSteps
                    );

                bool rightFilled =
                    InventoryShapeUtility.IsOccupiedInShape(
                        itemData,
                        x + 1,
                        y,
                        rotationSteps
                    );

                bool upFilled =
                    InventoryShapeUtility.IsOccupiedInShape(
                        itemData,
                        x,
                        y + 1,
                        rotationSteps
                    );

                bool downFilled =
                    InventoryShapeUtility.IsOccupiedInShape(
                        itemData,
                        x,
                        y - 1,
                        rotationSteps
                    );

                if (rightFilled && downFilled)
                {
                    DrawHeldPreviewGapCorner(
                        x,
                        y,
                        InventoryOutlineCorner.BottomRight
                    );
                }

                if (leftFilled && downFilled)
                {
                    DrawHeldPreviewGapCorner(
                        x,
                        y,
                        InventoryOutlineCorner.BottomLeft
                    );
                }

                if (rightFilled && upFilled)
                {
                    DrawHeldPreviewGapCorner(
                        x,
                        y,
                        InventoryOutlineCorner.TopRight
                    );
                }

                if (leftFilled && upFilled)
                {
                    DrawHeldPreviewGapCorner(
                        x,
                        y,
                        InventoryOutlineCorner.TopLeft
                    );
                }
            }
        }
    }

    private void DrawHeldPreviewGapCorner(
        int localX,
        int localY,
        InventoryOutlineCorner corner)
    {
        if (heldPreviewLayoutGroup == null)
            return;

        Vector2 cellSize =
            heldPreviewLayoutGroup.cellSize;

        Vector2 spacing =
            heldPreviewLayoutGroup.spacing;

        PlacedInventoryItem heldItem =
            HeldItem;

        if (heldItem == null)
            return;

        int rowFromTop =
            heldItem.Height - 1 - localY;

        float cellLeft =
            localX * (cellSize.x + spacing.x);

        float cellTop =
            -rowFromTop * (cellSize.y + spacing.y);

        Vector2 position;

        switch (corner)
        {
            case InventoryOutlineCorner.TopLeft:
                position =
                    new Vector2(
                        cellLeft - spacing.x * 0.5f,
                        cellTop + spacing.y * 0.5f
                    );
                break;

            case InventoryOutlineCorner.TopRight:
                position =
                    new Vector2(
                        cellLeft + cellSize.x + spacing.x * 0.5f,
                        cellTop + spacing.y * 0.5f
                    );
                break;

            case InventoryOutlineCorner.BottomLeft:
                position =
                    new Vector2(
                        cellLeft - spacing.x * 0.5f,
                        cellTop - cellSize.y - spacing.y * 0.5f
                    );
                break;

            default:
                position =
                    new Vector2(
                        cellLeft + cellSize.x + spacing.x * 0.5f,
                        cellTop - cellSize.y - spacing.y * 0.5f
                    );
                break;
        }

        CreateHeldPreviewOutlineRect(
            position,
            InventoryUIUtility.GetCornerSize(
                itemOutlineThickness
            )
        );
    }

    private void CreateHeldPreviewOutlineRect(
        Vector2 position,
        Vector2 size)
    {
        InventoryUIUtility.CreateImageRect(
            heldPreviewOutlineRoot,
            "HeldPreviewOutlinePiece",
            position,
            size,
            itemOutlineColor
        );
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

        InventoryUIUtility.ClearChildren(
            itemOutline
        );

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

        PlacedInventoryItem heldItem =
            HeldItem;

        if (heldItem != null &&
            heldItem.ItemData != null &&
            InventoryMenuController.IsInventoryOpen &&
            IsValidGridCoordinate(hoveredCoordinate))
        {
            Vector2Int previewOrigin =
                GetHeldPlacementOrigin(hoveredCoordinate);

            DrawItemOutline(
                heldItem.ItemData,
                previewOrigin,
                heldItem.RotationSteps,
                itemOutlineColor
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
                    !InventoryShapeUtility.IsOccupiedInShape(itemData, x, y + 1, rotationSteps);

                bool bottomOpen =
                    !InventoryShapeUtility.IsOccupiedInShape(itemData, x, y - 1, rotationSteps);

                bool leftOpen =
                    !InventoryShapeUtility.IsOccupiedInShape(itemData, x - 1, y, rotationSteps);

                bool rightOpen =
                    !InventoryShapeUtility.IsOccupiedInShape(itemData, x + 1, y, rotationSteps);

                if (topOpen)
                    DrawOutlineEdge(origin.x + x, origin.y + y, InventoryOutlineSide.Top, color);

                if (bottomOpen)
                    DrawOutlineEdge(origin.x + x, origin.y + y, InventoryOutlineSide.Bottom, color);

                if (leftOpen)
                    DrawOutlineEdge(origin.x + x, origin.y + y, InventoryOutlineSide.Left, color);

                if (rightOpen)
                    DrawOutlineEdge(origin.x + x, origin.y + y, InventoryOutlineSide.Right, color);

                if (fillPaddingBetweenCells)
                {
                    bool rightFilled =
                        InventoryShapeUtility.IsOccupiedInShape(itemData, x + 1, y, rotationSteps);

                    bool downFilled =
                        InventoryShapeUtility.IsOccupiedInShape(itemData, x, y - 1, rotationSteps);

                    if (topOpen &&
                        rightFilled &&
                        !InventoryShapeUtility.IsOccupiedInShape(itemData, x + 1, y + 1, rotationSteps))
                    {
                        DrawBridge(origin.x + x, origin.y + y, InventoryOutlineSide.Top, color);
                    }

                    if (bottomOpen &&
                        rightFilled &&
                        !InventoryShapeUtility.IsOccupiedInShape(itemData, x + 1, y - 1, rotationSteps))
                    {
                        DrawBridge(origin.x + x, origin.y + y, InventoryOutlineSide.Bottom, color);
                    }

                    if (leftOpen &&
                        downFilled &&
                        !InventoryShapeUtility.IsOccupiedInShape(itemData, x - 1, y - 1, rotationSteps))
                    {
                        DrawBridge(origin.x + x, origin.y + y, InventoryOutlineSide.Left, color);
                    }

                    if (rightOpen &&
                        downFilled &&
                        !InventoryShapeUtility.IsOccupiedInShape(itemData, x + 1, y - 1, rotationSteps))
                    {
                        DrawBridge(origin.x + x, origin.y + y, InventoryOutlineSide.Right, color);
                    }
                }

                if (topOpen && leftOpen)
                    DrawCorner(origin.x + x, origin.y + y, InventoryOutlineCorner.TopLeft, color);

                if (topOpen && rightOpen)
                    DrawCorner(origin.x + x, origin.y + y, InventoryOutlineCorner.TopRight, color);

                if (bottomOpen && leftOpen)
                    DrawCorner(origin.x + x, origin.y + y, InventoryOutlineCorner.BottomLeft, color);

                if (bottomOpen && rightOpen)
                    DrawCorner(origin.x + x, origin.y + y, InventoryOutlineCorner.BottomRight, color);
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

    private void DrawOutlineEdge(
        int gridX,
        int gridY,
        InventoryOutlineSide side,
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
            case InventoryOutlineSide.Top:
                position = new Vector2(
                    cellLeft + cellSize.x * 0.5f,
                    cellTop + halfSpacingY
                );

                size =
                    InventoryUIUtility.GetHorizontalEdgeSize(
                        cellSize.x,
                        itemOutlineThickness
                    );
                break;

            case InventoryOutlineSide.Bottom:
                position = new Vector2(
                    cellLeft + cellSize.x * 0.5f,
                    cellTop - cellSize.y - halfSpacingY
                );

                size =
                    InventoryUIUtility.GetHorizontalEdgeSize(
                        cellSize.x,
                        itemOutlineThickness
                    );
                break;

            case InventoryOutlineSide.Left:
                position = new Vector2(
                    cellLeft - halfSpacingX,
                    cellTop - cellSize.y * 0.5f
                );

                size =
                    InventoryUIUtility.GetVerticalEdgeSize(
                        itemOutlineThickness,
                        cellSize.y
                    );
                break;

            default:
                position = new Vector2(
                    cellLeft + cellSize.x + halfSpacingX,
                    cellTop - cellSize.y * 0.5f
                );

                size =
                    InventoryUIUtility.GetVerticalEdgeSize(
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
        InventoryOutlineSide side,
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
            case InventoryOutlineSide.Top:
                if (spacing.x <= 0f)
                    return;

                position = new Vector2(
                    cellLeft + cellSize.x + spacing.x * 0.5f,
                    cellTop + spacing.y * 0.5f
                );

                size =
                    InventoryUIUtility.GetHorizontalBridgeSize(
                        spacing.x,
                        itemOutlineThickness
                    );
                break;

            case InventoryOutlineSide.Bottom:
                if (spacing.x <= 0f)
                    return;

                position = new Vector2(
                    cellLeft + cellSize.x + spacing.x * 0.5f,
                    cellTop - cellSize.y - spacing.y * 0.5f
                );

                size =
                    InventoryUIUtility.GetHorizontalBridgeSize(
                        spacing.x,
                        itemOutlineThickness
                    );
                break;

            case InventoryOutlineSide.Left:
                if (spacing.y <= 0f)
                    return;

                position = new Vector2(
                    cellLeft - spacing.x * 0.5f,
                    cellTop - cellSize.y - spacing.y * 0.5f
                );

                size =
                    InventoryUIUtility.GetVerticalBridgeSize(
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

                size =
                    InventoryUIUtility.GetVerticalBridgeSize(
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
        InventoryOutlineCorner corner,
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
            case InventoryOutlineCorner.TopLeft:
                position = new Vector2(
                    cellLeft - halfSpacingX,
                    cellTop + halfSpacingY
                );
                break;

            case InventoryOutlineCorner.TopRight:
                position = new Vector2(
                    cellLeft + cellSize.x + halfSpacingX,
                    cellTop + halfSpacingY
                );
                break;

            case InventoryOutlineCorner.BottomLeft:
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
            InventoryUIUtility.GetCornerSize(
                itemOutlineThickness
            ),
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
                if (InventoryShapeUtility.IsOccupiedInShape(itemData, x, y, rotationSteps))
                    continue;

                bool leftFilled =
                    InventoryShapeUtility.IsOccupiedInShape(itemData, x - 1, y, rotationSteps);

                bool rightFilled =
                    InventoryShapeUtility.IsOccupiedInShape(itemData, x + 1, y, rotationSteps);

                bool upFilled =
                    InventoryShapeUtility.IsOccupiedInShape(itemData, x, y + 1, rotationSteps);

                bool downFilled =
                    InventoryShapeUtility.IsOccupiedInShape(itemData, x, y - 1, rotationSteps);

                if (rightFilled && downFilled)
                {
                    DrawGapCorner(
                        origin.x + x,
                        origin.y + y,
                        InventoryOutlineCorner.BottomRight,
                        color
                    );
                }

                if (leftFilled && downFilled)
                {
                    DrawGapCorner(
                        origin.x + x,
                        origin.y + y,
                        InventoryOutlineCorner.BottomLeft,
                        color
                    );
                }

                if (rightFilled && upFilled)
                {
                    DrawGapCorner(
                        origin.x + x,
                        origin.y + y,
                        InventoryOutlineCorner.TopRight,
                        color
                    );
                }

                if (leftFilled && upFilled)
                {
                    DrawGapCorner(
                        origin.x + x,
                        origin.y + y,
                        InventoryOutlineCorner.TopLeft,
                        color
                    );
                }
            }
        }
    }

    private void DrawGapCorner(
        int gridX,
        int gridY,
        InventoryOutlineCorner corner,
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
            case InventoryOutlineCorner.TopLeft:
                position = new Vector2(
                    cellLeft - spacing.x * 0.5f,
                    cellTop + spacing.y * 0.5f
                );
                break;

            case InventoryOutlineCorner.TopRight:
                position = new Vector2(
                    cellLeft + cellSize.x + spacing.x * 0.5f,
                    cellTop + spacing.y * 0.5f
                );
                break;

            case InventoryOutlineCorner.BottomLeft:
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
            InventoryUIUtility.GetCornerSize(
                itemOutlineThickness
            ),
            color
        );
    }

    private void CreateOutlineRect(
        Vector2 position,
        Vector2 size,
        Color color)
    {
        InventoryUIUtility.CreateImageRect(
            itemOutline,
            "ItemOutlinePiece",
            position,
            size,
            color
        );
    }
}