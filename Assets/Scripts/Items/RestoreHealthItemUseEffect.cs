using UnityEngine;

[CreateAssetMenu(
    menuName =
        "Game/Item Effects/Restore Health"
)]
public sealed class RestoreHealthItemUseEffect :
    ItemUseEffect
{
    [SerializeField]
    [Min(0f)]
    private float amount = 25f;

    public override bool TryApply(
        GameObject user)
    {
        if (user == null ||
            amount <= 0f)
        {
            return false;
        }

        EntityResources resources =
            user.GetComponent<EntityResources>();

        if (resources == null ||
            !resources.IsInitialized ||
            resources.CurrentHealth >=
                resources.MaxHealth)
        {
            return false;
        }

        float healthBefore =
            resources.CurrentHealth;

        resources.HealHealth(
            amount
        );

        return
            resources.CurrentHealth >
            healthBefore;
    }
}