using System;
using UnityEngine;

[RequireComponent(typeof(EntityResources))]
public sealed class PlayerDownedState :
    MonoBehaviour
{
    [SerializeField]
    private PlayerInput playerInput;

    [SerializeField]
    private Rigidbody playerBody;

    private EntityResources resources;

    public bool IsDowned { get; private set; }

    public event Action<bool> OnDownedChanged;

    private void Awake()
    {
        resources =
            GetComponent<EntityResources>();

        if (playerInput == null)
        {
            playerInput =
                GetComponent<PlayerInput>();
        }

        if (playerBody == null)
        {
            playerBody =
                GetComponent<Rigidbody>();
        }
    }

    private void OnEnable()
    {
        if (resources == null)
        {
            resources =
                GetComponent<EntityResources>();
        }

        resources.OnHealthDepleted +=
            HandleHealthDepleted;

        if (resources.IsHealthDepleted)
        {
            EnterDowned();
        }
    }

    private void OnDisable()
    {
        if (resources != null)
        {
            resources.OnHealthDepleted -=
                HandleHealthDepleted;
        }
    }

    private void HandleHealthDepleted(
        DamageContext? damage)
    {
        EnterDowned();
    }

    public void EnterDowned()
    {
        if (IsDowned)
            return;

        IsDowned = true;

        if (playerInput != null)
        {
            playerInput.enabled = false;
        }

        if (playerBody != null)
        {
            playerBody.linearVelocity =
                new Vector3(
                    0f,
                    playerBody.linearVelocity.y,
                    0f
                );

            playerBody.angularVelocity =
                Vector3.zero;
        }

        OnDownedChanged?.Invoke(true);
    }

    public bool TryRecover(
        float restoredHealth)
    {
        if (!IsDowned ||
            restoredHealth <= 0f)
        {
            return false;
        }

        resources.SetHealth(restoredHealth);

        if (resources.IsHealthDepleted)
            return false;

        IsDowned = false;

        if (playerInput != null)
        {
            playerInput.enabled = true;
        }

        OnDownedChanged?.Invoke(false);

        return true;
    }
}