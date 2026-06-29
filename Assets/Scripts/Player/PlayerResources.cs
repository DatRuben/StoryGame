using System;
using UnityEngine;

public class PlayerResources : MonoBehaviour
{
    [Header("Health")]
    private float maxHealth;
    private float currentHealth;

    [Header("SoulBarrier")]
    private float maxSoulBarrier;
    private float currentSoulBarrier;

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

    public event Action OnResourcesChanged;

    public void ApplyFinalStats(
    FinalCharacterStats finalStats,
    bool refillResources = true)
    {
        if (finalStats == null)
        {
            Debug.LogWarning(
                "PlayerResources could not apply final stats because FinalCharacterStats is missing.",
                this
            );

            return;
        }

        maxHealth = Mathf.Max(1f, finalStats.maxHealth);
        maxSoulBarrier = Mathf.Max(1f, finalStats.maxSoulBarrier);
        maxStamina = Mathf.Max(1f, finalStats.maxStamina);
        maxAether = Mathf.Max(1f, finalStats.maxAether);

        if (refillResources)
        {
            currentHealth = maxHealth;
            currentSoulBarrier = maxSoulBarrier;
            currentStamina = maxStamina;
            currentAether = maxAether;
        }
        else
        {
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
            currentSoulBarrier = Mathf.Clamp(currentSoulBarrier, 0f, maxSoulBarrier);
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
            currentAether = Mathf.Clamp(currentAether, 0f, maxAether);
        }

        IsInitialized = true;
        OnResourcesChanged?.Invoke();
    }

    public void SetHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0f, maxHealth);
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
}