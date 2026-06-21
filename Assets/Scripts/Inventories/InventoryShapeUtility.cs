using UnityEngine;

public static class InventoryShapeUtility
{
    public static bool IsOccupiedInShape(
        ItemData itemData,
        int x,
        int y,
        int rotationSteps)
    {
        if (itemData == null)
            return false;

        int width =
            itemData.GetWidth(rotationSteps);

        int height =
            itemData.GetHeight(rotationSteps);

        if (x < 0 ||
            y < 0 ||
            x >= width ||
            y >= height)
        {
            return false;
        }

        return itemData.IsCellOccupied(
            x,
            y,
            rotationSteps
        );
    }
}