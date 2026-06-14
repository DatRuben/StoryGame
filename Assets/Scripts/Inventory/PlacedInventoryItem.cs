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

    public int Quantity { get; private set; }

    public bool IsStackable
    {
        get
        {
            return ItemData != null &&
                   ItemData.isStackable;
        }
    }

    public int MaxStackSize
    {
        get
        {
            if (ItemData == null)
                return 1;

            return Mathf.Max(1, ItemData.maxStackSize);
        }
    }

    public bool HasRoomInStack
    {
        get
        {
            return IsStackable &&
                   Quantity < MaxStackSize;
        }
    }

    public PlacedInventoryItem(
        ItemData itemData,
        Vector2Int position,
        int rotationSteps,
        int quantity = 1)
    {
        ItemData = itemData;
        Position = position;

        RotationSteps =
            NormalizeRotationSteps(rotationSteps);

        SetQuantity(quantity);
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

    public void SetQuantity(int quantity)
    {
        int max =
            MaxStackSize;

        Quantity =
            Mathf.Clamp(
                quantity,
                1,
                max
            );
    }

    public int AddQuantity(int amount)
    {
        if (amount <= 0)
            return 0;

        if (!IsStackable)
            return 0;

        int room =
            MaxStackSize - Quantity;

        int added =
            Mathf.Min(room, amount);

        Quantity += added;

        return added;
    }

    public int RemoveQuantity(int amount)
    {
        if (amount <= 0)
            return 0;

        int removed =
            Mathf.Min(Quantity, amount);

        Quantity -= removed;

        return removed;
    }

    private int NormalizeRotationSteps(int rotationSteps)
    {
        rotationSteps %= 4;

        if (rotationSteps < 0)
            rotationSteps += 4;

        return rotationSteps;
    }
}