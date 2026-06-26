using UnityEngine;

public class PlayerCharacterProfile : MonoBehaviour
{
    public CharacterProfileData ProfileData { get; private set; }
    public CharacterAttributes FinalAttributes { get; private set; }
    public FinalCharacterStats FinalStats { get; private set; }
    public FinalMovementStats FinalMovementStats { get; private set; }

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

        FinalMovementStats =
            CharacterStatsResolver.ResolveMovementStats(
                raceProfile
            );

        PlayerBodySetup bodySetup =
            GetComponent<PlayerBodySetup>();

        if (bodySetup == null)
            bodySetup = gameObject.AddComponent<PlayerBodySetup>();

        bodySetup.ApplyRaceBody(
            raceProfile,
            FinalStats
        );

        PlayerInput playerInput =
            GetComponent<PlayerInput>();

        if (playerInput != null)
        {
            playerInput.ApplyMovementStats(FinalMovementStats);
        }
        else
        {
            Debug.LogWarning(
                "PlayerCharacterProfile could not apply movement stats because PlayerInput is missing.",
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
            $"Final stats for {ProfileData.characterName}: " +
            $"HP {FinalStats.maxHealth}, " +
            $"STA {FinalStats.maxStamina}, " +
            $"MANA {FinalStats.maxMana}, " +
            $"POISE {FinalStats.poise}",
            this
        );

        Debug.Log(
            $"Final movement for {ProfileData.characterName}: " +
            $"WALK {FinalMovementStats.walkSpeed}, " +
            $"SPRINT {FinalMovementStats.sprintSpeed}, " +
            $"GROUND ACCEL {FinalMovementStats.groundAcceleration}, " +
            $"AIR ACCEL {FinalMovementStats.airAcceleration}, " +
            $"DECEL {FinalMovementStats.deceleration}, " +
            $"JUMP {FinalMovementStats.jumpForce}",
            this
        );
    }
}