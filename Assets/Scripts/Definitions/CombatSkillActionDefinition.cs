using UnityEngine;

public abstract class CombatSkillActionDefinition :
    ScriptableObject
{
    public abstract bool TryCreateExecution(
        CombatSkillActionContext context,
        out CombatSkillExecution execution,
        out string error);
}