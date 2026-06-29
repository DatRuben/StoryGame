using UnityEngine;

public static class CharacterStatsResolver
{
    public static CharacterAttributes ResolveAttributes(
        RaceProfile raceProfile,
        CharacterProfileData characterProfile,
        LineageProfile[] lineageProfiles)
    {
        CharacterAttributes finalAttributes =
            CharacterAttributes.CreateDefault(0);

        if (raceProfile != null)
        {
            finalAttributes =
                CharacterAttributes.Add(
                    finalAttributes,
                    raceProfile.baseAttributes
                );
        }
        else
        {
            Debug.LogWarning(
                "CharacterStatsResolver could not resolve race base attributes because RaceProfile is missing."
            );
        }

        if (lineageProfiles != null)
        {
            for (int i = 0; i < lineageProfiles.Length; i++)
            {
                LineageProfile lineageProfile =
                    lineageProfiles[i];

                if (lineageProfile == null)
                    continue;

                finalAttributes =
                    CharacterAttributes.AddModifiers(
                        finalAttributes,
                        lineageProfile.attributeModifiers
                    );
            }
        }

        if (characterProfile != null)
        {
            finalAttributes =
                CharacterAttributes.Add(
                    finalAttributes,
                    characterProfile.allocatedAttributes
                );
        }
        else
        {
            Debug.LogWarning(
                "CharacterStatsResolver could not add allocated attributes because CharacterProfileData is missing."
            );
        }

        return CharacterAttributes.ClampMinimum(finalAttributes, 1);
    }

    public static FinalMovementStats ResolveMovementStats(
        RaceProfile raceProfile)
    {
        if (raceProfile == null)
        {
            Debug.LogWarning(
                "CharacterStatsResolver could not resolve movement stats because RaceProfile is missing."
            );

            return CreateSize2HumanoidMovement();
        }

        MovementBaseType movementBaseType =
            ResolveMovementBaseType(raceProfile);

        return ResolveMovementStats(
            raceProfile,
            movementBaseType
        );
    }

    public static FinalMovementStats ResolveMovementStats(
        RaceProfile raceProfile,
        MovementBaseType movementBaseType)
    {
        if (raceProfile == null)
        {
            Debug.LogWarning(
                "CharacterStatsResolver could not resolve movement stats because RaceProfile is missing."
            );

            return CreateSize2HumanoidMovement();
        }

        FinalMovementStats movementStats;

        switch (movementBaseType)
        {
            case MovementBaseType.Size2Feral:
                movementStats = CreateSize2FeralMovement();
                break;

            case MovementBaseType.Size2Humanoid:
            default:
                movementStats = CreateSize2HumanoidMovement();
                break;
        }

        ApplySizeMovementModifiers(
            movementStats,
            raceProfile.size
        );

        ApplyDodgeProfile(
            movementStats,
            ResolveDodgeType(raceProfile)
        );

        return movementStats;
    }

    private static FinalMovementStats CreateSize2HumanoidMovement()
    {
        FinalMovementStats movementStats =
            new FinalMovementStats
            {
                walkSpeed = 8f,
                sprintSpeed = 12f,
                groundAcceleration = 8f,
                airAcceleration = 2f,
                deceleration = 16f,
                jumpForce = 7f
            };

        return movementStats;
    }

    private static FinalMovementStats CreateSize2FeralMovement()
    {
        FinalMovementStats movementStats =
            new FinalMovementStats
            {
                walkSpeed = 12f,
                sprintSpeed = 18f,
                groundAcceleration = 10f,
                airAcceleration = 2.5f,
                deceleration = 12f,
                jumpForce = 6.5f
            };

        return movementStats;
    }

    private static MovementBaseType ResolveMovementBaseType(
        RaceProfile raceProfile)
    {
        if (raceProfile == null)
            return MovementBaseType.Size2Humanoid;

        switch (raceProfile.size)
        {
            case RaceSize.Size1Feral:
            case RaceSize.Size2Feral:
            case RaceSize.Size3Feral:
                return MovementBaseType.Size2Feral;
        }

        switch (raceProfile.baseRace)
        {
            case BaseRace.Human:
            case BaseRace.Animali:
            case BaseRace.Canispar:
            case BaseRace.Drakken:
            case BaseRace.Eastern:
                return MovementBaseType.Size2Humanoid;

            case BaseRace.Griffin:
            case BaseRace.WesternDragon:
                return ResolveCreatureMovementBaseType(raceProfile);
        }

        return raceProfile.bodyType == BodyType.Quadruped
            ? MovementBaseType.Size2Feral
            : MovementBaseType.Size2Humanoid;
    }

    private static MovementBaseType ResolveCreatureMovementBaseType(
    RaceProfile raceProfile)
    {
        if (raceProfile == null)
            return MovementBaseType.Size2Humanoid;

        switch (raceProfile.bodyType)
        {
            case BodyType.Humanoid:
                return MovementBaseType.Size2Humanoid;

            case BodyType.Quadruped:
            case BodyType.StanceSwitching:
            default:
                return MovementBaseType.Size2Feral;
        }
    }

    private static void ApplyDodgeProfile(
    FinalMovementStats movementStats,
    DodgeType dodgeType)
    {
        if (movementStats == null)
            return;

        movementStats.dodgeType = dodgeType;

        switch (dodgeType)
        {
            case DodgeType.HeavyStep:
                movementStats.dodgeDistance = 2.5f;
                movementStats.dodgeDuration = 0.22f;
                movementStats.dodgeCooldown = 0.75f;
                movementStats.dodgeStaminaCost = 20f;
                movementStats.dodgeControl = 0f;
                break;

            case DodgeType.LightBurst:
                movementStats.dodgeDistance = 4.75f;
                movementStats.dodgeDuration = 0.22f;
                movementStats.dodgeCooldown = 0.45f;
                movementStats.dodgeStaminaCost = 30f;
                movementStats.dodgeControl = 0.5f;
                break;

            case DodgeType.MediumDash:
            default:
                movementStats.dodgeDistance = 5f;
                movementStats.dodgeDuration = 0.3f; // use your tuned value here
                movementStats.dodgeCooldown = 0.7f;
                movementStats.dodgeStaminaCost = 25f;
                movementStats.dodgeControl = 0.15f;
                break;
        }
    }

    private static void ApplySizeMovementModifiers(
        FinalMovementStats movementStats,
        RaceSize raceSize)
    {
        if (movementStats == null)
            return;

        switch (raceSize)
        {
            case RaceSize.Size1:
                movementStats.walkSpeed *= 1.08f;
                movementStats.sprintSpeed *= 1.08f;
                movementStats.groundAcceleration *= 1.12f;
                movementStats.jumpForce *= 0.95f;
                break;

            case RaceSize.TallerSize2:
                movementStats.walkSpeed *= 1.02f;
                movementStats.sprintSpeed *= 1.02f;
                break;

            case RaceSize.Size3:
                movementStats.walkSpeed *= 0.92f;
                movementStats.sprintSpeed *= 0.92f;
                movementStats.groundAcceleration *= 0.9f;
                movementStats.jumpForce *= 0.9f;
                break;

            case RaceSize.Size1Feral:
                movementStats.walkSpeed *= 1.12f;
                movementStats.sprintSpeed *= 1.1f;
                movementStats.groundAcceleration *= 1.15f;
                movementStats.deceleration *= 0.85f;
                movementStats.jumpForce *= 0.9f;
                break;

            case RaceSize.Size3Feral:
                movementStats.walkSpeed *= 0.95f;
                movementStats.sprintSpeed *= 0.95f;
                movementStats.groundAcceleration *= 0.9f;
                movementStats.deceleration *= 0.85f;
                break;

            case RaceSize.Dragon:
                movementStats.walkSpeed *= 0.9f;
                movementStats.sprintSpeed *= 1.05f;
                movementStats.groundAcceleration *= 0.75f;
                movementStats.deceleration *= 0.75f;
                movementStats.jumpForce *= 0.8f;
                break;

            case RaceSize.BigDragon:
                movementStats.walkSpeed *= 0.8f;
                movementStats.sprintSpeed *= 0.95f;
                movementStats.groundAcceleration *= 0.6f;
                movementStats.deceleration *= 0.6f;
                movementStats.jumpForce *= 0.7f;
                break;
        }
    }

    private static DodgeType ResolveDodgeType(RaceProfile raceProfile)
    {
        if (raceProfile == null)
            return DodgeType.MediumDash;

        switch (raceProfile.size)
        {
            case RaceSize.Size1:
            case RaceSize.Size1Feral:
                return DodgeType.LightBurst;

            case RaceSize.Size3:
            case RaceSize.Size3Feral:
            case RaceSize.Dragon:
            case RaceSize.BigDragon:
                return DodgeType.HeavyStep;

            default:
                return DodgeType.MediumDash;
        }
    }

    public static FinalCharacterStats ResolveFinalStats(
        RaceProfile raceProfile,
        CharacterAttributes attributes)
    {
        if (raceProfile == null)
        {
            Debug.LogWarning(
                "CharacterStatsResolver could not resolve final stats because RaceProfile is missing."
            );

            raceProfile = ScriptableObject.CreateInstance<RaceProfile>();
        }

        attributes =
            CharacterAttributes.ClampMinimum(attributes, 1);

        return new FinalCharacterStats
        {
            maxHealth =
                raceProfile.baseHealth +
                attributes.vitality * raceProfile.vitalityToHealth,

            maxStamina =
                (
                    raceProfile.baseStamina +
                    attributes.endurance * raceProfile.enduranceToStamina
                ) * raceProfile.staminaMultiplier,

            maxAether =
                raceProfile.baseAether +
                attributes.spirit * raceProfile.spiritToAether,

            mass =
                raceProfile.baseMass,

            poise =
                raceProfile.basePoise +
                attributes.vitality * raceProfile.vitalityToPoise +
                attributes.strength * raceProfile.strengthToPoise,

            movementCostMultiplier =
                raceProfile.movementCostMultiplier,

            dodgeCostMultiplier =
                raceProfile.dodgeCostMultiplier,

            equipmentWeightMultiplier =
                raceProfile.equipmentWeightMultiplier
        };
    }
}