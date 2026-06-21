using UnityEngine;

public static class InventoryOutlineUtility
{
    public static void ClearChildren(RectTransform root)
    {
        if (root == null)
            return;

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(
                root.GetChild(i).gameObject
            );
        }
    }
}