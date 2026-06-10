using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemData))]
public class ItemDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ItemData item =
            (ItemData)target;

        EditorGUI.BeginChangeCheck();

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

        newWidth =
            Mathf.Max(1, newWidth);

        newHeight =
            Mathf.Max(1, newHeight);

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

            for (int i = 0; i < item.occupiedCells.Length; i++)
                item.occupiedCells[i] = false;

            EditorUtility.SetDirty(item);
        }

        if (EditorGUI.EndChangeCheck())
        {
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