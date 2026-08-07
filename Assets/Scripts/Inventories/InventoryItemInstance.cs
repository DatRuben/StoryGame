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
        quantity =
            Mathf.Clamp(
                newQuantity,
                0,
                MaxStackSize
            );

        if (!IsStackable &&
            quantity > 0)
        {
            quantity = 1;
        }
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

        quantity += added;

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

        quantity -= removed;

        return removed;
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