using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class CombatHurtbox :
    MonoBehaviour,
    DamageReceiver
{
    private DamageReceiver damageReceiver;

    private void Awake()
    {
        ResolveDamageReceiver();
    }

    public DamageResult TakeDamage(
        DamageContext damage)
    {
        if (damageReceiver == null)
        {
            ResolveDamageReceiver();
        }

        if (damageReceiver == null)
        {
            return new DamageResult(
                damage,
                null,
                0f,
                0f,
                false
            );
        }

        return damageReceiver.TakeDamage(
            damage
        );
    }

    private void ResolveDamageReceiver()
    {
        damageReceiver = null;

        Transform current =
            transform.parent;

        while (current != null)
        {
            MonoBehaviour[] behaviours =
                current.GetComponents<
                    MonoBehaviour>();

            for (int i = 0;
                 i < behaviours.Length;
                 i++)
            {
                if (behaviours[i] is
                    DamageReceiver receiver)
                {
                    damageReceiver =
                        receiver;

                    return;
                }
            }

            current =
                current.parent;
        }

        Debug.LogWarning(
            "CombatHurtbox could not find a DamageReceiver in its parents.",
            this
        );
    }
}