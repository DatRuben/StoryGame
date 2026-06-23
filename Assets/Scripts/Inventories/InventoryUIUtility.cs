using UnityEngine;
using UnityEngine.UI;

public static class InventoryUIUtility
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

    public static void CreateImageRect(
        RectTransform root,
        string objectName,
        Vector2 position,
        Vector2 size,
        Color color)
    {
        if (root == null)
            return;

        GameObject rectObject =
            new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(Image)
            );

        rectObject.transform.SetParent(
            root,
            false
        );

        RectTransform rect =
            rectObject.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image =
            rectObject.GetComponent<Image>();

        image.color = color;
        image.raycastTarget = false;
    }

    public static Vector2 GetCornerSize(float thickness)
    {
        return new Vector2(
            thickness,
            thickness
        );
    }

    public static Vector2 GetHorizontalBridgeSize(
        float spacingX,
        float thickness)
    {
        return new Vector2(
            spacingX,
            thickness
        );
    }

    public static Vector2 GetVerticalBridgeSize(
        float thickness,
        float spacingY)
    {
        return new Vector2(
            thickness,
            spacingY
        );
    }
}