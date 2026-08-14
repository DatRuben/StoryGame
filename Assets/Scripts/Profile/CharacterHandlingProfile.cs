using UnityEngine;

public sealed class CharacterHandlingProfile
{
    public SubraceDefinition SubraceDefinition
    {
        get;
    }

    public CharacterGripProfile GripProfile
    {
        get;
    }

    public CharacterAttributeOutput AttributeOutput
    {
        get;
    }

    public RaceSize Size =>
        SubraceDefinition != null
            ? SubraceDefinition.size
            : RaceSize.Size2;

    public int HandGripCount =>
        Mathf.Clamp(
            GripProfile.handGripCount,
            1,
            2
        );

    public int MouthGripCount =>
        Mathf.Clamp(
            GripProfile.mouthGripCount,
            0,
            1
        );

    public int MaxHandGripsWhileMoving =>
        Mathf.Clamp(
            GripProfile.maxHandGripsWhileMoving,
            0,
            HandGripCount
        );

    public int MaxHandGripsWhileSprinting =>
        Mathf.Clamp(
            GripProfile.maxHandGripsWhileSprinting,
            0,
            HandGripCount
        );

    public float HandCarryMoveMultiplier =>
        Mathf.Clamp01(
            GripProfile.handCarryMoveMultiplier
        );

    public ConventionalWeaponMode WeaponMode =>
        GripProfile.weaponMode;

    public bool HasMouthGrips =>
        MouthGripCount > 0;

    public float StrengthOutput =>
        AttributeOutput != null
            ? Mathf.Max(
                0f,
                AttributeOutput.strength
            )
            : 10f;

    public float SizeStrengthMultiplier =>
        CharacterHandlingResolver
            .GetSizeStrengthMultiplier(
                Size
            );

    public float PhysicalStrength =>
        StrengthOutput *
        SizeStrengthMultiplier;

    internal CharacterHandlingProfile(
        SubraceDefinition subraceDefinition,
        CharacterGripProfile gripProfile,
        CharacterAttributeOutput attributeOutput)
    {
        SubraceDefinition =
            subraceDefinition;

        GripProfile =
            gripProfile ??
            CharacterGripProfile
                .CreateHumanoidDefault();

        AttributeOutput =
            attributeOutput;
    }

    public bool CanOperateWith(
        GripType gripType)
    {
        return GripProfile.CanOperateWith(
            gripType
        );
    }
}

public static class CharacterHandlingResolver
{
    public static CharacterHandlingProfile Resolve(
        SubraceDefinition subraceDefinition,
        CharacterGripProfile gripProfile,
        CharacterAttributeOutput attributeOutput)
    {
        return new CharacterHandlingProfile(
            subraceDefinition,
            gripProfile,
            attributeOutput
        );
    }

    public static float GetSizeStrengthMultiplier(
        RaceSize raceSize)
    {
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