using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(
    typeof(PlayerWeaponLoadout),
    typeof(PlayerGripState),
    typeof(PlayerCharacterProfile)
)]
[RequireComponent(typeof(PlayerGameplayState))]

public sealed class PlayerWeaponDeployment :
    MonoBehaviour
{
    private readonly List<InventoryItemInstance>
        activeWeapons =
            new List<InventoryItemInstance>();

    private readonly List<InventoryItemInstance>
        conventionalWeapons =
            new List<InventoryItemInstance>();

    private readonly List<ItemDefinition>
        conventionalDefinitions =
            new List<ItemDefinition>();

    private readonly List<ResolvedItemHandling>
        usePlan =
            new List<ResolvedItemHandling>();

    private PlayerWeaponLoadout weaponLoadout;
    private PlayerGripState gripState;
    private PlayerCharacterProfile characterProfile;
    private PlayerEquipment playerEquipment;

    private PlayerGameplayState gameplayState;

    private InventoryInteractionController
        interactionController;

    private bool loadoutDeployed;

    public bool WeaponsDrawn =>
        loadoutDeployed;

    public bool TryGetPrimaryDeployedWeapon(
        out InventoryItemInstance weapon)
    {
        ResolveReferences();

        weapon = null;

        if (!loadoutDeployed ||
            weaponLoadout == null)
        {
            return false;
        }

        for (int slotIndex = 0;
             slotIndex < WeaponSet.SlotCount;
             slotIndex++)
        {
            InventoryItemInstance candidate =
                weaponLoadout.GetActiveWeapon(
                    slotIndex
                );

            if (candidate == null ||
                candidate.IsEmpty ||
                candidate.Definition == null ||
                !IsDeployed(candidate))
            {
                continue;
            }

            weapon = candidate;
            return true;
        }

        return false;
    }

    public event Action Changed;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();

        if (gameplayState == null)
            return;

        gameplayState.OnCapabilitiesInterrupted -=
            HandleCapabilitiesInterrupted;

        gameplayState.OnCapabilitiesInterrupted +=
            HandleCapabilitiesInterrupted;

        if (!gameplayState.Allows(
            PlayerGameplayCapability.ItemHandling))
        {
            SheatheWeapons();
        }
    }

    private void OnDisable()
    {
        if (gameplayState == null)
            return;

        gameplayState.OnCapabilitiesInterrupted -=
            HandleCapabilitiesInterrupted;
    }

    public bool IsDeployed(
        InventoryItemInstance weapon)
    {
        ResolveReferences();

        if (!loadoutDeployed ||
            !IsActiveWeapon(weapon) ||
            weapon == null ||
            weapon.Definition == null)
        {
            return false;
        }

        if (weapon.Definition.IsConventionalWeapon)
        {
            return gripState != null &&
                   gripState.IsHolding(
                       weapon
                   );
        }

        if (weapon.Definition.IsAttachedWeapon)
        {
            return playerEquipment != null &&
                   playerEquipment.IsEquipped(
                       weapon
                   );
        }

        return false;
    }

    public bool DrawWeapons()
    {
        ResolveReferences();

        if (gameplayState != null &&
            !gameplayState.Allows(
                PlayerGameplayCapability.ItemHandling))
        {
            return false;
        }

        if (weaponLoadout == null ||
            gripState == null)
        {
            return false;
        }

        CollectActiveWeapons();

        if (WeaponsDrawn)
            return true;

        ReleaseActiveConventionalWeapons();

        if (conventionalWeapons.Count > 0 &&
            interactionController != null &&
            !interactionController
                .TryStoreOrDropLooseHeldItems())
        {
            return false;
        }

        if (!ValidateActiveAttachments())
            return false;

        if (conventionalWeapons.Count > 0)
        {
            if (characterProfile == null ||
                characterProfile
                    .EffectiveHandlingProfile ==
                    null)
            {
                return false;
            }

            CharacterHandlingProfile handling =
                characterProfile
                    .EffectiveHandlingProfile;

            if (!WeaponUsePlanResolver
                .TryResolve(
                    conventionalDefinitions,
                    handling,
                    gripState.GetFreeGripCount(
                        GripType.Hand
                    ),
                    gripState.GetFreeGripCount(
                        GripType.Mouth
                    ),
                    usePlan))
            {
                return false;
            }

            int deployedCount = 0;

            for (int i = 0;
                 i < conventionalWeapons.Count;
                 i++)
            {
                ResolvedItemHandling resolved =
                    usePlan[i];

                if (resolved == null ||
                    !gripState.TryHold(
                        conventionalWeapons[i],
                        resolved.gripType,
                        resolved.assignedGripCount))
                {
                    RollbackDeployment(
                        deployedCount
                    );

                    return false;
                }

                deployedCount++;
            }
        }

        loadoutDeployed = true;

        Changed?.Invoke();

        return true;
    }

    public bool SheatheWeapons()
    {
        ResolveReferences();

        bool wasDeployed =
            loadoutDeployed;

        bool releasedWeapons =
            ReleaseActiveConventionalWeapons();

        loadoutDeployed = false;

        bool changed =
            wasDeployed ||
            releasedWeapons;

        if (changed)
            Changed?.Invoke();

        return changed;
    }

    public bool ToggleWeaponsDrawn()
    {
        if (WeaponsDrawn)
            return SheatheWeapons();

        return DrawWeapons();
    }

    public bool SetActiveWeaponSet(
        int setIndex)
    {
        ResolveReferences();

        if (weaponLoadout == null ||
            characterProfile == null ||
            weaponLoadout.GetWeaponSet(
                setIndex) == null)
        {
            return false;
        }

        if (weaponLoadout
                .ActiveWeaponSetIndex ==
            setIndex)
        {
            return true;
        }

        int previousSetIndex =
            weaponLoadout
                .ActiveWeaponSetIndex;

        CharacterForm previousForm =
            weaponLoadout.GetRequiredForm(
                previousSetIndex
            );

        CharacterForm targetForm =
            weaponLoadout.GetRequiredForm(
                setIndex
            );

        CharacterGripProfile targetGripProfile =
            characterProfile.SubraceDefinition != null
                ? characterProfile
                    .SubraceDefinition
                    .GetGripProfile(
                        targetForm
                    )
                : CharacterGripProfile
                    .CreateHumanoidDefault();

        if (targetGripProfile == null)
        {
            targetGripProfile =
                CharacterGripProfile
                    .CreateHumanoidDefault();
        }

        bool wasDeployed =
            loadoutDeployed;

        ReleaseActiveConventionalWeapons();

        loadoutDeployed = false;

        if (interactionController != null &&
            !interactionController
                .TryClearBlockingItems(
                    targetGripProfile
                ))
        {
            if (wasDeployed)
                DrawWeapons();

            return false;
        }

        if (!characterProfile.TrySetForm(
                targetForm))
        {
            if (wasDeployed)
                DrawWeapons();

            return false;
        }

        if (!weaponLoadout.SetActiveWeaponSet(
                setIndex))
        {
            characterProfile.TrySetForm(
                previousForm
            );

            if (wasDeployed)
                DrawWeapons();

            return false;
        }

        if (!wasDeployed)
        {
            loadoutDeployed = false;
            Changed?.Invoke();
            return true;
        }

        if (DrawWeapons())
            return true;

        weaponLoadout.SetActiveWeaponSet(
            previousSetIndex
        );

        characterProfile.TrySetForm(
            previousForm
        );

        DrawWeapons();

        return false;
    }

    private bool ReleaseActiveConventionalWeapons()
    {
        if (gripState == null)
            return false;

        CollectActiveWeapons();

        bool changed = false;

        for (int i = 0;
             i < conventionalWeapons.Count;
             i++)
        {
            InventoryItemInstance weapon =
                conventionalWeapons[i];

            if (!gripState.IsHolding(
                    weapon))
            {
                continue;
            }

            if (gripState.Release(
                weapon))
            {
                changed = true;
            }
        }

        return changed;
    }

    private void RollbackDeployment(
        int deployedCount)
    {
        for (int i = 0;
             i < deployedCount &&
             i < conventionalWeapons.Count;
             i++)
        {
            InventoryItemInstance weapon =
                conventionalWeapons[i];

            if (gripState.IsHolding(
                weapon))
            {
                gripState.Release(
                    weapon
                );
            }
        }

        loadoutDeployed = false;
    }

    private void CollectActiveWeapons()
    {
        activeWeapons.Clear();
        conventionalWeapons.Clear();
        conventionalDefinitions.Clear();

        if (weaponLoadout == null)
            return;

        for (int slotIndex = 0;
             slotIndex <
                WeaponSet.SlotCount;
             slotIndex++)
        {
            InventoryItemInstance weapon =
                weaponLoadout
                    .GetActiveWeapon(
                        slotIndex
                    );

            if (weapon == null ||
                weapon.IsEmpty ||
                weapon.Definition == null)
            {
                continue;
            }

            activeWeapons.Add(
                weapon
            );

            if (weapon.Definition
                .IsConventionalWeapon)
            {
                conventionalWeapons.Add(
                    weapon
                );

                conventionalDefinitions.Add(
                    weapon.Definition
                );
            }
        }
    }

    private bool ValidateActiveAttachments()
    {
        for (int i = 0;
             i < activeWeapons.Count;
             i++)
        {
            InventoryItemInstance weapon =
                activeWeapons[i];

            if (weapon == null ||
                weapon.Definition == null)
            {
                return false;
            }

            if (!weapon.Definition
                .IsAttachedWeapon)
            {
                continue;
            }

            if (playerEquipment == null ||
                !playerEquipment.IsEquipped(
                    weapon
                ))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsActiveWeapon(
        InventoryItemInstance weapon)
    {
        if (weaponLoadout == null ||
            weapon == null)
        {
            return false;
        }

        for (int slotIndex = 0;
             slotIndex <
                WeaponSet.SlotCount;
             slotIndex++)
        {
            if (ReferenceEquals(
                weaponLoadout
                    .GetActiveWeapon(
                        slotIndex
                    ),
                weapon))
            {
                return true;
            }
        }

        return false;
    }

    private void HandleCapabilitiesInterrupted(
        PlayerGameplayCapability interruptedCapabilities)
    {
        if ((interruptedCapabilities &
             PlayerGameplayCapability.ItemHandling) == 0)
        {
            return;
        }

        SheatheWeapons();
    }

    private void ResolveReferences()
    {
        if (weaponLoadout == null)
        {
            weaponLoadout =
                GetComponent<
                    PlayerWeaponLoadout>();
        }

        if (gripState == null)
        {
            gripState =
                GetComponent<
                    PlayerGripState>();
        }

        if (characterProfile == null)
        {
            characterProfile =
                GetComponent<
                    PlayerCharacterProfile>();
        }

        if (playerEquipment == null)
        {
            playerEquipment =
                GetComponent<PlayerEquipment>();
        }

        if (interactionController == null)
        {
            interactionController =
                GetComponent<
                    InventoryInteractionController>();
        }

        if (gameplayState == null)
        {
            gameplayState =
                GetComponent<PlayerGameplayState>();
        }
    }
}