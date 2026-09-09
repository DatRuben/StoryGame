using UnityEngine;

[RequireComponent(typeof(EntityResources))]
[RequireComponent(typeof(EntityCombatState))]
public sealed class AetherHealingBurnRecovery :
    MonoBehaviour
{
    [SerializeField]
    [Range(0f, 1f)]
    private float outOfCombatRecoveryPerSecond =
        0.05f;

    private EntityResources resources;
    private EntityCombatState combatState;

    private void Awake()
    {
        resources =
            GetComponent<EntityResources>();

        combatState =
            GetComponent<EntityCombatState>();
    }

    private void Update()
    {
        if (resources == null ||
            combatState == null ||
            !resources.IsInitialized ||
            combatState.IsInCombat ||
            resources.CurrentAetherHealingBurn <= 0f)
        {
            return;
        }

        float recoveryAmount =
            resources.AetherHealingTolerance *
            outOfCombatRecoveryPerSecond *
            Time.deltaTime;

        resources.RestoreAetherHealingBurn(
            recoveryAmount
        );
    }
}