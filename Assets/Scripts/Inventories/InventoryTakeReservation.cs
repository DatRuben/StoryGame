using UnityEngine;

public sealed class InventoryTakeReservation
{
    internal InventoryContainer Owner
    {
        get;
    }

    public InventoryItemInstance Item
    {
        get;
    }

    public Vector2Int Position
    {
        get;
    }

    public int RotationSteps
    {
        get;
    }

    public bool IsActive
    {
        get;
        internal set;
    }

    internal InventoryTakeReservation(
        InventoryContainer owner,
        InventoryItemInstance item,
        Vector2Int position,
        int rotationSteps)
    {
        Owner = owner;
        Item = item;
        Position = position;

        RotationSteps =
            ItemDefinition
                .NormalizeRotationSteps(
                    rotationSteps
                );

        IsActive = true;
    }
}