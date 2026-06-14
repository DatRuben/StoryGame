using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemData))]
public class ItemDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ItemData item = (ItemData)target;

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

    private void DrawIdentitySection(ItemData item)
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

    private void DrawItemTypeSection(ItemData item)
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

    private void DrawHeldSection(ItemData item)
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

    private void DrawWeaponUseSection(ItemData item)
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

    private void DrawCarryRequirementsSection(ItemData item)
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

    private void DrawStackingSection(ItemData item)
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

    private void DrawSaddleEquipmentSection(ItemData item)
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

    private void DrawInventoryShapeSection(ItemData item)
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

    private void DrawShapeGrid(ItemData item)
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

    private void ResizeOccupiedCells(ItemData item)
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

    private void EnsureOccupiedCells(ItemData item)
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