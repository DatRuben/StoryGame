using System.Collections.Generic;
using UnityEngine;

public class InventoryGrid
{
    private readonly int width;
    private readonly int height;

    private readonly PlacedInventoryItem[,] cells;

    public int Width => width;
    public int Height => height;

    public InventoryGrid(int width, int height)
    {
        this.width = width;
        this.height = height;

        cells = new PlacedInventoryItem[width, height];
    }

    public ItemData GetCell(int x, int y)
    {
        PlacedInventoryItem placedItem =
            GetPlacedItem(x, y);

        if (placedItem == null)
            return null;

        return placedItem.ItemData;
    }

    public PlacedInventoryItem GetPlacedItem(int x, int y)
    {
        if (!IsInsideGrid(x, y))
            return null;

        return cells[x, y];
    }

    public bool CanPlaceItem(
        ItemData item,
        int startX,
        int startY,
        int rotationSteps)
    {
        if (item == null)
            return false;

        rotationSteps =
            ItemData.NormalizeRotationSteps(rotationSteps);

        int itemWidth =
            item.GetWidth(rotationSteps);

        int itemHeight =
            item.GetHeight(rotationSteps);

        for (int y = 0; y < itemHeight; y++)
        {
            for (int x = 0; x < itemWidth; x++)
            {
                if (!item.IsCellOccupied(x, y, rotationSteps))
                    continue;

                int gridX = startX + x;
                int gridY = startY + y;

                if (!IsInsideGrid(gridX, gridY))
                    return false;

                if (cells[gridX, gridY] != null)
                    return false;
            }
        }

        return true;
    }

    public bool PlaceItem(
        ItemData item,
        int startX,
        int startY,
        int rotationSteps,
        int quantity = 1)
    {
        if (!CanPlaceItem(item, startX, startY, rotationSteps))
            return false;

        rotationSteps =
            ItemData.NormalizeRotationSteps(rotationSteps);

        PlacedInventoryItem placedItem =
            new PlacedInventoryItem(
                item,
                new Vector2Int(startX, startY),
                rotationSteps,
                quantity
            );

        int itemWidth =
            item.GetWidth(rotationSteps);

        int itemHeight =
            item.GetHeight(rotationSteps);

        for (int y = 0; y < itemHeight; y++)
        {
            for (int x = 0; x < itemWidth; x++)
            {
                if (!item.IsCellOccupied(x, y, rotationSteps))
                    continue;

                int gridX = startX + x;
                int gridY = startY + y;

                cells[gridX, gridY] = placedItem;
            }
        }

        return true;
    }

    public bool TryAddItemTopLeft(
    ItemData item,
    int startingRotationSteps,
    int quantity,
    out int remainingQuantity)
    {
        remainingQuantity =
            Mathf.Max(0, quantity);

        if (item == null ||
            remainingQuantity <= 0)
        {
            return false;
        }

        if (item.isStackable)
        {
            remainingQuantity =
                AddToExistingStacksTopLeft(
                    item,
                    remainingQuantity
                );
        }

        while (remainingQuantity > 0)
        {
            int amountToPlace =
                item.isStackable
                    ? Mathf.Min(
                        remainingQuantity,
                        Mathf.Max(1, item.maxStackSize)
                    )
                    : 1;

            bool foundSpace =
                TryFindFirstAvailableSpaceTopLeft(
                    item,
                    startingRotationSteps,
                    out Vector2Int position,
                    out int rotationSteps
                );

            if (!foundSpace)
                break;

            bool placed =
                PlaceItem(
                    item,
                    position.x,
                    position.y,
                    rotationSteps,
                    amountToPlace
                );

            if (!placed)
                break;

            remainingQuantity -= amountToPlace;
        }

        return remainingQuantity == 0;
    }

    public bool TryFindFirstAvailableSpaceTopLeft(
        ItemData item,
        int startingRotationSteps,
        out Vector2Int position,
        out int rotationSteps)
    {
        position = new Vector2Int(-1, -1);
        rotationSteps = 0;

        if (item == null)
            return false;

        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                for (int rotationOffset = 0; rotationOffset < 4; rotationOffset++)
                {
                    int testRotationSteps =
                        ItemData.NormalizeRotationSteps(
                            startingRotationSteps + rotationOffset
                        );

                    if (!CanPlaceItem(item, x, y, testRotationSteps))
                        continue;

                    position =
                        new Vector2Int(x, y);

                    rotationSteps =
                        testRotationSteps;

                    return true;
                }
            }
        }

        return false;
    }

    private int AddToExistingStacksTopLeft(
        ItemData item,
        int quantity)
    {
        if (item == null ||
            !item.isStackable ||
            quantity <= 0)
        {
            return quantity;
        }

        HashSet<PlacedInventoryItem> checkedStacks =
            new HashSet<PlacedInventoryItem>();

        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                PlacedInventoryItem placedItem =
                    cells[x, y];

                if (placedItem == null)
                    continue;

                if (checkedStacks.Contains(placedItem))
                    continue;

                checkedStacks.Add(placedItem);

                if (placedItem.ItemData != item)
                    continue;

                if (!placedItem.HasRoomInStack)
                    continue;

                int added =
                    placedItem.AddQuantity(quantity);

                quantity -= added;

                if (quantity <= 0)
                    return 0;
            }
        }

        return quantity;
    }

    public bool RemoveItem(PlacedInventoryItem placedItem)
    {
        if (placedItem == null)
            return false;

        bool removedAnyCell = false;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (cells[x, y] == placedItem)
                {
                    cells[x, y] = null;
                    removedAnyCell = true;
                }
            }
        }

        return removedAnyCell;
    }

    public bool RemoveItemAt(int x, int y)
    {
        PlacedInventoryItem placedItem =
            GetPlacedItem(x, y);

        if (placedItem == null)
            return false;

        return RemoveItem(placedItem);
    }

    public PlacedInventoryItem PickUpItemAt(int x, int y)
    {
        PlacedInventoryItem placedItem =
            GetPlacedItem(x, y);

        if (placedItem == null)
            return null;

        RemoveItem(placedItem);

        return placedItem;
    }

    private bool IsInsideGrid(int x, int y)
    {
        return x >= 0 &&
               y >= 0 &&
               x < width &&
               y < height;
    }

    public void DebugPrintGrid()
    {
        string output = "";

        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                output += cells[x, y] == null ? ". " : "X ";
            }

            output += "\n";
        }

        Debug.Log(output);
    }
}