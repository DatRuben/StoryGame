using UnityEngine;

public class PlayerCharacterProfile : MonoBehaviour
{
    public CharacterProfileData ProfileData { get; private set; }
    public CharacterAttributes FinalAttributes { get; private set; }

    public void Initialize(CharacterProfileData profileData)
    {
        ProfileData = profileData;

        RaceProfile raceProfile = null;
        LineageProfile[] lineageProfiles = null;

        FinalAttributes =
            CharacterStatsResolver.ResolveAttributes(
                raceProfile,
                ProfileData,
                lineageProfiles
            );

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
    }
}