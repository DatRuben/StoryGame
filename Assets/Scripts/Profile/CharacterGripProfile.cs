using System;
using UnityEngine;
using UnityEngine.Serialization;

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
    [SerializeField]
    [Range(0, 2)]
    private int maxHandGripUsage = 2;

    [SerializeField]
    [Range(0, 1)]
    private int mouthGripCount;

    [Header("Hand-Carry Locomotion")]
    [SerializeField]
    [Range(0, 2)]
    private int maxHandGripsWhileMoving = 2;

    [SerializeField]
    [Range(0, 2)]
    private int maxHandGripsWhileSprinting = 2;

    [SerializeField]
    [Range(0f, 1f)]
    private float handCarryMoveMultiplier = 1f;

    [Header("Operating Capability")]
    [SerializeField]
    private bool canOperateWithHands = true;

    [SerializeField]
    private bool canOperateWithMouth;

    public bool HasMouthGrips =>
        MouthGripCount > 0;

    [SerializeField]
    private ConventionalWeaponMode weaponMode =
        ConventionalWeaponMode.Humanoid;

    public ConventionalWeaponMode WeaponMode =>
        weaponMode;

    public int MaxHandGripUsage =>
        Mathf.Clamp(
            maxHandGripUsage,
            0,
            2
        );

    public int MouthGripCount =>
        Mathf.Clamp(
            mouthGripCount,
            0,
            1
        );

    public int MaxHandGripsWhileMoving =>
        Mathf.Clamp(
            maxHandGripsWhileMoving,
            0,
            MaxHandGripUsage
        );

    public int MaxHandGripsWhileSprinting =>
        Mathf.Clamp(
            maxHandGripsWhileSprinting,
            0,
            MaxHandGripUsage
        );

    public float HandCarryMoveMultiplier =>
        Mathf.Clamp01(
            handCarryMoveMultiplier
        );

    public int GetMaxGripUsage(
        GripType gripType)
    {
        return gripType ==
               GripType.Mouth
            ? MouthGripCount
            : MaxHandGripUsage;
    }

    public bool CanOperateWith(
        GripType gripType)
    {
        if (gripType ==
            GripType.Mouth)
        {
            return MouthGripCount > 0 &&
                   canOperateWithMouth;
        }

        return MaxHandGripUsage > 0 &&
            canOperateWithHands;
    }

    public void Clamp()
    {
        maxHandGripUsage =
            MaxHandGripUsage;

        mouthGripCount =
            MouthGripCount;

        maxHandGripsWhileMoving =
            MaxHandGripsWhileMoving;

        maxHandGripsWhileSprinting =
            MaxHandGripsWhileSprinting;

        handCarryMoveMultiplier =
            HandCarryMoveMultiplier;

        if (mouthGripCount == 0)
            canOperateWithMouth = false;
    }

    public static CharacterGripProfile
        CreateHumanoidDefault()
    {
        return new CharacterGripProfile
        {
            maxHandGripUsage = 2,
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