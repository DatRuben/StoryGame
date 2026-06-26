using UnityEngine;

public class PlayerCharacterProfile : MonoBehaviour
{
    public CharacterProfileData ProfileData { get; private set; }
    public CharacterAttributes FinalAttributes { get; private set; }
    public FinalCharacterStats FinalStats { get; private set; }

    public void Initialize(
        CharacterProfileData profileData,
        RaceProfile raceProfile,
        LineageProfile[] lineageProfiles)
    {
        ProfileData = profileData;

        FinalAttributes =
            CharacterStatsResolver.ResolveAttributes(
                raceProfile,
                ProfileData,
                lineageProfiles
            );

        FinalStats =
            CharacterStatsResolver.ResolveFinalStats(
                raceProfile,
                FinalAttributes
            );

        PlayerResources playerResources =
            GetComponent<PlayerResources>();

        if (playerResources != null)
        {
            playerResources.ApplyFinalStats(FinalStats, true);
        }
        else
        {
            Debug.LogWarning(
                "PlayerCharacterProfile could not apply final stats because PlayerResources is missing.",
                this
            );
        }

        Debug.Log(
            $"Resolved attributes for {ProfileData.characterName}: " +
            $"STR {FinalAttributes.strength}, " +
            $"DEX {FinalAttributes.dexterity}, " +
            $"AGI {FinalAttributes.agility}, " +
            $"VIT {FinalAttributes.vitality}, " +
            $"END {FinalAttributes.endurance}, " +
            $"INT {FinalAttributes.intelligence}, " +
            $"WIL {FinalAttributes.willpower}, " +
            $"SPI {FinalAttributes.spirit}, " +
            $"PER {FinalAttributes.perception}, " +
            $"TEC {FinalAttributes.technique}",
            this
        );

        Debug.Log(
            $"Derived stats for {ProfileData.characterName}: " +
            $"HP {FinalStats.maxHealth}, " +
            $"STA {FinalStats.maxStamina}, " +
            $"MANA {FinalStats.maxMana}, " +
            $"POISE {FinalStats.poise}",
            this
        );
    }
}