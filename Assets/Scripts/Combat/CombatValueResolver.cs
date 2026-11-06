
using System.Collections.Generic;
using UnityEngine;

public static class CombatValueResolver
{
    public static float ResolveMultiplier(
        GameObject actor,
        CombatValueType valueType)
    {
        if (actor == null)
            return 1f;

        float totalPercent = 0f;

        StatusEffects statuses =
            actor.GetComponent<StatusEffects>();

        if (statuses != null)
        {
            foreach (ActiveStatusEffect active in statuses.ActiveEffects)
            {
                if (active == null || active.definition == null)
                    continue;

                AddModifiers(
                    active.definition.combatModifiers,
                    valueType,
                    Mathf.Max(0, active.stacks),
                    ref totalPercent);
            }
        }

        PlayerEquipment equipment =
            actor.GetComponent<PlayerEquipment>();

        if (equipment != null)
        {
            foreach (InventoryItemInstance item in equipment.GetEquippedItems())
            {
                if (item == null ||
                    item.IsEmpty ||
                    item.Definition == null)
                {
                    continue;
                }

                AddModifiers(
                    item.Definition.combatModifiers,
                    valueType,
                    1,
                    ref totalPercent);
            }
        }

        return Mathf.Max(
            0f,
            1f + totalPercent / 100f);
    }

    private static void AddModifiers(
        IReadOnlyList<CombatModifier> modifiers,
        CombatValueType valueType,
        int stacks,
        ref float totalPercent)
    {
        if (modifiers == null || stacks <= 0)
            return;

        foreach (CombatModifier modifier in modifiers)
        {
            if (modifier == null ||
                modifier.valueType != valueType)
            {
                continue;
            }

            totalPercent += modifier.percent * stacks;
        }
    }
}
