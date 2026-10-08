using UnityEngine;

public sealed class CombatSkillActionContext
{
    public GameObject Actor { get; }

    public ResolvedCombatSkill Skill { get; }

    public InventoryItemInstance SourceItem { get; }

    public CombatSkillOrigin Origin { get; }

    public CombatSkillActionContext(
        GameObject actor,
        ResolvedCombatSkill skill,
        InventoryItemInstance sourceItem,
        CombatSkillOrigin origin)
    {
        Actor = actor;
        Skill = skill;
        SourceItem = sourceItem;
        Origin = origin;
    }
}