using System.Collections.Generic;
using UnityEngine;

public class PlacedInventoryItem
{
    public ItemDefinition ItemDefinition { get; private set; }
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
            return ItemDefinition != null &&
                   ItemDefinition.isStackable;
        }
    }

    public int MaxStackSize
    {
        get
        {
            if (ItemDefinition == null)
                return 1;

            return Mathf.Max(1, ItemDefinition.maxStackSize);
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
        ItemDefinition itemDefinition,
        Vector2Int position,
        int rotationSteps,
        int quantity = 1)
    {
        ItemDefinition = itemDefinition;
        Position = position;

        RotationSteps =
            NormalizeRotationSteps(rotationSteps);

        SetQuantity(quantity);
    }

    public int Width
    {
        get
        {
            if (ItemDefinition == null)
                return 1;

            return ItemDefinition.GetWidth(RotationSteps);
        }
    }

    public int Height
    {
        get
        {
            if (ItemDefinition == null)
                return 1;

            return ItemDefinition.GetHeight(RotationSteps);
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

    public List<Vector2Int> GetOccupiedCellsAt(
        Vector2Int origin)
    {
        List<Vector2Int> occupiedCells =
            new List<Vector2Int>();

        if (ItemDefinition == null)
            return occupiedCells;

        IReadOnlyList<Vector2Int> rotatedCells =
            ItemDefinition.GetOccupiedCells(
                RotationSteps
            );

        for (int i = 0; i < rotatedCells.Count; i++)
        {
            occupiedCells.Add(
                origin + rotatedCells[i]
            );
        }

        return occupiedCells;
    }

    public bool OccupiesCellAt(
        Vector2Int cellCoordinate,
        Vector2Int origin)
    {
        if (ItemDefinition == null)
            return false;

        IReadOnlyList<Vector2Int> rotatedCells =
            ItemDefinition.GetOccupiedCells(
                RotationSteps
            );

        for (int i = 0; i < rotatedCells.Count; i++)
        {
            if (origin + rotatedCells[i] == cellCoordinate)
                return true;
        }

        return false;
    }
}