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
    [Range(0, 2)]
    public int handGripCount = 2;

    [Range(0, 1)]
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
            Mathf.Clamp(
                handGripCount,
                0,
                2
            );

        mouthGripCount =
            Mathf.Clamp(
                mouthGripCount,
                0,
                1
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