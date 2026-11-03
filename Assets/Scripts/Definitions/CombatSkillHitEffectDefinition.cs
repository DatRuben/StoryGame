using UnityEngine;

public interface ICombatSkillHitEffect
{
    void Apply(CombatSkillHitContext context);
}

public abstract class CombatSkillHitEffectDefinition :
    ScriptableObject
{
    public abstract ICombatSkillHitEffect CreateRuntimeEffect();
}
