using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public sealed class InventoryGridUI :
    MonoBehaviour
{
    private static readonly
        List<InventoryGridUI> activeGrids =
            new List<InventoryGridUI>();

    [Header("References")]
    [SerializeField]
    private InventoryContainer inventoryContainer;

    [SerializeField]
    private InventoryInteractionController
        interactionController;

    [SerializeField]
    private InventoryGridUI quickTransferTarget;

    [SerializeField]
    private Transform cellParent;

    [SerializeField]
    private GameObject cellPrefab;

    [SerializeField]
    private RectTransform itemOutline;

    [Header("Drag Detection")]
    [SerializeField]
    private float dragStartDistance = 12f;

    [Header("Colors")]
    [SerializeField]
    private Color emptyColor =
        new Color(0f, 0f, 0f, 0.35f);

    [SerializeField]
    private Color occupiedColor =
        new Color(1f, 1f, 1f, 0.85f);

    [SerializeField]
    private Color validPlacementColor =
        new Color(0.2f, 1f, 0.2f, 0.85f);

    [SerializeField]
    private Color partialStackPlacementColor =
        new Color(1f, 0.85f, 0.15f, 0.85f);

    [SerializeField]
    private Color invalidPlacementColor =
        new Color(1f, 0.2f, 0.2f, 0.85f);

    [SerializeField]
    private Color heldPreviewColor =
        new Color(1f, 1f, 1f, 0.65f);

    [SerializeField]
    private Color dragOriginalGhostColor =
        new Color(0.45f, 0.45f, 0.45f, 0.35f);

    [Header("Item Outlines")]
    [SerializeField]
    private Color itemOutlineColor =
        new Color(1f, 1f, 1f, 0.95f);

    [SerializeField]
    private Color dragOriginalOutlineColor =
        new Color(0.15f, 0.15f, 0.15f, 0.9f);

    [SerializeField]
    private float itemOutlineThickness = 3f;

    [SerializeField]
    private bool fillPaddingBetweenCells = true;

    [Header("Selected Item Mouse Preview")]
    [SerializeField]
    private Vector2 heldPreviewOffset =
        Vector2.zero;

    private GridLayoutGroup gridLayoutGroup;
    private Canvas rootCanvas;

    private RectTransform heldPreviewRoot;
    private GridLayoutGroup heldPreviewLayoutGroup;
    private CanvasGroup heldPreviewCanvasGroup;

    private bool heldPreviewEnabled = true;

    private readonly List<InventoryCellUI> cells =
        new List<InventoryCellUI>();

    private readonly List<Vector2Int> cellCoordinates =
        new List<Vector2Int>();

    private Vector2Int hoveredCoordinate =
        new Vector2Int(-1, -1);

    private bool pointerIsDown;
    private bool pendingDragPickup;
    private bool isDraggingItem;
    private bool suppressNextClick;

    private Vector2 pointerDownScreenPosition;
    private Vector2Int pointerDownCoordinate;

    private InventoryContainer dragSourceContainer;
    private InventoryItemInstance draggedItem;
    private Vector2Int dragOriginalPosition;
    private int dragOriginalRotationSteps;

    public InventoryContainer Container =>
        inventoryContainer;

    private void Awake()
    {
        if (cellParent != null)
        {
            gridLayoutGroup =
                cellParent.GetComponent<
                    GridLayoutGroup>();
        }

        if (gridLayoutGroup == null)
        {
            gridLayoutGroup =
                GetComponent<GridLayoutGroup>();
        }

        rootCanvas =
            GetComponentInParent<Canvas>();

        CreateHeldPreviewRoot();
    }

    private void OnEnable()
    {
        if (!activeGrids.Contains(this))
            activeGrids.Add(this);

        SubscribeState();

        BuildGrid();
        HandleInteractionChanged();
    }

    private void OnDisable()
    {
        if (isDraggingItem &&
            interactionController != null &&
            interactionController.HasSelection &&
            dragSourceContainer != null &&
            draggedItem != null &&
            ReferenceEquals(
                interactionController.SelectedItem,
                draggedItem))
        {
            interactionController
                .TryReturnSelectionToContainer(
                    dragSourceContainer,
                    dragOriginalPosition,
                    dragOriginalRotationSteps
                );
        }

        activeGrids.Remove(this);

        UnsubscribeState();

        pointerIsDown = false;
        pendingDragPickup = false;
        isDraggingItem = false;

        dragSourceContainer = null;
        draggedItem = null;
    }

    private void Update()
    {
        HandleDragDetection();
        HandleOutsideSelectionDrop();
        HandleDragRelease();
        UpdateHoveredCoordinateFromMouse();
        UpdateHeldPreviewVisibility();
        UpdateHeldPreviewPosition();
    }

    private void HandleOutsideSelectionDrop()
    {
        if (!heldPreviewEnabled ||
            !InventoryMenuController
                .IsInventoryOpen ||
            interactionController == null ||
            !interactionController.HasSelection ||
            isDraggingItem ||
            Mouse.current == null ||
            !Mouse.current.leftButton
                .wasReleasedThisFrame)
        {
            return;
        }

        if (EventSystem.current != null &&
            EventSystem.current
                .IsPointerOverGameObject())
        {
            return;
        }

        interactionController
            .TryDropSelection();
    }

    public void BindPlayer(
        InventoryContainer newContainer,
        InventoryInteractionController
            newInteractionController,
        bool enableHeldPreview = true)
    {
        UnsubscribeState();

        inventoryContainer =
            newContainer;

        interactionController =
            newInteractionController;

        heldPreviewEnabled =
            enableHeldPreview;

        if (isActiveAndEnabled)
            SubscribeState();

        BuildGrid();
        HandleInteractionChanged();
    }

    public void BindContainer(
        InventoryContainer newContainer)
    {
        UnsubscribeState();

        inventoryContainer =
            newContainer;

        if (isActiveAndEnabled)
            SubscribeState();

        BuildGrid();
        Refresh();
    }

    public void SetQuickTransferTarget(
        InventoryGridUI target)
    {
        quickTransferTarget =
            target;
    }

    private void SubscribeState()
    {
        if (inventoryContainer != null)
        {
            inventoryContainer.Changed -=
                Refresh;

            inventoryContainer.Changed +=
                Refresh;
        }

        if (interactionController != null)
        {
            interactionController.Changed -=
                HandleInteractionChanged;

            interactionController.Changed +=
                HandleInteractionChanged;
        }
    }

    private void UnsubscribeState()
    {
        if (inventoryContainer != null)
        {
            inventoryContainer.Changed -=
                Refresh;
        }

        if (interactionController != null)
        {
            interactionController.Changed -=
                HandleInteractionChanged;
        }
    }

    private void BuildGrid()
    {
        ClearGrid();

        if (inventoryContainer == null ||
            cellParent == null ||
            cellPrefab == null)
        {
            return;
        }

        if (gridLayoutGroup != null)
        {
            gridLayoutGroup.constraint =
                GridLayoutGroup.Constraint
                    .FixedColumnCount;

            gridLayoutGroup.constraintCount =
                inventoryContainer.Width;
        }

        for (int y =
                 inventoryContainer.Height - 1;
             y >= 0;
             y--)
        {
            for (int x = 0;
                 x < inventoryContainer.Width;
                 x++)
            {
                GameObject cellObject =
                    Instantiate(
                        cellPrefab,
                        cellParent
                    );

                InventoryCellUI cellUI =
                    cellObject.GetComponent<
                        InventoryCellUI>();

                if (cellUI == null)
                {
                    cellUI =
                        cellObject.AddComponent<
                            InventoryCellUI>();
                }

                Vector2Int coordinate =
                    new Vector2Int(
                        x,
                        y
                    );

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

                cells.Add(
                    cellUI
                );

                cellCoordinates.Add(
                    coordinate
                );
            }
        }
    }

    private void ClearGrid()
    {
        if (cellParent != null)
        {
            for (int i =
                     cellParent.childCount - 1;
                 i >= 0;
                 i--)
            {
                Transform child =
                    cellParent.GetChild(i);

                if (itemOutline != null &&
                    child ==
                    itemOutline.transform)
                {
                    continue;
                }

                Destroy(
                    child.gameObject
                );
            }
        }

        cells.Clear();
        cellCoordinates.Clear();

        if (itemOutline != null)
        {
            InventoryUIUtility.ClearChildren(
                itemOutline
            );
        }
    }

    private void OnCellClicked(
        Vector2Int coordinate)
    {
        if (!InventoryMenuController
            .IsInventoryOpen ||
            inventoryContainer == null ||
            interactionController == null)
        {
            return;
        }

        if (suppressNextClick)
        {
            suppressNextClick = false;
            return;
        }

        if (interactionController.HasSelection)
        {
            if (interactionController
                .TryMergeSelectionIntoStackAt(
                    inventoryContainer,
                    coordinate))
            {
                RefreshAllGrids();
                return;
            }

            Vector2Int origin =
                coordinate -
                interactionController
                    .SelectedGrabOffset;

            interactionController
                .TryPlaceSelection(
                    inventoryContainer,
                    origin
                );

            RefreshAllGrids();
            return;
        }

        if (IsQuickTransferHeld() &&
            quickTransferTarget != null &&
            quickTransferTarget.Container != null)
        {
            bool transferred =
                interactionController
                    .TryQuickTransfer(
                        inventoryContainer,
                        quickTransferTarget.Container,
                        coordinate
                    );

            if (transferred)
            {
                RefreshAllGrids();
                return;
            }
        }

        interactionController
            .TryPickUpItemFromContainer(
                inventoryContainer,
                coordinate
            );

        RefreshAllGrids();
    }

    private void OnCellRightClicked(
        Vector2Int coordinate)
    {
        if (!InventoryMenuController
            .IsInventoryOpen ||
            inventoryContainer == null ||
            interactionController == null)
        {
            return;
        }

        if (interactionController.HasSelection)
        {
            Vector2Int origin =
                coordinate -
                interactionController
                    .SelectedGrabOffset;

            interactionController
                .TryPlaceOneSelection(
                    inventoryContainer,
                    origin
                );

            RefreshAllGrids();
            return;
        }

        interactionController
            .TrySplitStackFromContainer(
                inventoryContainer,
                coordinate
            );

        RefreshAllGrids();
    }

    private void OnCellPointerDown(
        Vector2Int coordinate)
    {
        if (!InventoryMenuController
            .IsInventoryOpen ||
            Mouse.current == null)
        {
            return;
        }

        if (suppressNextClick)
            suppressNextClick = false;

        pointerIsDown = true;
        pendingDragPickup = false;

        pointerDownScreenPosition =
            Mouse.current.position.ReadValue();

        pointerDownCoordinate =
            coordinate;

        if (inventoryContainer == null ||
            interactionController == null ||
            interactionController.HasSelection)
        {
            return;
        }

        pendingDragPickup =
            inventoryContainer.GetItemAt(
                coordinate.x,
                coordinate.y
            ) != null;
    }

    private void OnCellPointerUp(
        Vector2Int coordinate)
    {
        if (isDraggingItem)
        {
            CompleteDragDrop();
            return;
        }

        pointerIsDown = false;
        pendingDragPickup = false;
    }

    private void OnCellPointerEntered(
        Vector2Int coordinate)
    {
        hoveredCoordinate =
            coordinate;

        Refresh();
    }

    private void OnCellPointerExited(
        Vector2Int coordinate)
    {
        if (hoveredCoordinate ==
            coordinate)
        {
            hoveredCoordinate =
                new Vector2Int(-1, -1);

            Refresh();
        }
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

        if (!Mouse.current.leftButton
            .isPressed)
        {
            pointerIsDown = false;
            pendingDragPickup = false;
            return;
        }

        float distance =
            Vector2.Distance(
                pointerDownScreenPosition,
                Mouse.current.position
                    .ReadValue()
            );

        if (distance <
            dragStartDistance)
        {
            return;
        }

        StartDragPickup();
    }

    private void StartDragPickup()
    {
        if (inventoryContainer == null ||
            interactionController == null)
        {
            return;
        }

        PlacedInventoryItem placedItem =
            inventoryContainer.GetItemAt(
                pointerDownCoordinate.x,
                pointerDownCoordinate.y
            );

        if (placedItem == null)
            return;

        dragSourceContainer =
            inventoryContainer;

        dragOriginalPosition =
            placedItem.Position;

        dragOriginalRotationSteps =
            placedItem.RotationSteps;

        bool pickedUp =
            interactionController
                .TryPickUpItemFromContainer(
                    inventoryContainer,
                    pointerDownCoordinate
                );

        if (!pickedUp)
            return;

        draggedItem =
            interactionController
                .SelectedItem;

        isDraggingItem = true;
        pendingDragPickup = false;
        suppressNextClick = true;

        RefreshAllGrids();
    }

    private void HandleDragRelease()
    {
        if (!isDraggingItem ||
            Mouse.current == null)
        {
            return;
        }

        if (Mouse.current.leftButton
            .wasReleasedThisFrame)
        {
            CompleteDragDrop();
        }
    }

    private void CompleteDragDrop()
    {
        pointerIsDown = false;
        pendingDragPickup = false;
        suppressNextClick = true;

        if (interactionController == null ||
            !interactionController.HasSelection ||
            Mouse.current == null)
        {
            isDraggingItem = false;
            dragSourceContainer = null;
            draggedItem = null;

            RefreshAllGrids();
            return;
        }

        Vector2 screenPosition =
            Mouse.current.position.ReadValue();

        bool acceptedDrop = false;

        for (int i = 0;
             i < activeGrids.Count;
             i++)
        {
            InventoryGridUI grid =
                activeGrids[i];

            if (grid == null ||
                !grid.isActiveAndEnabled)
            {
                continue;
            }

            if (!grid.TryDropSelectionAtScreenPoint(
                screenPosition))
            {
                continue;
            }

            acceptedDrop = true;
            break;
        }

        if (!acceptedDrop &&
            dragSourceContainer != null &&
            draggedItem != null &&
            ReferenceEquals(
                interactionController.SelectedItem,
                draggedItem))
        {
            bool dropped =
                interactionController
                    .TryDropHeldItem(
                        draggedItem
                    );

            if (!dropped)
            {
                bool returned =
                    interactionController
                        .TryReturnSelectionToContainer(
                            dragSourceContainer,
                            dragOriginalPosition,
                            dragOriginalRotationSteps
                        );

                if (!returned)
                {
                    Debug.LogWarning(
                        "Dragged item could not be dropped or returned to its original inventory position.",
                        this
                    );
                }
            }
        }

        isDraggingItem = false;
        dragSourceContainer = null;
        draggedItem = null;

        RefreshAllGrids();
    }

    public bool TryDropSelectionAtScreenPoint(
        Vector2 screenPosition)
    {
        if (inventoryContainer == null ||
            interactionController == null ||
            !interactionController.HasSelection)
        {
            return false;
        }

        if (!TryGetGridCoordinateFromScreenPoint(
            screenPosition,
            out Vector2Int coordinate))
        {
            return false;
        }

        if (interactionController
            .TryMergeSelectionIntoStackAt(
                inventoryContainer,
                coordinate))
        {
            return true;
        }

        Vector2Int origin =
            coordinate -
            interactionController
                .SelectedGrabOffset;

        return interactionController
            .TryPlaceSelection(
                inventoryContainer,
                origin
            );
    }

    private bool IsQuickTransferHeld()
    {
        if (Keyboard.current == null)
            return false;

        return Keyboard.current.leftCtrlKey
                   .isPressed ||
               Keyboard.current.rightCtrlKey
                   .isPressed;
    }

    private void Refresh()
    {
        if (inventoryContainer == null)
        {
            if (itemOutline != null)
            {
                InventoryUIUtility.ClearChildren(
                    itemOutline
                );
            }

            return;
        }

        int expectedCellCount =
            inventoryContainer.Width *
            inventoryContainer.Height;

        if (cells.Count !=
            expectedCellCount)
        {
            BuildGrid();
        }

        bool hasSelection =
            interactionController != null &&
            interactionController.HasSelection &&
            interactionController
                .SelectedDefinition != null;

        bool hoverValid =
            IsValidGridCoordinate(
                hoveredCoordinate
            );

        Vector2Int previewOrigin =
            hoverValid &&
            interactionController != null
                ? hoveredCoordinate -
                  interactionController
                      .SelectedGrabOffset
                : new Vector2Int(
                    -999,
                    -999
                );

        bool canPlace =
            hasSelection &&
            hoverValid &&
            interactionController
                .CanPlaceSelection(
                    inventoryContainer,
                    previewOrigin
                );

        for (int i = 0;
             i < cells.Count;
             i++)
        {
            InventoryCellUI cell =
                cells[i];

            if (cell == null)
                continue;

            Vector2Int coordinate =
                cellCoordinates[i];

            if (hasSelection &&
                hoverValid &&
                TryGetStackPreviewColor(
                    coordinate,
                    out Color stackColor))
            {
                cell.SetColor(
                    stackColor
                );

                cell.SetQuantityText(
                    GetQuantityText(
                        coordinate
                    )
                );

                continue;
            }

            if (hasSelection &&
                hoverValid &&
                IsSelectionPreviewCell(
                    coordinate,
                    previewOrigin))
            {
                cell.SetColor(
                    canPlace
                        ? validPlacementColor
                        : invalidPlacementColor
                );

                cell.SetQuantityText("");
                continue;
            }

            if (isDraggingItem &&
                ReferenceEquals(
                    inventoryContainer,
                    dragSourceContainer) &&
                IsOriginalDragFootprint(
                    coordinate))
            {
                cell.SetColor(
                    dragOriginalGhostColor
                );

                cell.SetQuantityText("");
                continue;
            }

            PlacedInventoryItem item =
                inventoryContainer.GetItemAt(
                    coordinate.x,
                    coordinate.y
                );

            cell.SetColor(
                item == null
                    ? emptyColor
                    : occupiedColor
            );

            cell.SetQuantityText(
                InventoryQuantityTextUtility
                    .GetTextForCell(
                        item,
                        coordinate
                    )
            );
        }

        BuildItemOutlines();
    }

    private string GetQuantityText(
        Vector2Int coordinate)
    {
        PlacedInventoryItem item =
            inventoryContainer.GetItemAt(
                coordinate.x,
                coordinate.y
            );

        return InventoryQuantityTextUtility
            .GetTextForCell(
                item,
                coordinate
            );
    }

    private bool TryGetStackPreviewColor(
        Vector2Int coordinate,
        out Color color)
    {
        color =
            invalidPlacementColor;

        if (interactionController == null ||
            inventoryContainer == null ||
            !IsValidGridCoordinate(
                hoveredCoordinate))
        {
            return false;
        }

        if (!interactionController
            .CanMergeSelectionIntoStackAt(
                inventoryContainer,
                hoveredCoordinate,
                out bool fullyFits))
        {
            return false;
        }

        PlacedInventoryItem target =
            inventoryContainer.GetItemAt(
                hoveredCoordinate.x,
                hoveredCoordinate.y
            );

        if (target == null)
            return false;

        PlacedInventoryItem cellItem =
            inventoryContainer.GetItemAt(
                coordinate.x,
                coordinate.y
            );

        if (!ReferenceEquals(
            target,
            cellItem))
        {
            return false;
        }

        color =
            fullyFits
                ? validPlacementColor
                : partialStackPlacementColor;

        return true;
    }

    private bool IsSelectionPreviewCell(
        Vector2Int coordinate,
        Vector2Int origin)
    {
        if (interactionController == null)
            return false;

        ItemDefinition definition =
            interactionController
                .SelectedDefinition;

        if (definition == null)
            return false;

        int localX =
            coordinate.x -
            origin.x;

        int localY =
            coordinate.y -
            origin.y;

        if (localX < 0 ||
            localY < 0 ||
            localX >= definition.GetWidth(
                interactionController
                    .SelectedRotationSteps) ||
            localY >= definition.GetHeight(
                interactionController
                    .SelectedRotationSteps))
        {
            return false;
        }

        return definition.IsCellOccupied(
            localX,
            localY,
            interactionController
                .SelectedRotationSteps
        );
    }

    private bool IsOriginalDragFootprint(
        Vector2Int coordinate)
    {
        if (!isDraggingItem ||
            draggedItem == null ||
            draggedItem.Definition == null)
        {
            return false;
        }

        return InventoryShapeUtility
            .IsOccupiedInShape(
                draggedItem.Definition,
                coordinate.x -
                    dragOriginalPosition.x,
                coordinate.y -
                    dragOriginalPosition.y,
                dragOriginalRotationSteps
            );
    }

    private bool TryGetGridCoordinateFromScreenPoint(
        Vector2 screenPosition,
        out Vector2Int coordinate)
    {
        coordinate =
            new Vector2Int(-1, -1);

        if (inventoryContainer == null ||
            cellParent == null ||
            gridLayoutGroup == null ||
            rootCanvas == null)
        {
            return false;
        }

        RectTransform rect =
            cellParent as RectTransform;

        if (rect == null)
            return false;

        Camera canvasCamera =
            rootCanvas.renderMode ==
                RenderMode.ScreenSpaceOverlay
                ? null
                : rootCanvas.worldCamera;

        if (!RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                rect,
                screenPosition,
                canvasCamera,
                out Vector2 localPoint))
        {
            return false;
        }

        Rect bounds =
            rect.rect;

        float xFromLeft =
            localPoint.x -
            bounds.xMin -
            gridLayoutGroup.padding.left;

        float yFromTop =
            bounds.yMax -
            localPoint.y -
            gridLayoutGroup.padding.top;

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
            cellSize.x +
            spacing.x;

        float pitchY =
            cellSize.y +
            spacing.y;

        if (pitchX <= 0f ||
            pitchY <= 0f)
        {
            return false;
        }

        int x =
            Mathf.FloorToInt(
                xFromLeft /
                pitchX
            );

        int rowFromTop =
            Mathf.FloorToInt(
                yFromTop /
                pitchY
            );

        if (x < 0 ||
            rowFromTop < 0 ||
            x >= inventoryContainer.Width ||
            rowFromTop >=
                inventoryContainer.Height)
        {
            return false;
        }

        int y =
            inventoryContainer.Height -
            1 -
            rowFromTop;

        coordinate =
            new Vector2Int(
                x,
                y
            );

        return true;
    }

    private bool IsValidGridCoordinate(
        Vector2Int coordinate)
    {
        return inventoryContainer != null &&
               coordinate.x >= 0 &&
               coordinate.y >= 0 &&
               coordinate.x <
                   inventoryContainer.Width &&
               coordinate.y <
                   inventoryContainer.Height;
    }

    private void HandleInteractionChanged()
    {
        BuildHeldItemPreview();
        RefreshAllGrids();
    }

    private void UpdateHoveredCoordinateFromMouse()
    {
        if (Mouse.current == null ||
            !InventoryMenuController
                .IsInventoryOpen)
        {
            if (hoveredCoordinate.x >= 0 ||
                hoveredCoordinate.y >= 0)
            {
                hoveredCoordinate =
                    new Vector2Int(-1, -1);

                Refresh();
            }

            return;
        }

        Vector2Int newCoordinate;

        bool found =
            TryGetGridCoordinateFromScreenPoint(
                Mouse.current.position
                    .ReadValue(),
                out newCoordinate
            );

        if (!found)
        {
            newCoordinate =
                new Vector2Int(-1, -1);
        }

        if (newCoordinate ==
            hoveredCoordinate)
        {
            return;
        }

        hoveredCoordinate =
            newCoordinate;

        Refresh();
    }

    public static bool TryGetHoveredItem(
        out InventoryContainer container,
        out Vector2Int coordinate)
    {
        container = null;
        coordinate =
            new Vector2Int(-1, -1);

        if (!InventoryMenuController
            .IsInventoryOpen)
        {
            return false;
        }

        for (int i = activeGrids.Count - 1;
             i >= 0;
             i--)
        {
            InventoryGridUI grid =
                activeGrids[i];

            if (grid == null ||
                !grid.isActiveAndEnabled ||
                grid.inventoryContainer == null ||
                !grid.IsValidGridCoordinate(
                    grid.hoveredCoordinate))
            {
                continue;
            }

            PlacedInventoryItem item =
                grid.inventoryContainer.GetItemAt(
                    grid.hoveredCoordinate.x,
                    grid.hoveredCoordinate.y
                );

            if (item == null ||
                item.ItemInstance == null ||
                item.ItemInstance.IsEmpty)
            {
                continue;
            }

            container =
                grid.inventoryContainer;

            coordinate =
                grid.hoveredCoordinate;

            return true;
        }

        return false;
    }

    private void CreateHeldPreviewRoot()
    {
        if (rootCanvas == null ||
            heldPreviewRoot != null)
        {
            return;
        }

        GameObject previewObject =
            new GameObject(
                "InventorySelectedItemPreview",
                typeof(RectTransform),
                typeof(CanvasGroup),
                typeof(GridLayoutGroup)
            );

        previewObject.transform.SetParent(
            rootCanvas.transform,
            false
        );

        heldPreviewRoot =
            previewObject.GetComponent<
                RectTransform>();

        heldPreviewRoot.anchorMin =
            new Vector2(0.5f, 0.5f);

        heldPreviewRoot.anchorMax =
            new Vector2(0.5f, 0.5f);

        heldPreviewRoot.pivot =
            new Vector2(0f, 1f);

        heldPreviewCanvasGroup =
            previewObject.GetComponent<
                CanvasGroup>();

        heldPreviewCanvasGroup.blocksRaycasts =
            false;

        heldPreviewCanvasGroup.interactable =
            false;

        heldPreviewLayoutGroup =
            previewObject.GetComponent<
                GridLayoutGroup>();

        heldPreviewLayoutGroup.startCorner =
            GridLayoutGroup.Corner.UpperLeft;

        heldPreviewLayoutGroup.startAxis =
            GridLayoutGroup.Axis.Horizontal;

        heldPreviewLayoutGroup.childAlignment =
            TextAnchor.UpperLeft;

        heldPreviewLayoutGroup.constraint =
            GridLayoutGroup.Constraint
                .FixedColumnCount;

        if (gridLayoutGroup != null)
        {
            heldPreviewLayoutGroup.cellSize =
                gridLayoutGroup.cellSize;

            heldPreviewLayoutGroup.spacing =
                gridLayoutGroup.spacing;
        }

        previewObject.SetActive(false);
    }

    private void BuildHeldItemPreview()
    {
        if (heldPreviewRoot == null ||
            heldPreviewLayoutGroup == null ||
            cellPrefab == null)
        {
            return;
        }

        InventoryUIUtility.ClearChildren(
            heldPreviewRoot
        );

        if (!heldPreviewEnabled ||
            interactionController == null ||
            !interactionController.HasSelection ||
            interactionController
                .SelectedDefinition == null)
        {
            heldPreviewRoot.gameObject
                .SetActive(false);

            return;
        }

        ItemDefinition definition =
            interactionController
                .SelectedDefinition;

        int rotation =
            interactionController
                .SelectedRotationSteps;

        int width =
            definition.GetWidth(
                rotation
            );

        int height =
            definition.GetHeight(
                rotation
            );

        heldPreviewLayoutGroup.constraintCount =
            width;

        bool quantityAssigned =
            false;

        for (int y = height - 1;
             y >= 0;
             y--)
        {
            for (int x = 0;
                 x < width;
                 x++)
            {
                GameObject cellObject =
                    Instantiate(
                        cellPrefab,
                        heldPreviewRoot
                    );

                InventoryCellUI cellUI =
                    cellObject.GetComponent<
                        InventoryCellUI>();

                if (cellUI != null)
                    cellUI.enabled = false;

                Button cellButton =
                    cellObject.GetComponent<
                        Button>();

                if (cellButton != null)
                    cellButton.interactable = false;

                bool occupied =
                    definition.IsCellOccupied(
                        x,
                        y,
                        rotation
                    );

                Image image =
                    cellObject.GetComponent<Image>();

                if (image != null)
                {
                    image.raycastTarget = false;

                    image.color =
                        occupied
                            ? heldPreviewColor
                            : new Color(
                                0f,
                                0f,
                                0f,
                                0f
                            );
                }

                TextMeshProUGUI quantityText =
                    cellObject.GetComponentInChildren<
                        TextMeshProUGUI>(
                        true
                    );

                bool showQuantity =
                    occupied &&
                    !quantityAssigned &&
                    interactionController
                        .SelectedItem != null &&
                    interactionController
                        .SelectedItem
                        .IsStackable &&
                    interactionController
                        .SelectedItem
                        .Quantity > 1;

                if (quantityText != null)
                {
                    quantityText.text =
                        showQuantity
                            ? interactionController
                                .SelectedItem
                                .Quantity
                                .ToString()
                            : "";

                    quantityText.gameObject
                        .SetActive(
                            showQuantity
                        );
                }

                if (showQuantity)
                    quantityAssigned = true;
            }
        }

        UpdateHeldPreviewVisibility();
    }

    private void UpdateHeldPreviewVisibility()
    {
        if (heldPreviewRoot == null)
            return;

        bool shouldShow =
            heldPreviewEnabled &&
            interactionController != null &&
            interactionController.HasSelection &&
            InventoryMenuController
                .IsInventoryOpen;

        if (heldPreviewRoot.gameObject
            .activeSelf != shouldShow)
        {
            heldPreviewRoot.gameObject
                .SetActive(
                    shouldShow
                );
        }
    }

    private void UpdateHeldPreviewPosition()
    {
        if (heldPreviewRoot == null ||
            !heldPreviewRoot.gameObject.activeSelf ||
            heldPreviewLayoutGroup == null ||
            interactionController == null ||
            rootCanvas == null ||
            Mouse.current == null)
        {
            return;
        }

        RectTransform canvasRect =
            rootCanvas.transform
                as RectTransform;

        if (canvasRect == null)
            return;

        Camera canvasCamera =
            rootCanvas.renderMode ==
                RenderMode.ScreenSpaceOverlay
                ? null
                : rootCanvas.worldCamera;

        if (!RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                Mouse.current.position
                    .ReadValue(),
                canvasCamera,
                out Vector2 localPoint))
        {
            return;
        }

        heldPreviewRoot.anchoredPosition =
            localPoint -
            GetPreviewGrabPoint() +
            heldPreviewOffset;
    }

    private Vector2 GetPreviewGrabPoint()
    {
        ItemDefinition definition =
            interactionController
                .SelectedDefinition;

        if (definition == null)
            return Vector2.zero;

        int rotation =
            interactionController
                .SelectedRotationSteps;

        int height =
            definition.GetHeight(
                rotation
            );

        Vector2 cellSize =
            heldPreviewLayoutGroup.cellSize;

        Vector2 spacing =
            heldPreviewLayoutGroup.spacing;

        Vector2Int grabOffset =
            interactionController
                .SelectedGrabOffset;

        int visualRowFromTop =
            height -
            1 -
            grabOffset.y;

        float x =
            grabOffset.x *
            (cellSize.x + spacing.x) +
            cellSize.x * 0.5f;

        float y =
            -visualRowFromTop *
            (cellSize.y + spacing.y) -
            cellSize.y * 0.5f;

        return new Vector2(
            x,
            y
        );
    }

    private void BuildItemOutlines()
    {
        if (itemOutline == null ||
            inventoryContainer == null ||
            gridLayoutGroup == null)
        {
            return;
        }

        InventoryUIUtility.ClearChildren(
            itemOutline
        );

        HashSet<PlacedInventoryItem> outlined =
            new HashSet<PlacedInventoryItem>();

        for (int y = 0;
             y < inventoryContainer.Height;
             y++)
        {
            for (int x = 0;
                 x < inventoryContainer.Width;
                 x++)
            {
                PlacedInventoryItem item =
                    inventoryContainer.GetItemAt(
                        x,
                        y
                    );

                if (item == null ||
                    outlined.Contains(item))
                {
                    continue;
                }

                outlined.Add(item);

                DrawItemOutline(
                    item.ItemDefinition,
                    item.Position,
                    item.RotationSteps,
                    itemOutlineColor
                );
            }
        }

        if (isDraggingItem &&
            ReferenceEquals(
                inventoryContainer,
                dragSourceContainer) &&
            draggedItem != null)
        {
            DrawItemOutline(
                draggedItem.Definition,
                dragOriginalPosition,
                dragOriginalRotationSteps,
                dragOriginalOutlineColor
            );
        }

        if (interactionController != null &&
            interactionController.HasSelection &&
            interactionController
                .SelectedDefinition != null &&
            IsValidGridCoordinate(
                hoveredCoordinate))
        {
            Vector2Int origin =
                hoveredCoordinate -
                interactionController
                    .SelectedGrabOffset;

            DrawItemOutline(
                interactionController
                    .SelectedDefinition,
                origin,
                interactionController
                    .SelectedRotationSteps,
                itemOutlineColor
            );
        }
    }

    private void DrawItemOutline(
        ItemDefinition definition,
        Vector2Int origin,
        int rotationSteps,
        Color color)
    {
        if (definition == null)
            return;

        int width =
            definition.GetWidth(
                rotationSteps
            );

        int height =
            definition.GetHeight(
                rotationSteps
            );

        for (int y = 0;
             y < height;
             y++)
        {
            for (int x = 0;
                 x < width;
                 x++)
            {
                if (!definition.IsCellOccupied(
                    x,
                    y,
                    rotationSteps))
                {
                    continue;
                }

                bool topOpen =
                    !InventoryShapeUtility
                        .IsOccupiedInShape(
                            definition,
                            x,
                            y + 1,
                            rotationSteps
                        );

                bool bottomOpen =
                    !InventoryShapeUtility
                        .IsOccupiedInShape(
                            definition,
                            x,
                            y - 1,
                            rotationSteps
                        );

                bool leftOpen =
                    !InventoryShapeUtility
                        .IsOccupiedInShape(
                            definition,
                            x - 1,
                            y,
                            rotationSteps
                        );

                bool rightOpen =
                    !InventoryShapeUtility
                        .IsOccupiedInShape(
                            definition,
                            x + 1,
                            y,
                            rotationSteps
                        );

                int gridX =
                    origin.x + x;

                int gridY =
                    origin.y + y;

                if (topOpen)
                    DrawOutlineEdge(
                        gridX,
                        gridY,
                        InventoryOutlineSide.Top,
                        color
                    );

                if (bottomOpen)
                    DrawOutlineEdge(
                        gridX,
                        gridY,
                        InventoryOutlineSide.Bottom,
                        color
                    );

                if (leftOpen)
                    DrawOutlineEdge(
                        gridX,
                        gridY,
                        InventoryOutlineSide.Left,
                        color
                    );

                if (rightOpen)
                    DrawOutlineEdge(
                        gridX,
                        gridY,
                        InventoryOutlineSide.Right,
                        color
                    );

                if (topOpen && leftOpen)
                    DrawCorner(
                        gridX,
                        gridY,
                        InventoryOutlineCorner.TopLeft,
                        color
                    );

                if (topOpen && rightOpen)
                    DrawCorner(
                        gridX,
                        gridY,
                        InventoryOutlineCorner.TopRight,
                        color
                    );

                if (bottomOpen && leftOpen)
                    DrawCorner(
                        gridX,
                        gridY,
                        InventoryOutlineCorner.BottomLeft,
                        color
                    );

                if (bottomOpen && rightOpen)
                    DrawCorner(
                        gridX,
                        gridY,
                        InventoryOutlineCorner.BottomRight,
                        color
                    );
            }
        }
    }

    private void DrawOutlineEdge(
        int gridX,
        int gridY,
        InventoryOutlineSide side,
        Color color)
    {
        if (itemOutline == null ||
            gridLayoutGroup == null ||
            inventoryContainer == null)
        {
            return;
        }

        Vector2 cellSize =
            gridLayoutGroup.cellSize;

        Vector2 spacing =
            gridLayoutGroup.spacing;

        RectOffset padding =
            gridLayoutGroup.padding;

        float halfSpacingX =
            InventoryUIUtility
                .GetHalfSpacing(
                    spacing.x,
                    fillPaddingBetweenCells
                );

        float halfSpacingY =
            InventoryUIUtility
                .GetHalfSpacing(
                    spacing.y,
                    fillPaddingBetweenCells
                );

        int rowFromTop =
            inventoryContainer.Height -
            1 -
            gridY;

        float cellLeft =
            padding.left +
            gridX *
            (cellSize.x + spacing.x);

        float cellTop =
            -padding.top -
            rowFromTop *
            (cellSize.y + spacing.y);

        Vector2 position;
        Vector2 size;

        switch (side)
        {
            case InventoryOutlineSide.Top:
                position =
                    new Vector2(
                        cellLeft +
                            cellSize.x * 0.5f,
                        cellTop +
                            halfSpacingY
                    );

                size =
                    InventoryUIUtility
                        .GetHorizontalEdgeSize(
                            cellSize.x,
                            itemOutlineThickness
                        );
                break;

            case InventoryOutlineSide.Bottom:
                position =
                    new Vector2(
                        cellLeft +
                            cellSize.x * 0.5f,
                        cellTop -
                            cellSize.y -
                            halfSpacingY
                    );

                size =
                    InventoryUIUtility
                        .GetHorizontalEdgeSize(
                            cellSize.x,
                            itemOutlineThickness
                        );
                break;

            case InventoryOutlineSide.Left:
                position =
                    new Vector2(
                        cellLeft -
                            halfSpacingX,
                        cellTop -
                            cellSize.y * 0.5f
                    );

                size =
                    InventoryUIUtility
                        .GetVerticalEdgeSize(
                            itemOutlineThickness,
                            cellSize.y
                        );
                break;

            default:
                position =
                    new Vector2(
                        cellLeft +
                            cellSize.x +
                            halfSpacingX,
                        cellTop -
                            cellSize.y * 0.5f
                    );

                size =
                    InventoryUIUtility
                        .GetVerticalEdgeSize(
                            itemOutlineThickness,
                            cellSize.y
                        );
                break;
        }

        InventoryUIUtility.CreateImageRect(
            itemOutline,
            "ItemOutlineEdge",
            position,
            size,
            color
        );
    }

    private void DrawCorner(
        int gridX,
        int gridY,
        InventoryOutlineCorner corner,
        Color color)
    {
        if (itemOutline == null ||
            gridLayoutGroup == null ||
            inventoryContainer == null)
        {
            return;
        }

        Vector2 cellSize =
            gridLayoutGroup.cellSize;

        Vector2 spacing =
            gridLayoutGroup.spacing;

        RectOffset padding =
            gridLayoutGroup.padding;

        float halfSpacingX =
            InventoryUIUtility
                .GetHalfSpacing(
                    spacing.x,
                    fillPaddingBetweenCells
                );

        float halfSpacingY =
            InventoryUIUtility
                .GetHalfSpacing(
                    spacing.y,
                    fillPaddingBetweenCells
                );

        int rowFromTop =
            inventoryContainer.Height -
            1 -
            gridY;

        float cellLeft =
            padding.left +
            gridX *
            (cellSize.x + spacing.x);

        float cellTop =
            -padding.top -
            rowFromTop *
            (cellSize.y + spacing.y);

        Vector2 position;

        switch (corner)
        {
            case InventoryOutlineCorner.TopLeft:
                position =
                    new Vector2(
                        cellLeft -
                            halfSpacingX,
                        cellTop +
                            halfSpacingY
                    );
                break;

            case InventoryOutlineCorner.TopRight:
                position =
                    new Vector2(
                        cellLeft +
                            cellSize.x +
                            halfSpacingX,
                        cellTop +
                            halfSpacingY
                    );
                break;

            case InventoryOutlineCorner.BottomLeft:
                position =
                    new Vector2(
                        cellLeft -
                            halfSpacingX,
                        cellTop -
                            cellSize.y -
                            halfSpacingY
                    );
                break;

            default:
                position =
                    new Vector2(
                        cellLeft +
                            cellSize.x +
                            halfSpacingX,
                        cellTop -
                            cellSize.y -
                            halfSpacingY
                    );
                break;
        }

        InventoryUIUtility.CreateImageRect(
            itemOutline,
            "ItemOutlineCorner",
            position,
            InventoryUIUtility
                .GetCornerSize(
                    itemOutlineThickness
                ),
            color
        );
    }

    private static void RefreshAllGrids()
    {
        for (int i = 0;
             i < activeGrids.Count;
             i++)
        {
            InventoryGridUI grid =
                activeGrids[i];

            if (grid != null)
                grid.Refresh();
        }
    }
}