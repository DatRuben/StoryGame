using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EntityResources))]
public sealed class EntityDamageLog :
    MonoBehaviour
{
    private EntityResources resources;

    private readonly List<DamageResult>
        entries =
            new List<DamageResult>();

    public IReadOnlyList<DamageResult>
        Entries =>
            entries;

    private void Awake()
    {
        resources =
            GetComponent<EntityResources>();

        if (resources != null)
        {
            resources.OnDamageResolved +=
                HandleDamageResolved;
        }
    }

    private void HandleDamageResolved(
        DamageResult result)
    {
        entries.Add(
            result
        );
    }

    private void OnDestroy()
    {
        if (resources != null)
        {
            resources.OnDamageResolved -=
                HandleDamageResolved;
        }
    }
}