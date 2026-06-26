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
                    CharacterAttributes.Add(
                        finalAttributes,
                        lineageProfile.attributeBonuses
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

        return finalAttributes;
    }
}