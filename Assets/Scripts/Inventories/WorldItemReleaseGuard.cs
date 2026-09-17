using System.Collections.Generic;
using UnityEngine;

public sealed class WorldItemReleaseGuard :
    MonoBehaviour
{
    private const int
        MaxResolveIterations = 12;

    private const float
        SeparationPadding = 0.002f;

    private readonly List<Collider>
        itemColliders =
            new List<Collider>();

    private readonly List<Collider>
        sourceColliders =
            new List<Collider>();

    private Rigidbody body;
    private bool active;

    public bool Begin(
        Transform sourceRoot)
    {
        if (sourceRoot == null)
            return false;

        body =
            GetComponent<Rigidbody>();

        if (body == null)
            return false;

        CollectItemColliders();
        CollectSourceColliders(
            sourceRoot
        );

        if (itemColliders.Count == 0)
            return false;

        SetSourceCollisionIgnored(
            true
        );

        ResolveEnvironmentPenetration();

        body.isKinematic = false;

        body.linearVelocity =
            Vector3.zero;

        body.angularVelocity =
            Vector3.zero;

        active = true;

        return true;
    }

    private void FixedUpdate()
    {
        if (!active)
            return;

        if (StillOverlapsSource())
            return;

        SetSourceCollisionIgnored(
            false
        );

        active = false;

        Destroy(this);
    }

    private void CollectItemColliders()
    {
        itemColliders.Clear();

        Collider[] colliders =
            GetComponentsInChildren<
                Collider>(true);

        for (int i = 0;
             i < colliders.Length;
             i++)
        {
            Collider collider =
                colliders[i];

            if (collider == null ||
                !collider.enabled ||
                collider.isTrigger)
            {
                continue;
            }

            itemColliders.Add(
                collider
            );
        }
    }

    private void CollectSourceColliders(
        Transform sourceRoot)
    {
        sourceColliders.Clear();

        Collider[] colliders =
            sourceRoot
                .GetComponentsInChildren<
                    Collider>(true);

        for (int i = 0;
             i < colliders.Length;
             i++)
        {
            Collider collider =
                colliders[i];

            if (collider == null ||
                !collider.enabled ||
                collider.isTrigger)
            {
                continue;
            }

            sourceColliders.Add(
                collider
            );
        }
    }

    private void SetSourceCollisionIgnored(
        bool ignored)
    {
        for (int i = 0;
             i < itemColliders.Count;
             i++)
        {
            Collider itemCollider =
                itemColliders[i];

            if (itemCollider == null)
                continue;

            for (int j = 0;
                 j < sourceColliders.Count;
                 j++)
            {
                Collider sourceCollider =
                    sourceColliders[j];

                if (sourceCollider == null)
                    continue;

                Physics.IgnoreCollision(
                    itemCollider,
                    sourceCollider,
                    ignored
                );
            }
        }
    }

    private bool StillOverlapsSource()
    {
        Physics.SyncTransforms();

        for (int i = 0;
             i < itemColliders.Count;
             i++)
        {
            Collider itemCollider =
                itemColliders[i];

            if (itemCollider == null)
                continue;

            for (int j = 0;
                 j < sourceColliders.Count;
                 j++)
            {
                Collider sourceCollider =
                    sourceColliders[j];

                if (sourceCollider == null)
                    continue;

                if (Physics.ComputePenetration(
                        itemCollider,
                        itemCollider.transform.position,
                        itemCollider.transform.rotation,
                        sourceCollider,
                        sourceCollider.transform.position,
                        sourceCollider.transform.rotation,
                        out _,
                        out _))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void ResolveEnvironmentPenetration()
    {
        for (int iteration = 0;
             iteration <
                 MaxResolveIterations;
             iteration++)
        {
            Physics.SyncTransforms();

            if (!TryResolveOnePenetration())
                return;
        }
    }

    private bool TryResolveOnePenetration()
    {
        for (int i = 0;
             i < itemColliders.Count;
             i++)
        {
            Collider itemCollider =
                itemColliders[i];

            if (itemCollider == null)
                continue;

            Bounds bounds =
                itemCollider.bounds;

            Collider[] nearby =
                Physics.OverlapBox(
                    bounds.center,
                    bounds.extents,
                    Quaternion.identity,
                    ~0,
                    QueryTriggerInteraction.Ignore
                );

            for (int j = 0;
                 j < nearby.Length;
                 j++)
            {
                Collider other =
                    nearby[j];

                if (other == null ||
                    IsPartOfItem(other) ||
                    IsSourceCollider(other))
                {
                    continue;
                }

                if (!Physics.ComputePenetration(
                        itemCollider,
                        itemCollider.transform.position,
                        itemCollider.transform.rotation,
                        other,
                        other.transform.position,
                        other.transform.rotation,
                        out Vector3 direction,
                        out float distance))
                {
                    continue;
                }

                transform.position +=
                    direction *
                    (distance +
                     SeparationPadding);

                return true;
            }
        }

        return false;
    }

    private bool IsPartOfItem(
        Collider collider)
    {
        return
            collider.transform ==
                transform ||
            collider.transform
                .IsChildOf(transform);
    }

    private bool IsSourceCollider(
        Collider collider)
    {
        return
            sourceColliders.Contains(
                collider
            );
    }

    private void OnDestroy()
    {
        if (active)
        {
            SetSourceCollisionIgnored(
                false
            );
        }
    }
}