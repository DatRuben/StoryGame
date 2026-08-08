using System;
using UnityEngine;

[Serializable]
public class InventoryItemInstance
{
    [SerializeField]
    private string instanceId;

    [SerializeField]
    private ItemDefinition definition;

    [SerializeField]
    [Min(0)]
    private int quantity = 1;

    public string InstanceId
    {
        get
        {
            EnsureId();
            return instanceId;
        }
    }

    public event Action Changed;

    public ItemDefinition Definition =>
        definition;

    public int Quantity =>
        quantity;

    public bool IsEmpty =>
        quantity <= 0;

    public bool IsStackable =>
        definition != null &&
        definition.isStackable;

    public int MaxStackSize
    {
        get
        {
            if (definition == null)
                return 1;

            return Mathf.Max(
                1,
                definition.maxStackSize
            );
        }
    }

    public bool HasRoomInStack =>
        IsStackable &&
        quantity < MaxStackSize;

    public InventoryItemInstance(
        ItemDefinition definition,
        int quantity = 1)
    {
        instanceId = CreateId();
        this.definition = definition;

        SetQuantity(
            Mathf.Max(
                1,
                quantity
            )
        );
    }

    public void EnsureValid()
    {
        EnsureId();
        SetQuantity(quantity);
    }

    public void SetQuantity(
        int newQuantity)
    {
        int normalizedQuantity =
            Mathf.Clamp(
                newQuantity,
                0,
                MaxStackSize
            );

        if (!IsStackable &&
            normalizedQuantity > 0)
        {
            normalizedQuantity = 1;
        }

        if (quantity ==
            normalizedQuantity)
        {
            return;
        }

        quantity =
            normalizedQuantity;

        Changed?.Invoke();
    }

    public int AddQuantity(
        int amount)
    {
        if (amount <= 0 ||
            !IsStackable)
        {
            return 0;
        }

        int availableSpace =
            MaxStackSize - quantity;

        int added =
            Mathf.Min(
                availableSpace,
                amount
            );

        if (added <= 0)
            return 0;

        quantity += added;

        Changed?.Invoke();

        return added;
    }

    public int RemoveQuantity(
        int amount)
    {
        if (amount <= 0)
            return 0;

        int removed =
            Mathf.Min(
                quantity,
                amount
            );

        if (removed <= 0)
            return 0;

        quantity -= removed;

        Changed?.Invoke();

        return removed;
    }

    public bool CanStackWith(
    InventoryItemInstance other)
    {
        if (other == null ||
            ReferenceEquals(this, other))
        {
            return false;
        }

        return IsStackable &&
               other.IsStackable &&
               Definition != null &&
               Definition == other.Definition;
    }

    public int MoveQuantityTo(
        InventoryItemInstance target,
        int amount)
    {
        if (amount <= 0 ||
            IsEmpty ||
            !CanStackWith(target))
        {
            return 0;
        }

        int amountToMove =
            Mathf.Min(
                Quantity,
                amount
            );

        int moved =
            target.AddQuantity(
                amountToMove
            );

        if (moved > 0)
            RemoveQuantity(moved);

        return moved;
    }

    public bool TrySplit(
        int amount,
        out InventoryItemInstance splitInstance)
    {
        splitInstance = null;

        if (!IsStackable ||
            amount <= 0 ||
            amount >= Quantity)
        {
            return false;
        }

        RemoveQuantity(amount);

        splitInstance =
            new InventoryItemInstance(
                Definition,
                amount
            );

        return true;
    }

    private void EnsureId()
    {
        if (!string.IsNullOrWhiteSpace(
            instanceId))
        {
            return;
        }

        instanceId = CreateId();
    }

    private static string CreateId()
    {
        return Guid.NewGuid()
            .ToString("N");
    }
}