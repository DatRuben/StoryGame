using System.Collections.Generic;
using UnityEngine;

public static class CharacterStatsResolver
{
    public static FinalCharacterStats ResolveFinalStats(
        CharacterBaseStats totalBaseStats,
        CharacterAttributes attributes)
    {
        attributes =
            CharacterAttributes.ClampMinimum(
                attributes,
                1
            );

        if (totalBaseStats == null)
        {
            CharacterBaseStats baseStats =
                CharacterBaseStats.CreateHumanDefault();

            CharacterBaseStats attributeBonuses =
                ResolveAttributeStatBonuses(attributes);

            totalBaseStats =
                CharacterBaseStats.Add(
                    baseStats,
                    attributeBonuses
                );
        }

        return new FinalCharacterStats
        {
            maxHealth =
                Mathf.Max(
                    1f,
                    totalBaseStats.health
                ),

            maxSoulBarrier =
                Mathf.Max(
                    0f,
                    20f +
                    attributes.spirit * 4f +
                    attributes.willpower * 6f
                ),

            maxStamina =
                Mathf.Max(
                    1f,
                    totalBaseStats.stamina
                ),

            maxAether =
                Mathf.Max(
                    0f,
                    totalBaseStats.mana
                ),

            mass =
                Mathf.Max(
                    1f,
                    60f +
                    attributes.strength * 1.5f +
                    attributes.vitality
                ),

            poise =
                Mathf.Max(
                    0f,
                    totalBaseStats.staggerResist
                ),

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

    public static CharacterBaseStats ResolveBaseStats(
        RaceDefinition raceDefinition,
        SubraceDefinition subraceDefinition)
    {
        CharacterBaseStats raceBaseStats =
            raceDefinition != null
                ? raceDefinition.baseStats
                : CharacterBaseStats.CreateHumanDefault();

        CharacterBaseStats subraceModifiers =
            subraceDefinition != null
                ? subraceDefinition.baseStatModifiers
                : CharacterBaseStats.CreateZero();

        return CharacterBaseStats.Add(
            raceBaseStats,
            subraceModifiers
        );
    }

    public static CharacterBaseStats ResolveAttributeStatBonuses(
        CharacterAttributes attributes)
    {
        if (attributes == null)
            attributes = CharacterAttributes.CreateDefault(10);

        return new CharacterBaseStats
        {
            health =
                (attributes.vitality - 10) * 10,

            stamina =
                (attributes.endurance - 10) * 8 +
                (attributes.agility - 10) * 2,

            mana =
                (attributes.intelligence - 10) * 6 +
                (attributes.spirit - 10) * 4,

            staggerResist =
                (attributes.vitality - 10) * 3 +
                (attributes.endurance - 10) * 4 +
                (attributes.willpower - 10) * 2,

            carryWeight =
                (attributes.strength - 10) * 3 +
                (attributes.endurance - 10)
        };
    }

    public static ResolvedCharacterStats ResolveCharacter(
        RaceDefinition race,
        SubraceDefinition subrace,
        List<LineageDefinition> lineages)
    {
        return ResolveCharacter(
            race,
            subrace,
            lineages,
            null,
            null
        );
    }

    public static ResolvedCharacterStats ResolveCharacter(
        RaceDefinition race,
        SubraceDefinition subrace,
        List<LineageDefinition> lineages,
        BackgroundDefinition background,
        List<TraitDefinition> traits)
    {
        if (lineages == null)
            lineages = new List<LineageDefinition>();

        CharacterAttributePreview attributePreview =
            CharacterAttributeResolver.CreatePreview(
                race,
                subrace,
                lineages,
                background,
                traits
            );

        CharacterAttributes finalAttributes =
            CharacterAttributes.ClampMinimum(
                CharacterAttributes.Copy(
                    attributePreview.levelOneAttributes
                ),
                1
            );

        CharacterBaseStats baseStats =
            ResolveBaseStats(
                race,
                subrace
            );

        CharacterBaseStats attributeBonuses =
            ResolveAttributeStatBonuses(
                finalAttributes
            );

        CharacterBaseStats totalBaseStats =
            CharacterBaseStats.Add(
                baseStats,
                attributeBonuses
            );

        FinalCharacterStats finalStats =
            ResolveFinalStats(
                totalBaseStats,
                finalAttributes
            );

        FinalMovementStats movementStats =
            ResolveMovementStats(
                subrace
            );

        return new ResolvedCharacterStats
        {
            attributePreview = attributePreview,
            finalAttributes = finalAttributes,
            baseStats = baseStats,
            attributeBonuses = attributeBonuses,
            totalBaseStats = totalBaseStats,
            finalStats = finalStats,
            movementStats = movementStats
        };
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