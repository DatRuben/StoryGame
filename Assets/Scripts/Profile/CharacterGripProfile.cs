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
    [Range(1, 2)]
    public int handGripCount = 2;

    [Range(0, 1)]
    public int mouthGripCount;

    [Header("Hand-Carry Locomotion")]
    [Range(0, 2)]
    public int maxHandGripsWhileMoving = 2;

    [Range(0, 2)]
    public int maxHandGripsWhileSprinting = 2;

    [Range(0f, 1f)]
    public float handCarryMoveMultiplier = 1f;

    [Header("Operating Capability")]
    public bool canOperateWithHands = true;

    public bool canOperateWithMouth;    

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

        return canOperateWithHands;
    }

    public void Clamp()
    {
        handGripCount =
            Mathf.Clamp(
                handGripCount,
                1,
                2
            );

        maxHandGripsWhileMoving =
            Mathf.Clamp(
                maxHandGripsWhileMoving,
                0,
                handGripCount
            );

        maxHandGripsWhileSprinting =
            Mathf.Clamp(
                maxHandGripsWhileSprinting,
                0,
                handGripCount
            );

        handCarryMoveMultiplier =
            Mathf.Clamp01(
                handCarryMoveMultiplier
            );

        mouthGripCount =
            Mathf.Clamp(
                mouthGripCount,
                0,
                1
            );

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

            maxHandGripsWhileMoving = 2,
            maxHandGripsWhileSprinting = 2,
            handCarryMoveMultiplier = 1f,

            canOperateWithHands = true,
            canOperateWithMouth = false,

            weaponMode =
                ConventionalWeaponMode.Humanoid
        };
    }
}