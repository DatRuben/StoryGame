using System;
using UnityEngine;

public enum GripType
{
    Hand,
    Mouth
}

public enum ConventionalWeaponMode
{
    Humanoid,
    MouthOnly,
    MouthOrOneHand
}

[Serializable]
public class CharacterGripProfile
{
    [Range(0, 2)]
    public int handGripCount = 2;

    [Range(0, 1)]
    public int mouthGripCount;

    [Header("Operating Capability")]
    public bool canOperateWithHands = true;

    public bool canOperateWithMouth;

    public bool HasHandGrips =>
        handGripCount > 0;

    public bool HasMouthGrips =>
        mouthGripCount > 0;

    public ConventionalWeaponMode weaponMode =
    ConventionalWeaponMode.Humanoid;

    public int GetGripCount(
        GripType gripType)
    {
        return gripType ==
               GripType.Mouth
            ? mouthGripCount
            : handGripCount;
    }

    public bool CanOperateWith(
        GripType gripType)
    {
        if (gripType ==
            GripType.Mouth)
        {
            return mouthGripCount > 0 &&
                   canOperateWithMouth;
        }

        return handGripCount > 0 &&
               canOperateWithHands;
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

        if (handGripCount == 0)
            canOperateWithHands = false;

        if (mouthGripCount == 0)
            canOperateWithMouth = false;
    }

    public static CharacterGripProfile
        CreateHumanoidDefault()
    {
        return new CharacterGripProfile
        {
            handGripCount = 2,
            mouthGripCount = 0,
            canOperateWithHands = true,
            canOperateWithMouth = false
        };
    }
}