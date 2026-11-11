using UnityEngine;

public interface ICombatSkillHitEffect
{
    void Apply(CombatSkillHitContext context);
}

public abstract class CombatSkillHitEffectDefinition :
    GameDefinition
{
    public abstract ICombatSkillHitEffect CreateRuntimeEffect();
}
