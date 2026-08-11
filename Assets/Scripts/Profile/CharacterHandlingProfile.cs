using System;
using UnityEngine;

[Serializable]
public class CharacterHandlingProfile
{
    public RaceSize raceSize =
        RaceSize.Size2;

    [Min(0)]
    public int handGripCount = 2;

    [Min(0)]
    public int mouthGripCount = 0;

    public bool canOperateWithHands = true;

    public bool canOperateWithMouth;

    public ConventionalWeaponMode weaponMode =
    ConventionalWeaponMode.Humanoid;

    public bool CanOperateWith(
        GripType gripType)
    {
        switch (gripType)
        {
            case GripType.Mouth:
                return mouthGripCount > 0 &&
                       canOperateWithMouth;

            case GripType.Hand:
            default:
                return handGripCount > 0 &&
                       canOperateWithHands;
        }
    }

    [Min(0f)]
    public float strengthOutput = 10f;

    [Min(0f)]
    public float sizeStrengthMultiplier = 1f;

    [Min(0f)]
    public float physicalStrength = 10f;

    public bool HasHandGrips =>
        handGripCount > 0;

    public bool HasMouthGrips =>
        mouthGripCount > 0;
}

public static class CharacterHandlingResolver
{
    public static CharacterHandlingProfile Resolve(
        SubraceDefinition subraceDefinition,
        CharacterAttributeOutput attributeOutput)
    {
        RaceSize raceSize =
            subraceDefinition != null
                ? subraceDefinition.size
                : RaceSize.Size2;

        CharacterGripProfile gripProfile =
            subraceDefinition != null
                ? subraceDefinition.gripProfile
                : null;

        if (gripProfile == null)
        {
            gripProfile =
                CharacterGripProfile
                    .CreateHumanoidDefault();
        }

        float strengthOutput =
            attributeOutput != null
                ? Mathf.Max(
                    0f,
                    attributeOutput.strength
                )
                : 10f;

        float sizeStrengthMultiplier =
            GetSizeStrengthMultiplier(
                raceSize
            );

        return new CharacterHandlingProfile
        {
            raceSize = raceSize,

            handGripCount =
                Mathf.Max(
                    0,
                    gripProfile.handGripCount
                ),

            mouthGripCount =
                Mathf.Max(
                    0,
                    gripProfile.mouthGripCount
                ),

            canOperateWithHands =
                gripProfile.CanOperateWith(
                    GripType.Hand
                ),

            canOperateWithMouth =
                gripProfile.CanOperateWith(
                    GripType.Mouth
                ),

            strengthOutput =
                strengthOutput,

            sizeStrengthMultiplier =
                sizeStrengthMultiplier,

            physicalStrength =
                strengthOutput *
                sizeStrengthMultiplier
        };
    }

    public static float GetSizeStrengthMultiplier(
        RaceSize raceSize)
    {
        // Kept separate from visual body scaling so
        // physical balance can be changed independently.
        switch (raceSize)
        {
            case RaceSize.Size1:
                return 0.7f;

            case RaceSize.Size2:
                return 1f;

            case RaceSize.TallerSize2:
                return 1.25f;

            case RaceSize.Size3:
                return 1.3f;

            case RaceSize.Size1Feral:
                return 0.5f;

            case RaceSize.Size2Feral:
                return 1f;

            case RaceSize.Size3Feral:
                return 1.5f;

            case RaceSize.Dragon:
                return 1.75f;

            case RaceSize.BigDragon:
                return 2f;

            default:
                return 1f;
        }
    }
}