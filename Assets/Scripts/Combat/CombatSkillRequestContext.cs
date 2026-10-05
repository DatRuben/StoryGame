using UnityEngine;

public sealed class CombatSkillRequestContext
{
    public GameObject Actor { get; }
    public CombatSkillDefinition Skill { get; }
    public InventoryItemInstance SourceItem { get; }

    public CombatSkillRequestContext(
        GameObject actor,
        CombatSkillDefinition skill,
        InventoryItemInstance sourceItem = null)
    {
        Actor = actor;
        Skill = skill;
        SourceItem = sourceItem;
    }
}