using UnityEngine;

public enum ItemHandlingOperationType
{
    None,
    Pickup,
    Store,
    Retrieve,
    Transfer,
    Drop,
    Equip,
    Unequip
}

public sealed class ItemHandlingOperation
{
    public ItemHandlingOperationType Type
    {
        get;
    }

    public InventoryItemInstance Item
    {
        get;
    }

    public InventoryContainer SourceContainer
    {
        get;
    }

    public InventoryContainer TargetContainer
    {
        get;
    }

    public InventoryTransferReservation
        TransferReservation
    {
        get;
    }

    public float Duration
    {
        get;
    }

    public float Elapsed
    {
        get;
        private set;
    }

    public float Progress01
    {
        get
        {
            if (Duration <= 0f)
                return 1f;

            return Mathf.Clamp01(
                Elapsed / Duration
            );
        }
    }

    public bool IsComplete =>
        Elapsed >= Duration;

    internal ItemHandlingOperation(
        ItemHandlingOperationType type,
        InventoryItemInstance item,
        float duration,
        InventoryContainer sourceContainer = null,
        InventoryContainer targetContainer = null,
        InventoryTransferReservation
            transferReservation = null)
    {
        Type = type;
        Item = item;

        Duration =
            Mathf.Max(
                0.05f,
                duration
            );

        SourceContainer =
            sourceContainer;

        TargetContainer =
            targetContainer;

        TransferReservation =
            transferReservation;
    }

    internal void Advance(
        float deltaTime)
    {
        if (deltaTime <= 0f)
            return;

        Elapsed =
            Mathf.Min(
                Duration,
                Elapsed + deltaTime
            );
    }
}