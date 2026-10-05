using UnityEngine;

public interface ICombatSkillOrigin
{
    string Name { get; }

    bool TryGetPose(
        out Pose pose);
}

public sealed class TransformCombatSkillOrigin :
    ICombatSkillOrigin
{
    private readonly Transform point;

    public string Name =>
        point != null
            ? point.name
            : "Missing";

    public TransformCombatSkillOrigin(
        Transform point)
    {
        this.point =
            point;
    }

    public bool TryGetPose(
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