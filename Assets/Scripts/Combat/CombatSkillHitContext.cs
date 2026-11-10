using UnityEngine;

public readonly struct CombatSkillHitContext
{
    public CombatSkillActionContext Action { get; }
    public CombatHurtbox Target { get; }
    public Vector3 Point { get; }
    public Vector3 Direction { get; }
    public float Distance { get; }
    public float ReferenceDistance { get; }

    public float NormalizedDistance =>
        ReferenceDistance > 0f
            ? Mathf.Clamp01(Distance / ReferenceDistance)
            : 0f;

    public CombatSkillHitContext(
        CombatSkillActionContext action,
        CombatHurtbox target,
        Vector3 point,
        Vector3 direction,
        float distance,
        float referenceDistance)
    {
        Action = action;
        Target = target;
        Point = point;
        Direction = direction;
        Distance = Mathf.Max(0f, distance);
        ReferenceDistance = Mathf.Max(0f, referenceDistance);
    }
}
