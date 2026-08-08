using System;
using UnityEngine;

public sealed class InventoryCursor
{
    private InventoryItemInstance selectedItem;

    private int rotationSteps;

    private Vector2Int grabOffset;

    public event Action Changed;

    public bool HasSelection =>
        selectedItem != null &&
        !selectedItem.IsEmpty;

    public ItemDefinition ItemDefinition =>
        selectedItem != null
            ? selectedItem.Definition
            : null;

    public int Quantity =>
        selectedItem != null
            ? selectedItem.Quantity
            : 0;

    public int RotationSteps =>
        rotationSteps;

    public Vector2Int GrabOffset =>
        grabOffset;

    internal InventoryItemInstance SelectedItem =>
        selectedItem;

    internal bool Select(
        InventoryItemInstance itemInstance,
        int newRotationSteps,
        Vector2Int newGrabOffset)
    {
        if (itemInstance == null ||
            itemInstance.IsEmpty)
        {
            return false;
        }

        if (selectedItem !=
            itemInstance)
        {
            UnsubscribeItem();

            selectedItem =
                itemInstance;

            SubscribeItem();
        }

        rotationSteps =
            global::ItemDefinition
                .NormalizeRotationSteps(
                    newRotationSteps
                );

        grabOffset =
            newGrabOffset;

        ClampGrabOffset();

        Changed?.Invoke();

        return true;
    }

    internal void RotateCounterClockwise()
    {
        if (!HasSelection)
            return;

        rotationSteps =
            global::ItemDefinition
                .NormalizeRotationSteps(
                    rotationSteps - 1
                );

        ClampGrabOffset();

        Changed?.Invoke();
    }

    internal void ClearSelection()
    {
        if (selectedItem == null)
            return;

        UnsubscribeItem();

        selectedItem = null;
        rotationSteps = 0;
        grabOffset = Vector2Int.zero;

        Changed?.Invoke();
    }

    private void SubscribeItem()
    {
        if (selectedItem == null)
            return;

        selectedItem.Changed -=
            OnItemChanged;

        selectedItem.Changed +=
            OnItemChanged;
    }

    private void UnsubscribeItem()
    {
        if (selectedItem == null)
            return;

        selectedItem.Changed -=
            OnItemChanged;
    }

    private void OnItemChanged()
    {
        if (selectedItem == null)
            return;

        if (selectedItem.IsEmpty)
        {
            ClearSelection();
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