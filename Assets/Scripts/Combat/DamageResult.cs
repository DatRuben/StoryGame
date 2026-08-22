using UnityEngine;

public readonly struct DamageResult
{
    public DamageContext Context { get; }

    public GameObject Target { get; }

    public float HealthDamage { get; }
    public float SoulBarrierDamage { get; }

    public bool HealthDepleted { get; }

    public bool DidDamage =>
        HealthDamage > 0f ||
        SoulBarrierDamage > 0f;

    public DamageResult(
        DamageContext context,
        GameObject target,
        float healthDamage,
        float soulBarrierDamage,
        bool healthDepleted)
    {
        Context = context;

        Target = target;

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