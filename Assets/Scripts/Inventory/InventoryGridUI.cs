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

    [Header("Colors")]
    [SerializeField] private Color emptyColor = new Color(0f, 0f, 0f, 0.35f);
    [SerializeField] private Color occupiedColor = new Color(1f, 1f, 1f, 0.85f);
    [SerializeField] private Color validPlacementColor = new Color(0.2f, 1f, 0.2f, 0.85f);
    [SerializeField] private Color invalidPlacementColor = new Color(1f, 0.2f, 0.2f, 0.85f);
    [SerializeField] private Color heldPreviewColor = new Color(1f, 1f, 1f, 0.65f);

    [Header("Held Item Mouse Preview")]
    [SerializeField] private Vector2 heldPreviewOffset = Vector2.zero;

    private GridLayoutGroup gridLayoutGroup;
    private Canvas rootCanvas;

    private RectTransform heldPreviewRoot;
    private GridLayoutGroup heldPreviewLayoutGroup;
    private CanvasGroup heldPreviewCanvasGroup;

    private readonly List<InventoryCellUI> cells = new List<InventoryCellUI>();
    private readonly List<Vector2Int> cellCoordinates = new List<Vector2Int>();

    private Vector2Int hoveredCoordinate = new Vector2Int(-1, -1);

    // This is the important new part.
    // Example:
    // If you click the top-right cell of a 2x2 item,
    // heldGrabOffset becomes (1, 1).
    private Vector2Int heldGrabOffset = Vector2Int.zero;

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
        gridLayoutGroup = GetComponent<GridLayoutGroup>();
        rootCanvas = GetComponentInParent<Canvas>();
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
                    OnCellPointerExited
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
                IsCoordinateInsideHeldItemPreview(
                    coordinate,
                    placementOrigin
                ))
            {
                cell.SetColor(
                    canPlaceHeldItem
                    ? validPlacementColor
                    : invalidPlacementColor
                );

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
    }

    private void OnCellClicked(Vector2Int coordinate)
    {
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

    private bool IsCoordinateInsideHeldItemPreview(
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
            heldItem.Rotated
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
                            heldItem.Rotated
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
}