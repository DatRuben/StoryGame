using UnityEngine;

public class PlacedInventoryItem
{
    public ItemData ItemData { get; private set; }
    public Vector2Int Position { get; private set; }
    public bool Rotated { get; private set; }

    public PlacedInventoryItem(
        ItemData itemData,
        Vector2Int position,
        bool rotated)
    {
        ItemData = itemData;
        Position = position;
        Rotated = rotated;
    }

    public int Width
    {
        get
        {
            return ItemData.GetWidth(Rotated);
        }
    }

    public int Height
    {
        get
        {
            return ItemData.GetHeight(Rotated);
        }
    }
}