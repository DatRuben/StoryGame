using UnityEngine;

public class InventoryGrid
{
    private readonly int width;
    private readonly int height;

    private readonly ItemData[,] cells;

    public InventoryGrid(int width, int height)
    {
        this.width = width;
        this.height = height;

        cells = new ItemData[width, height];
    }

    public bool CanPlaceItem(
        ItemData item,
        int startX,
        int startY,
        bool rotated)
    {
        if (item == null)
            return false;

        int itemWidth = item.GetWidth(rotated);
        int itemHeight = item.GetHeight(rotated);

        for (int y = 0; y < itemHeight; y++)
        {
            for (int x = 0; x < itemWidth; x++)
            {
                if (!item.IsCellOccupied(x, y, rotated))
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
        bool rotated)
    {
        if (!CanPlaceItem(item, startX, startY, rotated))
            return false;

        int itemWidth = item.GetWidth(rotated);
        int itemHeight = item.GetHeight(rotated);

        for (int y = 0; y < itemHeight; y++)
        {
            for (int x = 0; x < itemWidth; x++)
            {
                if (!item.IsCellOccupied(x, y, rotated))
                    continue;

                int gridX = startX + x;
                int gridY = startY + y;

                cells[gridX, gridY] = item;
            }
        }

        return true;
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