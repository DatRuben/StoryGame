using System;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffects : MonoBehaviour
{
    [Header("Initial Effects")]
    [Tooltip(
        "Effects active when this object starts. " +
        "Useful for scenarios, enemies, and testing."
    )]
    [SerializeField]
    private List<StatusEffectDefinition>
        initialEffects =
            new List<StatusEffectDefinition>();

    private readonly List<ActiveStatusEffect>
        activeEffects =
            new List<ActiveStatusEffect>();

    public IReadOnlyList<ActiveStatusEffect>
        ActiveEffects =>
            activeEffects;

    public event Action EffectsChanged;

    private void Start()
    {
        bool changed = false;

        foreach (StatusEffectDefinition definition
                 in initialEffects)
        {
            if (ApplyEffect(
                definition,
                false))
            {
                changed = true;
            }
        }

        if (changed)
            EffectsChanged?.Invoke();
    }

    private void Update()
    {
        bool changed = false;

        for (int i = activeEffects.Count - 1;
             i >= 0;
             i--)
        {
            ActiveStatusEffect activeEffect =
                activeEffects[i];

            if (activeEffect == null ||
                activeEffect.definition == null)
            {
                activeEffects.RemoveAt(i);
                changed = true;
                continue;
            }

            activeEffect.Tick(
                Time.deltaTime
            );

            if (!activeEffect.IsExpired)
                continue;

            activeEffects.RemoveAt(i);
            changed = true;
        }

        if (changed)
            EffectsChanged?.Invoke();
    }

    public bool ApplyEffect(
        StatusEffectDefinition definition)
    {
        return ApplyEffect(
            definition,
            true
        );
    }

    private bool ApplyEffect(
        StatusEffectDefinition definition,
        bool notify)
    {
        if (definition == null ||
            string.IsNullOrWhiteSpace(
                definition.effectId))
        {
            Debug.LogWarning(
                "StatusEffects cannot apply an invalid status-effect definition.",
                this
            );

            return false;
        }

        EntityClassification classification =
            GetComponent<EntityClassification>();

        if (!definition.CanApplyTo(
                classification))
        {
            return false;
        }

        int existingIndex =
            FindEffectIndex(
                definition.effectId
            );

        if (existingIndex < 0)
        {
            activeEffects.Add(
                new ActiveStatusEffect(
                    definition
                )
            );

            if (notify)
                EffectsChanged?.Invoke();

            return true;
        }

        ActiveStatusEffect existing =
            activeEffects[existingIndex];

        switch (definition.stacking)
        {
            case StatusEffectStacking.AddStack:
                existing.definition =
                    definition;

                existing.AddStack();
                existing.RefreshDuration();
                break;

            case StatusEffectStacking.Replace:
                activeEffects[existingIndex] =
                    new ActiveStatusEffect(
                        definition
                    );
                break;

            case StatusEffectStacking.RefreshDuration:
            default:
                existing.definition =
                    definition;

                existing.RefreshDuration();
                break;
        }

        if (notify)
            EffectsChanged?.Invoke();

        return true;
    }

    public bool RemoveEffect(
        StatusEffectDefinition definition)
    {
        if (definition == null)
            return false;

        return RemoveEffect(
            definition.effectId
        );
    }

    public bool RemoveEffect(
        string effectId)
    {
        int index =
            FindEffectIndex(effectId);

        if (index < 0)
            return false;

        activeEffects.RemoveAt(index);
        EffectsChanged?.Invoke();

        return true;
    }

    public bool HasEffect(
        StatusEffectDefinition definition)
    {
        return definition != null &&
               HasEffect(
                   definition.effectId
               );
    }

    public bool HasEffect(
        string effectId)
    {
        return FindEffectIndex(effectId) >= 0;
    }

    public void ClearEffects()
    {
        if (activeEffects.Count == 0)
            return;

        activeEffects.Clear();
        EffectsChanged?.Invoke();
    }

    public CharacterAttributeModifiers
        GetAttributeModifiers()
    {
        CharacterAttributeModifiers total =
            CharacterAttributeModifiers.CreateZero();

        foreach (ActiveStatusEffect activeEffect
                 in activeEffects)
        {
            if (activeEffect == null ||
                activeEffect.definition == null)
            {
                continue;
            }

            CharacterAttributeModifiers
                stackedModifiers =
                    CharacterAttributeModifiers
                        .Multiply(
                            activeEffect.definition
                                .attributeModifiers,
                            activeEffect.stacks
                        );

            total =
                CharacterAttributeModifiers.Add(
                    total,
                    stackedModifiers
                );
        }

        return total;
    }

    private int FindEffectIndex(
        string effectId)
    {
        if (string.IsNullOrWhiteSpace(effectId))
            return -1;

        for (int i = 0;
             i < activeEffects.Count;
             i++)
        {
            ActiveStatusEffect activeEffect =
                activeEffects[i];

            if (activeEffect?.definition == null)
                continue;

            if (string.Equals(
                activeEffect.definition.effectId,
                effectId,
                StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }
}