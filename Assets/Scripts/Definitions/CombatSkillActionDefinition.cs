using UnityEngine;

public abstract class CombatSkillActionDefinition :
    ScriptableObject
{
    public virtual void BuildResolvedComponents(
        CombatSkillRequestContext context,
        ResolvedCombatSkill skill)
    {
    }

    public abstract bool TryCreateExecution(
        CombatSkillActionContext context,
        out CombatSkillExecution execution,
        out string error);
}