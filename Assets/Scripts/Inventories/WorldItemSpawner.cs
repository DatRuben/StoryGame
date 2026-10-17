using UnityEngine;

[RequireComponent(typeof(ItemVisualResolver))]
public sealed class WorldItemSpawner :
    MonoBehaviour
{
    [SerializeField]
    private WorldItem worldItemPrefab;

    private ItemVisualResolver visualResolver;

    public ItemVisualResolver VisualResolver
    {
        get
        {
            if (visualResolver == null)
            {
                visualResolver =
                    GetComponent<ItemVisualResolver>();
            }

            return visualResolver;
        }
    }

    private void Awake()
    {
        visualResolver =
            GetComponent<ItemVisualResolver>();
    }

    public bool TrySpawn(
        InventoryItemInstance item,
        Vector3 position,
        Quaternion rotation,
        out WorldItem worldItem)
    {
        worldItem = null;

        if (worldItemPrefab == null ||
            item == null ||
            item.IsEmpty ||
            item.Definition == null)
        {
            return false;
        }

        GameObject visualPrefab =
            VisualResolver != null
                ? VisualResolver.ResolveWorldPrefab(
                    item.Definition
                )
                : null;

        if (visualPrefab == null)
        {
            return false;
        }

        worldItem =
            Instantiate(
                worldItemPrefab,
                position,
                rotation
            );

        if (worldItem == null)
            return false;

        if (worldItem.Initialize(
            item,
            visualPrefab
        ))
        {
            worldItem.LiftAboveSurface(
                position.y
            );

            return true;
        }

        Destroy(worldItem.gameObject);
        worldItem = null;

        return false;
    }

    public bool TrySpawnForRelease(
        InventoryItemInstance item,
        Pose releasePose,
        out WorldItem worldItem)
    {
        worldItem = null;

        if (worldItemPrefab == null ||
            item == null ||
            item.IsEmpty ||
            item.Definition == null)
        {
            return false;
        }

        GameObject visualPrefab =
            VisualResolver != null
                ? VisualResolver.ResolveWorldPrefab(
                    item.Definition
                )
                : null;

        if (visualPrefab == null)
        {
            return false;
        }

        worldItem =
            Instantiate(
                worldItemPrefab,
                releasePose.position,
                releasePose.rotation
            );

        if (worldItem == null)
            return false;

        Rigidbody body =
            worldItem.GetComponent<Rigidbody>();

        if (body != null)
        {
            body.isKinematic = true;
        }

        if (worldItem.Initialize(
            item,
            visualPrefab
        ))
        {
            return true;
        }

        Destroy(worldItem.gameObject);
        worldItem = null;

        return false;
    }
}