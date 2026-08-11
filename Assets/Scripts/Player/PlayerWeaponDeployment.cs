using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(
    typeof(PlayerWeaponLoadout),
    typeof(PlayerGripState),
    typeof(PlayerCharacterProfile)
)]
public sealed class PlayerWeaponDeployment :
    MonoBehaviour
{
    private readonly List<InventoryItemInstance>
        activeWeapons =
            new List<InventoryItemInstance>();

    private readonly List<ItemDefinition>
        activeDefinitions =
            new List<ItemDefinition>();

    private readonly List<ResolvedItemHandling>
        usePlan =
            new List<ResolvedItemHandling>();

    private PlayerWeaponLoadout weaponLoadout;
    private PlayerGripState gripState;
    private PlayerCharacterProfile characterProfile;

    public bool WeaponsDrawn
    {
        get
        {
            ResolveReferences();
            CollectActiveWeapons();

            if (activeWeapons.Count == 0 ||
                gripState == null)
            {
                return false;
            }

            for (int i = 0;
                 i < activeWeapons.Count;
                 i++)
            {
                if (!gripState.IsHolding(
                    activeWeapons[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public event Action Changed;

    private void Awake()
    {
        ResolveReferences();
    }

    public bool IsDeployed(
        InventoryItemInstance weapon)
    {
        ResolveReferences();

        return IsActiveWeapon(
                   weapon) &&
               gripState != null &&
               gripState.IsHolding(
                   weapon
               );
    }

    public bool DrawWeapons()
    {
        ResolveReferences();

        if (weaponLoadout == null ||
            gripState == null ||
            characterProfile == null ||
            characterProfile
                .EffectiveHandlingProfile ==
                null)
        {
            return false;
        }

        CollectActiveWeapons();

        if (activeWeapons.Count == 0)
            return false;

        if (WeaponsDrawn)
            return true;

        // Clear any partial deployment before planning
        // the complete set again.
        ReleaseActiveWeapons();

        CharacterHandlingProfile handling =
            characterProfile
                .EffectiveHandlingProfile;

        if (!ItemHandlingResolver
            .TryResolveUseSet(
                activeDefinitions,
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
             i < activeWeapons.Count;
             i++)
        {
            ResolvedItemHandling resolved =
                usePlan[i];

            if (resolved == null ||
                !gripState.TryHold(
                    activeWeapons[i],
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

        Changed?.Invoke();

        return true;
    }

    public bool SheatheWeapons()
    {
        ResolveReferences();

        bool changed =
            ReleaseActiveWeapons();

        if (changed)
            Changed?.Invoke();

        return changed;
    }

    internal bool SheatheWeapon(
        InventoryItemInstance weapon)
    {
        ResolveReferences();

        if (!IsActiveWeapon(
                weapon) ||
            gripState == null ||
            !gripState.IsHolding(
                weapon))
        {
            return false;
        }

        if (!gripState.Release(
            weapon))
        {
            return false;
        }

        Changed?.Invoke();

        return true;
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

        bool redraw =
            WeaponsDrawn;

        ReleaseActiveWeapons();

        if (!weaponLoadout
            .SetActiveWeaponSet(
                setIndex))
        {
            return false;
        }

        if (!redraw)
        {
            Changed?.Invoke();
            return true;
        }

        if (DrawWeapons())
            return true;

        weaponLoadout.SetActiveWeaponSet(
            previousSetIndex
        );

        DrawWeapons();

        return false;
    }

    private bool ReleaseActiveWeapons()
    {
        if (gripState == null)
            return false;

        CollectActiveWeapons();

        bool changed = false;

        for (int i = 0;
             i < activeWeapons.Count;
             i++)
        {
            InventoryItemInstance weapon =
                activeWeapons[i];

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
             i < activeWeapons.Count;
             i++)
        {
            InventoryItemInstance weapon =
                activeWeapons[i];

            if (gripState.IsHolding(
                weapon))
            {
                gripState.Release(
                    weapon
                );
            }
        }
    }

    private void CollectActiveWeapons()
    {
        activeWeapons.Clear();
        activeDefinitions.Clear();

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

            activeDefinitions.Add(
                weapon.Definition
            );
        }
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
    }
}