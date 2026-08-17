using UnityEngine;

public sealed class WorldItem :
    MonoBehaviour
{
    [Header("Visual")]
    [SerializeField]
    private Transform visualRoot;

    [SerializeField]
    private BoxCollider physicalCollider;

    [SerializeField]
    private Rigidbody rigidbodyComponent;

    [SerializeField]
    [Min(0.01f)]
    private float minimumColliderSize = 0.1f;

    [SerializeField]
    [Min(0f)]
    private float spawnClearance = 0.05f;

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

        if (!BuildVisual())
            return false;

        return FitColliderToVisual();
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

    private bool FitColliderToVisual()
    {
        if (visualInstance == null)
            return false;

        if (physicalCollider == null)
        {
            physicalCollider =
                GetComponent<BoxCollider>();
        }

        if (rigidbodyComponent == null)
        {
            rigidbodyComponent =
                GetComponent<Rigidbody>();
        }

        if (physicalCollider == null ||
            rigidbodyComponent == null)
        {
            return false;
        }

        Renderer[] renderers =
            visualInstance.GetComponentsInChildren<
                Renderer>(true);

        if (renderers.Length == 0)
            return false;

        bool hasBounds = false;
        Bounds localBounds =
            new Bounds();

        for (int i = 0;
             i < renderers.Length;
             i++)
        {
            Renderer renderer =
                renderers[i];

            if (renderer == null)
                continue;

            Bounds worldBounds =
                renderer.bounds;

            Vector3 min =
                worldBounds.min;

            Vector3 max =
                worldBounds.max;

            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        Vector3 worldCorner =
                            new Vector3(
                                x == 0 ? min.x : max.x,
                                y == 0 ? min.y : max.y,
                                z == 0 ? min.z : max.z
                            );

                        Vector3 localCorner =
                            transform.InverseTransformPoint(
                                worldCorner
                            );

                        if (!hasBounds)
                        {
                            localBounds =
                                new Bounds(
                                    localCorner,
                                    Vector3.zero
                                );

                            hasBounds = true;
                        }
                        else
                        {
                            localBounds.Encapsulate(
                                localCorner
                            );
                        }
                    }
                }
            }
        }

        if (!hasBounds)
            return false;

        Vector3 size =
            localBounds.size;

        size.x =
            Mathf.Max(
                minimumColliderSize,
                size.x
            );

        size.y =
            Mathf.Max(
                minimumColliderSize,
                size.y
            );

        size.z =
            Mathf.Max(
                minimumColliderSize,
                size.z
            );

        physicalCollider.center =
            localBounds.center;

        physicalCollider.size =
            size;

        physicalCollider.isTrigger =
            false;

        return true;
    }

    internal void LiftAboveSurface(
        float surfaceHeight)
    {
        if (physicalCollider == null)
            return;

        float targetBottom =
            surfaceHeight +
            spawnClearance;

        float currentBottom =
            physicalCollider.bounds.min.y;

        float lift =
            targetBottom -
            currentBottom;

        if (lift <= 0f)
            return;

        transform.position +=
            Vector3.up * lift;
    }

    public Bounds GetPhysicalBounds()
    {
        Collider[] colliders =
            GetComponentsInChildren<
                Collider>();

        bool found = false;
        Bounds bounds =
            new Bounds(
                transform.position,
                Vector3.zero
            );

        for (int i = 0;
             i < colliders.Length;
             i++)
        {
            Collider collider =
                colliders[i];

            if (collider == null ||
                collider.isTrigger)
            {
                continue;
            }

            if (!found)
            {
                bounds =
                    collider.bounds;

                found = true;
            }
            else
            {
                bounds.Encapsulate(
                    collider.bounds
                );
            }
        }

        return bounds;
    }

    internal void LiftAboveSurface(
        float surfaceHeight,
        float clearance = 0.05f)
    {
        Bounds bounds =
            GetPhysicalBounds();

        float requiredBottom =
            surfaceHeight +
            clearance;

        float lift =
            requiredBottom -
            bounds.min.y;

        if (lift > 0f)
        {
            transform.position +=
                Vector3.up * lift;
        }
    }

    private void ClearVisual()
    {
        if (visualInstance == null)
            return;

        Destroy(visualInstance);

        visualInstance = null;
    }
}