using UnityEngine;

public class PlayerCharacterProfile : MonoBehaviour
{
    public CharacterProfileData ProfileData { get; private set; }
    public RaceDefinition RaceDefinition { get; private set; }
    public SubraceDefinition SubraceDefinition { get; private set; }
    public LineageDefinition[] LineageDefinitions { get; private set; }

    public CharacterAttributes FinalAttributes { get; private set; }
    public FinalCharacterStats FinalStats { get; private set; }
    public FinalMovementStats FinalMovementStats { get; private set; }

    public void Initialize(
        CharacterProfileData profileData,
        RaceDefinition raceDefinition,
        SubraceDefinition subraceDefinition,
        LineageDefinition[] lineageDefinitions)
    {
        ProfileData = profileData;
        RaceDefinition = raceDefinition;
        SubraceDefinition = subraceDefinition;
        LineageDefinitions = lineageDefinitions;

        FinalAttributes =
            CharacterStatsResolver.ResolveAttributes(
                raceDefinition,
                subraceDefinition,
                profileData,
                lineageDefinitions
            );

        FinalStats =
            CharacterStatsResolver.ResolveFinalStats(
                FinalAttributes
            );

        FinalMovementStats =
            CharacterStatsResolver.ResolveMovementStats(
                subraceDefinition
            );

        ApplyResources();
        ApplyBody();
        ApplyEquipmentRules();
        ApplyInput();

        LogResolvedCharacter();
    }

    private void ApplyResources()
    {
        PlayerResources playerResources =
            GetComponent<PlayerResources>();

        if (playerResources == null)
        {
            Debug.LogWarning(
                "PlayerCharacterProfile could not apply final stats because PlayerResources is missing.",
                this
            );

            return;
        }

        playerResources.ApplyFinalStats(
            FinalStats,
            true
        );
    }

    private void ApplyBody()
    {
        PlayerBodySetup bodySetup =
            GetComponent<PlayerBodySetup>();

        if (bodySetup == null)
            bodySetup = gameObject.AddComponent<PlayerBodySetup>();

        bodySetup.ApplyBody(
            SubraceDefinition,
            FinalStats
        );
    }

    private void ApplyInput()
    {
        PlayerInput playerInput =
            GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogWarning(
                "PlayerCharacterProfile could not apply movement/final stats because PlayerInput is missing.",
                this
            );

            return;
        }

        playerInput.ApplyMovementStats(FinalMovementStats);
        playerInput.ApplyFinalStats(FinalStats);
    }

    private void ApplyEquipmentRules()
    {
        PlayerHolding playerHolding =
            GetComponent<PlayerHolding>();

        if (playerHolding != null)
        {
            playerHolding.ApplySubraceDefinition(
                SubraceDefinition
            );
        }

        PlayerWeaponSlots playerWeaponSlots =
            GetComponent<PlayerWeaponSlots>();

        if (playerWeaponSlots != null)
        {
            playerWeaponSlots.ApplySubraceDefinition(
                SubraceDefinition
            );
        }

        PlayerEquipment playerEquipment =
            GetComponent<PlayerEquipment>();

        if (playerEquipment != null)
        {
            playerEquipment.ApplySubraceDefinition(
                SubraceDefinition
            );
        }
    }

    private void LogResolvedCharacter()
    {
        if (ProfileData == null ||
            FinalAttributes == null ||
            FinalStats == null ||
            FinalMovementStats == null)
        {
            return;
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
            $"PER {FinalAttributes.perception}, ",
            this
        );

        Debug.Log(
            $"Final stats for {ProfileData.characterName}: " +
            $"HP {FinalStats.maxHealth}, " +
            $"SOUL BARRIER {FinalStats.maxSoulBarrier}, " +
            $"STA {FinalStats.maxStamina}, " +
            $"AETHER {FinalStats.maxAether}, " +
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