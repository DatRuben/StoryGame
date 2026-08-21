using UnityEngine;

[RequireComponent(typeof(EntityResources))]
public sealed class EntityResourceSetup :
    MonoBehaviour
{
    [Header("Starting Maximums")]

    [SerializeField]
    [Min(0f)]
    private float health = 100f;

    [SerializeField]
    [Min(0f)]
    private float soulBarrier;

    [SerializeField]
    [Min(0f)]
    private float stamina;

    [SerializeField]
    [Min(0f)]
    private float aether;

    private void Awake()
    {
        EntityResources resources =
            GetComponent<EntityResources>();

        if (resources == null ||
            resources.IsInitialized)
        {
            return;
        }

        resources.ApplyResourceMaximums(
            health,
            soulBarrier,
            stamina,
            aether,
            true
        );
    }

    private void OnValidate()
    {
        health =
            Mathf.Max(
                0f,
                health
            );

        soulBarrier =
            Mathf.Max(
                0f,
                soulBarrier
            );

        stamina =
            Mathf.Max(
                0f,
                stamina
            );

        aether =
            Mathf.Max(
                0f,
                aether
            );
    }
}