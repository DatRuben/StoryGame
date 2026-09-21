using System.Collections.Generic;
using UnityEngine;

public sealed class InventoryTransferReservation
{
    private readonly List<
        InventoryStackTransferReservation>
        stackTransfers =
            new List<
                InventoryStackTransferReservation>();

    internal InventoryContainer Owner
    {
        get;
    }

    internal InventoryItemInstance SourceItem
    {
        get;
    }

    internal IReadOnlyList<
        InventoryStackTransferReservation>
        StackTransfers =>
            stackTransfers;

    internal bool HasPlacement
    {
        get;
        private set;
    }

    internal Vector2Int PlacementPosition
    {
        get;
        private set;
    }

    internal int PlacementRotationSteps
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

internal readonly struct
    InventoryStackTransferReservation
{
    internal InventoryItemInstance Target
    {
        get;
    }

    internal int Quantity
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