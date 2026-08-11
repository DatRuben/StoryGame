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
        deployedWeapons =
            new List<InventoryItemInstance>();

    private readonly List<InventoryItemInstance>
        newlyDeployedWeapons =
            new List<InventoryItemInstance>();

    private PlayerWeaponLoadout weaponLoadout;
    private PlayerGripState gripState;
    private PlayerCharacterProfile characterProfile;

    public bool WeaponsDrawn =>
        deployedWeapons.Count > 0;

    public event Action Changed;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();

        if (weaponLoadout != null)
        {
            weaponLoadout.Changed -=
                OnLoadoutChanged;

            weaponLoadout.Changed +=
                OnLoadoutChanged;
        }

        ValidateDeployment();
    }

    private void OnDisable()
    {
        if (weaponLoadout != null)
        {
            weaponLoadout.Changed -=
                OnLoadoutChanged;
        }
    }

    public bool IsDeployed(
        InventoryItemInstance weapon)
    {
        return weapon != null &&
               deployedWeapons.Contains(
                   weapon
               );
    }

    public bool DrawWeapons()
    {
        ResolveReferences();

        if (weaponLoadout == null ||
            gripState == null ||
            characterProfile == null)
        {
            return false;
        }

        WeaponSet activeSet =
            weaponLoadout.ActiveWeaponSet;

        if (activeSet == null ||
            !activeSet.HasAnyWeapon)
        {
            return false;
        }

        newlyDeployedWeapons.Clear();

        for (int slotIndex = 0;
             slotIndex <
                WeaponSet.SlotCount;
             slotIndex++)
        {
            InventoryItemInstance weapon =
                activeSet.GetWeapon(
                    slotIndex
                );

            if (weapon == null)
                continue;

            if (IsDeployed(weapon))
                continue;

            if (!TryDeployWeapon(
                weapon))
            {
                RollbackNewDeployments();

                return false;
            }

            newlyDeployedWeapons.Add(
                weapon
            );
        }

        newlyDeployedWeapons.Clear();

        if (deployedWeapons.Count == 0)
            return false;

        Changed?.Invoke();

        return true;
    }

    public bool SheatheWeapons()
    {
        if (deployedWeapons.Count == 0)
            return false;

        for (int i =
                 deployedWeapons.Count - 1;
             i >= 0;
             i--)
        {
            InventoryItemInstance weapon =
                deployedWeapons[i];

            if (weapon != null &&
                gripState != null &&
                gripState.IsHolding(
                    weapon))
            {
                gripState.Release(
                    weapon
                );
            }
        }

        deployedWeapons.Clear();
        newlyDeployedWeapons.Clear();

        Changed?.Invoke();

        return true;
    }

    internal bool SheatheWeapon(
        InventoryItemInstance weapon)
    {
        if (weapon == null)
            return false;

        int index =
            deployedWeapons.IndexOf(
                weapon
            );

        if (index < 0)
            return false;

        if (gripState != null &&
            gripState.IsHolding(
                weapon))
        {
            gripState.Release(
                weapon
            );
        }

        deployedWeapons.RemoveAt(
            index
        );

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

        bool redrawWeapons =
            WeaponsDrawn;

        if (redrawWeapons)
            SheatheWeapons();

        if (!weaponLoadout
            .SetActiveWeaponSet(
                setIndex))
        {
            return false;
        }

        if (!redrawWeapons)
            return true;

        if (DrawWeapons())
            return true;

        weaponLoadout.SetActiveWeaponSet(
            previousSetIndex
        );

        DrawWeapons();

        return false;
    }

    internal void GetDeployedWeapons(
        List<InventoryItemInstance> results)
    {
        if (results == null)
            return;

        results.Clear();

        for (int i = 0;
             i < deployedWeapons.Count;
             i++)
        {
            results.Add(
                deployedWeapons[i]
            );
        }
    }

    private bool TryDeployWeapon(
        InventoryItemInstance weapon)
    {
        if (weapon == null ||
            weapon.IsEmpty ||
            weapon.Definition == null ||
            gripState == null)
        {
            return false;
        }

        if (!IsInActiveSet(
            weapon))
        {
            return false;
        }

        if (gripState.IsHolding(
            weapon))
        {
            return false;
        }

        if (!TryFindUsePlan(
            weapon,
            out GripType gripType,
            out int gripCount))
        {
            return false;
        }

        if (!gripState.TryHold(
            weapon,
            gripType,
            gripCount))
        {
            return false;
        }

        deployedWeapons.Add(
            weapon
        );

        return true;
    }

    private bool TryFindUsePlan(
        InventoryItemInstance weapon,
        out GripType gripType,
        out int gripCount)
    {
        gripType =
            GripType.Hand;

        gripCount = 0;

        if (weapon == null ||
            weapon.Definition == null ||
            characterProfile == null ||
            characterProfile
                .EffectiveHandlingProfile ==
                null ||
            gripState == null)
        {
            return false;
        }

        CharacterHandlingProfile handling =
            characterProfile
                .EffectiveHandlingProfile;

        int freeHands =
            gripState.GetFreeGripCount(
                GripType.Hand
            );

        for (int count = 1;
             count <= freeHands;
             count++)
        {
            ResolvedItemHandling resolved =
                ItemHandlingResolver.Resolve(
                    weapon.Definition,
                    handling,
                    GripType.Hand,
                    count
                );

            if (resolved == null ||
                !resolved.canUse)
            {
                continue;
            }

            gripType =
                GripType.Hand;

            gripCount =
                count;

            return true;
        }

        int freeMouth =
            gripState.GetFreeGripCount(
                GripType.Mouth
            );

        if (freeMouth > 0)
        {
            ResolvedItemHandling resolved =
                ItemHandlingResolver.Resolve(
                    weapon.Definition,
                    handling,
                    GripType.Mouth,
                    1
                );

            if (resolved != null &&
                resolved.canUse)
            {
                gripType =
                    GripType.Mouth;

                gripCount = 1;

                return true;
            }
        }

        return false;
    }

    private void RollbackNewDeployments()
    {
        for (int i =
                 newlyDeployedWeapons.Count - 1;
             i >= 0;
             i--)
        {
            InventoryItemInstance weapon =
                newlyDeployedWeapons[i];

            if (weapon != null &&
                gripState != null &&
                gripState.IsHolding(
                    weapon))
            {
                gripState.Release(
                    weapon
                );
            }

            deployedWeapons.Remove(
                weapon
            );
        }

        newlyDeployedWeapons.Clear();
    }

    private bool IsInActiveSet(
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
                weaponLoadout.GetActiveWeapon(
                    slotIndex
                ),
                weapon))
            {
                return true;
            }
        }

        return false;
    }

    private void ValidateDeployment()
    {
        bool changed = false;

        for (int i =
                 deployedWeapons.Count - 1;
             i >= 0;
             i--)
        {
            InventoryItemInstance weapon =
                deployedWeapons[i];

            bool stillValid =
                weapon != null &&
                IsInActiveSet(
                    weapon
                ) &&
                gripState != null &&
                gripState.IsHolding(
                    weapon
                );

            if (stillValid)
                continue;

            if (weapon != null &&
                gripState != null &&
                gripState.IsHolding(
                    weapon))
            {
                gripState.Release(
                    weapon
                );
            }

            deployedWeapons.RemoveAt(
                i
            );

            changed = true;
        }

        if (changed)
            Changed?.Invoke();
    }

    private void OnLoadoutChanged()
    {
        ValidateDeployment();
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