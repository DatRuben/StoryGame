using System;
using UnityEngine;

public sealed class InventoryCursor
{
    private InventoryItemInstance itemInstance;

    private int rotationSteps;

    private InventoryContainer sourceContainer;

    private Vector2Int sourcePosition;

    private int sourceRotationSteps;

    private bool hasOriginalPlacement;

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

    internal InventoryContainer SourceContainer =>
        sourceContainer;

    internal Vector2Int SourcePosition =>
        sourcePosition;

    internal int SourceRotationSteps =>
        sourceRotationSteps;

    internal bool HasOriginalPlacement =>
        hasOriginalPlacement;

    internal bool HoldPlacedItem(
        InventoryContainer source,
        PlacedInventoryItem placedItem,
        Vector2Int itemGrabOffset)
    {
        if (HasItem ||
            source == null ||
            placedItem == null ||
            placedItem.ItemInstance == null)
        {
            return false;
        }

        sourceContainer =
            source;

        sourcePosition =
            placedItem.Position;

        sourceRotationSteps =
            placedItem.RotationSteps;

        hasOriginalPlacement =
            true;

        grabOffset =
            itemGrabOffset;

        SetItem(
            placedItem.ItemInstance,
            placedItem.RotationSteps
        );

        return true;
    }

    internal bool HoldSplitItem(
        InventoryContainer source,
        InventoryItemInstance splitInstance,
        int itemRotationSteps,
        Vector2Int itemGrabOffset)
    {
        if (HasItem ||
            source == null ||
            splitInstance == null ||
            splitInstance.IsEmpty)
        {
            return false;
        }

        sourceContainer =
            source;

        sourcePosition =
            Vector2Int.zero;

        sourceRotationSteps =
            ItemDefinition.NormalizeRotationSteps(
                itemRotationSteps
            );

        hasOriginalPlacement =
            false;

        grabOffset =
            itemGrabOffset;

        SetItem(
            splitInstance,
            itemRotationSteps
        );

        return true;
    }

    internal void RotateCounterClockwise()
    {
        if (!HasItem)
            return;

        rotationSteps =
            ItemDefinition.NormalizeRotationSteps(
                rotationSteps - 1
            );

        ClampGrabOffset();

        Changed?.Invoke();
    }

    internal void Clear()
    {
        UnsubscribeItem();

        itemInstance = null;
        rotationSteps = 0;
        sourceContainer = null;
        sourcePosition = Vector2Int.zero;
        sourceRotationSteps = 0;
        hasOriginalPlacement = false;
        grabOffset = Vector2Int.zero;

        Changed?.Invoke();
    }

    private void SetItem(
        InventoryItemInstance newItemInstance,
        int newRotationSteps)
    {
        UnsubscribeItem();

        itemInstance =
            newItemInstance;

        rotationSteps =
            ItemDefinition.NormalizeRotationSteps(
                newRotationSteps
            );

        SubscribeItem();

        ClampGrabOffset();

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