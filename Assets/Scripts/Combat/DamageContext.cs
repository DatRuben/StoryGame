using UnityEngine;

public readonly struct DamageContext
{
    public float Amount { get; }
    public DamageType DamageType { get; }

    public GameObject Source { get; }

    public Vector3 HitPoint { get; }
    public Vector3 HitDirection { get; }

    public DamageContext(
        float amount,
        DamageType damageType,
        GameObject source,
        Vector3 hitPoint,
        Vector3 hitDirection)
    {
        Amount = amount;
        DamageType = damageType;

        Source = source;

        HitPoint = hitPoint;
        HitDirection = hitDirection;
    }
}