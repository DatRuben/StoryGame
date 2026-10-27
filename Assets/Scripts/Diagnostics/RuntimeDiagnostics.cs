using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Cinemachine;
using UnityEngine;

[Flags]
public enum RuntimeDiagnosticRequirement
{
    None = 0,

    PlayerCore = 1 << 0,
    Movement = 1 << 1,
    Camera = 1 << 2,
    Inventory = 1 << 3,
    ItemHandling = 1 << 4,
    WorldInteraction = 1 << 5,
    WorldItems = 1 << 6,
    Combat = 1 << 7,
    ElementalCombat = 1 << 8,
    UI = 1 << 9,

    FullGameplay =
        PlayerCore |
        Movement |
        Camera |
        Inventory |
        ItemHandling |
        WorldInteraction |
        WorldItems |
        Combat |
        ElementalCombat |
        UI
}

public enum RuntimeDiagnosticStatus
{
    Ready,
    Blocked,
    Optional,
    Missing,
    Misbound,
    Invalid,
    Warning
}

public readonly struct RuntimeDiagnosticCheck
{
    public string Category { get; }

    public string Id { get; }

    public string Label { get; }

    public RuntimeDiagnosticStatus Status
    {
        get;
    }

    public string Details { get; }

    public RuntimeDiagnosticCheck(
        string category,
        string id,
        string label,
        RuntimeDiagnosticStatus status,
        string details = "")
    {
        Category = category;
        Id = id;
        Label = label;
        Status = status;
        Details = details;
    }
}

[DisallowMultipleComponent]
public sealed class RuntimeDiagnostics :
    MonoBehaviour
{
    [Header("Expected Runtime")]

    [SerializeField]
    private RuntimeDiagnosticRequirement
        requiredSystems =
            RuntimeDiagnosticRequirement
                .FullGameplay;

    private readonly List<
        RuntimeDiagnosticCheck>
        checks =
            new List<
                RuntimeDiagnosticCheck>();

    public IReadOnlyList<
        RuntimeDiagnosticCheck>
        Checks =>
            checks;

    public RuntimeDiagnosticRequirement
        RequiredSystems
    {
        get
        {
            if ((int)requiredSystems == -1)
            {
                return RuntimeDiagnosticRequirement
                    .FullGameplay;
            }

            return requiredSystems;
        }
    }

    public void Refresh()
    {
        checks.Clear();

        PlayerSpawner playerSpawner =
            CheckSceneSingleton<
                PlayerSpawner>(
                "Scene",
                "scene.player_spawner",
                "Player Spawner",
                RuntimeDiagnosticRequirement
                    .PlayerCore
            );

        CharacterRuntimeBinder binder =
            CheckSceneSingleton<
                CharacterRuntimeBinder>(
                "Scene",
                "scene.runtime_binder",
                "Character Runtime Binder",
                RuntimeDiagnosticRequirement
                    .PlayerCore
            );

        WorldItemSpawner worldItemSpawner =
            CheckSceneSingleton<
                WorldItemSpawner>(
                "World",
                "world.item_spawner",
                "World Item Spawner",
                RuntimeDiagnosticRequirement
                    .WorldItems
            );

        ElementBuildupSystem
            elementBuildupSystem =
                CheckSceneSingleton<
                    ElementBuildupSystem>(
                    "Combat",
                    "combat.element_buildup.system",
                    "Element Buildup System",
                    RuntimeDiagnosticRequirement
                        .ElementalCombat
                );

        InventoryMenuController inventoryMenu =
            CheckSceneSingleton<
                InventoryMenuController>(
                "Inventory",
                "inventory.menu",
                "Inventory Menu Controller",
                RuntimeDiagnosticRequirement
                    .Inventory
            );

        Camera mainCamera =
            Camera.main;

        AddPresence(
            "Camera",
            "camera.main",
            "Main Camera",
            mainCamera,
            RuntimeDiagnosticRequirement.Camera
        );

        CheckBinderSceneReferences(
            binder,
            worldItemSpawner,
            inventoryMenu
        );

        CheckBinderUIReferences(
            binder
        );

        CheckWorldItemSystem(
            worldItemSpawner
        );

        CheckElementalCombat(
            elementBuildupSystem
        );

        GameObject player =
            playerSpawner != null
                ? playerSpawner.SpawnedPlayer
                : null;

        if (player == null)
        {
            AddCheck(
                "Player",
                "player.runtime",
                "Runtime Player",
                Requires(
                    RuntimeDiagnosticRequirement
                        .PlayerCore)
                    ? RuntimeDiagnosticStatus
                        .Warning
                    : RuntimeDiagnosticStatus
                        .Optional,
                "No runtime player is currently spawned."
            );

            return;
        }

        AddCheck(
            "Player",
            "player.runtime",
            "Runtime Player",
            RuntimeDiagnosticStatus.Ready,
            player.name
        );

        CheckPlayerCore(
            player
        );

        CheckCapabilities(
            player
        );

        CheckMovement(
            player,
            mainCamera
        );

        CheckCamera(
            player,
            binder,
            mainCamera
        );

        CheckInventory(
            player,
            inventoryMenu,
            worldItemSpawner
        );

        CheckCombat(
            player
        );
    }

    private void CheckPlayerCore(
        GameObject player)
    {
        PlayerCharacterProfile profile =
            CheckPlayerComponent<
                PlayerCharacterProfile>(
                player,
                "Player",
                "player.profile",
                "Character Profile",
                RuntimeDiagnosticRequirement
                    .PlayerCore
            );

        EntityResources resources =
            CheckPlayerComponent<
                EntityResources>(
                player,
                "Player",
                "player.resources",
                "Entity Resources",
                RuntimeDiagnosticRequirement
                    .PlayerCore
            );

        PlayerDownedState downedState =
            CheckPlayerComponent<
                PlayerDownedState>(
                player,
                "Player",
                "player.downed_state",
                "Downed State",
                RuntimeDiagnosticRequirement
                    .PlayerCore
            );

        CheckPlayerComponent<
            StatusEffects>(
                player,
                "Player",
                "player.status_effects",
                "Status Effects",
                RuntimeDiagnosticRequirement
                    .PlayerCore
            );

        if (profile != null)
        {
            AddCheck(
                "Player",
                "player.profile.initialized",
                "Character Profile Data",
                profile.ProfileData != null
                    ? RuntimeDiagnosticStatus.Ready
                    : RuntimeDiagnosticStatus.Invalid,
                profile.ProfileData != null
                    ? profile.ProfileData
                        .characterName
                    : "PlayerCharacterProfile has not been initialized."
            );
        }

        if (resources != null)
        {
            AddCheck(
                "Player",
                "player.resources.initialized",
                "Resources Initialized",
                resources.IsInitialized
                    ? RuntimeDiagnosticStatus.Ready
                    : RuntimeDiagnosticStatus.Warning,
                resources.IsInitialized
                    ? "Runtime resource maximums have been applied."
                    : "EntityResources exists but has not been initialized."
            );
        }

        if (downedState != null)
        {
            AddCheck(
                "Player",
                "player.downed",
                "Player Downed",
                downedState.IsDowned
                    ? RuntimeDiagnosticStatus.Blocked
                    : RuntimeDiagnosticStatus.Ready,
                downedState.IsDowned
                    ? "Player is currently downed."
                    : "Player is active."
            );
        }
    }

    private void CheckCapabilities(
        GameObject player)
    {
        PlayerGameplayState gameplayState =
            player.GetComponent<
                PlayerGameplayState>();

        if (gameplayState == null)
            return;

        CheckCapability(
            gameplayState,
            PlayerGameplayCapability.Movement,
            "Movement"
        );

        CheckCapability(
            gameplayState,
            PlayerGameplayCapability.Combat,
            "Combat"
        );

        CheckCapability(
            gameplayState,
            PlayerGameplayCapability.Inventory,
            "Inventory"
        );

        CheckCapability(
            gameplayState,
            PlayerGameplayCapability.WorldInteraction,
            "World Interaction"
        );

        CheckCapability(
            gameplayState,
            PlayerGameplayCapability.ItemHandling,
            "Item Handling"
        );
    }

    private void CheckMovement(
        GameObject player,
        Camera mainCamera)
    {
        Rigidbody rigidbody =
            CheckPlayerComponent<
                Rigidbody>(
                player,
                "Movement",
                "movement.rigidbody",
                "Rigidbody",
                RuntimeDiagnosticRequirement
                    .Movement
            );

        PlayerInputRouter inputRouter =
            CheckPlayerComponent<
                PlayerInputRouter>(
                player,
                "Movement",
                "movement.input_router",
                "Player Input Router",
                RuntimeDiagnosticRequirement
                    .Movement
            );

        PlayerInput playerInput =
            CheckPlayerComponent<
                PlayerInput>(
                player,
                "Movement",
                "movement.player_input",
                "Player Input",
                RuntimeDiagnosticRequirement
                    .Movement
            );

        if (playerInput == null)
            return;

        CheckReferenceField(
            "Movement",
            "movement.binding.input_router",
            "Player Input → Input Router",
            playerInput,
            "inputRouter",
            inputRouter,
            RuntimeDiagnosticRequirement
                .Movement
        );

        CheckReferenceField(
            "Movement",
            "movement.binding.resources",
            "Player Input → Resources",
            playerInput,
            "playerResources",
            player.GetComponent<
                EntityResources>(),
            RuntimeDiagnosticRequirement
                .Movement
        );

        CheckReferenceField(
            "Movement",
            "movement.binding.gameplay_state",
            "Player Input → Gameplay State",
            playerInput,
            "gameplayState",
            player.GetComponent<
                PlayerGameplayState>(),
            RuntimeDiagnosticRequirement
                .Movement
        );

        CheckReferenceField(
            "Movement",
            "movement.binding.primary_action",
            "Player Input → Primary Action",
            playerInput,
            "primaryActionController",
            player.GetComponent<
                PlayerPrimaryActionController>(),
            RuntimeDiagnosticRequirement
                .Movement
        );

        CheckGroundConfiguration(
            playerInput
        );

        if (mainCamera != null)
        {
            CheckReferenceField(
                "Camera",
                "camera.binding.player_input",
                "Player Input → Main Camera",
                playerInput,
                "playerCamera",
                mainCamera,
                RuntimeDiagnosticRequirement
                    .Camera
            );
        }

        if (rigidbody != null)
        {
            AddCheck(
                "Movement",
                "movement.rigidbody.kinematic",
                "Rigidbody Physics",
                rigidbody.isKinematic
                    ? RuntimeDiagnosticStatus.Warning
                    : RuntimeDiagnosticStatus.Ready,
                rigidbody.isKinematic
                    ? "Runtime player Rigidbody is kinematic."
                    : "Runtime physics is active."
            );
        }
    }

    private void CheckGroundConfiguration(
        PlayerInput playerInput)
    {
        bool readGroundCheck =
            TryReadField(
                playerInput,
                "groundCheck",
                out Transform groundCheck
            );

        bool readGroundLayer =
            TryReadField(
                playerInput,
                "groundLayer",
                out LayerMask groundLayer
            );

        if (!readGroundCheck ||
            !readGroundLayer)
        {
            AddCheck(
                "Movement",
                "movement.ground_configuration",
                "Ground Detection",
                RuntimeDiagnosticStatus.Warning,
                "Diagnostics could not inspect PlayerInput ground configuration."
            );

            return;
        }

        bool valid =
            groundCheck != null &&
            groundLayer.value != 0;

        AddCheck(
            "Movement",
            "movement.ground_configuration",
            "Ground Detection",
            valid
                ? RuntimeDiagnosticStatus.Ready
                : RuntimeDiagnosticStatus.Invalid,
            valid
                ? "Ground check and ground layer mask are configured."
                : "Ground check is missing or the ground layer mask is empty."
        );
    }

    private void CheckCamera(
        GameObject player,
        CharacterRuntimeBinder binder,
        Camera mainCamera)
    {
        if (binder == null)
            return;

        bool readCinemachine =
            TryReadField(
                binder,
                "cinemachineCamera",
                out CinemachineCamera
                    cinemachineCamera
            );

        if (!readCinemachine)
        {
            AddInspectionWarning(
                "Camera",
                "camera.cinemachine",
                "Cinemachine Camera",
                binder,
                "cinemachineCamera"
            );
        }
        else
        {
            AddPresence(
                "Camera",
                "camera.cinemachine",
                "Cinemachine Camera",
                cinemachineCamera,
                RuntimeDiagnosticRequirement
                    .Camera
            );
        }

        bool readCollision =
            TryReadField(
                binder,
                "cameraCollision",
                out CameraCollision
                    cameraCollision
            );

        if (!readCollision)
        {
            AddInspectionWarning(
                "Camera",
                "camera.collision",
                "Camera Collision",
                binder,
                "cameraCollision"
            );
        }
        else
        {
            AddPresence(
                "Camera",
                "camera.collision",
                "Camera Collision",
                cameraCollision,
                RuntimeDiagnosticRequirement
                    .Camera
            );
        }

        if (cinemachineCamera != null)
        {
            Transform follow =
                cinemachineCamera.Follow;

            bool validFollow =
                IsPlayerTransform(
                    follow,
                    player
                );

            AddCheck(
                "Camera",
                "camera.cinemachine.follow",
                "Cinemachine Follow Target",
                validFollow
                    ? RuntimeDiagnosticStatus.Ready
                    : RuntimeDiagnosticStatus.Misbound,
                follow == null
                    ? "No follow target is assigned."
                    : validFollow
                        ? follow.name
                        : "Follow target is not part of the runtime player."
            );
        }

        if (cameraCollision != null)
        {
            if (!TryReadField(
                    cameraCollision,
                    "cameraPivot",
                    out Transform cameraPivot))
            {
                AddInspectionWarning(
                    "Camera",
                    "camera.collision.pivot",
                    "Camera Collision Pivot",
                    cameraCollision,
                    "cameraPivot"
                );
            }
            else
            {
                bool validPivot =
                    IsPlayerTransform(
                        cameraPivot,
                        player
                    );

                AddCheck(
                    "Camera",
                    "camera.collision.pivot",
                    "Camera Collision Pivot",
                    validPivot
                        ? RuntimeDiagnosticStatus.Ready
                        : RuntimeDiagnosticStatus.Misbound,
                    cameraPivot == null
                        ? "No camera pivot is assigned."
                        : validPivot
                            ? cameraPivot.name
                            : "Camera pivot is not part of the runtime player."
                );
            }
        }

        if (mainCamera != null &&
            cameraCollision != null)
        {
            CameraCollision
                mainCameraCollision =
                    mainCamera.GetComponent<
                        CameraCollision>();

            if (mainCameraCollision != null)
            {
                AddCheck(
                    "Camera",
                    "camera.collision.owner",
                    "Main Camera Collision",
                    ReferenceEquals(
                        mainCameraCollision,
                        cameraCollision)
                        ? RuntimeDiagnosticStatus.Ready
                        : RuntimeDiagnosticStatus.Misbound,
                    ReferenceEquals(
                        mainCameraCollision,
                        cameraCollision)
                        ? "CharacterRuntimeBinder uses the Main Camera collision component."
                        : "CharacterRuntimeBinder references a different CameraCollision component."
                );
            }
        }
    }

    private void CheckInventory(
        GameObject player,
        InventoryMenuController inventoryMenu,
        WorldItemSpawner worldItemSpawner)
    {
        CheckPlayerComponent<
            InventoryContainer>(
                player,
                "Inventory",
                "inventory.container",
                "Inventory Container",
                RuntimeDiagnosticRequirement
                    .Inventory
            );

        CheckPlayerComponent<
            PlayerEquipment>(
                player,
                "Inventory",
                "inventory.equipment",
                "Player Equipment",
                RuntimeDiagnosticRequirement
                    .Inventory
            );

        CheckPlayerComponent<
            PlayerWeaponLoadout>(
                player,
                "Inventory",
                "inventory.weapon_loadout",
                "Weapon Loadout",
                RuntimeDiagnosticRequirement
                    .Inventory
            );

        PlayerGripState gripState =
            CheckPlayerComponent<
                PlayerGripState>(
                player,
                "Item Handling",
                "item_handling.grip_state",
                "Grip State",
                RuntimeDiagnosticRequirement
                    .ItemHandling
            );

        InventoryInteractionController
            interactionController =
                CheckPlayerComponent<
                    InventoryInteractionController>(
                    player,
                    "Item Handling",
                    "item_handling.interaction",
                    "Inventory Interaction Controller",
                    RuntimeDiagnosticRequirement
                        .ItemHandling
                );

        PlayerItemHandlingController
            itemHandlingController =
                CheckPlayerComponent<
                    PlayerItemHandlingController>(
                    player,
                    "Item Handling",
                    "item_handling.controller",
                    "Item Handling Controller",
                    RuntimeDiagnosticRequirement
                        .ItemHandling
                );

        PlayerHeldItemPresenter
            heldItemPresenter =
                CheckPlayerComponent<
                    PlayerHeldItemPresenter>(
                    player,
                    "Item Handling",
                    "item_handling.presenter",
                    "Held Item Presenter",
                    RuntimeDiagnosticRequirement
                        .ItemHandling
                );

        PlayerStorageContainerInteract
            storageInteract =
                CheckPlayerComponent<
                    PlayerStorageContainerInteract>(
                    player,
                    "World Interaction",
                    "world_interaction.storage",
                    "Storage Interaction",
                    RuntimeDiagnosticRequirement
                        .WorldInteraction
                );

        if (interactionController != null)
        {
            CheckReferenceField(
                "Item Handling",
                "item_handling.binding.interaction_world_spawner",
                "Inventory Interaction → World Item Spawner",
                interactionController,
                "worldItemSpawner",
                worldItemSpawner,
                RuntimeDiagnosticRequirement
                    .WorldItems
            );
        }

        if (itemHandlingController != null)
        {
            CheckReferenceField(
                "Item Handling",
                "item_handling.binding.controller_world_spawner",
                "Item Handling → World Item Spawner",
                itemHandlingController,
                "worldItemSpawner",
                worldItemSpawner,
                RuntimeDiagnosticRequirement
                    .WorldItems
            );

            CheckReferenceField(
                "Item Handling",
                "item_handling.binding.grip_state",
                "Item Handling → Grip State",
                itemHandlingController,
                "gripState",
                gripState,
                RuntimeDiagnosticRequirement
                    .ItemHandling
            );

            CheckReferenceField(
                "Item Handling",
                "item_handling.binding.presenter",
                "Item Handling → Held Item Presenter",
                itemHandlingController,
                "heldItemPresenter",
                heldItemPresenter,
                RuntimeDiagnosticRequirement
                    .ItemHandling
            );
        }

        if (heldItemPresenter != null &&
            worldItemSpawner != null)
        {
            CheckReferenceField(
                "Item Handling",
                "item_handling.binding.visual_resolver",
                "Held Item → Item Visual Resolver",
                heldItemPresenter,
                "visualResolver",
                worldItemSpawner.VisualResolver,
                RuntimeDiagnosticRequirement
                    .ItemHandling
            );
        }

        if (inventoryMenu != null)
        {
            PlayerInputRouter inputRouter =
                player.GetComponent<
                    PlayerInputRouter>();

            PlayerGameplayState gameplayState =
                player.GetComponent<
                    PlayerGameplayState>();

            CheckReferenceField(
                "Inventory",
                "inventory.binding.input",
                "Inventory Menu → Input Router",
                inventoryMenu,
                "inputRouter",
                inputRouter,
                RuntimeDiagnosticRequirement
                    .Inventory
            );

            CheckReferenceField(
                "Inventory",
                "inventory.binding.storage",
                "Inventory Menu → Storage Interaction",
                inventoryMenu,
                "storageInteract",
                storageInteract,
                RuntimeDiagnosticRequirement
                    .WorldInteraction
            );

            CheckReferenceField(
                "Inventory",
                "inventory.binding.interaction",
                "Inventory Menu → Inventory Interaction",
                inventoryMenu,
                "interactionController",
                interactionController,
                RuntimeDiagnosticRequirement
                    .Inventory
            );

            CheckReferenceField(
                "Inventory",
                "inventory.binding.gameplay_state",
                "Inventory Menu → Gameplay State",
                inventoryMenu,
                "gameplayState",
                gameplayState,
                RuntimeDiagnosticRequirement
                    .Inventory
            );
        }
    }

    private void CheckWorldItemSystem(
        WorldItemSpawner worldItemSpawner)
    {
        if (worldItemSpawner == null)
            return;

        if (!TryReadField(
                worldItemSpawner,
                "worldItemPrefab",
                out WorldItem worldItemPrefab))
        {
            AddInspectionWarning(
                "World",
                "world.item_prefab",
                "World Item Prefab",
                worldItemSpawner,
                "worldItemPrefab"
            );
        }
        else
        {
            AddPresence(
                "World",
                "world.item_prefab",
                "World Item Prefab",
                worldItemPrefab,
                RuntimeDiagnosticRequirement
                    .WorldItems
            );
        }

        ItemVisualResolver resolver =
            worldItemSpawner.VisualResolver;

        AddPresence(
            "World",
            "world.item_visual_resolver",
            "Item Visual Resolver",
            resolver,
            RuntimeDiagnosticRequirement
                .WorldItems
        );

        if (resolver == null)
            return;

        if (!TryReadField(
                resolver,
                "visualDefaults",
                out ItemVisualDefaults
                    visualDefaults))
        {
            AddInspectionWarning(
                "World",
                "world.item_visual_defaults",
                "Item Visual Defaults",
                resolver,
                "visualDefaults"
            );

            return;
        }

        AddPresence(
            "World",
            "world.item_visual_defaults",
            "Item Visual Defaults",
            visualDefaults,
            RuntimeDiagnosticRequirement
                .WorldItems
        );

        if (visualDefaults == null)
            return;

        AddPresence(
            "World",
            "world.item_fallback_prefab",
            "Fallback Item Visual",
            visualDefaults.FallbackWorldPrefab,
            RuntimeDiagnosticRequirement
                .WorldItems
        );
    }

    private void CheckCombat(
        GameObject player)
    {
        PlayerWeaponDeployment
            weaponDeployment =
                CheckPlayerComponent<
                    PlayerWeaponDeployment>(
                    player,
                    "Combat",
                    "combat.weapon_deployment",
                    "Weapon Deployment",
                    RuntimeDiagnosticRequirement
                        .Combat
                );

        PlayerGameplayState gameplayState =
            player.GetComponent<
                PlayerGameplayState>();

        EntityCombatState combatState =
            CheckPlayerComponent<
                EntityCombatState>(
                player,
                "Combat",
                "combat.entity_state",
                "Entity Combat State",
                RuntimeDiagnosticRequirement
                    .Combat
            );

        PlayerCombatController
            combatController =
                CheckPlayerComponent<
                    PlayerCombatController>(
                    player,
                    "Combat",
                    "combat.player_controller",
                    "Player Combat Controller",
                    RuntimeDiagnosticRequirement
                        .Combat
                );

        CheckPlayerComponent<
            PlayerPrimaryActionController>(
                player,
                "Combat",
                "combat.primary_action",
                "Primary Action Controller",
                RuntimeDiagnosticRequirement
                    .Combat
            );

        CombatSkillLoadout skillLoadout =
            CheckPlayerComponent<
                CombatSkillLoadout>(
                player,
                "Combat",
                "combat.skill_loadout",
                "Combat Skill Loadout",
                RuntimeDiagnosticRequirement
                    .Combat
            );

        CombatSkillController
            skillController =
                CheckPlayerComponent<
                    CombatSkillController>(
                    player,
                    "Combat",
                    "combat.skill_controller",
                    "Combat Skill Controller",
                    RuntimeDiagnosticRequirement
                        .Combat
                );

        CheckPlayerComponent<
            EntityElementalState>(
                player,
                "Combat",
                "combat.elemental_state",
                "Entity Elemental State",
                RuntimeDiagnosticRequirement
                    .ElementalCombat
            );

        if (combatController != null)
        {
            CheckReferenceField(
                "Combat",
                "combat.binding.weapon_deployment",
                "Combat Controller → Weapon Deployment",
                combatController,
                "weaponDeployment",
                weaponDeployment,
                RuntimeDiagnosticRequirement
                    .Combat
            );

            CheckReferenceField(
                "Combat",
                "combat.binding.gameplay_state",
                "Combat Controller → Gameplay State",
                combatController,
                "gameplayState",
                gameplayState,
                RuntimeDiagnosticRequirement
                    .Combat
            );

            CheckReferenceField(
                "Combat",
                "combat.binding.combat_state",
                "Combat Controller → Entity Combat State",
                combatController,
                "combatState",
                combatState,
                RuntimeDiagnosticRequirement
                    .Combat
            );
        }

        if (skillController != null)
        {
            CheckReferenceField(
                "Combat",
                "combat.skill_binding.loadout",
                "Skill Controller → Skill Loadout",
                skillController,
                "skillLoadout",
                skillLoadout,
                RuntimeDiagnosticRequirement
                    .Combat
            );

            CheckReferenceField(
                "Combat",
                "combat.skill_binding.gameplay_state",
                "Skill Controller → Gameplay State",
                skillController,
                "gameplayState",
                gameplayState,
                RuntimeDiagnosticRequirement
                    .Combat
            );
        }

        SimpleEnemyAttackAI[] enemyAIs =
            FindSceneObjects<
                SimpleEnemyAttackAI>();

        if (enemyAIs.Length > 0)
        {
            AddCheck(
                "Combat",
                "combat.target_eligibility",
                "NPC Target Eligibility",
                RuntimeDiagnosticStatus.Warning,
                "Target eligibility is not centralized yet. SimpleEnemyAttackAI still finds PlayerCharacterProfile directly."
            );
        }
    }

    private void CheckElementalCombat(
        ElementBuildupSystem system)
    {
        if (system == null)
            return;

        if (!system.HasBuildupRules)
        {
            AddCheck(
                "Combat",
                "combat.element_buildup.rules",
                "Element Buildup Rules",
                RuntimeDiagnosticStatus.Missing,
                "ElementBuildupSystem has no rules asset assigned."
            );

            return;
        }

        AddCheck(
            "Combat",
            "combat.element_buildup.rules",
            "Element Buildup Rules",
            RuntimeDiagnosticStatus.Ready,
            "Rules asset is assigned."
        );

        CombatElement[] elements =
        {
            CombatElement.Fire,
            CombatElement.Ice,
            CombatElement.Water,
            CombatElement.Earth,
            CombatElement.Light,
            CombatElement.Dark
        };

        for (int i = 0;
             i < elements.Length;
             i++)
        {
            CombatElement element =
                elements[i];

            bool exists =
                system.TryGetRule(
                    element,
                    out _
                );

            AddCheck(
                "Combat",
                "combat.element_buildup.rule." +
                element
                    .ToString()
                    .ToLowerInvariant(),
                element +
                " Buildup Rule",
                exists
                    ? RuntimeDiagnosticStatus.Ready
                    : RuntimeDiagnosticStatus.Invalid,
                exists
                    ? "Rule exists."
                    : "No buildup rule exists for " +
                      element +
                      "."
            );
        }
    }

    private void CheckBinderSceneReferences(
        CharacterRuntimeBinder binder,
        WorldItemSpawner worldItemSpawner,
        InventoryMenuController inventoryMenu)
    {
        if (binder == null)
            return;

        CheckReferenceField(
            "Scene",
            "scene.binding.world_item_spawner",
            "Runtime Binder → World Item Spawner",
            binder,
            "worldItemSpawner",
            worldItemSpawner,
            RuntimeDiagnosticRequirement
                .WorldItems
        );

        CheckReferenceField(
            "Scene",
            "scene.binding.inventory_menu",
            "Runtime Binder → Inventory Menu",
            binder,
            "inventoryMenuController",
            inventoryMenu,
            RuntimeDiagnosticRequirement
                .Inventory
        );
    }

    private void CheckBinderUIReferences(
        CharacterRuntimeBinder binder)
    {
        if (binder == null)
            return;

        CheckFieldPresence<
            InventoryGridUI>(
                "UI",
                "ui.inventory_grid",
                "Inventory Grid UI",
                binder,
                "inventoryGridUI",
                RuntimeDiagnosticRequirement.UI
            );

        CheckFieldPresence<
            InventoryFollow>(
                "UI",
                "ui.inventory_follow",
                "Inventory Follow",
                binder,
                "inventoryFollow",
                RuntimeDiagnosticRequirement.UI
            );

        CheckFieldPresence<
            HeldItemUI>(
                "UI",
                "ui.held_item",
                "Held Item UI",
                binder,
                "heldItemUI",
                RuntimeDiagnosticRequirement.UI
            );

        CheckFieldPresence<
            PlayerResourcesUI>(
                "UI",
                "ui.resources",
                "Player Resources UI",
                binder,
                "playerResourcesUI",
                RuntimeDiagnosticRequirement.UI
            );

        CheckFieldPresence<
            PlayerCharacterInfoUI>(
                "UI",
                "ui.character_info",
                "Character Info UI",
                binder,
                "playerCharacterInfoUI",
                RuntimeDiagnosticRequirement.UI
            );

        CheckFieldPresence<
            PlayerCrosshair>(
                "UI",
                "ui.crosshair",
                "Player Crosshair",
                binder,
                "playerCrosshair",
                RuntimeDiagnosticRequirement.UI
            );

        CheckFieldPresence<
            DamageNumberManager>(
                "UI",
                "ui.damage_numbers",
                "Damage Number Manager",
                binder,
                "damageNumberManager",
                RuntimeDiagnosticRequirement.UI
            );

        CheckFieldPresence<
            PlayerStatusEffectsUI>(
                "UI",
                "ui.status_effects",
                "Status Effects UI",
                binder,
                "playerStatusEffectsUI",
                RuntimeDiagnosticRequirement.UI
            );

        CheckFieldPresence<
            ItemHandlingProgressUI>(
                "UI",
                "ui.item_handling_progress",
                "Item Handling Progress UI",
                binder,
                "itemHandlingProgressUI",
                RuntimeDiagnosticRequirement.UI
            );
    }

    private void CheckCapability(
        PlayerGameplayState gameplayState,
        PlayerGameplayCapability capability,
        string label)
    {
        bool allowed =
            gameplayState.Allows(
                capability
            );

        AddCheck(
            "Capabilities",
            "capability." +
            capability
                .ToString()
                .ToLowerInvariant(),
            label,
            allowed
                ? RuntimeDiagnosticStatus.Ready
                : RuntimeDiagnosticStatus.Blocked,
            allowed
                ? "Currently allowed."
                : "Currently blocked by PlayerGameplayState."
        );
    }

    private T CheckPlayerComponent<T>(
        GameObject player,
        string category,
        string id,
        string label,
        RuntimeDiagnosticRequirement requirement)
        where T : Component
    {
        T component =
            player != null
                ? player.GetComponent<T>()
                : null;

        AddPresence(
            category,
            id,
            label,
            component,
            requirement
        );

        return component;
    }

    private T CheckSceneSingleton<T>(
        string category,
        string id,
        string label,
        RuntimeDiagnosticRequirement requirement)
        where T : Component
    {
        T[] instances =
            FindSceneObjects<T>();

        if (instances.Length == 1)
        {
            AddCheck(
                category,
                id,
                label,
                RuntimeDiagnosticStatus.Ready,
                instances[0].name
            );

            return instances[0];
        }

        if (instances.Length == 0)
        {
            AddCheck(
                category,
                id,
                label,
                MissingStatus(
                    requirement
                ),
                Requires(requirement)
                    ? "Required runtime system is missing."
                    : "Not required by the current diagnostic configuration."
            );

            return null;
        }

        AddCheck(
            category,
            id,
            label,
            RuntimeDiagnosticStatus.Invalid,
            instances.Length +
            " instances exist. Expected one."
        );

        return instances[0];
    }

    private T[] FindSceneObjects<T>()
        where T : UnityEngine.Object
    {
        return FindObjectsByType<T>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
    }

    private void CheckFieldPresence<T>(
        string category,
        string id,
        string label,
        object source,
        string fieldName,
        RuntimeDiagnosticRequirement requirement)
        where T : UnityEngine.Object
    {
        if (!TryReadField(
                source,
                fieldName,
                out T value))
        {
            AddInspectionWarning(
                category,
                id,
                label,
                source,
                fieldName
            );

            return;
        }

        AddPresence(
            category,
            id,
            label,
            value,
            requirement
        );
    }

    private void CheckReferenceField<T>(
        string category,
        string id,
        string label,
        object source,
        string fieldName,
        T expected,
        RuntimeDiagnosticRequirement requirement)
        where T : UnityEngine.Object
    {
        if (!TryReadField(
                source,
                fieldName,
                out T actual))
        {
            AddInspectionWarning(
                category,
                id,
                label,
                source,
                fieldName
            );

            return;
        }

        if (expected == null)
        {
            AddCheck(
                category,
                id,
                label,
                Requires(requirement)
                    ? RuntimeDiagnosticStatus.Warning
                    : RuntimeDiagnosticStatus.Optional,
                "The authoritative object is unavailable, so this binding cannot be validated."
            );

            return;
        }

        if (actual == null)
        {
            AddCheck(
                category,
                id,
                label,
                MissingStatus(
                    requirement
                ),
                "No runtime binding exists."
            );

            return;
        }

        if (!ReferenceEquals(
                actual,
                expected))
        {
            AddCheck(
                category,
                id,
                label,
                RuntimeDiagnosticStatus.Misbound,
                "Bound to '" +
                actual.name +
                "' but expected '" +
                expected.name +
                "'."
            );

            return;
        }

        AddCheck(
            category,
            id,
            label,
            RuntimeDiagnosticStatus.Ready,
            actual.name
        );
    }

    private bool TryReadField<T>(
        object source,
        string fieldName,
        out T value)
    {
        value = default;

        if (source == null ||
            string.IsNullOrWhiteSpace(
                fieldName))
        {
            return false;
        }

        FieldInfo field =
            source.GetType().GetField(
                fieldName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        if (field == null)
            return false;

        object rawValue =
            field.GetValue(source);

        if (rawValue == null)
            return true;

        if (!(rawValue is T typedValue))
            return false;

        value = typedValue;

        return true;
    }

    private void AddPresence(
        string category,
        string id,
        string label,
        UnityEngine.Object value,
        RuntimeDiagnosticRequirement requirement)
    {
        if (value != null)
        {
            AddCheck(
                category,
                id,
                label,
                RuntimeDiagnosticStatus.Ready,
                value.name
            );

            return;
        }

        AddCheck(
            category,
            id,
            label,
            MissingStatus(
                requirement
            ),
            Requires(requirement)
                ? "Required component, object, or service is missing."
                : "Not required by the current diagnostic configuration."
        );
    }

    private void AddInspectionWarning(
        string category,
        string id,
        string label,
        object source,
        string fieldName)
    {
        string sourceName =
            source != null
                ? source.GetType().Name
                : "unknown";

        AddCheck(
            category,
            id,
            label,
            RuntimeDiagnosticStatus.Warning,
            "Diagnostics could not inspect '" +
            fieldName +
            "' on " +
            sourceName +
            ". The runtime class may have changed."
        );
    }

    private bool IsPlayerTransform(
        Transform target,
        GameObject player)
    {
        if (target == null ||
            player == null)
        {
            return false;
        }

        return
            target == player.transform ||
            target.IsChildOf(
                player.transform
            );
    }

    private RuntimeDiagnosticStatus
        MissingStatus(
            RuntimeDiagnosticRequirement
                requirement)
    {
        return Requires(requirement)
            ? RuntimeDiagnosticStatus.Missing
            : RuntimeDiagnosticStatus.Optional;
    }

    private bool Requires(
        RuntimeDiagnosticRequirement
            requirement)
    {
        return
            (RequiredSystems &
             requirement) != 0;
    }

    private void AddCheck(
        string category,
        string id,
        string label,
        RuntimeDiagnosticStatus status,
        string details)
    {
        checks.Add(
            new RuntimeDiagnosticCheck(
                category,
                id,
                label,
                status,
                details
            )
        );
    }
}