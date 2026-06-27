using System;
using UnityEngine;

public class PlayerResources : MonoBehaviour
{
    [Header("Health")]
    private float maxHealth;
    private float currentHealth;


    [Header("Stamina")]
    private float maxStamina;
    private float currentStamina;

    [Header("Mana")]
    private float maxMana;
    private float currentMana;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float HealthPercent => GetPercent(currentHealth, maxHealth);

    public float MaxStamina => maxStamina;
    public float CurrentStamina => currentStamina;
    public float StaminaPercent => GetPercent(currentStamina, maxStamina);

    public float MaxMana => maxMana;
    public float CurrentMana => currentMana;
    public float ManaPercent => GetPercent(currentMana, maxMana);

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
        maxStamina = Mathf.Max(1f, finalStats.maxStamina);
        maxMana = Mathf.Max(1f, finalStats.maxMana);

        if (refillResources)
        {
            currentHealth = maxHealth;
            currentStamina = maxStamina;
            currentMana = maxMana;
        }
        else
        {
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
            currentMana = Mathf.Clamp(currentMana, 0f, maxMana);
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

    public void SetMana(float value)
    {
        currentMana = Mathf.Clamp(value, 0f, maxMana);
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

    public void AddMana(float amount)
    {
        SetMana(currentMana + amount);
    }

    private float GetPercent(float current, float max)
    {
        if (max <= 0f)
            return 0f;

        return Mathf.Clamp01(current / max);
    }
}