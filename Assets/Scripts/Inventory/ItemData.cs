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

    public bool canRotate = true;

    private void OnValidate()
    {
        int requiredSize = Mathf.Max(1, shapeWidth * shapeHeight);

        if (occupiedCells == null || occupiedCells.Length != requiredSize)
        {
            bool[] newCells = new bool[requiredSize];

            if (occupiedCells != null)
            {
                int copyLength = Mathf.Min(occupiedCells.Length, newCells.Length);

                for (int i = 0; i < copyLength; i++)
                    newCells[i] = occupiedCells[i];
            }

            occupiedCells = newCells;
        }
    }

    public int GetWidth(bool rotated)
    {
        return rotated ? shapeHeight : shapeWidth;
    }

    public int GetHeight(bool rotated)
    {
        return rotated ? shapeWidth : shapeHeight;
    }

    public bool IsCellOccupied(int x, int y, bool rotated)
    {
        if (!rotated)
        {
            return occupiedCells[y * shapeWidth + x];
        }

        int originalX = y;
        int originalY = shapeHeight - 1 - x;

        return occupiedCells[originalY * shapeWidth + originalX];
    }
}