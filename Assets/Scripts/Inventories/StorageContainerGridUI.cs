using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class StorageContainerGridUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StorageContainer storageContainer;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private Transform cellParent;
    [SerializeField] private GameObject cellPrefab;

    [Header("Colors")]
    [SerializeField] private Color emptyColor = new Color(0f, 0f, 0f, 0.35f);
    [SerializeField] private Color occupiedColor = new Color(1f, 1f, 1f, 0.85f);

    private GridLayoutGroup gridLayoutGroup;

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

    private void PickUpContainerItem(
        Vector2Int coordinate)
    {
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

    private void TryPlaceOrMergeHeldItemIntoContainer(
        Vector2Int coordinate)
    {
        bool merged =
            TryMergeHeldItemIntoContainerStackAt(
                coordinate.x,
                coordinate.y
            );

        if (merged)
        {
            Refresh();
            return;
        }

        PlacedInventoryItem heldItem =
            playerInventory.HeldItem;

        if (heldItem == null ||
            heldItem.ItemData == null)
        {
            return;
        }

        bool placed =
            storageContainer.PlaceItem(
                heldItem.ItemData,
                coordinate.x,
                coordinate.y,
                heldItem.RotationSteps,
                heldItem.Quantity
            );

        if (!placed)
            return;

        playerInventory.ClearHeldItemAfterExternalMove();
        Refresh();
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

    private void OnCellPointerEntered(Vector2Int coordinate)
    {
    }

    private void OnCellPointerExited(Vector2Int coordinate)
    {
    }

    private void OnCellPointerDown(Vector2Int coordinate)
    {
    }

    private void OnCellPointerUp(Vector2Int coordinate)
    {
    }
}