using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EntityResources))]
public sealed class PlayerDownedState :
    MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private PlayerInput playerInput;

    [SerializeField]
    private Rigidbody playerBody;

    [Header("Automatic Recovery")]

    [SerializeField]
    [Min(0f)]
    private float recoveryDelay = 10f;

    [SerializeField]
    [Range(0.01f, 1f)]
    private float recoveryHealthPercent = 1f;

    private EntityResources resources;
    private Coroutine recoveryRoutine;

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

        if (recoveryRoutine != null)
        {
            StopCoroutine(recoveryRoutine);
            recoveryRoutine = null;
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

        recoveryRoutine =
            StartCoroutine(
                RecoverAfterDelay()
            );
    }

    private IEnumerator RecoverAfterDelay()
    {
        yield return new WaitForSeconds(
            recoveryDelay
        );

        recoveryRoutine = null;

        TryRecover(
            resources.MaxHealth *
            recoveryHealthPercent
        );
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

        if (recoveryRoutine != null)
        {
            StopCoroutine(recoveryRoutine);
            recoveryRoutine = null;
        }

        IsDowned = false;

        if (playerInput != null)
        {
            playerInput.enabled = true;
        }

        OnDownedChanged?.Invoke(false);

        return true;
    }
}