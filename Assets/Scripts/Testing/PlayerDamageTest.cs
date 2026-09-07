using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDamageTest : MonoBehaviour
{
    [SerializeField]
    private EntityResources resources;

    [Header("Test Damage")]
    [SerializeField] private float physicalDamage = 25f;
    [SerializeField] private float aetherDamage = 25f;
    [SerializeField] private float healAmount = 25f;
    [SerializeField] private float barrierRestoreAmount = 25f;
    [SerializeField]
    private float aetherHealAmount = 25f;

    [SerializeField]
    private float burnRecoveryAmount = 25f;

    private void Awake()
    {
        if (resources == null)
            resources = GetComponent<EntityResources>();
    }

    private void Update()
    {
        if (resources == null || !resources.IsInitialized)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            resources.TakeDamage(
                new DamageContext(
                    physicalDamage,
                    DamageType.Physical,
                    gameObject,
                    transform.position,
                    Vector3.zero
                )
            );
            PrintResources("Physical damage");
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            resources.TakeDamage(
                new DamageContext(
                    aetherDamage,
                    DamageType.Aether,
                    gameObject,
                    transform.position,
                    Vector3.zero
                )
            );
            PrintResources("Aether damage");
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            resources.HealHealth(healAmount);
            PrintResources("Heal health");
        }

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            resources.AddSoulBarrier(barrierRestoreAmount);
            PrintResources("Restore Soul Barrier");
        }

        if (Keyboard.current.digit5Key.wasPressedThisFrame)
        {
            float healed =
                resources.HealHealthWithAether(
                    aetherHealAmount
                );

            Debug.Log(
                $"Aether healing restored " +
                $"{healed:0.##} health.",
                this
            );

            PrintResources(
                "Aether heal"
            );
        }

        if (Keyboard.current.digit6Key.wasPressedThisFrame)
        {
            resources.RestoreAetherHealingBurn(
                burnRecoveryAmount
            );

            PrintResources(
                "Restore Aether Healing Burn"
            );
        }
    }

    private void PrintResources(string label)
    {
        Debug.Log(
            $"{label}: " +
            $"HP {Mathf.CeilToInt(resources.CurrentHealth)}/{Mathf.CeilToInt(resources.MaxHealth)}, " +
            $"Soul Barrier {Mathf.CeilToInt(resources.CurrentSoulBarrier)}/{Mathf.CeilToInt(resources.MaxSoulBarrier)}" +
            $"Aether Healing Burn " +
            $"{resources.CurrentAetherHealingBurn:0.##}/" +
            $"{resources.AetherHealingTolerance:0.##}, " +
            $"Healing Efficiency " +
            $"{resources.AetherHealingEfficiency:P0}",
            this
        );
    }
}