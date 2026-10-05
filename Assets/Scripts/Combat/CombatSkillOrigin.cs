using UnityEngine;

public abstract class CombatSkillOrigin
{
    public abstract string Name { get; }

    public abstract bool TryGetPose(
        out Pose pose);
}

public sealed class TransformCombatSkillOrigin :
    CombatSkillOrigin
{
    private readonly Transform point;

    public override string Name =>
        point != null
            ? point.name
            : "Missing";

    public TransformCombatSkillOrigin(
        Transform point)
    {
        this.point = point;
    }

    public override bool TryGetPose(
        out Pose pose)
    {
        pose = default;

        if (point == null)
            return false;

        pose =
            new Pose(
                point.position,
                point.rotation
            );

        return true;
    }
}