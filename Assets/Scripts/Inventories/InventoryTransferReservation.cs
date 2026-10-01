using System.Collections.Generic;
using UnityEngine;

public sealed class InventoryTransferReservation
{
    private readonly List<
        InventoryStackTransferReservation>
        stackTransfers =
            new List<
                InventoryStackTransferReservation>();

    public InventoryContainer Owner
    {
        get;
    }

    public InventoryItemInstance SourceItem
    {
        get;
    }

    public IReadOnlyList<
        InventoryStackTransferReservation>
        StackTransfers =>
            stackTransfers;

    public bool HasPlacement
    {
        get;
        private set;
    }

    public Vector2Int PlacementPosition
    {
        get;
        private set;
    }

    public int PlacementRotationSteps
    {
        get;
        private set;
    }

    public bool IsActive
    {
        get;
        internal set;
    }

    public int ReservedQuantity
    {
        get;
        private set;
    }

    internal InventoryTransferReservation(
        InventoryContainer owner,
        InventoryItemInstance sourceItem)
    {
        Owner = owner;
        SourceItem = sourceItem;
        IsActive = true;
    }

    internal void AddStackTransfer(
        InventoryItemInstance target,
        int quantity)
    {
        if (target == null ||
            quantity <= 0)
        {
            return;
        }

        stackTransfers.Add(
            new InventoryStackTransferReservation(
                target,
                quantity
            )
        );

        ReservedQuantity +=
            quantity;
    }

    internal void ReservePlacement(
        Vector2Int position,
        int rotationSteps,
        int quantity)
    {
        if (quantity <= 0)
            return;

        PlacementPosition =
            position;

        PlacementRotationSteps =
            ItemDefinition
                .NormalizeRotationSteps(
                    rotationSteps
                );

        HasPlacement = true;

        ReservedQuantity +=
            quantity;
    }
}

public readonly struct
    InventoryStackTransferReservation
{
    public InventoryItemInstance Target
    {
        get;
    }

    public int Quantity
    {
        get;
    }

    internal InventoryStackTransferReservation(
        InventoryItemInstance target,
        int quantity)
    {
        Target = target;
        Quantity = quantity;
    }
}