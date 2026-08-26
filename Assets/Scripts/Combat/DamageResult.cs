using UnityEngine;

public enum DamageOutcome
{
    Unresolved,
    Applied,
    Blocked,
    Immune
}

public readonly struct DamageResult
{
    public DamageContext Context { get; }

    public GameObject Target { get; }
    public DamageOutcome Outcome { get; }

    public float HealthDamage { get; }
    public float SoulBarrierDamage { get; }

    public bool HealthDepleted { get; }

    public bool DidDamage =>
        HealthDamage > 0f ||
        SoulBarrierDamage > 0f;

    public DamageResult(
        DamageContext context,
        GameObject target,
        DamageOutcome outcome,
        float healthDamage,
        float soulBarrierDamage,
        bool healthDepleted)
    {
        Context = context;

        Target = target;

        Outcome =
            outcome;

        HealthDamage =
            Mathf.Max(
                0f,
                healthDamage
            );

        SoulBarrierDamage =
            Mathf.Max(
                0f,
                soulBarrierDamage
            );

        HealthDepleted =
            healthDepleted;
    }
}