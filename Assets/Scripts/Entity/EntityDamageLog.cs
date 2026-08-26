using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EntityResources))]
public sealed class EntityDamageLog :
    MonoBehaviour
{
    public readonly struct Entry
    {
        public GameObject Source { get; }

        public DamageType DamageType { get; }

        public float HealthDamage { get; }
        public float SoulBarrierDamage { get; }

        public bool WasFinalBlow { get; }

        public Entry(
            DamageResult result)
        {
            Source =
                result.Context.Source;

            DamageType =
                result.Context.DamageType;

            HealthDamage =
                result.HealthDamage;

            SoulBarrierDamage =
                result.SoulBarrierDamage;

            WasFinalBlow =
                result.HealthDepleted;
        }
    }

    private EntityResources resources;

    private readonly List<Entry>
        entries =
            new List<Entry>();

    public IReadOnlyList<Entry>
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
            new Entry(
                result
            )
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