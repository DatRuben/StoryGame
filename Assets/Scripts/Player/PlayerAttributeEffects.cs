using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttributeEffects : MonoBehaviour
{
    private readonly Dictionary<
        string,
        CharacterAttributeModifiers
    > modifiersBySource =
        new Dictionary<
            string,
            CharacterAttributeModifiers
        >(
            StringComparer.OrdinalIgnoreCase
        );

    public event Action ModifiersChanged;

    public bool SetModifier(
        string sourceId,
        CharacterAttributeModifiers modifiers)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
        {
            Debug.LogWarning(
                "PlayerAttributeEffects cannot set a modifier without a source ID.",
                this
            );

            return false;
        }

        if (modifiers == null ||
            !modifiers.HasAny())
        {
            return RemoveModifier(sourceId);
        }

        modifiersBySource[sourceId] =
            CharacterAttributeModifiers.Copy(
                modifiers
            );

        ModifiersChanged?.Invoke();
        return true;
    }

    public bool RemoveModifier(
        string sourceId)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
            return false;

        if (!modifiersBySource.Remove(sourceId))
            return false;

        ModifiersChanged?.Invoke();
        return true;
    }

    public void ClearModifiers()
    {
        if (modifiersBySource.Count == 0)
            return;

        modifiersBySource.Clear();
        ModifiersChanged?.Invoke();
    }

    public bool HasModifier(
        string sourceId)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
            return false;

        return modifiersBySource.ContainsKey(
            sourceId
        );
    }

    public CharacterAttributeModifiers
        GetTotalModifiers()
    {
        CharacterAttributeModifiers total =
            CharacterAttributeModifiers.CreateZero();

        foreach (
            CharacterAttributeModifiers modifiers
            in modifiersBySource.Values)
        {
            total =
                CharacterAttributeModifiers.Add(
                    total,
                    modifiers
                );
        }

        return total;
    }
}
