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