using UnityEngine;

[RequireComponent(
    typeof(Rigidbody)
)]
public sealed class WorldItem :
    MonoBehaviour
{
    [Header("Visual")]
    [SerializeField]
    private Transform visualRoot;

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

        return BuildAutomaticColliders();
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

    private bool BuildAutomaticColliders()
    {
        if (visualInstance == null)
            return false;

        MeshFilter[] meshFilters =
            visualInstance.GetComponentsInChildren<
                MeshFilter>(true);

        bool foundMesh = false;

        for (int i = 0;
             i < meshFilters.Length;
             i++)
        {
            MeshFilter meshFilter =
                meshFilters[i];

            if (meshFilter == null ||
                meshFilter.sharedMesh == null)
            {
                continue;
            }

            if (IsDuplicateColliderSource(
                    meshFilters,
                    i))
            {
                continue;
            }

            meshFilter.gameObject.layer =
                gameObject.layer;

            MeshCollider meshCollider =
                meshFilter.GetComponent<
                    MeshCollider>();

            if (meshCollider == null)
            {
                meshCollider =
                    meshFilter.gameObject
                        .AddComponent<
                            MeshCollider>();
            }

            meshCollider.sharedMesh =
                meshFilter.sharedMesh;

            meshCollider.convex = true;
            meshCollider.isTrigger = false;

            foundMesh = true;
        }

        return foundMesh;
    }

    private bool IsDuplicateColliderSource(
        MeshFilter[] meshFilters,
        int currentIndex)
    {
        MeshFilter current =
            meshFilters[currentIndex];

        if (current == null ||
            current.sharedMesh == null)
        {
            return false;
        }

        Transform currentTransform =
            current.transform;

        for (int i = 0;
             i < currentIndex;
             i++)
        {
            MeshFilter previous =
                meshFilters[i];

            if (previous == null ||
                previous.sharedMesh == null ||
                previous.sharedMesh !=
                    current.sharedMesh)
            {
                continue;
            }

            Transform previousTransform =
                previous.transform;

            bool samePosition =
                (previousTransform.position -
                 currentTransform.position)
                    .sqrMagnitude <
                0.000001f;

            bool sameRotation =
                Quaternion.Angle(
                    previousTransform.rotation,
                    currentTransform.rotation
                ) <
                0.01f;

            bool sameScale =
                (previousTransform.lossyScale -
                 currentTransform.lossyScale)
                    .sqrMagnitude <
                0.000001f;

            if (samePosition &&
                sameRotation &&
                sameScale)
            {
                return true;
            }
        }

        return false;
    }

    internal void LiftAboveSurface(
        float surfaceHeight)
    {
        if (!TryGetPhysicalBounds(
                out Bounds bounds))
        {
            return;
        }

        float requiredBottom =
            surfaceHeight +
            spawnClearance;

        float lift =
            requiredBottom -
            bounds.min.y;

        if (lift <= 0f)
            return;

        transform.position +=
            Vector3.up * lift;
    }

    private bool TryGetPhysicalBounds(
        out Bounds bounds)
    {
        bounds =
            new Bounds(
                transform.position,
                Vector3.zero
            );

        if (visualInstance == null)
            return false;

        Collider[] colliders =
            visualInstance
                .GetComponentsInChildren<
                    Collider>(true);

        bool found = false;

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

        return found;
    }

    private void ClearVisual()
    {
        if (visualInstance == null)
            return;

        Destroy(
            visualInstance
        );

        visualInstance = null;
    }
}