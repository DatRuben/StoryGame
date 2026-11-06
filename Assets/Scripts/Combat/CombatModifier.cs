
using System;

public enum CombatValueType
{
    CastSpeed,
    AttackRange,
    Damage,
    SummonDamage
}

[Serializable]
public sealed class CombatModifier
{
    public CombatValueType valueType;
    public float percent;
}
