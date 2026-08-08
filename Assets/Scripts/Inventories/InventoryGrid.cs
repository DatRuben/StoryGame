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

    public ItemDefinition GetCell(int x, int y)
    {
        PlacedInventoryItem placedItem =
            GetPlacedItem(x, y);

        if (placedItem == null)
            return null;

        return placedItem.ItemDefinition;
    }

    public PlacedInventoryItem GetPlacedItem(int x, int y)
    {
        if (!IsInsideGrid(x, y))
            return null;

        return cells[x, y];
    }

    public bool CanPlaceItem(
        ItemDefinition item,
        int startX,
        int startY,
        int rotationSteps)
    {
        if (item == null)
            return false;

        rotationSteps =
            ItemDefinition.NormalizeRotationSteps(rotationSteps);

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
        InventoryItemInstance itemInstance,
        int startX,
        int startY,
        int rotationSteps)
    {
        if (itemInstance == null)
            return false;

        itemInstance.EnsureValid();

        ItemDefinition item =
            itemInstance.Definition;

        if (item == null ||
            itemInstance.IsEmpty)
        {
            return false;
        }

        if (!CanPlaceItem(
            item,
            startX,
            startY,
            rotationSteps))
        {
            return false;
        }

        rotationSteps =
            ItemDefinition.NormalizeRotationSteps(
                rotationSteps
            );

        PlacedInventoryItem placedItem =
            new PlacedInventoryItem(
                itemInstance,
                new Vector2Int(
                    startX,
                    startY
                ),
                rotationSteps
            );

        int itemWidth =
            item.GetWidth(rotationSteps);

        int itemHeight =
            item.GetHeight(rotationSteps);

        for (int y = 0;
             y < itemHeight;
             y++)
        {
            for (int x = 0;
                 x < itemWidth;
                 x++)
            {
                if (!item.IsCellOccupied(
                    x,
                    y,
                    rotationSteps))
                {
                    continue;
                }

                int gridX =
                    startX + x;

                int gridY =
                    startY + y;

                cells[gridX, gridY] =
                    placedItem;
            }
        }

        return true;
    }

    public bool TryFindFirstAvailableSpaceTopLeft(
        ItemDefinition item,
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
                        ItemDefinition.NormalizeRotationSteps(
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