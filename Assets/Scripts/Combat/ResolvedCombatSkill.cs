using System.Collections.Generic;
using UnityEngine;

public sealed class ResolvedCombatSkill
{
    public CombatSkillDefinition Definition
    {
        get;
    }

    public string SkillName
    {
        get;
    }

    public CombatSkillActionDefinition Action
    {
        get;
    }

    public float CastTime
    {
        get;
    }

    public float Cooldown
    {
        get;
    }

    public float AetherCost
    {
        get;
    }

    public CombatSkillOriginSource OriginSource
    {
        get;
    }

    public CharacterActionPointType
        CharacterActionPoint
    {
        get;
    }

    public string CustomActionPointId
    {
        get;
    }

    public CombatSkillHeldItemSource
        HeldItemSource
    {
        get;
    }

    private readonly List<
        ResolvedCombatSkillComponent>
        components =
            new List<
                ResolvedCombatSkillComponent>();

    public IReadOnlyList<
        ResolvedCombatSkillComponent>
        Components =>
            components;

    internal void AddComponent(
        ResolvedCombatSkillComponent component)
    {
        if (component == null)
            return;

        components.Add(
            component
        );
    }

    public bool TryGetComponent<T>(
        out T component)
        where T :
            ResolvedCombatSkillComponent
    {
        for (int i = 0;
             i < components.Count;
             i++)
        {
            if (components[i] is T match)
            {
                component =
                    match;

                return true;
            }
        }

        component = null;

        return false;
    }

    public bool HasComponent<T>()
        where T :
            ResolvedCombatSkillComponent
    {
        return TryGetComponent<T>(
            out _
        );
    }

    internal ResolvedCombatSkill(
        CombatSkillDefinition definition)
    {
        Definition =
            definition;

        SkillName =
            definition != null
                ? definition.skillName
                : "";

        Action =
            definition != null
                ? definition.action
                : null;

        CastTime =
            definition != null
                ? Mathf.Max(
                    0f,
                    definition.castTime
                )
                : 0f;

        Cooldown =
            definition != null
                ? Mathf.Max(
                    0f,
                    definition.cooldown
                )
                : 0f;

        AetherCost =
            definition != null
                ? Mathf.Max(
                    0f,
                    definition.aetherCost
                )
                : 0f;

        OriginSource =
            definition != null
                ? definition.originSource
                : CombatSkillOriginSource.None;

        CharacterActionPoint =
            definition != null
                ? definition.characterActionPoint
                : CharacterActionPointType.Center;

        CustomActionPointId =
            definition != null
                ? definition.customActionPointId
                : "";

        HeldItemSource =
            definition != null
                ? definition.heldItemSource
                : CombatSkillHeldItemSource.SkillSourceItem;
    }
}