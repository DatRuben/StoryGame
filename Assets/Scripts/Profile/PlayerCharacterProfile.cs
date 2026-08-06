using System;
using UnityEngine;

public class PlayerCharacterProfile : MonoBehaviour
{
    public CharacterProfileData ProfileData
    {
        get;
        private set;
    }

    public RaceDefinition RaceDefinition
    {
        get;
        private set;
    }

    public SubraceDefinition SubraceDefinition
    {
        get;
        private set;
    }

    public LineageSelection[] LineageSelections
    {
        get;
        private set;
    }

    public CharacterAttributes PermanentAttributes
    {
        get;
        private set;
    }

    public CharacterAttributes EffectiveAttributes
    {
        get;
        private set;
    }

    // Temporary compatibility alias.
    // Derived calculations should use EffectiveAttributes.
    public CharacterAttributes FinalAttributes =>
        EffectiveAttributes;

    public FinalCharacterStats FinalStats
    {
        get;
        private set;
    }

    public FinalMovementStats FinalMovementStats
    {
        get;
        private set;
    }

    public CharacterAppearanceData Appearance =>
        ProfileData != null
            ? CharacterAppearanceData.Copy(
                ProfileData.appearance
            )
            : CharacterAppearanceData.CreateDefault();

    public event Action AttributesChanged;

    private PlayerAttributeEffects attributeEffects;

    private void Awake()
    {
        ResolveAttributeEffects();
    }

    private void OnEnable()
    {
        ResolveAttributeEffects();

        if (attributeEffects == null)
            return;

        attributeEffects.ModifiersChanged -=
            HandleAttributeModifiersChanged;

        attributeEffects.ModifiersChanged +=
            HandleAttributeModifiersChanged;
    }

    private void OnDisable()
    {
        if (attributeEffects == null)
            return;

        attributeEffects.ModifiersChanged -=
            HandleAttributeModifiersChanged;
    }

    private void ResolveAttributeEffects()
    {
        if (attributeEffects != null)
            return;

        attributeEffects =
            GetComponent<PlayerAttributeEffects>();

        if (attributeEffects == null)
        {
            attributeEffects =
                gameObject.AddComponent<
                    PlayerAttributeEffects>();
        }
    }

    public void Initialize(
        CharacterProfileData profileData,
        RaceDefinition raceDefinition,
        SubraceDefinition subraceDefinition,
        LineageSelection[] lineageSelections)
    {
        if (profileData == null)
        {
            Debug.LogError(
                "PlayerCharacterProfile cannot initialize because ProfileData is missing.",
                this
            );

            return;
        }

        ResolveAttributeEffects();

        ProfileData = profileData;
        RaceDefinition = raceDefinition;
        SubraceDefinition = subraceDefinition;
        LineageSelections = lineageSelections;

        PermanentAttributes =
            CharacterAttributes.ClampMinimum(
                CharacterAttributes.Copy(
                    ProfileData.currentAttributes
                ),
                1
            );

        ResolveEffectiveValues();

        CharacterAppearanceData appearance =
            Appearance;

        float bodyScale =
            appearance.SafeBodyScale;

        ApplyResources(true);
        ApplyBody(bodyScale);
        ApplyAppearance(appearance);
        ApplyEquipmentRules();
        ApplyInput(bodyScale);

        AttributesChanged?.Invoke();

        LogResolvedCharacter();
    }

    private void HandleAttributeModifiersChanged()
    {
        if (ProfileData == null ||
            PermanentAttributes == null)
        {
            return;
        }

        ResolveEffectiveValues();

        ApplyResources(false);

        ApplyInput(
            Appearance.SafeBodyScale
        );

        AttributesChanged?.Invoke();

        LogResolvedCharacter();
    }

    private void ResolveEffectiveValues()
    {
        CharacterAttributeModifiers
            temporaryModifiers =
                attributeEffects != null
                    ? attributeEffects
                        .GetTotalModifiers()
                    : CharacterAttributeModifiers
                        .CreateZero();

        EffectiveAttributes =
            CharacterAttributes.ClampMinimum(
                CharacterAttributes.AddModifiers(
                    PermanentAttributes,
                    temporaryModifiers
                ),
                1
            );

        FinalStats =
            CharacterStatsResolver.ResolveFinalStats(
                ProfileData.currentBaseStats,
                EffectiveAttributes
            );

        FinalMovementStats =
            CharacterStatsResolver.ResolveMovementStats(
                SubraceDefinition,
                EffectiveAttributes
            );
    }

    private void ApplyResources(
        bool refillResources)
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
            refillResources
        );
    }

    private void ApplyBody(
        float bodyScale)
    {
        PlayerBodySetup bodySetup =
            GetComponent<PlayerBodySetup>();

        if (bodySetup == null)
        {
            bodySetup =
                gameObject.AddComponent<
                    PlayerBodySetup>();
        }

        bodySetup.ApplyBody(
            SubraceDefinition,
            FinalStats,
            bodyScale
        );
    }

    private void ApplyInput(
        float bodyScale)
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

        playerInput.ApplyMovementStats(
            FinalMovementStats
        );

        playerInput.ApplyFinalStats(
            FinalStats
        );

        playerInput.ApplyBodyScale(
            bodyScale
        );
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

    private void ApplyAppearance(
        CharacterAppearanceData appearance)
    {
        CharacterAppearanceApplier
            appearanceApplier =
                GetComponent<
                    CharacterAppearanceApplier>();

        if (appearanceApplier == null)
        {
            appearanceApplier =
                gameObject.AddComponent<
                    CharacterAppearanceApplier>();
        }

        appearanceApplier.ApplyAppearance(
            appearance
        );
    }

    private void LogResolvedCharacter()
    {
        if (ProfileData == null ||
            PermanentAttributes == null ||
            EffectiveAttributes == null ||
            FinalStats == null ||
            FinalMovementStats == null)
        {
            return;
        }

        Debug.Log(
            $"Permanent attributes for " +
            $"{ProfileData.characterName}: " +
            $"STR {PermanentAttributes.strength}, " +
            $"DEX {PermanentAttributes.dexterity}, " +
            $"AGI {PermanentAttributes.agility}, " +
            $"VIT {PermanentAttributes.vitality}, " +
            $"END {PermanentAttributes.endurance}, " +
            $"INT {PermanentAttributes.intelligence}, " +
            $"WIL {PermanentAttributes.willpower}, " +
            $"SPI {PermanentAttributes.spirit}, " +
            $"PER {PermanentAttributes.perception}",
            this
        );

        Debug.Log(
            $"Effective attributes for " +
            $"{ProfileData.characterName}: " +
            $"STR {EffectiveAttributes.strength}, " +
            $"DEX {EffectiveAttributes.dexterity}, " +
            $"AGI {EffectiveAttributes.agility}, " +
            $"VIT {EffectiveAttributes.vitality}, " +
            $"END {EffectiveAttributes.endurance}, " +
            $"INT {EffectiveAttributes.intelligence}, " +
            $"WIL {EffectiveAttributes.willpower}, " +
            $"SPI {EffectiveAttributes.spirit}, " +
            $"PER {EffectiveAttributes.perception}",
            this
        );

        Debug.Log(
            $"Final stats for " +
            $"{ProfileData.characterName}: " +
            $"HP {FinalStats.maxHealth}, " +
            $"SOUL BARRIER " +
            $"{FinalStats.maxSoulBarrier}, " +
            $"STA {FinalStats.maxStamina}, " +
            $"AETHER {FinalStats.maxAether}, " +
            $"POISE {FinalStats.poise}",
            this
        );

        Debug.Log(
            $"Final movement for " +
            $"{ProfileData.characterName}: " +
            $"WALK {FinalMovementStats.walkSpeed}, " +
            $"SPRINT " +
            $"{FinalMovementStats.sprintSpeed}, " +
            $"GROUND ACCEL " +
            $"{FinalMovementStats.groundAcceleration}, " +
            $"AIR ACCEL " +
            $"{FinalMovementStats.airAcceleration}, " +
            $"DECEL " +
            $"{FinalMovementStats.deceleration}, " +
            $"JUMP " +
            $"{FinalMovementStats.jumpForce}",
            this
        );
    }
}