using System.Collections.Generic;
using TMPro;
using UnityEngine;

public sealed class PlayerCharacterInfoUI :
    MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI characterNameText;

    [SerializeField]
    private TextMeshProUGUI attributesText;

    [SerializeField]
    private TextMeshProUGUI characterDetailsText;

    [SerializeField]
    private TextMeshProUGUI statsText;

    [SerializeField]
    private TextMeshProUGUI movementText;

    private PlayerCharacterProfile characterProfile;

    private EntityResources playerResources;

    public void BindPlayer(
        PlayerCharacterProfile newCharacterProfile,
        EntityResources newPlayerResources)
    {
        Unsubscribe();

        characterProfile =
            newCharacterProfile;

        playerResources =
            newPlayerResources;

        Subscribe();

        Refresh();
    }

    private void Subscribe()
    {
        if (characterProfile != null)
        {
            characterProfile.AttributesChanged +=
                Refresh;
        }

        if (playerResources != null)
        {
            playerResources.OnResourcesChanged +=
                RefreshStats;
        }
    }

    private void Unsubscribe()
    {
        if (characterProfile != null)
        {
            characterProfile.AttributesChanged -=
                Refresh;
        }

        if (playerResources != null)
        {
            playerResources.OnResourcesChanged -=
                RefreshStats;
        }
    }

    private void OnEnable()
    {
        Unsubscribe();
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Refresh()
    {
        if (characterProfile == null ||
            characterProfile.ProfileData == null)
        {
            if (characterNameText != null)
                characterNameText.text = "";

            if (attributesText != null)
                attributesText.text = "";

            if (characterDetailsText != null)
                characterDetailsText.text = "";

            if (statsText != null)
                statsText.text = "";

            if (movementText != null)
                movementText.text = "";

            return;
        }

        if (characterNameText != null)
        {
            characterNameText.text =
                characterProfile.ProfileData.characterName;
        }

        RefreshDetails();
        RefreshAttributes();
        RefreshStats();
        RefreshMovement();
    }

    private void RefreshDetails()
    {
        if (characterDetailsText == null)
            return;

        CharacterProfileData profile =
            characterProfile.ProfileData;

        string raceName =
            characterProfile.RaceDefinition != null
                ? characterProfile.RaceDefinition.displayName
                : profile.raceId;

        string subraceName =
            characterProfile.SubraceDefinition != null
                ? characterProfile.SubraceDefinition.displayName
                : profile.subraceId;

        string backgroundName =
            characterProfile.BackgroundDefinition != null
                ? characterProfile.BackgroundDefinition.displayName
                : profile.backgroundId;

        string lineageText = "None";

        LineageSelection[] lineages =
            characterProfile.LineageSelections;

        if (lineages != null &&
            lineages.Length > 0)
        {
            lineageText = "";

            for (int i = 0;
                 i < lineages.Length;
                 i++)
            {
                LineageSelection lineage =
                    lineages[i];

                if (lineage == null ||
                    string.IsNullOrWhiteSpace(
                        lineage.DisplayName))
                {
                    continue;
                }

                if (lineageText.Length > 0)
                {
                    lineageText += ", ";
                }

                lineageText += lineage.DisplayName;
            }

            if (lineageText.Length == 0)
            {
                lineageText = "None";
            }
        }

        string traitsText = "None";

        IReadOnlyList<TraitDefinition> traits =
            characterProfile.TraitDefinitions;

        if (traits != null &&
            traits.Count > 0)
        {
            traitsText = "";

            for (int i = 0;
                 i < traits.Count;
                 i++)
            {
                TraitDefinition trait =
                    traits[i];

                if (trait == null ||
                    string.IsNullOrWhiteSpace(
                        trait.displayName))
                {
                    continue;
                }

                if (traitsText.Length > 0)
                {
                    traitsText += ", ";
                }

                traitsText += trait.displayName;
            }

            if (traitsText.Length == 0)
            {
                traitsText = "None";
            }
        }

        string formLine = "";

        if (characterProfile.SubraceDefinition != null &&
            characterProfile.SubraceDefinition.bodyType !=
                BodyType.Humanoid)
        {
            formLine =
                $"Form: {characterProfile.CurrentForm}\n";
        }

        characterDetailsText.text =
            "Details\n" +
            $"Level: {profile.level}\n" +
            $"Gender: {profile.gender}\n" +
            $"Race: {raceName}\n" +
            $"Subrace: {subraceName}\n" +
            formLine +
            $"Lineage: {lineageText}\n" +
            $"Background: {backgroundName}\n" +
            $"Traits: {traitsText}";
    }

    private void RefreshAttributes()
    {
        if (attributesText == null)
            return;

        CharacterAttributes permanent =
            characterProfile.PermanentAttributes;

        CharacterAttributes effective =
            characterProfile.EffectiveAttributes;

        if (permanent == null ||
            effective == null)
        {
            attributesText.text =
                "Attributes\nUnavailable";

            return;
        }

        attributesText.text =
            "Attributes\n" +
            FormatAttribute(
                "Strength",
                permanent.strength,
                effective.strength
            ) + "\n" +
            FormatAttribute(
                "Dexterity",
                permanent.dexterity,
                effective.dexterity
            ) + "\n" +
            FormatAttribute(
                "Agility",
                permanent.agility,
                effective.agility
            ) + "\n" +
            FormatAttribute(
                "Vitality",
                permanent.vitality,
                effective.vitality
            ) + "\n" +
            FormatAttribute(
                "Endurance",
                permanent.endurance,
                effective.endurance
            ) + "\n" +
            FormatAttribute(
                "Intelligence",
                permanent.intelligence,
                effective.intelligence
            ) + "\n" +
            FormatAttribute(
                "Willpower",
                permanent.willpower,
                effective.willpower
            ) + "\n" +
            FormatAttribute(
                "Spirit",
                permanent.spirit,
                effective.spirit
            ) + "\n" +
            FormatAttribute(
                "Perception",
                permanent.perception,
                effective.perception
            );
    }

    private void RefreshStats()
    {
        if (statsText == null)
            return;

        if (characterProfile == null)
        {
            statsText.text = "";
            return;
        }

        FinalCharacterStats stats =
            characterProfile.FinalStats;

        CharacterBaseStats baseStats =
            characterProfile.EffectiveBaseStats;

        if (stats == null ||
            baseStats == null)
        {
            statsText.text =
                "Stats\nUnavailable";

            return;
        }

        string resourceText;

        if (playerResources != null &&
            playerResources.IsInitialized)
        {
            resourceText =
                $"Health: " +
                $"{playerResources.CurrentHealth:0.##} / " +
                $"{playerResources.MaxHealth:0.##}\n" +

                $"Soul Barrier: " +
                $"{playerResources.CurrentSoulBarrier:0.##} / " +
                $"{playerResources.MaxSoulBarrier:0.##}\n" +

                $"Stamina: " +
                $"{playerResources.CurrentStamina:0.##} / " +
                $"{playerResources.MaxStamina:0.##}\n" +

                $"Aether: " +
                $"{playerResources.CurrentAether:0.##} / " +
                $"{playerResources.MaxAether:0.##}\n" +

                $"Aether Healing Tolerance: " +
                $"{playerResources.AetherHealingTolerance:0.##}\n" +

                $"Aether Healing Burn: " +
                $"{playerResources.CurrentAetherHealingBurn:0.##} / " +
                $"{playerResources.AetherHealingTolerance:0.##}\n" +

                $"Aether Healing Efficiency: " +
                $"{playerResources.AetherHealingEfficiency:P0}\n";
        }
        else
        {
            resourceText =
                $"Max Health: {stats.maxHealth:0.##}\n" +
                $"Soul Barrier: {stats.maxSoulBarrier:0.##}\n" +
                $"Max Stamina: {stats.maxStamina:0.##}\n" +
                $"Max Aether: {stats.maxAether:0.##}\n" +
                $"Aether Healing Tolerance: " +
                $"{stats.aetherHealingTolerance:0.##}\n";
        }

        statsText.text =
            "Stats\n" +
            resourceText +
            $"Mass: {stats.mass:0.##}\n" +
            $"Poise: {stats.poise:0.##}\n" +
            $"Stagger Resist: {baseStats.staggerResist}\n" +
            $"Carry Weight: {baseStats.carryWeight}\n" +
            $"Movement Cost: x{stats.movementCostMultiplier:0.##}\n" +
            $"Dodge Cost: x{stats.dodgeCostMultiplier:0.##}\n" +
            $"Equipment Weight: x{stats.equipmentWeightMultiplier:0.##}";
    }

    private void RefreshMovement()
    {
        if (movementText == null)
            return;

        FinalMovementStats movement =
            characterProfile.FinalMovementStats;

        if (movement == null)
        {
            movementText.text =
                "Movement\nUnavailable";

            return;
        }

        movementText.text =
            "Movement\n" +
            $"Walk Speed: {movement.walkSpeed:0.##}\n" +
            $"Sprint Speed: {movement.sprintSpeed:0.##}\n" +
            $"Ground Acceleration: {movement.groundAcceleration:0.##}\n" +
            $"Air Acceleration: {movement.airAcceleration:0.##}\n" +
            $"Deceleration: {movement.deceleration:0.##}\n" +
            $"Jump Force: {movement.jumpForce:0.##}\n" +
            $"Dodge Type: {movement.dodgeType}\n" +
            $"Dodge Distance: {movement.dodgeDistance:0.##}\n" +
            $"Dodge Duration: {movement.dodgeDuration:0.##}\n" +
            $"Dodge Cooldown: {movement.dodgeCooldown:0.##}\n" +
            $"Dodge Stamina Cost: {movement.dodgeStaminaCost:0.##}\n" +
            $"Dodge Control: {movement.dodgeControl:0.##}";
    }

    private static string FormatAttribute(
        string label,
        int permanentValue,
        int effectiveValue)
    {
        if (permanentValue == effectiveValue)
        {
            return
                $"{label}: {permanentValue}";
        }

        return
            $"{label}: {permanentValue} ({effectiveValue})";
    }
}