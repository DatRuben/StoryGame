using System;
using UnityEngine;

public enum GripType
{
    Hand,
    Mouth
}

[Serializable]
public class CharacterGripProfile
{
    [Min(0)]
    public int handGripCount = 2;

    [Min(0)]
    public int mouthGripCount = 0;

    public bool HasHandGrips =>
        handGripCount > 0;

    public bool HasMouthGrips =>
        mouthGripCount > 0;

    public int GetGripCount(
        GripType gripType)
    {
        switch (gripType)
        {
            case GripType.Mouth:
                return mouthGripCount;

            case GripType.Hand:
            default:
                return handGripCount;
        }
    }

    public void Clamp()
    {
        handGripCount =
            Mathf.Max(
                0,
                handGripCount
            );

        mouthGripCount =
            Mathf.Max(
                0,
                mouthGripCount
            );
    }

    public static CharacterGripProfile
        CreateHumanoidDefault()
    {
        return new CharacterGripProfile
        {
            handGripCount = 2,
            mouthGripCount = 0
        };
    }
}