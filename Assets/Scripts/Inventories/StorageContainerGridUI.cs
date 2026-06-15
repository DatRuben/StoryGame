using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class StorageContainerGridUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StorageContainer storageContainer;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private InventoryGridUI playerInventoryGridUI;
    [SerializeField] private Transform cellParent;
    [SerializeField] private GameObject cellPrefab;

    [Header("Drag")]
    [SerializeField] private bool allowDragToPlayerInventory = true;
    [SerializeField] private float dragStartDistance = 12f;

    [Header("Colors")]
    [SerializeField] private Color emptyColor = new Color(0f, 0f, 0f, 0.35f);
    [SerializeField] private Color occupiedColor = new Color(1f, 1f, 1f, 0.85f);

    private GridLayoutGroup gridLayoutGroup;
    private Canvas rootCanvas;

    private bool suppressNextContainerClick;
    private bool pointerIsDown;
    private bool pendingStorageDragPickup;
    private bool isDraggingStorageItem;
    private Vector2 pointerDownScreenPosition;
    private Vector2Int pointerDownCoordinate;

    private static StorageContainerGridUI pendingStorageDragUI;
    private static StorageContainer pendingSourceContainer;
    private static Vector2Int pendingSourcePosition;
    private static int pendingSourceRotationSteps;
    private static ItemData pendingSourceItemData;
    private static int pendingSourceQuantity;

    public static bool HasPendingStorageDrag =>
        pendingStorageDragUI != null &&
        pendingSourceContainer != null &&
        pendingSourceItemData != null;

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

        if (pendingStorageDragUI == this)
            ClearPendingStorageDragSourceOnly();

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

        storageContainer = newStorageContainer;

        if (storageContainer != null)
            storageContainer.OnContainerChanged += Refresh;

        BuildGrid();
        Refresh();
    }

    public static void CommitPendingStorageDrag()
    {
        if (!HasPendingStorageDrag)
            return;

        StorageContainerGridUI sourceUI =
            pendingStorageDragUI;

        StorageContainer sourceContainer =
            pendingSourceContainer;

        Vector2Int sourcePosition =
            pendingSourcePosition;

        PlacedInventoryItem removedSource =
            sourceContainer.PickUpItemAt(
                sourcePosition.x,
                sourcePosition.y
            );

        if (removedSource == null)
        {
            Debug.LogWarning(
                "Could not remove pending storage drag source. It may have already moved."
            );
        }

        ClearPendingStorageDragSourceOnly();

        if (sourceUI != null)
            sourceUI.Refresh();
    }

    public static void CancelPendingStorageDrag(
        PlayerInventory playerInventory)
    {
        if (playerInventory != null &&
            playerInventory.IsHoldingItem)
        {
            playerInventory.ClearHeldItemAfterExternalMove();
        }

        StorageContainerGridUI sourceUI =
            pendingStorageDragUI;

        ClearPendingStorageDragSourceOnly();

        if (sourceUI != null)
            sourceUI.Refresh();
    }

    private static void ClearPendingStorageDragSourceOnly()
    {
        pendingStorageDragUI = null;
        pendingSourceContainer = null;
        pendingSourcePosition = Vector2Int.zero;
        pendingSourceRotationSteps = 0;
        pendingSourceItemData = null;
        pendingSourceQuantity = 0;
    }

    private bool IsPendingStorageDragFromThisContainer()
    {
        return HasPendingStorageDrag &&
               pendingStorageDragUI == this &&
               pendingSourceContainer == storageContainer;
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

        for (int i = 0; i < cells.Count; i++)
        {
            InventoryCellUI cell =
                cells[i];

            if (cell == null)
                continue;

            Vector2Int coordinate =
                cellCoordinates[i];

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

        BeginPendingStorageDragFromCell(
            coordinate
        );
    }

    private void OnCellPointerDown(Vector2Int coordinate)
    {
        if (storageContainer == null ||
            storageContainer.Grid == null ||
            playerInventory == null)
        {
            return;
        }

        if (Mouse.current == null)
            return;

        pointerIsDown = true;
        pendingStorageDragPickup = false;
        isDraggingStorageItem = false;

        pointerDownScreenPosition =
            Mouse.current.position.ReadValue();

        pointerDownCoordinate = coordinate;

        if (playerInventory.IsHoldingItem)
            return;

        if (IsQuickTransferHeld())
            return;

        PlacedInventoryItem sourceItem =
            storageContainer.Grid.GetPlacedItem(
                coordinate.x,
                coordinate.y
            );

        pendingStorageDragPickup =
            sourceItem != null;
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
            BeginPendingStorageDragFromCell(
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
        if (!isDraggingStorageItem)
            return;

        if (Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasReleasedThisFrame)
            return;

        CompleteStorageDragRelease();
    }

    private void CompleteStorageDragRelease()
    {
        isDraggingStorageItem = false;
        pointerIsDown = false;
        pendingStorageDragPickup = false;
        suppressNextContainerClick = true;

        if (!playerInventory.IsHoldingItem ||
            !IsPendingStorageDragFromThisContainer())
        {
            return;
        }

        if (IsMouseOverStorageGrid(out Vector2Int storageCoordinate))
        {
            TryPlaceOrMergeHeldItemIntoContainer(
                storageCoordinate
            );

            return;
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
                Refresh();
                return;
            }
        }

        CancelPendingStorageDrag(playerInventory);
    }

    private bool BeginPendingStorageDragFromCell(
        Vector2Int coordinate)
    {
        PlacedInventoryItem sourceItem =
            storageContainer.Grid.GetPlacedItem(
                coordinate.x,
                coordinate.y
            );

        if (sourceItem == null ||
            sourceItem.ItemData == null)
        {
            return false;
        }

        if (HasPendingStorageDrag)
            CancelPendingStorageDrag(playerInventory);

        pendingStorageDragUI = this;
        pendingSourceContainer = storageContainer;
        pendingSourcePosition = sourceItem.Position;
        pendingSourceRotationSteps = sourceItem.RotationSteps;
        pendingSourceItemData = sourceItem.ItemData;
        pendingSourceQuantity = sourceItem.Quantity;

        playerInventory.SetMouseHeldItemFromExternal(
            sourceItem.ItemData,
            sourceItem.RotationSteps,
            false,
            sourceItem.Quantity
        );

        Refresh();

        return true;
    }

    private void PickUpContainerItem(
        Vector2Int coordinate)
    {
        BeginPendingStorageDragFromCell(
            coordinate
        );
    }

    private bool TryPlaceOrMergeHeldItemIntoContainer(
        Vector2Int coordinate)
    {
        if (IsPendingStorageDragFromThisContainer())
        {
            return TryPlaceOrMergePendingStorageDragIntoThisContainer(
                coordinate
            );
        }

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
            GetCenteredHeldPlacementOrigin(
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

    private bool TryPlaceOrMergePendingStorageDragIntoThisContainer(
        Vector2Int coordinate)
    {
        if (!IsPendingStorageDragFromThisContainer() ||
            playerInventory == null ||
            !playerInventory.IsHoldingItem ||
            playerInventory.HeldItem == null)
        {
            return false;
        }

        PlacedInventoryItem heldItem =
            playerInventory.HeldItem;

        PlacedInventoryItem removedSource =
            storageContainer.PickUpItemAt(
                pendingSourcePosition.x,
                pendingSourcePosition.y
            );

        if (removedSource == null)
        {
            CancelPendingStorageDrag(playerInventory);
            return false;
        }

        bool completedDrop = false;

        PlacedInventoryItem targetStack =
            storageContainer.Grid.GetPlacedItem(
                coordinate.x,
                coordinate.y
            );

        if (targetStack != null &&
            targetStack.ItemData == heldItem.ItemData &&
            targetStack.ItemData != null &&
            targetStack.ItemData.isStackable)
        {
            int addedQuantity =
                targetStack.AddQuantity(
                    heldItem.Quantity
                );

            if (addedQuantity > 0)
            {
                int remainingQuantity =
                    heldItem.Quantity - addedQuantity;

                if (remainingQuantity <= 0)
                    playerInventory.ClearHeldItemAfterExternalMove();
                else
                    playerInventory.SetHeldItemQuantityAfterExternalMove(
                        remainingQuantity
                    );

                completedDrop = true;
            }
        }

        if (!completedDrop &&
            playerInventory.IsHoldingItem &&
            playerInventory.HeldItem != null)
        {
            Vector2Int placementOrigin =
                GetCenteredHeldPlacementOrigin(
                    coordinate
                );

            completedDrop =
                storageContainer.PlaceItem(
                    heldItem.ItemData,
                    placementOrigin.x,
                    placementOrigin.y,
                    heldItem.RotationSteps,
                    heldItem.Quantity
                );

            if (completedDrop)
            {
                playerInventory.ClearHeldItemAfterExternalMove();
            }
        }

        if (completedDrop)
        {
            ClearPendingStorageDragSourceOnly();
            storageContainer.NotifyChanged();
            Refresh();
            return true;
        }

        bool restored =
            storageContainer.PlaceItem(
                removedSource.ItemData,
                removedSource.Position.x,
                removedSource.Position.y,
                removedSource.RotationSteps,
                removedSource.Quantity
            );

        if (!restored)
        {
            Debug.LogError(
                "Could not restore storage item after failed storage drag."
            );
        }

        CancelPendingStorageDrag(playerInventory);
        Refresh();
        return false;
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

    private void QuickTransferContainerItemToPlayer(
        Vector2Int coordinate)
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

        bool fullyAddedToPlayer =
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

    private Vector2Int GetCenteredHeldPlacementOrigin(
        Vector2Int clickedCoordinate)
    {
        if (playerInventory == null ||
            playerInventory.HeldItem == null)
        {
            return clickedCoordinate;
        }

        PlacedInventoryItem heldItem =
            playerInventory.HeldItem;

        int offsetX =
            Mathf.Max(
                0,
                heldItem.Width / 2
            );

        int offsetY =
            Mathf.Max(
                0,
                heldItem.Height / 2
            );

        return new Vector2Int(
            clickedCoordinate.x - offsetX,
            clickedCoordinate.y - offsetY
        );
    }

    private bool IsMouseOverStorageGrid(
        out Vector2Int coordinate)
    {
        coordinate = new Vector2Int(-1, -1);

        if (Mouse.current == null)
            return false;

        return TryGetGridCoordinateFromScreenPoint(
            Mouse.current.position.ReadValue(),
            out coordinate
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

        Canvas rootCanvas =
            GetComponentInParent<Canvas>();

        if (rootCanvas == null)
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
            Mathf.FloorToInt(xFromLeft / pitchX);

        int rowFromTop =
            Mathf.FloorToInt(yFromTop / pitchY);

        int y =
            storageContainer.Grid.Height - 1 - rowFromTop;

        if (x < 0 ||
            y < 0 ||
            x >= storageContainer.Grid.Width ||
            y >= storageContainer.Grid.Height)
        {
            return false;
        }

        coordinate = new Vector2Int(x, y);
        return true;
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

        if (!TryGetCellCoordinateFromScreenPosition(
                screenPosition,
                out Vector2Int coordinate))
        {
            return false;
        }

        return TryPlaceOrMergeHeldItemIntoContainer(
            coordinate
        );
    }

    private bool TryGetCellCoordinateFromScreenPosition(
        Vector2 screenPosition,
        out Vector2Int coordinate)
    {
        coordinate = new Vector2Int(-1, -1);

        Camera eventCamera =
            GetEventCamera();

        for (int i = 0; i < cells.Count; i++)
        {
            InventoryCellUI cell =
                cells[i];

            if (cell == null)
                continue;

            RectTransform rectTransform =
                cell.GetComponent<RectTransform>();

            if (rectTransform == null)
                continue;

            bool containsPoint =
                RectTransformUtility.RectangleContainsScreenPoint(
                    rectTransform,
                    screenPosition,
                    eventCamera
                );

            if (!containsPoint)
                continue;

            coordinate = cellCoordinates[i];
            return true;
        }

        return false;
    }

    private Camera GetEventCamera()
    {
        if (rootCanvas == null)
            return null;

        if (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        return rootCanvas.worldCamera;
    }

    private void OnCellPointerEntered(Vector2Int coordinate)
    {
    }

    private void OnCellPointerExited(Vector2Int coordinate)
    {
    }
}
