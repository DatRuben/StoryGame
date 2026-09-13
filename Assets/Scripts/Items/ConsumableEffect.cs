using System;
using UnityEngine;

public enum ConsumableEffectType
{
    RestoreHealth,
    ApplyStatusEffect
}

[Serializable]
public class ConsumableEffect
{
    public ConsumableEffectType type;

    [Min(0f)]
    public float amount;

    public StatusEffectDefinition statusEffect;

    public bool IsConfigured
    {
        get
        {
            switch (type)
            {
                case ConsumableEffectType.RestoreHealth:
                    return amount > 0f;

                case ConsumableEffectType.ApplyStatusEffect:
                    return statusEffect != null;

                default:
                    return false;
            }
        }
    }
}