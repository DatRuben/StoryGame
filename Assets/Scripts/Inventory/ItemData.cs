using UnityEngine;

public enum ItemCategory
{
    Weapon,
    Armor,
    Equipment,
    Consumable,
    Food,
    Ingredients,
    Material,
    Misc,
    Unique
}

[CreateAssetMenu(menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Identity")]
    public string itemName;

    [Header("Item Type")]
    public ItemCategory itemCategory = ItemCategory.Misc;

    [Header("Inventory Shape")]
    [Min(1)] public int shapeWidth = 1;
    [Min(1)] public int shapeHeight = 1;

    public bool[] occupiedCells = new bool[1] { true };

    private void OnValidate()
    {
        int requiredSize = Mathf.Max(1, shapeWidth * shapeHeight);

        if (occupiedCells == null || occupiedCells.Length != requiredSize)
        {
            bool[] newCells = new bool[requiredSize];

            if (occupiedCells != null)
            {
                int copyLength =
                    Mathf.Min(occupiedCells.Length, newCells.Length);

                for (int i = 0; i < copyLength; i++)
                    newCells[i] = occupiedCells[i];
            }

            occupiedCells = newCells;
        }
    }

    public int GetWidth(int rotationSteps)
    {
        rotationSteps = NormalizeRotationSteps(rotationSteps);

        bool swapsWidthAndHeight =
            rotationSteps == 1 ||
            rotationSteps == 3;

        return swapsWidthAndHeight ? shapeHeight : shapeWidth;
    }

    public int GetHeight(int rotationSteps)
    {
        rotationSteps = NormalizeRotationSteps(rotationSteps);

        bool swapsWidthAndHeight =
            rotationSteps == 1 ||
            rotationSteps == 3;

        return swapsWidthAndHeight ? shapeWidth : shapeHeight;
    }

    public bool IsCellOccupied(
        int x,
        int y,
        int rotationSteps)
    {
        rotationSteps = NormalizeRotationSteps(rotationSteps);

        int originalX;
        int originalY;

        switch (rotationSteps)
        {
            case 1:
                // 90 degrees clockwise.
                originalX = y;
                originalY = shapeHeight - 1 - x;
                break;

            case 2:
                // 180 degrees.
                originalX = shapeWidth - 1 - x;
                originalY = shapeHeight - 1 - y;
                break;

            case 3:
                // 270 degrees clockwise.
                originalX = shapeWidth - 1 - y;
                originalY = x;
                break;

            default:
                // 0 degrees.
                originalX = x;
                originalY = y;
                break;
        }

        if (originalX < 0 ||
            originalY < 0 ||
            originalX >= shapeWidth ||
            originalY >= shapeHeight)
        {
            return false;
        }

        int index =
            originalY * shapeWidth + originalX;

        if (occupiedCells == null ||
            index < 0 ||
            index >= occupiedCells.Length)
        {
            return false;
        }

        return occupiedCells[index];
    }

    public static int NormalizeRotationSteps(int rotationSteps)
    {
        rotationSteps %= 4;

        if (rotationSteps < 0)
            rotationSteps += 4;

        return rotationSteps;
    }
}