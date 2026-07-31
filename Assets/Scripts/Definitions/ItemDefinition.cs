using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

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

[CreateAssetMenu(menuName = "Game/Item Definition")]
public class ItemDefinition : ScriptableObject
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
        int requiredSize =
            Mathf.Max(
                1,
                shapeWidth * shapeHeight
            );

        if (!isStackable)
        {
            maxStackSize = 1;
        }
        else
        {
            maxStackSize =
                Mathf.Max(
                    2,
                    maxStackSize
                );
        }

        if (occupiedCells == null ||
            occupiedCells.Length != requiredSize)
        {
            bool[] newCells =
                new bool[requiredSize];

            if (occupiedCells != null)
            {
                int copyLength =
                    Mathf.Min(
                        occupiedCells.Length,
                        newCells.Length
                    );

                for (int i = 0; i < copyLength; i++)
                    newCells[i] = occupiedCells[i];
            }

            occupiedCells = newCells;
        }

        if (!HasAnyOccupiedCell())
            occupiedCells[0] = true;

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

    private bool HasAnyOccupiedCell()
    {
        if (occupiedCells == null)
            return false;

        for (int i = 0; i < occupiedCells.Length; i++)
        {
            if (occupiedCells[i])
                return true;
        }

        return false;
    }

    public int GetWidth(int rotationSteps)
    {
        rotationSteps =
            NormalizeRotationSteps(
                rotationSteps
            );

        bool swapsWidthAndHeight =
            rotationSteps == 1 ||
            rotationSteps == 3;

        return swapsWidthAndHeight
            ? shapeHeight
            : shapeWidth;
    }

    public int GetHeight(int rotationSteps)
    {
        rotationSteps =
            NormalizeRotationSteps(
                rotationSteps
            );

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
        rotationSteps =
            NormalizeRotationSteps(
                rotationSteps
            );

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

    public IReadOnlyList<Vector2Int> GetOccupiedCells(
        int rotationSteps)
    {
        List<Vector2Int> result =
            new List<Vector2Int>();

        int rotatedWidth =
            GetWidth(rotationSteps);

        int rotatedHeight =
            GetHeight(rotationSteps);

        for (int y = 0; y < rotatedHeight; y++)
        {
            for (int x = 0; x < rotatedWidth; x++)
            {
                if (!IsCellOccupied(x, y, rotationSteps))
                    continue;

                result.Add(
                    new Vector2Int(x, y)
                );
            }
        }

        if (result.Count == 0)
            result.Add(Vector2Int.zero);

        return result;
    }

    public static int NormalizeRotationSteps(
        int rotationSteps)
    {
        rotationSteps %= 4;

        if (rotationSteps < 0)
            rotationSteps += 4;

        return rotationSteps;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ItemDefinition))]
public class ItemDefinitionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ItemDefinition item = (ItemDefinition)target;

        EditorGUI.BeginChangeCheck();

        DrawIdentitySection(item);
        DrawItemTypeSection(item);
        DrawHeldSection(item);
        DrawWeaponUseSection(item);
        DrawCarryRequirementsSection(item);
        DrawStackingSection(item);
        DrawSaddleEquipmentSection(item);
        DrawInventoryShapeSection(item);

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(item);
        }
    }

    private void DrawIdentitySection(ItemDefinition item)
    {
        EditorGUILayout.LabelField(
            "Identity",
            EditorStyles.boldLabel
        );

        item.itemName =
            EditorGUILayout.TextField(
                "Item Name",
                item.itemName
            );

        EditorGUILayout.Space();
    }

    private void DrawItemTypeSection(ItemDefinition item)
    {
        EditorGUILayout.LabelField(
            "Item Type",
            EditorStyles.boldLabel
        );

        item.itemCategory =
            (ItemCategory)EditorGUILayout.EnumPopup(
                "Item Category",
                item.itemCategory
            );

        EditorGUILayout.Space();
    }

    private void DrawHeldSection(ItemDefinition item)
    {
        EditorGUILayout.LabelField(
            "Held UI",
            EditorStyles.boldLabel
        );

        item.itemIcon =
            (Sprite)EditorGUILayout.ObjectField(
                "Item Icon",
                item.itemIcon,
                typeof(Sprite),
                false
            );

        item.handUsage =
            (ItemHandUsage)EditorGUILayout.EnumPopup(
                "Hand Usage",
                item.handUsage
            );

        EditorGUILayout.LabelField("Held Controls Text");

        item.heldControlsText =
            EditorGUILayout.TextArea(
                item.heldControlsText,
                GUILayout.MinHeight(40f)
            );

        EditorGUILayout.LabelField("Weapon Controls Text");

        item.weaponControlsText =
            EditorGUILayout.TextArea(
                item.weaponControlsText,
                GUILayout.MinHeight(40f)
            );

        EditorGUILayout.Space();
    }

    private void DrawWeaponUseSection(ItemDefinition item)
    {
        if (item.itemCategory != ItemCategory.Weapon)
        {
            item.weaponUseType = WeaponUseType.HandWeapon;
            return;
        }

        EditorGUILayout.LabelField(
            "Weapon Use",
            EditorStyles.boldLabel
        );

        item.weaponUseType =
            (WeaponUseType)EditorGUILayout.EnumPopup(
                "Weapon Use Type",
                item.weaponUseType
            );

        EditorGUILayout.Space();
    }

    private void DrawCarryRequirementsSection(ItemDefinition item)
    {
        EditorGUILayout.LabelField(
            "Future Carry Requirements",
            EditorStyles.boldLabel
        );

        item.carrySize =
            Mathf.Max(
                1,
                EditorGUILayout.IntField(
                    "Carry Size",
                    item.carrySize
                )
            );

        item.carryWeight =
            Mathf.Max(
                0f,
                EditorGUILayout.FloatField(
                    "Carry Weight",
                    item.carryWeight
                )
            );

        EditorGUILayout.Space();
    }

    private void DrawStackingSection(ItemDefinition item)
    {
        EditorGUILayout.LabelField(
            "Stacking",
            EditorStyles.boldLabel
        );

        item.isStackable =
            EditorGUILayout.Toggle(
                "Is Stackable",
                item.isStackable
            );

        if (item.isStackable)
        {
            item.maxStackSize =
                Mathf.Max(
                    2,
                    EditorGUILayout.IntField(
                        "Max Stack Size",
                        item.maxStackSize
                    )
                );
        }
        else
        {
            item.maxStackSize = 1;
        }

        EditorGUILayout.Space();
    }

    private void DrawSaddleEquipmentSection(ItemDefinition item)
    {
        if (item.itemCategory != ItemCategory.Equipment)
        {
            item.hasManualSaddleTurret = false;
            item.manualSaddleTurretControlsText = "";
            return;
        }

        EditorGUILayout.LabelField(
            "Equipment",
            EditorStyles.boldLabel
        );

        item.equipmentSlotType =
            (EquipmentSlotType)EditorGUILayout.EnumPopup(
                "Equipment Slot Type",
                item.equipmentSlotType
            );

        EditorGUILayout.Space();

        if (item.equipmentSlotType != EquipmentSlotType.Saddle)
        {
            item.hasManualSaddleTurret = false;
            item.manualSaddleTurretControlsText = "";
            return;
        }

        EditorGUILayout.LabelField(
            "Saddle Equipment",
            EditorStyles.boldLabel
        );

        item.hasManualSaddleTurret =
            EditorGUILayout.Toggle(
                "Has Manual Saddle Turret",
                item.hasManualSaddleTurret
            );

        if (item.hasManualSaddleTurret)
        {
            EditorGUILayout.LabelField(
                "Manual Saddle Turret Controls Text"
            );

            item.manualSaddleTurretControlsText =
                EditorGUILayout.TextArea(
                    item.manualSaddleTurretControlsText,
                    GUILayout.MinHeight(40f)
                );
        }
        else
        {
            item.manualSaddleTurretControlsText = "";
        }

        EditorGUILayout.Space();
    }

    private void DrawInventoryShapeSection(ItemDefinition item)
    {
        EditorGUILayout.LabelField(
            "Inventory Shape",
            EditorStyles.boldLabel
        );

        int newWidth =
            EditorGUILayout.IntField(
                "Shape Width",
                item.shapeWidth
            );

        int newHeight =
            EditorGUILayout.IntField(
                "Shape Height",
                item.shapeHeight
            );

        newWidth = Mathf.Max(1, newWidth);
        newHeight = Mathf.Max(1, newHeight);

        if (newWidth != item.shapeWidth ||
            newHeight != item.shapeHeight)
        {
            Undo.RecordObject(
                item,
                "Resize Item Shape"
            );

            item.shapeWidth = newWidth;
            item.shapeHeight = newHeight;

            ResizeOccupiedCells(item);

            EditorUtility.SetDirty(item);
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField(
            "Shape Grid",
            EditorStyles.boldLabel
        );

        DrawShapeGrid(item);

        EditorGUILayout.Space();

        if (GUILayout.Button("Fill All Cells"))
        {
            Undo.RecordObject(
                item,
                "Fill Item Shape"
            );

            EnsureOccupiedCells(item);

            for (int i = 0; i < item.occupiedCells.Length; i++)
                item.occupiedCells[i] = true;

            EditorUtility.SetDirty(item);
        }

        if (GUILayout.Button("Clear All Cells"))
        {
            Undo.RecordObject(
                item,
                "Clear Item Shape"
            );

            EnsureOccupiedCells(item);

            for (int i = 0; i < item.occupiedCells.Length; i++)
                item.occupiedCells[i] = false;

            EditorUtility.SetDirty(item);
        }
    }

    private void DrawShapeGrid(ItemDefinition item)
    {
        EnsureOccupiedCells(item);

        for (int y = item.shapeHeight - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();

            for (int x = 0; x < item.shapeWidth; x++)
            {
                int index =
                    y * item.shapeWidth + x;

                bool occupied =
                    item.occupiedCells[index];

                GUIStyle style =
                    new GUIStyle(GUI.skin.button);

                style.fixedWidth = 30;
                style.fixedHeight = 30;

                string label =
                    occupied ? "X" : ".";

                if (GUILayout.Button(label, style))
                {
                    Undo.RecordObject(
                        item,
                        "Toggle Item Shape Cell"
                    );

                    item.occupiedCells[index] =
                        !item.occupiedCells[index];

                    EditorUtility.SetDirty(item);
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private void ResizeOccupiedCells(ItemDefinition item)
    {
        int requiredSize =
            Mathf.Max(
                1,
                item.shapeWidth * item.shapeHeight
            );

        bool[] newCells =
            new bool[requiredSize];

        if (item.occupiedCells != null)
        {
            int oldWidth = item.shapeWidth;
            int oldHeight = item.shapeHeight;

            int copyLength =
                Mathf.Min(
                    item.occupiedCells.Length,
                    newCells.Length
                );

            for (int i = 0; i < copyLength; i++)
                newCells[i] = item.occupiedCells[i];
        }

        item.occupiedCells = newCells;
    }

    private void EnsureOccupiedCells(ItemDefinition item)
    {
        int requiredSize =
            Mathf.Max(
                1,
                item.shapeWidth * item.shapeHeight
            );

        if (item.occupiedCells == null ||
            item.occupiedCells.Length != requiredSize)
        {
            ResizeOccupiedCells(item);
            EditorUtility.SetDirty(item);
        }
    }
}
#endif