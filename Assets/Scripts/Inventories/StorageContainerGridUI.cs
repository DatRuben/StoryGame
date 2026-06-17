using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class StorageContainerGridUI : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
    [Header("References")]
    [SerializeField] private StorageContainer storageContainer;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private InventoryGridUI playerInventoryGridUI;
    [SerializeField] private Transform cellParent;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private RectTransform itemOutline;

    [Header("Drag")]
    [SerializeField] private bool allowDragToPlayerInventory = true;
    [SerializeField] private float dragStartDistance = 12f;

    [Header("Colors")]
    [SerializeField] private Color emptyColor = new Color(0f, 0f, 0f, 0.35f);
    [SerializeField] private Color occupiedColor = new Color(1f, 1f, 1f, 0.85f);
    [SerializeField] private Color validPlacementColor = new Color(0.2f, 1f, 0.2f, 0.65f);
    [SerializeField] private Color invalidPlacementColor = new Color(1f, 0.2f, 0.2f, 0.65f);
    [SerializeField] private Color dragOriginalGhostColor = new Color(0.45f, 0.45f, 0.45f, 0.35f);

    [Header("Item Outlines")]
    [SerializeField] private Color itemOutlineColor = new Color(1f, 1f, 1f, 0.95f);
    [SerializeField] private Color dragOriginalOutlineColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
    [SerializeField] private float itemOutlineThickness = 3f;
    [SerializeField] private bool fillPaddingBetweenCells = true;

    private GridLayoutGroup gridLayoutGroup;
    private Canvas rootCanvas;

    private bool suppressNextContainerClick;
    private bool pointerIsDown;
    private bool pendingStorageDragPickup;
    private bool isDraggingStorageItem;
    private Vector2 pointerDownScreenPosition;
    private Vector2Int pointerDownCoordinate;
    private bool wasShowingHeldPreview;
    private Vector2 lastHeldPreviewMousePosition;

    private ItemData dragOriginalItemData;
    private Vector2Int dragOriginalPosition;
    private int dragOriginalRotationSteps;
    private int dragOriginalQuantity;

    private static StorageContainerGridUI activeStorageDragUI;

    public static bool HasPendingStorageDrag =>
        activeStorageDragUI != null;

    private readonly List<InventoryCellUI> cells =
        new List<InventoryCellUI>();

    private readonly List<Vector2Int> cellCoordinates =
        new List<Vector2Int>();

    private void Awake()
    {
        if (cellParent != null)
            gridLayoutGroup = cellParent.GetComponent<GridLayoutGroup>();

        if (gridLayoutGroup == null)
            gridLayoutGroup = GetComponent<GridLayoutGroup>();

        if (playerInventoryGridUI == null)
            playerInventoryGridUI = FindFirstObjectByType<InventoryGridUI>();

        rootCanvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        HandleStorageDragDetection();
        HandleStorageDragRelease();
        RefreshHeldPreviewIfNeeded();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (suppressNextContainerClick)
            suppressNextContainerClick = false;

        Refresh();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (storageContainer == null ||
            storageContainer.Grid == null ||
            playerInventory == null)
        {
            return;
        }

        if (!InventoryMenuController.IsInventoryOpen)
            return;

        if (!playerInventory.IsHoldingItem)
            return;

        if (suppressNextContainerClick)
        {
            suppressNextContainerClick = false;
            return;
        }

        if (!TryGetGridCoordinateFromScreenPoint(
                eventData.position,
                out Vector2Int coordinate))
        {
            return;
        }

        OnCellClicked(coordinate);
    }

    private void OnEnable()
    {
        if (storageContainer != null)
            storageContainer.OnContainerChanged += Refresh;

        BuildGrid();
        Refresh();
    }

    private void OnDisable()
    {
        if (storageContainer != null)
            storageContainer.OnContainerChanged -= Refresh;

        if (activeStorageDragUI == this)
            CancelActiveStorageDrag();

        pointerIsDown = false;
        pendingStorageDragPickup = false;
        isDraggingStorageItem = false;
        suppressNextContainerClick = false;
    }

    private void Start()
    {
        BuildGrid();
        Refresh();
    }

    public void SetStorageContainer(StorageContainer newStorageContainer)
    {
        if (storageContainer != null)
            storageContainer.OnContainerChanged -= Refresh;

        if (activeStorageDragUI == this)
            CancelActiveStorageDrag();

        storageContainer = newStorageContainer;

        if (storageContainer != null)
            storageContainer.OnContainerChanged += Refresh;

        BuildGrid();
        Refresh();
    }

    public static void CommitPendingStorageDrag()
    {
        if (activeStorageDragUI == null)
            return;

        activeStorageDragUI.ClearActiveStorageDragStateOnly();
    }

    public static void CancelPendingStorageDrag(
        PlayerInventory playerInventory)
    {
        if (activeStorageDragUI == null)
        {
            if (playerInventory != null &&
                playerInventory.IsHoldingItem)
            {
                playerInventory.ClearHeldItemAfterExternalMove();
            }

            return;
        }

        activeStorageDragUI.CancelActiveStorageDrag();
    }

    private void BuildGrid()
    {
        ClearGrid();

        if (storageContainer == null ||
            storageContainer.Grid == null ||
            cellParent == null ||
            cellPrefab == null)
        {
            return;
        }

        InventoryGrid grid =
            storageContainer.Grid;

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
                    Instantiate(
                        cellPrefab,
                        cellParent
                    );

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

    private void ClearGrid()
    {
        if (cellParent != null)
        {
            foreach (Transform child in cellParent)
            {
                Destroy(child.gameObject);
            }
        }

        cells.Clear();
        cellCoordinates.Clear();
    }

    private void Refresh()
    {
        if (storageContainer == null ||
            storageContainer.Grid == null)
        {
            return;
        }

        if (cells.Count == 0)
            BuildGrid();

        InventoryGrid grid =
            storageContainer.Grid;

        bool showHeldPreview =
            playerInventory != null &&
            playerInventory.IsHoldingItem &&
            Mouse.current != null;

        Vector2Int previewCenter =
            new Vector2Int(-999, -999);

        Vector2Int previewOrigin =
            new Vector2Int(-999, -999);

        bool previewHasGridCoordinate = false;
        bool previewCanPlace = false;

        if (showHeldPreview)
        {
            previewHasGridCoordinate =
                TryGetGridCoordinateFromScreenPoint(
                    Mouse.current.position.ReadValue(),
                    out previewCenter
                );

            if (previewHasGridCoordinate)
            {
                previewOrigin =
                    GetHeldPlacementOrigin(
                        previewCenter
                    );

                PlacedInventoryItem heldItem =
                    playerInventory.HeldItem;

                if (heldItem != null &&
                    heldItem.ItemData != null)
                {
                    previewCanPlace =
                        grid.CanPlaceItem(
                            heldItem.ItemData,
                            previewOrigin.x,
                            previewOrigin.y,
                            heldItem.RotationSteps
                        );
                }
            }
        }

        for (int i = 0; i < cells.Count; i++)
        {
            InventoryCellUI cell =
                cells[i];

            if (cell == null)
                continue;

            Vector2Int coordinate =
                cellCoordinates[i];

            bool isPreviewCell =
                previewHasGridCoordinate &&
                IsHeldItemPreviewCell(
                    coordinate,
                    previewOrigin
                );

            if (isPreviewCell)
            {
                cell.SetColor(
                    previewCanPlace
                        ? validPlacementColor
                        : invalidPlacementColor
                );

                cell.SetQuantityText("");
                continue;
            }

            if (isDraggingStorageItem &&
                IsOriginalFootprint(coordinate))
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
        if (placedItem == null ||
            placedItem.ItemData == null)
        {
            return "";
        }

        if (!placedItem.ItemData.isStackable)
            return "";

        if (placedItem.Quantity <= 1)
            return "";

        if (placedItem.Position != coordinate)
            return "";

        return placedItem.Quantity.ToString();
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
        if (storageContainer == null ||
            storageContainer.Grid == null ||
            playerInventory == null)
        {
            return;
        }

        if (suppressNextContainerClick)
        {
            suppressNextContainerClick = false;
            return;
        }

        if (IsQuickTransferHeld() &&
            !playerInventory.IsHoldingItem)
        {
            QuickTransferContainerItemToPlayer(
                coordinate
            );

            return;
        }

        if (playerInventory.IsHoldingItem)
        {
            TryPlaceOrMergeHeldItemIntoContainer(
                coordinate
            );

            return;
        }

        PickUpContainerItem(
            coordinate
        );
    }

    private void OnCellPointerDown(Vector2Int coordinate)
    {
        if (suppressNextContainerClick)
            suppressNextContainerClick = false;

        if (storageContainer == null ||
            storageContainer.Grid == null ||
            playerInventory == null ||
            Mouse.current == null)
        {
            return;
        }

        pointerIsDown = true;
        pendingStorageDragPickup = false;
        isDraggingStorageItem = false;
        pointerDownCoordinate = coordinate;
        pointerDownScreenPosition =
            Mouse.current.position.ReadValue();

        if (playerInventory.IsHoldingItem ||
            IsQuickTransferHeld())
        {
            return;
        }

        PlacedInventoryItem sourceItem =
            storageContainer.Grid.GetPlacedItem(
                coordinate.x,
                coordinate.y
            );

        pendingStorageDragPickup =
            sourceItem != null &&
            sourceItem.ItemData != null;
    }

    private void OnCellPointerUp(Vector2Int coordinate)
    {
        if (isDraggingStorageItem)
        {
            CompleteStorageDragRelease();
            return;
        }

        pointerIsDown = false;
        pendingStorageDragPickup = false;
    }

    private void HandleStorageDragDetection()
    {
        if (!pointerIsDown ||
            !pendingStorageDragPickup ||
            isDraggingStorageItem ||
            Mouse.current == null)
        {
            return;
        }

        if (!Mouse.current.leftButton.isPressed)
        {
            pointerIsDown = false;
            pendingStorageDragPickup = false;
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

        bool beganDrag =
            BeginStorageDragFromCell(
                pointerDownCoordinate
            );

        if (!beganDrag)
        {
            pointerIsDown = false;
            pendingStorageDragPickup = false;
            return;
        }

        isDraggingStorageItem = true;
        pendingStorageDragPickup = false;
        suppressNextContainerClick = true;
    }

    private void HandleStorageDragRelease()
    {
        if (!isDraggingStorageItem ||
            Mouse.current == null)
        {
            return;
        }

        if (!Mouse.current.leftButton.wasReleasedThisFrame)
            return;

        CompleteStorageDragRelease();
    }

    private bool BeginStorageDragFromCell(Vector2Int coordinate)
    {
        if (storageContainer == null ||
            storageContainer.Grid == null ||
            playerInventory == null)
        {
            return false;
        }

        if (activeStorageDragUI != null)
            activeStorageDragUI.CancelActiveStorageDrag();

        PlacedInventoryItem sourceItem =
            storageContainer.PickUpItemAt(
                coordinate.x,
                coordinate.y
            );

        if (sourceItem == null ||
            sourceItem.ItemData == null)
        {
            return false;
        }

        activeStorageDragUI = this;
        dragOriginalItemData = sourceItem.ItemData;
        dragOriginalPosition = sourceItem.Position;
        dragOriginalRotationSteps = sourceItem.RotationSteps;
        dragOriginalQuantity = sourceItem.Quantity;

        playerInventory.SetMouseHeldItemFromExternal(
            sourceItem.ItemData,
            sourceItem.RotationSteps,
            false,
            sourceItem.Quantity
        );

        Refresh();
        return true;
    }

    private void CompleteStorageDragRelease()
    {
        isDraggingStorageItem = false;
        pointerIsDown = false;
        pendingStorageDragPickup = false;
        suppressNextContainerClick = true;

        if (activeStorageDragUI != this ||
            playerInventory == null ||
            !playerInventory.IsHoldingItem)
        {
            return;
        }

        if (Mouse.current != null &&
            TryGetGridCoordinateFromScreenPoint(
                Mouse.current.position.ReadValue(),
                out Vector2Int storageCoordinate))
        {
            bool droppedIntoStorage =
                TryPlaceOrMergeHeldItemIntoContainer(
                    storageCoordinate
                );

            if (droppedIntoStorage)
            {
                ClearActiveStorageDragStateOnly();
                Refresh();
                return;
            }
        }

        if (allowDragToPlayerInventory &&
            playerInventoryGridUI != null &&
            Mouse.current != null)
        {
            bool droppedIntoPlayerInventory =
                playerInventoryGridUI.TryDropHeldItemAtScreenPoint(
                    Mouse.current.position.ReadValue()
                );

            if (droppedIntoPlayerInventory)
            {
                ClearActiveStorageDragStateOnly();
                Refresh();
                return;
            }
        }

        CancelActiveStorageDrag();
    }

    private void PickUpContainerItem(Vector2Int coordinate)
    {
        if (storageContainer == null ||
            storageContainer.Grid == null ||
            playerInventory == null ||
            playerInventory.IsHoldingItem)
        {
            return;
        }

        PlacedInventoryItem pickedItem =
            storageContainer.PickUpItemAt(
                coordinate.x,
                coordinate.y
            );

        if (pickedItem == null ||
            pickedItem.ItemData == null)
        {
            return;
        }

        playerInventory.SetMouseHeldItemFromExternal(
            pickedItem.ItemData,
            pickedItem.RotationSteps,
            true,
            pickedItem.Quantity
        );

        Refresh();
    }

    private bool TryPlaceOrMergeHeldItemIntoContainer(Vector2Int coordinate)
    {
        bool merged =
            TryMergeHeldItemIntoContainerStackAt(
                coordinate.x,
                coordinate.y
            );

        if (merged)
        {
            Refresh();
            return true;
        }

        PlacedInventoryItem heldItem =
            playerInventory.HeldItem;

        if (heldItem == null ||
            heldItem.ItemData == null)
        {
            return false;
        }

        Vector2Int placementOrigin =
            GetHeldPlacementOrigin(
                coordinate
            );

        bool placed =
            storageContainer.PlaceItem(
                heldItem.ItemData,
                placementOrigin.x,
                placementOrigin.y,
                heldItem.RotationSteps,
                heldItem.Quantity
            );

        if (!placed)
            return false;

        playerInventory.ClearHeldItemAfterExternalMove();
        Refresh();
        return true;
    }

    private bool TryMergeHeldItemIntoContainerStackAt(
        int x,
        int y)
    {
        if (storageContainer == null ||
            storageContainer.Grid == null ||
            playerInventory == null ||
            !playerInventory.IsHoldingItem)
        {
            return false;
        }

        PlacedInventoryItem heldItem =
            playerInventory.HeldItem;

        if (heldItem == null ||
            heldItem.ItemData == null ||
            !heldItem.ItemData.isStackable)
        {
            return false;
        }

        PlacedInventoryItem targetStack =
            storageContainer.Grid.GetPlacedItem(
                x,
                y
            );

        if (targetStack == null ||
            targetStack.ItemData == null)
        {
            return false;
        }

        if (targetStack.ItemData != heldItem.ItemData)
            return false;

        int addedQuantity =
            targetStack.AddQuantity(
                heldItem.Quantity
            );

        if (addedQuantity <= 0)
            return false;

        int remainingQuantity =
            heldItem.Quantity - addedQuantity;

        if (remainingQuantity <= 0)
        {
            playerInventory.ClearHeldItemAfterExternalMove();
        }
        else
        {
            playerInventory.SetHeldItemQuantityAfterExternalMove(
                remainingQuantity
            );
        }

        storageContainer.NotifyChanged();
        return true;
    }

    private void QuickTransferContainerItemToPlayer(Vector2Int coordinate)
    {
        if (storageContainer == null ||
            storageContainer.Grid == null ||
            playerInventory == null ||
            playerInventory.IsHoldingItem)
        {
            return;
        }

        PlacedInventoryItem pickedItem =
            storageContainer.PickUpItemAt(
                coordinate.x,
                coordinate.y
            );

        if (pickedItem == null ||
            pickedItem.ItemData == null)
        {
            return;
        }

        playerInventory.TryAddItemToFirstAvailableSpace(
            pickedItem.ItemData,
            pickedItem.RotationSteps,
            pickedItem.Quantity,
            out int remainingQuantity
        );

        if (remainingQuantity > 0)
        {
            storageContainer.PlaceItem(
                pickedItem.ItemData,
                pickedItem.Position.x,
                pickedItem.Position.y,
                pickedItem.RotationSteps,
                remainingQuantity
            );
        }

        Refresh();
    }

    private void CancelActiveStorageDrag()
    {
        if (activeStorageDragUI != this)
            return;

        if (storageContainer != null &&
            dragOriginalItemData != null)
        {
            bool restored =
                storageContainer.PlaceItem(
                    dragOriginalItemData,
                    dragOriginalPosition.x,
                    dragOriginalPosition.y,
                    dragOriginalRotationSteps,
                    dragOriginalQuantity
                );

            if (!restored)
            {
                Debug.LogError(
                    "Could not restore storage item after failed drag.",
                    this
                );
            }
        }

        if (playerInventory != null &&
            playerInventory.IsHoldingItem)
        {
            playerInventory.ClearHeldItemAfterExternalMove();
        }

        ClearActiveStorageDragStateOnly();
        Refresh();
    }

    private void ClearActiveStorageDragStateOnly()
    {
        if (activeStorageDragUI == this)
            activeStorageDragUI = null;

        dragOriginalItemData = null;
        dragOriginalPosition = Vector2Int.zero;
        dragOriginalRotationSteps = 0;
        dragOriginalQuantity = 0;
    }

    private Vector2Int GetHeldPlacementOrigin(
        Vector2Int hoveredCell)
    {
        PlacedInventoryItem heldItem =
            playerInventory != null
                ? playerInventory.HeldItem
                : null;

        if (heldItem == null)
            return hoveredCell;

        return hoveredCell - GetHeldGrabOffset(heldItem);
    }

    private Vector2Int GetHeldGrabOffset(
        PlacedInventoryItem heldItem)
    {
        if (heldItem == null)
            return Vector2Int.zero;

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

        return new Vector2Int(
            centerX,
            centerY
        );
    }

    private bool TryGetGridCoordinateFromScreenPoint(
        Vector2 screenPosition,
        out Vector2Int coordinate)
    {
        coordinate = new Vector2Int(-1, -1);

        if (storageContainer == null ||
            storageContainer.Grid == null ||
            cellParent == null ||
            gridLayoutGroup == null)
        {
            return false;
        }

        RectTransform cellParentRect =
            cellParent as RectTransform;

        if (cellParentRect == null)
            return false;

        Camera canvasCamera =
            GetEventCamera();

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

        Vector2 contentOffset =
            GetGridContentAlignmentOffset();

        float xFromLeft =
            localPoint.x - rect.xMin;

        float yFromTop =
            rect.yMax - localPoint.y;

        xFromLeft -=
            gridLayoutGroup.padding.left +
            contentOffset.x;

        yFromTop -=
            gridLayoutGroup.padding.top +
            contentOffset.y;

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
                storageContainer.Grid.Width
            );

        int rowFromTop =
            GetNearestGridIndex(
                yFromTop,
                cellSize.y,
                spacing.y,
                pitchY,
                storageContainer.Grid.Height
            );

        if (x < 0 ||
            rowFromTop < 0)
        {
            return false;
        }

        int y =
            storageContainer.Grid.Height - 1 - rowFromTop;

        coordinate =
            new Vector2Int(x, y);

        return coordinate.x >= 0 &&
               coordinate.y >= 0 &&
               coordinate.x < storageContainer.Grid.Width &&
               coordinate.y < storageContainer.Grid.Height;
    }


    private Vector2 GetGridContentAlignmentOffset()
    {
        if (storageContainer == null ||
            storageContainer.Grid == null ||
            cellParent == null ||
            gridLayoutGroup == null)
        {
            return Vector2.zero;
        }

        RectTransform cellParentRect =
            cellParent as RectTransform;

        if (cellParentRect == null)
            return Vector2.zero;

        Rect rect =
            cellParentRect.rect;

        RectOffset padding =
            gridLayoutGroup.padding;

        Vector2 cellSize =
            gridLayoutGroup.cellSize;

        Vector2 spacing =
            gridLayoutGroup.spacing;

        int gridWidth =
            storageContainer.Grid.Width;

        int gridHeight =
            storageContainer.Grid.Height;

        float contentWidth =
            gridWidth * cellSize.x +
            Mathf.Max(0, gridWidth - 1) * spacing.x;

        float contentHeight =
            gridHeight * cellSize.y +
            Mathf.Max(0, gridHeight - 1) * spacing.y;

        float innerWidth =
            rect.width -
            padding.left -
            padding.right;

        float innerHeight =
            rect.height -
            padding.top -
            padding.bottom;

        float freeX =
            Mathf.Max(0f, innerWidth - contentWidth);

        float freeY =
            Mathf.Max(0f, innerHeight - contentHeight);

        return new Vector2(
            GetHorizontalAlignmentOffset(freeX),
            GetVerticalAlignmentOffset(freeY)
        );
    }

    private float GetHorizontalAlignmentOffset(float freeSpace)
    {
        switch (gridLayoutGroup.childAlignment)
        {
            case TextAnchor.UpperCenter:
            case TextAnchor.MiddleCenter:
            case TextAnchor.LowerCenter:
                return freeSpace * 0.5f;

            case TextAnchor.UpperRight:
            case TextAnchor.MiddleRight:
            case TextAnchor.LowerRight:
                return freeSpace;

            default:
                return 0f;
        }
    }

    private float GetVerticalAlignmentOffset(float freeSpace)
    {
        switch (gridLayoutGroup.childAlignment)
        {
            case TextAnchor.MiddleLeft:
            case TextAnchor.MiddleCenter:
            case TextAnchor.MiddleRight:
                return freeSpace * 0.5f;

            case TextAnchor.LowerLeft:
            case TextAnchor.LowerCenter:
            case TextAnchor.LowerRight:
                return freeSpace;

            default:
                return 0f;
        }
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

    public bool TryPlaceHeldItemAtScreenPosition(Vector2 screenPosition)
    {
        if (storageContainer == null ||
            storageContainer.Grid == null ||
            playerInventory == null ||
            !playerInventory.IsHoldingItem)
        {
            return false;
        }

        if (!TryGetGridCoordinateFromScreenPoint(
                screenPosition,
                out Vector2Int coordinate))
        {
            return false;
        }

        return TryPlaceOrMergeHeldItemIntoContainer(
            coordinate
        );
    }

    private Camera GetEventCamera()
    {
        if (rootCanvas == null)
            return null;

        if (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        return rootCanvas.worldCamera;
    }

    private bool IsHeldItemPreviewCell(
        Vector2Int cellCoordinate,
        Vector2Int placementOrigin)
    {
        if (playerInventory == null ||
            playerInventory.HeldItem == null)
        {
            return false;
        }

        return playerInventory.HeldItem.OccupiesCellAt(
            cellCoordinate,
            placementOrigin
        );
    }

    private bool IsOriginalFootprint(
        Vector2Int coordinate)
    {
        if (!isDraggingStorageItem ||
            dragOriginalItemData == null)
        {
            return false;
        }

        int localX =
            coordinate.x - dragOriginalPosition.x;

        int localY =
            coordinate.y - dragOriginalPosition.y;

        int width =
            dragOriginalItemData.GetWidth(
                dragOriginalRotationSteps
            );

        int height =
            dragOriginalItemData.GetHeight(
                dragOriginalRotationSteps
            );

        if (localX < 0 ||
            localY < 0 ||
            localX >= width ||
            localY >= height)
        {
            return false;
        }

        return dragOriginalItemData.IsCellOccupied(
            localX,
            localY,
            dragOriginalRotationSteps
        );
    }

    private void BuildItemOutlines()
    {
        if (itemOutline == null ||
            gridLayoutGroup == null ||
            storageContainer == null ||
            storageContainer.Grid == null)
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
            storageContainer.Grid;

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

        if (isDraggingStorageItem &&
            dragOriginalItemData != null)
        {
            DrawItemOutline(
                dragOriginalItemData,
                dragOriginalPosition,
                dragOriginalRotationSteps,
                dragOriginalOutlineColor
            );
        }

        PlacedInventoryItem heldItem =
            playerInventory != null
            ? playerInventory.HeldItem
            : null;

        if (heldItem != null &&
            heldItem.ItemData != null &&
            Mouse.current != null &&
            TryGetGridCoordinateFromScreenPoint(
                Mouse.current.position.ReadValue(),
                out Vector2Int previewCoordinate))
        {
            Vector2Int previewOrigin =
                GetHeldPlacementOrigin(previewCoordinate);

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
            storageContainer.Grid.Height - 1 - gridY;

        Vector2 contentOffset =
            GetGridContentAlignmentOffset();

        float cellLeft =
            padding.left +
            contentOffset.x +
            gridX * (cellSize.x + spacing.x);

        float cellTop =
            -padding.top -
            contentOffset.y -
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
            storageContainer.Grid.Height - 1 - gridY;

        Vector2 contentOffset =
            GetGridContentAlignmentOffset();

        float cellLeft =
            padding.left +
            contentOffset.x +
            gridX * (cellSize.x + spacing.x);

        float cellTop =
            -padding.top -
            contentOffset.y -
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
            storageContainer.Grid.Height - 1 - gridY;

        Vector2 contentOffset =
            GetGridContentAlignmentOffset();

        float cellLeft =
            padding.left +
            contentOffset.x +
            gridX * (cellSize.x + spacing.x);

        float cellTop =
            -padding.top -
            contentOffset.y -
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
            storageContainer.Grid.Height - 1 - gridY;

        Vector2 contentOffset =
            GetGridContentAlignmentOffset();

        float cellLeft =
            padding.left +
            contentOffset.x +
            gridX * (cellSize.x + spacing.x);

        float cellTop =
            -padding.top -
            contentOffset.y -
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
                "StorageItemOutlinePiece",
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

    private void RefreshHeldPreviewIfNeeded()
    {
        bool isShowingHeldPreview =
            playerInventory != null &&
            playerInventory.IsHoldingItem &&
            InventoryMenuController.IsInventoryOpen;

        if (isShowingHeldPreview != wasShowingHeldPreview)
        {
            wasShowingHeldPreview = isShowingHeldPreview;
            Refresh();
            return;
        }

        if (!isShowingHeldPreview ||
            Mouse.current == null)
        {
            return;
        }

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        if ((mousePosition - lastHeldPreviewMousePosition).sqrMagnitude < 0.01f)
            return;

        lastHeldPreviewMousePosition = mousePosition;
        Refresh();
    }

    private void OnCellPointerEntered(Vector2Int coordinate)
    {
        Refresh();
    }

    private void OnCellPointerExited(Vector2Int coordinate)
    {
        Refresh();
    }
}
