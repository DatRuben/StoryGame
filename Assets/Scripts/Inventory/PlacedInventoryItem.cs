using UnityEngine;

public class PlacedInventoryItem
{
    public ItemData ItemData { get; private set; }
    public Vector2Int Position { get; private set; }

    // 0 = 0°
    // 1 = 90°
    // 2 = 180°
    // 3 = 270°
    public int RotationSteps { get; private set; }

    public PlacedInventoryItem(
        ItemData itemData,
        Vector2Int position,
        int rotationSteps)
    {
        ItemData = itemData;
        Position = position;

        RotationSteps =
            NormalizeRotationSteps(rotationSteps);
    }

    public int Width
    {
        get
        {
            return ItemData.GetWidth(RotationSteps);
        }
    }

    public int Height
    {
        get
        {
            return ItemData.GetHeight(RotationSteps);
        }
    }

    public void RotateCounterClockwise()
    {
        RotationSteps =
            NormalizeRotationSteps(RotationSteps - 1);
    }

    public void SetRotationSteps(int rotationSteps)
    {
        RotationSteps =
            NormalizeRotationSteps(rotationSteps);
    }

    private int NormalizeRotationSteps(int rotationSteps)
    {
        rotationSteps %= 4;

        if (rotationSteps < 0)
            rotationSteps += 4;

        return rotationSteps;
    }
}