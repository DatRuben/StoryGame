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

    public PlayerStatusEffects StatusEffects =>
        statusEffects;

    public CharacterAppearanceData Appearance =>
        ProfileData != null
            ? CharacterAppearanceData.Copy(
                ProfileData.appearance
            )
            : CharacterAppearanceData.CreateDefault();

    public event Action AttributesChanged;

    private PlayerStatusEffects statusEffects;

    private void Awake()
    {
        ResolveStatusEffects();
    }

    private void OnEnable()
    {
        ResolveStatusEffects();

        if (statusEffects == null)
            return;

        statusEffects.EffectsChanged -=
            HandleStatusEffectsChanged;

        statusEffects.EffectsChanged +=
            HandleStatusEffectsChanged;
    }

    private void OnDisable()
    {
        if (statusEffects == null)
            return;

        statusEffects.EffectsChanged -=
            HandleStatusEffectsChanged;
    }

    private void ResolveStatusEffects()
    {
        if (statusEffects != null)
            return;

        statusEffects =
            GetComponent<PlayerStatusEffects>();

        if (statusEffects == null)
        {
            statusEffects =
                gameObject.AddComponent<
                    PlayerStatusEffects>();
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

        ResolveStatusEffects();

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

    private void HandleStatusEffectsChanged()
    {
        if (ProfileData == null ||
            PermanentAttributes == null)
        {
            return;
        }

        ResolveEffectiveValues();

        float bodyScale =
            Appearance.SafeBodyScale;


        ApplyResources(false);

        ApplyBody(bodyScale);

        ApplyInput(bodyScale);

        AttributesChanged?.Invoke();

        LogResolvedCharacter();
    }

    private void ResolveEffectiveValues()
    {
        CharacterAttributeModifiers
            statusModifiers =
                statusEffects != null
                    ? statusEffects
                        .GetAttributeModifiers()
                    : CharacterAttributeModifiers
                        .CreateZero();

        EffectiveAttributes =
            CharacterAttributes.ClampMinimum(
                CharacterAttributes.AddModifiers(
                    PermanentAttributes,
                    statusModifiers
                ),
                1
            );

        CharacterBaseStats effectiveBaseStats =
            ResolveEffectiveBaseStats();

        FinalStats =
            CharacterStatsResolver.ResolveFinalStats(
                effectiveBaseStats,
                EffectiveAttributes
            );

        FinalMovementStats =
            CharacterStatsResolver.ResolveMovementStats(
                SubraceDefinition,
                EffectiveAttributes
            );
    }

    private CharacterBaseStats
        ResolveEffectiveBaseStats()
    {
        CharacterBaseStats effectiveBonuses =
            CharacterStatsResolver
                .ResolveAttributeStatBonuses(
                    EffectiveAttributes
                );

        if (ProfileData == null ||
            ProfileData.currentBaseStats == null)
        {
            CharacterBaseStats liveBaseStats =
                CharacterStatsResolver.ResolveBaseStats(
                    RaceDefinition,
                    SubraceDefinition
                );

            return CharacterBaseStats.Add(
                liveBaseStats,
                effectiveBonuses
            );
        }

        CharacterBaseStats savedTotalStats =
            CharacterBaseStats.Copy(
                ProfileData.currentBaseStats
            );

        CharacterBaseStats permanentBonuses =
            CharacterStatsResolver
                .ResolveAttributeStatBonuses(
                    PermanentAttributes
                );

        return new CharacterBaseStats
        {
            health =
                savedTotalStats.health -
                permanentBonuses.health +
                effectiveBonuses.health,

            stamina =
                savedTotalStats.stamina -
                permanentBonuses.stamina +
                effectiveBonuses.stamina,

            mana =
                savedTotalStats.mana -
                permanentBonuses.mana +
                effectiveBonuses.mana,

            staggerResist =
                savedTotalStats.staggerResist -
                permanentBonuses.staggerResist +
                effectiveBonuses.staggerResist,

            carryWeight =
                savedTotalStats.carryWeight -
                permanentBonuses.carryWeight +
                effectiveBonuses.carryWeight
        };
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