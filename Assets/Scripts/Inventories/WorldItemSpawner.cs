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

        Rigidbody body =
            worldItem.GetComponent<Rigidbody>();

        if (body != null)
        {
            body.isKinematic = true;
        }

        Vector3 rayStart =
            position + Vector3.up * 2f;

        bool foundSurface =
            Physics.Raycast(
                rayStart,
                Vector3.down,
                out RaycastHit hit,
                5f
            );

        if (worldItem.Initialize(item))
        {
            if (foundSurface)
            {
                worldItem.LiftAboveSurface(
                    hit.point.y
                );
            }

            if (body != null)
            {
                body.linearVelocity =
                    Vector3.zero;

                body.angularVelocity =
                    Vector3.zero;

                body.isKinematic = false;
            }

            return true;
        }

        Destroy(worldItem.gameObject);
        worldItem = null;

        return false;
    }
}