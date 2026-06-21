using UnityEngine;

public static class InventoryStackPreviewUtility
{
    public static bool TryGetPreviewColor(
        InventoryGrid grid,
        PlacedInventoryItem heldItem,
        Vector2Int targetCoordinate,
        Vector2Int cellCoordinate,
        Color validColor,
        Color partialColor,
        Color invalidColor,
        out Color previewColor)
    {
        previewColor = invalidColor;

        if (grid == null ||
            heldItem == null ||
            heldItem.ItemData == null ||
            !heldItem.ItemData.isStackable)
        {
            return false;
        }

        PlacedInventoryItem targetStack =
            grid.GetPlacedItem(
                targetCoordinate.x,
                targetCoordinate.y
            );

        if (targetStack == null ||
            targetStack.ItemData == null)
        {
            return false;
        }

        if (targetStack.ItemData != heldItem.ItemData)
            return false;

        if (!targetStack.ItemData.isStackable)
            return false;

        PlacedInventoryItem cellStack =
            grid.GetPlacedItem(
                cellCoordinate.x,
                cellCoordinate.y
            );

        if (cellStack != targetStack)
            return false;

        int maxStackSize =
            Mathf.Max(
                1,
                targetStack.ItemData.maxStackSize
            );

        int roomLeft =
            maxStackSize - targetStack.Quantity;

        if (roomLeft <= 0)
        {
            previewColor = invalidColor;
            return true;
        }

        if (roomLeft >= heldItem.Quantity)
        {
            previewColor = validColor;
            return true;
        }

        previewColor = partialColor;
        return true;
    }
}