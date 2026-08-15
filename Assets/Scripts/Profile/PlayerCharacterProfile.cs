using System;
using UnityEngine;
using System.Collections.Generic;

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

    public BackgroundDefinition BackgroundDefinition
    {
        get;
        private set;
    }

    public IReadOnlyList<TraitDefinition>
        TraitDefinitions =>
            traitDefinitions;

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

    public CharacterAttributeOutput PermanentAttributeOutput
    {
        get;
        private set;
    }

    public CharacterAttributeOutput EffectiveAttributeOutput
    {
        get;
        private set;
    }

    public CharacterHandlingProfile
    PermanentHandlingProfile
    {
        get;
        private set;
    }

    public CharacterHandlingProfile
        EffectiveHandlingProfile
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

    private readonly List<TraitDefinition>
        traitDefinitions =
            new List<TraitDefinition>();

    public CharacterAppearanceData Appearance =>
        ProfileData != null
            ? CharacterAppearanceData.Copy(
                ProfileData.appearance
            )
            : CharacterAppearanceData.CreateDefault();

    public event Action AttributesChanged;

    private PlayerStatusEffects statusEffects;
    private PlayerFormState formState;

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

        if (formState != null)
        {
            formState.Changed -=
                HandleFormChanged;

            formState.Changed +=
                HandleFormChanged;
        }
    }

    private void OnDisable()
    {
        if (statusEffects != null)
        {
            statusEffects.EffectsChanged -=
                HandleStatusEffectsChanged;
        }

        if (formState != null)
        {
            formState.Changed -=
                HandleFormChanged;
        }
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
        LineageSelection[] lineageSelections,
        BackgroundDefinition backgroundDefinition,
        List<TraitDefinition> resolvedTraitDefinitions)
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

        formState =
            GetComponent<PlayerFormState>();

        if (formState == null)
        {
            formState =
                gameObject.AddComponent<
                    PlayerFormState>();
        }

        formState.Configure(
            SubraceDefinition != null
                ? SubraceDefinition.bodyType
                : BodyType.Humanoid
        );

        PlayerWeaponLoadout weaponLoadout =
            GetComponent<PlayerWeaponLoadout>();

        if (weaponLoadout != null)
        {
            weaponLoadout.ConfigureForms(
                SubraceDefinition != null
                    ? SubraceDefinition.bodyType
                    : BodyType.Humanoid
            );
        }

        formState.Changed -=
            HandleFormChanged;

        formState.Changed +=
            HandleFormChanged;

        LineageSelections =
            lineageSelections ??
            new LineageSelection[0];

        BackgroundDefinition =
            backgroundDefinition;

        traitDefinitions.Clear();

        if (resolvedTraitDefinitions != null)
        {
            foreach (TraitDefinition traitDefinition
                     in resolvedTraitDefinitions)
            {
                if (traitDefinition != null)
                {
                    traitDefinitions.Add(
                        traitDefinition
                    );
                }
            }
        }

        ResolvePermanentAttributes();

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

    private void HandleFormChanged()
    {
        if (ProfileData == null ||
            PermanentAttributeOutput == null ||
            EffectiveAttributeOutput == null)
        {
            return;
        }

        CharacterGripProfile gripProfile =
            GetCurrentGripProfile();

        PermanentHandlingProfile =
            CharacterHandlingResolver.Resolve(
                SubraceDefinition,
                gripProfile,
                PermanentAttributeOutput
            );

        EffectiveHandlingProfile =
            CharacterHandlingResolver.Resolve(
                SubraceDefinition,
                gripProfile,
                EffectiveAttributeOutput
            );

        ApplyEquipmentRules();

        AttributesChanged?.Invoke();
    }

    private CharacterGripProfile
        GetCurrentGripProfile()
    {
        CharacterForm form =
            formState != null
                ? formState.CurrentForm
                : CharacterForm.Standing;

        if (SubraceDefinition == null)
        {
            return CharacterGripProfile
                .CreateHumanoidDefault();
        }

        CharacterGripProfile gripProfile =
            SubraceDefinition.GetGripProfile(
                form
            );

        return gripProfile ??
               CharacterGripProfile
                   .CreateHumanoidDefault();
    }

    private void ResolvePermanentAttributes()
    {
        List<LineageSelection> lineages =
            new List<LineageSelection>(
                LineageSelections
            );

        ResolvedCharacterStats liveResolvedStats =
            CharacterStatsResolver.ResolveCharacter(
                RaceDefinition,
                SubraceDefinition,
                lineages,
                BackgroundDefinition,
                traitDefinitions
            );

        CharacterAttributes savedCreatedAttributes =
            ProfileData.createdAttributes ??
            ProfileData.currentAttributes;

        CharacterAttributes savedCurrentAttributes =
            ProfileData.currentAttributes ??
            savedCreatedAttributes;

        CharacterAttributeModifiers
            permanentProgression =
                CharacterAttributeModifiers
                    .FromDifference(
                        savedCurrentAttributes,
                        savedCreatedAttributes
                    );

        PermanentAttributes =
            CharacterAttributes.ClampMinimum(
                CharacterAttributes.AddModifiers(
                    liveResolvedStats.finalAttributes,
                    permanentProgression
                ),
                1
            );

        PermanentAttributeOutput =
            ResolveAttributeOutput(
                PermanentAttributes
            );

        PermanentHandlingProfile =
            CharacterHandlingResolver.Resolve(
                SubraceDefinition,
                GetCurrentGripProfile(),
                PermanentAttributeOutput
            );
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

        EffectiveAttributeOutput =
            ResolveAttributeOutput(
                EffectiveAttributes
            );

        EffectiveHandlingProfile =
            CharacterHandlingResolver.Resolve(
                SubraceDefinition,
                GetCurrentGripProfile(),
                EffectiveAttributeOutput
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

    private CharacterAttributeOutput ResolveAttributeOutput(
        CharacterAttributes attributes)
    {
        CharacterAttributeScaling scaling =
            RaceDefinition != null
                ? RaceDefinition.attributeScaling
                : null;

        return CharacterAttributeOutputResolver.Resolve(
            attributes,
            scaling
        );
    }

    private CharacterBaseStats
        ResolveEffectiveBaseStats()
    {
        CharacterBaseStats liveBaseStats =
            CharacterStatsResolver.ResolveBaseStats(
                RaceDefinition,
                SubraceDefinition
            );

        CharacterBaseStats createdBaseStats =
            ProfileData != null
                ? ProfileData.createdBaseStats
                : null;

        CharacterBaseStats currentBaseStats =
            ProfileData != null
                ? ProfileData.currentBaseStats
                : null;

        if (createdBaseStats == null)
        {
            createdBaseStats =
                currentBaseStats ??
                liveBaseStats;
        }

        if (currentBaseStats == null)
        {
            currentBaseStats =
                createdBaseStats;
        }

        CharacterBaseStats
            permanentBaseProgression =
                CharacterBaseStats.FromDifference(
                    currentBaseStats,
                    createdBaseStats
                );

        CharacterBaseStats progressedBaseStats =
            CharacterBaseStats.Add(
                liveBaseStats,
                permanentBaseProgression
            );

        CharacterBaseStats effectiveBonuses =
            CharacterStatsResolver
                .ResolveAttributeStatBonuses(
                    EffectiveAttributes
                );

        return CharacterBaseStats.Add(
            progressedBaseStats,
            effectiveBonuses
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
        CharacterGripProfile gripProfile =
            GetCurrentGripProfile();

        PlayerGripState gripState =
            GetComponent<PlayerGripState>();

        if (gripState != null)
        {
            bool configured =
                gripState.Configure(
                    gripProfile
                );

            if (!configured)
            {
                Debug.LogWarning(
                    "Player grip state could not apply the new body grip configuration because currently held items occupy unavailable grips.",
                    this
                );
            }
        }

        PlayerEquipment playerEquipment =
            GetComponent<PlayerEquipment>();

        if (playerEquipment != null)
        {
            bool configured =
                playerEquipment.Configure(
                    SubraceDefinition
                );

            if (!configured)
            {
                Debug.LogWarning(
                    "Player equipment could not apply the new body equipment configuration because currently equipped items are incompatible with it.",
                    this
                );
            }
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