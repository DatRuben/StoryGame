using UnityEngine;

public enum ItemCategory
{
    Weapon,
    Equipment,
    Consumable,
    Food,
    Ingredients,
    Material,
    Misc,
    Unique
}

public enum ItemHandUsage
{
    OneHanded,
    TwoHanded
}

public enum EquipmentSlotType
{
    Armor,
    Helmet,
    Saddle,
    Accessory
}

public enum WeaponUseType
{
    HandWeapon,
    MouthWeapon
}

[CreateAssetMenu(menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Identity")]
    public string itemName;

    [Header("Item Type")]
    public ItemCategory itemCategory = ItemCategory.Misc;

    [Header("Held UI")]
    public Sprite itemIcon;

    [Tooltip("Temporary/default hand usage. Later this can be calculated from race size, strength, carry size, and weight.")]
    public ItemHandUsage handUsage = ItemHandUsage.OneHanded;

    [TextArea]
    public string heldControlsText;

    [TextArea]
    public string weaponControlsText;

    [Header("Weapon Use")]
    public WeaponUseType weaponUseType = WeaponUseType.HandWeapon;

    [Header("Future Carry Requirements")]
    [Min(1)] public int carrySize = 1;
    [Min(0f)] public float carryWeight = 1f;

    [Header("Equipment")]
    public EquipmentSlotType equipmentSlotType = EquipmentSlotType.Saddle;

    [Header("Saddle Equipment")]
    public bool hasManualSaddleTurret;

    [TextArea]
    public string manualSaddleTurretControlsText;

    [Header("Stacking")]
    public bool isStackable = false;

    [Min(1)]
    public int maxStackSize = 1;

    [Header("Inventory Shape")]
    [Min(1)] public int shapeWidth = 1;
    [Min(1)] public int shapeHeight = 1;

    public bool[] occupiedCells = new bool[1] { true };

    private void OnValidate()
    {
        int requiredSize = Mathf.Max(1, shapeWidth * shapeHeight);

        if (!isStackable)
        {
            maxStackSize = 1;
        }
        else
        {
            maxStackSize = Mathf.Max(2, maxStackSize);
        }

        if (occupiedCells == null ||
            occupiedCells.Length != requiredSize)
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

        if (itemCategory != ItemCategory.Weapon)
        {
            weaponUseType = WeaponUseType.HandWeapon;
        }

        if (itemCategory != ItemCategory.Equipment ||
            equipmentSlotType != EquipmentSlotType.Saddle)
        {
            hasManualSaddleTurret = false;
            manualSaddleTurretControlsText = "";
        }
    }

    public int GetWidth(int rotationSteps)
    {
        rotationSteps = NormalizeRotationSteps(rotationSteps);

        bool swapsWidthAndHeight =
            rotationSteps == 1 ||
            rotationSteps == 3;

        return swapsWidthAndHeight
            ? shapeHeight
            : shapeWidth;
    }

    public int GetHeight(int rotationSteps)
    {
        rotationSteps = NormalizeRotationSteps(rotationSteps);

        bool swapsWidthAndHeight =
            rotationSteps == 1 ||
            rotationSteps == 3;

        return swapsWidthAndHeight
            ? shapeWidth
            : shapeHeight;
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
                originalX = y;
                originalY = shapeHeight - 1 - x;
                break;

            case 2:
                originalX = shapeWidth - 1 - x;
                originalY = shapeHeight - 1 - y;
                break;

            case 3:
                originalX = shapeWidth - 1 - y;
                originalY = x;
                break;

            default:
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