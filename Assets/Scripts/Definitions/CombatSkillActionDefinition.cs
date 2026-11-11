using UnityEngine;

public abstract class CombatSkillActionDefinition :
    GameDefinition
{
    public abstract bool TryCreateExecution(
        CombatSkillActionContext context,
        out CombatSkillExecution execution,
        out string error);
}
