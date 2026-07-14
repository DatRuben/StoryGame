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
            heldItem.ItemDefinition == null ||
            !heldItem.ItemDefinition.isStackable)
        {
            return false;
        }

        PlacedInventoryItem targetStack =
            grid.GetPlacedItem(
                targetCoordinate.x,
                targetCoordinate.y
            );

        if (targetStack == null ||
            targetStack.ItemDefinition == null)
        {
            return false;
        }

        if (targetStack.ItemDefinition != heldItem.ItemDefinition)
            return false;

        if (!targetStack.ItemDefinition.isStackable)
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
                targetStack.ItemDefinition.maxStackSize
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