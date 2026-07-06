using System.Collections.Generic;
using UnityEngine;

public static class CharacterStatsResolver
{
    public static CharacterAttributes ResolveAttributes(
        RaceDefinition raceDefinition,
        SubraceDefinition subraceDefinition,
        CharacterProfileData characterProfile,
        LineageDefinition[] lineageDefinitions)
    {
        CharacterAttributes finalAttributes =
            CreateHumanBaselineAttributes();

        if (raceDefinition != null)
        {
            finalAttributes =
                CharacterAttributes.AddModifiers(
                    finalAttributes,
                    raceDefinition.modifiersFromHuman
                );
        }
        else
        {
            Debug.LogWarning(
                "CharacterStatsResolver could not apply race modifiers because RaceDefinition is missing."
            );
        }

        ApplySubraceModifiers(
            ref finalAttributes,
            subraceDefinition
        );

        ApplyLineageModifiers(
            ref finalAttributes,
            lineageDefinitions
        );

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

        return CharacterAttributes.ClampMinimum(
            finalAttributes,
            1
        );
    }

    public static FinalCharacterStats ResolveFinalStats(
        CharacterAttributes attributes)
    {
        attributes =
            CharacterAttributes.ClampMinimum(
                attributes,
                1
            );

        return new FinalCharacterStats
        {
            maxHealth =
                75f +
                attributes.vitality * 10f,

            maxSoulBarrier =
                20f +
                attributes.spirit * 4f +
                attributes.willpower * 6f,

            maxStamina =
                50f +
                attributes.endurance * 8f,

            maxAether =
                20f +
                attributes.spirit * 7f +
                attributes.intelligence * 3f,

            mass =
                60f +
                attributes.strength * 1.5f +
                attributes.vitality,

            poise =
                10f +
                attributes.vitality * 1.5f +
                attributes.strength,

            movementCostMultiplier =
                Mathf.Max(
                    0.5f,
                    1f - attributes.endurance * 0.005f
                ),

            dodgeCostMultiplier =
                Mathf.Max(
                    0.5f,
                    1f - attributes.agility * 0.004f
                ),

            equipmentWeightMultiplier =
                Mathf.Max(
                    0.5f,
                    1f - attributes.strength * 0.005f
                )
        };
    }

    public static FinalMovementStats ResolveMovementStats(
        SubraceDefinition subraceDefinition)
    {
        if (subraceDefinition == null)
        {
            Debug.LogWarning(
                "CharacterStatsResolver could not resolve movement stats because SubraceDefinition is missing."
            );

            return CreateSize2HumanoidMovement();
        }

        MovementBaseType movementBaseType =
            ResolveMovementBaseType(subraceDefinition);

        return ResolveMovementStats(
            subraceDefinition,
            movementBaseType
        );
    }

    public static FinalMovementStats ResolveMovementStats(
        SubraceDefinition subraceDefinition,
        MovementBaseType movementBaseType)
    {
        if (subraceDefinition == null)
        {
            Debug.LogWarning(
                "CharacterStatsResolver could not resolve movement stats because SubraceDefinition is missing."
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
            subraceDefinition.size
        );

        ApplyDodgeProfile(
            movementStats,
            ResolveDodgeType(subraceDefinition)
        );

        return movementStats;
    }

    private static CharacterAttributes CreateHumanBaselineAttributes()
    {
        return CharacterAttributes.CreateDefault(10);
    }

    private static void ApplySubraceModifiers(
        ref CharacterAttributes attributes,
        SubraceDefinition subraceDefinition)
    {
        if (subraceDefinition == null)
            return;

        HashSet<SubraceDefinition> visited = new();
        SubraceDefinition current = subraceDefinition;

        while (current != null)
        {
            if (!visited.Add(current))
            {
                Debug.LogWarning(
                    $"Subrace comparison loop detected at {current.displayName}."
                );

                break;
            }

            attributes =
                CharacterAttributes.AddModifiers(
                    attributes,
                    current.modifiersFromComparison
                );

            current = current.compareToSubrace;
        }
    }

    private static void ApplyLineageModifiers(
        ref CharacterAttributes attributes,
        LineageDefinition[] lineageDefinitions)
    {
        if (lineageDefinitions == null)
            return;

        foreach (LineageDefinition lineageDefinition in lineageDefinitions)
        {
            if (lineageDefinition == null)
                continue;

            attributes =
                CharacterAttributes.AddModifiers(
                    attributes,
                    lineageDefinition.modifiers
                );
        }
    }

    private static FinalMovementStats CreateSize2HumanoidMovement()
    {
        return new FinalMovementStats
        {
            walkSpeed = 8f,
            sprintSpeed = 12f,
            groundAcceleration = 8f,
            airAcceleration = 2f,
            deceleration = 16f,
            jumpForce = 7f
        };
    }

    private static FinalMovementStats CreateSize2FeralMovement()
    {
        return new FinalMovementStats
        {
            walkSpeed = 12f,
            sprintSpeed = 18f,
            groundAcceleration = 10f,
            airAcceleration = 2.5f,
            deceleration = 12f,
            jumpForce = 6.5f
        };
    }

    private static MovementBaseType ResolveMovementBaseType(
        SubraceDefinition subraceDefinition)
    {
        if (subraceDefinition == null)
            return MovementBaseType.Size2Humanoid;

        switch (subraceDefinition.size)
        {
            case RaceSize.Size1Feral:
            case RaceSize.Size2Feral:
            case RaceSize.Size3Feral:
                return MovementBaseType.Size2Feral;
        }

        switch (subraceDefinition.bodyType)
        {
            case BodyType.Quadruped:
            case BodyType.StanceSwitching:
                return MovementBaseType.Size2Feral;

            case BodyType.Humanoid:
            default:
                return MovementBaseType.Size2Humanoid;
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
                movementStats.dodgeDuration = 0.3f;
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

    private static DodgeType ResolveDodgeType(
        SubraceDefinition subraceDefinition)
    {
        if (subraceDefinition == null)
            return DodgeType.MediumDash;

        switch (subraceDefinition.size)
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
}