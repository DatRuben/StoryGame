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

        Rigidbody body =
            worldItem.GetComponent<Rigidbody>();

        if (body != null)
        {
            body.isKinematic = true;
        }

        if (worldItem == null)
            return false;

        if (worldItem.Initialize(item))
        {
            Vector3 rayStart =
                position + Vector3.up * 2f;

            if (Physics.Raycast(
                    rayStart,
                    Vector3.down,
                    out RaycastHit hit,
                    5f))
            {
                worldItem.LiftAboveSurface(
                    hit.point.y
                );
            }

            return true;
        }

        Destroy(worldItem.gameObject);
        worldItem = null;

        return false;
    }
}