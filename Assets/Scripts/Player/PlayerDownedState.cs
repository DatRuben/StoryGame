using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EntityResources))]
[RequireComponent(typeof(PlayerGameplayState))]
public sealed class PlayerDownedState :
    MonoBehaviour
{
    [Header("References")]

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

    private PlayerGameplayState gameplayState;

    private const PlayerGameplayCapability
        DownedRestrictions =
            PlayerGameplayCapability.Movement |
            PlayerGameplayCapability.Combat |
            PlayerGameplayCapability.Inventory |
            PlayerGameplayCapability.WorldInteraction |
            PlayerGameplayCapability.ItemHandling;

    public bool IsDowned { get; private set; }

    public event Action<bool> OnDownedChanged;

    private void Awake()
    {
        resources =
            GetComponent<EntityResources>();

        gameplayState =
            GetComponent<PlayerGameplayState>();

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

        if (IsDowned)
        {
            IsDowned = false;

            if (gameplayState != null)
            {
                gameplayState.ClearRestriction(
                    this
                );
            }
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

        gameplayState.SetRestriction(
            this,
            DownedRestrictions
        );

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

        gameplayState.ClearRestriction(
            this
        );

        OnDownedChanged?.Invoke(false);

        return true;
    }
}