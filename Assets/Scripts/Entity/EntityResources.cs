using System;
using UnityEngine;

public class EntityResources :
    MonoBehaviour,
    DamageReceiver
{
    [Header("Health")]
    private float maxHealth;
    private float currentHealth;

    [Header("Soul Barrier")]
    private float maxSoulBarrier;
    private float currentSoulBarrier;

    [Header("Damage Tuning")]
    [SerializeField] private float maxAetherBodyDamageReduction = 0.5f;

    [Header("Stamina")]
    private float maxStamina;
    private float currentStamina;

    [Header("Aether")]
    private float maxAether;
    private float currentAether;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float HealthPercent => GetPercent(currentHealth, maxHealth);

    public float MaxStamina => maxStamina;
    public float CurrentStamina => currentStamina;
    public float StaminaPercent => GetPercent(currentStamina, maxStamina);

    public float MaxAether => maxAether;
    public float CurrentAether => currentAether;
    public float AetherPercent => GetPercent(currentAether, maxAether);
    public float MaxSoulBarrier => maxSoulBarrier;
    public float CurrentSoulBarrier => currentSoulBarrier;

    public float SoulBarrierPercent =>
        maxSoulBarrier > 0f
            ? currentSoulBarrier / maxSoulBarrier
            : 0f;

    public bool IsInitialized { get; private set; }

    public bool IsHealthDepleted =>
        IsInitialized &&
        maxHealth > 0f &&
        currentHealth <= 0f;

    public event Action OnResourcesChanged;

    public event Action<DamageResult>
        OnDamageResolved;

    public event Action<DamageContext?>
        OnHealthDepleted;

    public void ApplyFinalStats(
        FinalCharacterStats finalStats,
        bool refillResources = true)
    {
        if (finalStats == null)
        {
            Debug.LogWarning(
                "EntityResources could not apply final stats because FinalCharacterStats is missing.",
                this
            );

            return;
        }

        ApplyResourceMaximums(
            finalStats.maxHealth,
            finalStats.maxSoulBarrier,
            finalStats.maxStamina,
            finalStats.maxAether,
            refillResources
        );
    }

    public void SetHealth(
        float value)
    {
        bool wasHealthDepleted =
            IsHealthDepleted;

        currentHealth =
            Mathf.Clamp(
                value,
                0f,
                maxHealth
            );

        OnResourcesChanged?.Invoke();

        if (!wasHealthDepleted &&
            IsHealthDepleted)
        {
            OnHealthDepleted?.Invoke(
                null
            );
        }
    }

    public void SetSoulBarrier(float value)
    {
        currentSoulBarrier =
            Mathf.Clamp(
                value,
                0f,
                maxSoulBarrier
            );

        OnResourcesChanged?.Invoke();
    }

    public DamageResult TakeDamage(
        DamageContext damage)
    {
        bool wasHealthDepleted =
            IsHealthDepleted;

        float healthBefore =
            currentHealth;

        float soulBarrierBefore =
            currentSoulBarrier;

        float amount =
            Mathf.Max(
                0f,
                damage.Amount
            );

        switch (damage.DamageType)
        {
            case DamageType.Physical:
                DamageHealth(amount);
                break;

            case DamageType.Aether:
                DamageAether(amount);
                break;
        }

        float healthDamage =
            Mathf.Max(
                0f,
                healthBefore -
                currentHealth
            );

        float soulBarrierDamage =
            Mathf.Max(
                0f,
                soulBarrierBefore -
                currentSoulBarrier
            );

        bool healthDepleted =
            !wasHealthDepleted &&
            IsHealthDepleted;

        DamageResult result =
            new DamageResult(
                damage,
                gameObject,
                healthDamage,
                soulBarrierDamage,
                healthDepleted
            );

        OnResourcesChanged?.Invoke();

        OnDamageResolved?.Invoke(
            result
        );

        if (healthDepleted)
        {
            OnHealthDepleted?.Invoke(
                damage
            );
        }

        return result;
    }

    public void HealHealth(float amount)
    {
        amount = Mathf.Max(0f, amount);

        currentHealth =
            Mathf.Clamp(
                currentHealth + amount,
                0f,
                maxHealth
            );

        OnResourcesChanged?.Invoke();
    }

    public bool CanSpendStamina(float amount)
    {
        if (amount <= 0f)
            return true;

        return currentStamina >= amount;
    }

    public bool SpendStamina(float amount)
    {
        if (amount <= 0f)
            return true;

        if (!CanSpendStamina(amount))
            return false;

        SetStamina(currentStamina - amount);
        return true;
    }

    public void SetStamina(float value)
    {
        currentStamina = Mathf.Clamp(value, 0f, maxStamina);
        OnResourcesChanged?.Invoke();
    }

    public void SetAether(float value)
    {
        currentAether = Mathf.Clamp(value, 0f, maxAether);
        OnResourcesChanged?.Invoke();
    }

    public void AddHealth(float amount)
    {
        SetHealth(currentHealth + amount);
    }

    public void AddSoulBarrier(float amount)
    {
        SetSoulBarrier(currentSoulBarrier + amount);
    }

    public void AddStamina(float amount)
    {
        SetStamina(currentStamina + amount);
    }

    public void AddAether(float amount)
    {
        SetAether(currentAether + amount);
    }

    private float GetPercent(float current, float max)
    {
        if (max <= 0f)
            return 0f;

        return Mathf.Clamp01(current / max);
    }

    private void DamageHealth(float amount)
    {
        currentHealth =
            Mathf.Clamp(
                currentHealth - amount,
                0f,
                maxHealth
            );
    }

    private void DamageAether(float amount)
    {
        float barrierPercent =
            GetPercent(
                currentSoulBarrier,
                maxSoulBarrier
            );

        float bodyDamageReduction =
            Mathf.Lerp(
                0f,
                maxAetherBodyDamageReduction,
                barrierPercent
            );

        float bodyDamage =
            amount * (1f - bodyDamageReduction);

        float barrierDamage = amount;

        currentSoulBarrier =
            Mathf.Clamp(
                currentSoulBarrier - barrierDamage,
                0f,
                maxSoulBarrier
            );

        currentHealth =
            Mathf.Clamp(
                currentHealth - bodyDamage,
                0f,
                maxHealth
            );
    }

    public void ApplyResourceMaximums(
        float health,
        float soulBarrier,
        float stamina,
        float aether,
        bool refillResources = true)
    {
        maxHealth =
            Mathf.Max(
                0f,
                health
            );

        maxSoulBarrier =
            Mathf.Max(
                0f,
                soulBarrier
            );

        maxStamina =
            Mathf.Max(
                0f,
                stamina
            );

        maxAether =
            Mathf.Max(
                0f,
                aether
            );

        if (refillResources)
        {
            currentHealth =
                maxHealth;

            currentSoulBarrier =
                maxSoulBarrier;

            currentStamina =
                maxStamina;

            currentAether =
                maxAether;
        }
        else
        {
            currentHealth =
                Mathf.Clamp(
                    currentHealth,
                    0f,
                    maxHealth
                );

            currentSoulBarrier =
                Mathf.Clamp(
                    currentSoulBarrier,
                    0f,
                    maxSoulBarrier
                );

            currentStamina =
                Mathf.Clamp(
                    currentStamina,
                    0f,
                    maxStamina
                );

            currentAether =
                Mathf.Clamp(
                    currentAether,
                    0f,
                    maxAether
                );
        }

        IsInitialized = true;

        OnResourcesChanged?.Invoke();
    }
}