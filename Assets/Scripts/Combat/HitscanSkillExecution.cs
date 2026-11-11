using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class HitscanSkillExecution :
    CombatSkillExecution
{
    private readonly CombatSkillActionContext context;
    private readonly float maximumRange;
    private readonly int collisionLayers;
    private readonly IReadOnlyList<ICombatSkillHitEffect> effects;
    private readonly HitscanBeamVisual beamVisualPrefab;

    public HitscanSkillExecution(
        CombatSkillActionContext context,
        float maximumRange,
        int collisionLayers,
        IReadOnlyList<ICombatSkillHitEffect> effects,
        HitscanBeamVisual beamVisualPrefab)
    {
        this.context = context;
        this.maximumRange = maximumRange;
        this.collisionLayers = collisionLayers;
        this.effects = effects;
        this.beamVisualPrefab = beamVisualPrefab;
    }

    public override bool TryBegin(out string error)
    {
        error = "";

        if (context.Actor == null ||
            context.Origin == null ||
            !context.Origin.TryGetPose(out Pose pose))
        {
            error = "Hitscan origin is unavailable.";
            return false;
        }

        Fire(pose);
        IsComplete = true;

        return true;
    }

    public override void Tick(float deltaTime)
    {
    }

    private void Fire(Pose pose)
    {
        Vector3 origin = pose.position;
        Vector3 direction = pose.rotation * Vector3.forward;

        float effectiveRange = Mathf.Max(
            0f,
            maximumRange *
            CombatValueResolver.ResolveMultiplier(
                context.Actor,
                CombatValueType.AttackRange));

        float impactDistance = effectiveRange;

        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            direction,
            effectiveRange,
            collisionLayers,
            QueryTriggerInteraction.Collide);

        Array.Sort(
            hits,
            (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            Collider collider = hit.collider;

            if (collider == null)
                continue;

            Transform targetTransform = collider.transform;

            if (targetTransform == context.Actor.transform ||
                targetTransform.IsChildOf(
                    context.Actor.transform))
            {
                continue;
            }

            CombatHurtbox hurtbox =
                ResolveHurtbox(collider);

            if (collider.isTrigger && hurtbox == null)
                continue;

            impactDistance = hit.distance;

            if (hurtbox != null)
            {
                CombatSkillHitContext hitContext =
                    new CombatSkillHitContext(
                        context,
                        hurtbox,
                        hit.point,
                        direction,
                        hit.distance,
                        maximumRange);

                foreach (ICombatSkillHitEffect effect in effects)
                {
                    effect.Apply(hitContext);
                }
            }

            break;
        }

        if (beamVisualPrefab != null)
        {
            HitscanBeamVisual beam =
                Object.Instantiate(beamVisualPrefab);

            beam.Show(
                origin,
                origin + direction * impactDistance
            );
        }
    }

    private static CombatHurtbox ResolveHurtbox(
        Collider collider)
    {
        CombatHurtbox hurtbox =
            collider.GetComponentInParent<CombatHurtbox>();

        if (hurtbox != null)
            return hurtbox;

        EntityResources resources =
            collider.GetComponentInParent<EntityResources>();

        if (resources == null)
            return null;

        return resources.GetComponentInChildren<
            CombatHurtbox>(true);
    }
}
