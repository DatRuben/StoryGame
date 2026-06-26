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
                raceProfile.baseStamina +
                attributes.endurance * raceProfile.enduranceToStamina,

            maxMana =
                raceProfile.baseMana +
                attributes.spirit * raceProfile.spiritToMana,

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