using UnityEngine;

public static class InventoryShapeUtility
{
    public static bool IsOccupiedInShape(
        ItemDefinition itemDefinition,
        int x,
        int y,
        int rotationSteps)
    {
        if (itemDefinition == null)
            return false;

        int width =
            itemDefinition.GetWidth(rotationSteps);

        int height =
            itemDefinition.GetHeight(rotationSteps);

        if (x < 0 ||
            y < 0 ||
            x >= width ||
            y >= height)
        {
            return false;
        }

        return itemDefinition.IsCellOccupied(
            x,
            y,
            rotationSteps
        );
    }
}