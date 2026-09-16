using UnityEngine;

public sealed class DefeatTargetObjective :
    GameObjective
{
    [SerializeField]
    private EntityResources target;

    private void OnEnable()
    {
        Subscribe();

        if (target != null &&
            target.IsHealthDepleted)
        {
            Complete();
        }
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (target == null)
            return;

        target.OnHealthDepleted -=
            HandleTargetHealthDepleted;

        target.OnHealthDepleted +=
            HandleTargetHealthDepleted;
    }

    private void Unsubscribe()
    {
        if (target == null)
            return;

        target.OnHealthDepleted -=
            HandleTargetHealthDepleted;
    }

    private void HandleTargetHealthDepleted(
        DamageContext? context)
    {
        Complete();
    }
}