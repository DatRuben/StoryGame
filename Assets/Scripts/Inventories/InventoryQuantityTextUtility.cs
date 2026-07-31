using UnityEngine;

public static class InventoryQuantityTextUtility
{
    public static string GetTextForCell(
        PlacedInventoryItem placedItem,
        Vector2Int coordinate)
    {
        if (placedItem == null ||
            placedItem.ItemDefinition == null)
        {
            return "";
        }

        if (!placedItem.ItemDefinition.isStackable)
            return "";

        if (placedItem.Quantity <= 1)
            return "";

        if (placedItem.Position != coordinate)
            return "";

        return placedItem.Quantity.ToString();
    }
}