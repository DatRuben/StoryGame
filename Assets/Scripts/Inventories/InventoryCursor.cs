using System;
using UnityEngine;

public sealed class InventoryCursor
{
    private InventoryItemInstance itemInstance;

    private int rotationSteps;

    private Vector2Int grabOffset;

    public event Action Changed;

    public bool HasItem =>
        itemInstance != null &&
        !itemInstance.IsEmpty;

    public ItemDefinition ItemDefinition =>
        itemInstance != null
            ? itemInstance.Definition
            : null;

    public int Quantity =>
        itemInstance != null
            ? itemInstance.Quantity
            : 0;

    public int RotationSteps =>
        rotationSteps;

    public Vector2Int GrabOffset =>
        grabOffset;

    internal InventoryItemInstance ItemInstance =>
        itemInstance;

    internal bool Hold(
        InventoryItemInstance newItemInstance,
        int newRotationSteps,
        Vector2Int newGrabOffset)
    {
        if (HasItem ||
            newItemInstance == null ||
            newItemInstance.IsEmpty)
        {
            return false;
        }

        itemInstance =
            newItemInstance;

        rotationSteps =
            global::ItemDefinition.NormalizeRotationSteps(
                newRotationSteps
            );

        grabOffset =
            newGrabOffset;

        ClampGrabOffset();

        SubscribeItem();

        Changed?.Invoke();

        return true;
    }

    internal void RotateCounterClockwise()
    {
        if (!HasItem)
            return;

        rotationSteps =
            global::ItemDefinition.NormalizeRotationSteps(
                rotationSteps - 1
            );

        ClampGrabOffset();

        Changed?.Invoke();
    }

    internal void Clear()
    {
        if (itemInstance == null)
            return;

        UnsubscribeItem();

        itemInstance = null;
        rotationSteps = 0;
        grabOffset = Vector2Int.zero;

        Changed?.Invoke();
    }

    private void SubscribeItem()
    {
        if (itemInstance == null)
            return;

        itemInstance.Changed -=
            OnItemChanged;

        itemInstance.Changed +=
            OnItemChanged;
    }

    private void UnsubscribeItem()
    {
        if (itemInstance == null)
            return;

        itemInstance.Changed -=
            OnItemChanged;
    }

    private void OnItemChanged()
    {
        if (itemInstance == null)
            return;

        if (itemInstance.IsEmpty)
        {
            Clear();
            return;
        }

        Changed?.Invoke();
    }

    private void ClampGrabOffset()
    {
        if (ItemDefinition == null)
        {
            grabOffset =
                Vector2Int.zero;

            return;
        }

        int width =
            ItemDefinition.GetWidth(
                rotationSteps
            );

        int height =
            ItemDefinition.GetHeight(
                rotationSteps
            );

        grabOffset.x =
            Mathf.Clamp(
                grabOffset.x,
                0,
                Mathf.Max(
                    0,
                    width - 1
                )
            );

        grabOffset.y =
            Mathf.Clamp(
                grabOffset.y,
                0,
                Mathf.Max(
                    0,
                    height - 1
                )
            );
    }
}