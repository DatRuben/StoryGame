using UnityEngine;

public sealed class WorldItemSpawner :
    MonoBehaviour
{
    [SerializeField]
    private WorldItem worldItemPrefab;

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
            item.Definition == null ||
            item.Definition.worldPrefab == null)
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

        if (worldItem.Initialize(item))
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
}