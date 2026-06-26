using System;
using UnityEngine;

public class PlayerResources : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float currentStamina = 100f;

    [Header("Mana")]
    [SerializeField] private float maxMana = 100f;
    [SerializeField] private float currentMana = 100f;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float HealthPercent => GetPercent(currentHealth, maxHealth);

    public float MaxStamina => maxStamina;
    public float CurrentStamina => currentStamina;
    public float StaminaPercent => GetPercent(currentStamina, maxStamina);

    public float MaxMana => maxMana;
    public float CurrentMana => currentMana;
    public float ManaPercent => GetPercent(currentMana, maxMana);

    public event Action OnResourcesChanged;

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        maxStamina = Mathf.Max(1f, maxStamina);
        maxMana = Mathf.Max(1f, maxMana);

        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        currentMana = Mathf.Clamp(currentMana, 0f, maxMana);
    }

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

        OnResourcesChanged?.Invoke();
    }

    public void SetHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0f, maxHealth);
        OnResourcesChanged?.Invoke();
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