using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDamageTest : MonoBehaviour
{
    [SerializeField] private PlayerResources resources;

    [Header("Test Damage")]
    [SerializeField] private float physicalDamage = 25f;
    [SerializeField] private float aetherDamage = 25f;

    private void Awake()
    {
        if (resources == null)
            resources = GetComponent<PlayerResources>();
    }

    private void Update()
    {
        if (resources == null || !resources.IsInitialized)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            resources.TakeDamage(physicalDamage, DamageType.Physical);
            PrintResources("Physical damage");
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            resources.TakeDamage(aetherDamage, DamageType.Aether);
            PrintResources("Aether damage");
        }
    }

    private void PrintResources(string label)
    {
        Debug.Log(
            $"{label}: " +
            $"HP {Mathf.CeilToInt(resources.CurrentHealth)}/{Mathf.CeilToInt(resources.MaxHealth)}, " +
            $"Soul Barrier {Mathf.CeilToInt(resources.CurrentSoulBarrier)}/{Mathf.CeilToInt(resources.MaxSoulBarrier)}",
            this
        );
    }
}