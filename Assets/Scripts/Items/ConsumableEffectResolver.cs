using UnityEngine;

public static class ConsumableEffectResolver
{
    public static bool TryApply(
        ConsumableEffect effect,
        GameObject user)
    {
        if (effect == null ||
            !effect.IsConfigured ||
            user == null)
        {
            return false;
        }

        switch (effect.type)
        {
            case ConsumableEffectType.RestoreHealth:
                return TryRestoreHealth(
                    effect,
                    user
                );

            case ConsumableEffectType.ApplyStatusEffect:
                return TryApplyStatusEffect(
                    effect,
                    user
                );

            default:
                return false;
        }
    }

    private static bool TryRestoreHealth(
        ConsumableEffect effect,
        GameObject user)
    {
        EntityResources resources =
            user.GetComponent<EntityResources>();

        if (resources == null ||
            !resources.IsInitialized)
        {
            return false;
        }

        float healthBefore =
            resources.CurrentHealth;

        resources.HealHealth(
            effect.amount
        );

        return resources.CurrentHealth >
            healthBefore;
    }

    private static bool TryApplyStatusEffect(
        ConsumableEffect effect,
        GameObject user)
    {
        StatusEffects statusEffects =
            user.GetComponent<StatusEffects>();

        if (statusEffects == null)
            return false;

        return statusEffects.ApplyEffect(
            effect.statusEffect
        );
    }
}