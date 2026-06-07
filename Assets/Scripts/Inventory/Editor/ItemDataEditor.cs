using NUnit.Framework.Interfaces;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemData))]
public class ItemDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ItemData item = (ItemData)target;

        EditorGUILayout.LabelField("Identity", EditorStyles.boldLabel);
        item.itemName = EditorGUILayout.TextField("Item Name", item.itemName);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Inventory Shape", EditorStyles.boldLabel);

        int newWidth = EditorGUILayout.IntField("Shape Width", item.shapeWidth);
        int newHeight = EditorGUILayout.IntField("Shape Height", item.shapeHeight);

        newWidth = Mathf.Max(1, newWidth);
        newHeight = Mathf.Max(1, newHeight);

        if (newWidth != item.shapeWidth || newHeight != item.shapeHeight)
        {
            Undo.RecordObject(item, "Resize Item Shape");

            item.shapeWidth = newWidth;
            item.shapeHeight = newHeight;

            ResizeOccupiedCells(item);

            EditorUtility.SetDirty(item);
        }

        item.canRotate = EditorGUILayout.Toggle("Can Rotate", item.canRotate);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Shape Grid", EditorStyles.boldLabel);

        DrawShapeGrid(item);

        EditorGUILayout.Space();

        if (GUILayout.Button("Fill All Cells"))
        {
            Undo.RecordObject(item, "Fill Item Shape");

            for (int i = 0; i < item.occupiedCells.Length; i++)
                item.occupiedCells[i] = true;

            EditorUtility.SetDirty(item);
        }

        if (GUILayout.Button("Clear All Cells"))
        {
            Undo.RecordObject(item, "Clear Item Shape");

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
                int index = y * item.shapeWidth + x;

                bool occupied = item.occupiedCells[index];

                GUIStyle style = new GUIStyle(GUI.skin.button);
                style.fixedWidth = 30;
                style.fixedHeight = 30;

                string label = occupied ? "X" : ".";

                if (GUILayout.Button(label, style))
                {
                    Undo.RecordObject(item, "Toggle Item Shape Cell");

                    item.occupiedCells[index] = !occupied;

                    EditorUtility.SetDirty(item);
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private void ResizeOccupiedCells(ItemData item)
    {
        bool[] oldCells = item.occupiedCells;

        bool[] newCells = new bool[item.shapeWidth * item.shapeHeight];

        if (oldCells != null)
        {
            int copyLength = Mathf.Min(oldCells.Length, newCells.Length);

            for (int i = 0; i < copyLength; i++)
                newCells[i] = oldCells[i];
        }

        item.occupiedCells = newCells;
    }

    private void EnsureOccupiedCells(ItemData item)
    {
        int requiredSize = item.shapeWidth * item.shapeHeight;

        if (item.occupiedCells == null || item.occupiedCells.Length != requiredSize)
        {
            ResizeOccupiedCells(item);
        }
    }
}