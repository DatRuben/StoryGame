using System;
using UnityEngine;

[RequireComponent(typeof(EntityResources))]
public sealed class EntityCombatState :
    MonoBehaviour
{
    [SerializeField]
    [Min(0f)]
    private float combatExitDelay = 5f;

    private EntityResources resources;

    private float lastCombatActivityTime =
        float.NegativeInfinity;

    public bool IsInCombat { get; private set; }

    public event Action<bool>
        OnCombatStateChanged;

    private void Awake()
    {
        resources =
            GetComponent<EntityResources>();
    }

    private void OnEnable()
    {
        if (resources != null)
        {
            resources.OnDamageResolved +=
                HandleDamageReceived;
        }
    }

    private void OnDisable()
    {
        if (resources != null)
        {
            resources.OnDamageResolved -=
                HandleDamageReceived;
        }
    }

    private void Update()
    {
        if (!IsInCombat)
            return;

        if (Time.time <
            lastCombatActivityTime +
            combatExitDelay)
        {
            return;
        }

        SetInCombat(false);
    }

    public void MarkCombatActivity()
    {
        lastCombatActivityTime =
            Time.time;

        SetInCombat(true);
    }

    private void HandleDamageReceived(
        DamageResult result)
    {
        if (!result.DidDamage)
            return;

        MarkCombatActivity();
    }

    private void SetInCombat(bool value)
    {
        if (IsInCombat == value)
            return;

        IsInCombat = value;

        OnCombatStateChanged?.Invoke(
            IsInCombat
        );
    }
}