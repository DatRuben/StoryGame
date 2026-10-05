using UnityEngine;

public sealed class CombatSkillActionContext
{
    public GameObject Actor { get; }

    public CombatSkillDefinition Skill { get; }

    public InventoryItemInstance SourceItem { get; }

    public CombatSkillOrigin Origin { get; }

    public CombatSkillActionContext(
        GameObject actor,
        CombatSkillDefinition skill,
        InventoryItemInstance sourceItem,
        CombatSkillOrigin origin)
    {
        Actor = actor;
        Skill = skill;
        SourceItem = sourceItem;
        Origin = origin;
    }
}