using UnityEngine;

public sealed class WorldItem :
    MonoBehaviour
{
    [Header("Visual")]
    [SerializeField]
    private Transform visualRoot;

    private InventoryItemInstance item;
    private GameObject visualInstance;

    public InventoryItemInstance Item =>
        item;

    public bool Initialize(
        InventoryItemInstance itemInstance)
    {
        if (itemInstance == null ||
            itemInstance.IsEmpty ||
            itemInstance.Definition == null ||
            itemInstance.Definition.worldPrefab == null)
        {
            return false;
        }

        item =
            itemInstance;

        return BuildVisual();
    }

    internal bool ReleaseItem(
        InventoryItemInstance itemInstance)
    {
        if (itemInstance == null ||
            !ReferenceEquals(
                item,
                itemInstance))
        {
            return false;
        }

        item = null;

        Destroy(
            gameObject
        );

        return true;
    }

    private bool BuildVisual()
    {
        ClearVisual();

        if (item == null ||
            item.Definition == null ||
            item.Definition.worldPrefab == null)
        {
            return false;
        }

        Transform parent =
            visualRoot != null
                ? visualRoot
                : transform;

        visualInstance =
            Instantiate(
                item.Definition.worldPrefab,
                parent,
                false
            );

        return visualInstance != null;
    }

    private void ClearVisual()
    {
        if (visualInstance == null)
            return;

        Destroy(visualInstance);

        visualInstance = null;
    }
}