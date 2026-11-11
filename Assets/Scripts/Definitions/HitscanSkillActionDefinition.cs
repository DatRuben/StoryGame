using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    menuName = "Game/Combat/Actions/Hitscan")]
public sealed class HitscanSkillActionDefinition :
    CombatSkillActionDefinition
{
    [Header("Delivery")]
    [SerializeField, Min(0.1f)]
    private float maximumRange = 40f;

    [SerializeField]
    private LayerMask collisionLayers = ~0;

    [Header("Hit Effects")]
    [SerializeField]
    private List<CombatSkillHitEffectDefinition> hitEffects =
        new List<CombatSkillHitEffectDefinition>();

    [Header("Visual Effect")]
    [SerializeField]
    private HitscanBeamVisual beamVisualPrefab;

    public override bool TryCreateExecution(
        CombatSkillActionContext context,
        out CombatSkillExecution execution,
        out string error)
    {
        execution = null;
        error = "";

        if (context == null ||
            context.Actor == null ||
            context.Origin == null)
        {
            error = "Hitscan action requires an actor and origin.";
            return false;
        }

        if (hitEffects == null || hitEffects.Count == 0)
        {
            error = "Hitscan action has no hit effects.";
            return false;
        }

        List<ICombatSkillHitEffect> resolvedEffects =
            new List<ICombatSkillHitEffect>();

        foreach (CombatSkillHitEffectDefinition effect in
                 hitEffects)
        {
            if (effect == null)
            {
                error = "Hitscan action contains a missing effect.";
                return false;
            }

            ICombatSkillHitEffect resolved =
                effect.CreateRuntimeEffect();

            if (resolved == null)
            {
                error = $"Could not resolve effect '{effect.name}'.";
                return false;
            }

            resolvedEffects.Add(resolved);
        }

        execution = new HitscanSkillExecution(
            context,
            Mathf.Max(0.1f, maximumRange),
            collisionLayers.value,
            resolvedEffects,
            beamVisualPrefab);

        return true;
    }
}
