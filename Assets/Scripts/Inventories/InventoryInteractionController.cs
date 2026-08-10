using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(
    typeof(PlayerGripState),
    typeof(PlayerWeaponLoadout),
    typeof(PlayerCharacterProfile)
)]
public sealed class InventoryInteractionController :
    MonoBehaviour
{
    private readonly InventoryCursor cursor =
        new InventoryCursor();

    private readonly List<InventoryItemInstance>
        heldItems =
            new List<InventoryItemInstance>();

    private PlayerGripState gripState;
    private PlayerWeaponLoadout weaponLoadout;
    private PlayerCharacterProfile characterProfile;

    public bool HasSelection =>
        cursor.HasSelection;

    public InventoryItemInstance SelectedItem =>
        cursor.SelectedItem;

    public ItemDefinition SelectedDefinition =>
        cursor.ItemDefinition;

    public int SelectedRotationSteps =>
        cursor.RotationSteps;

    public Vector2Int SelectedGrabOffset =>
        cursor.GrabOffset;

    public event Action Changed;

    private void Awake()
    {
        gripState =
            GetComponent<PlayerGripState>();

        weaponLoadout =
            GetComponent<PlayerWeaponLoadout>();

        characterProfile =
            GetComponent<PlayerCharacterProfile>();

        cursor.Changed +=
            OnStateChanged;

        gripState.Changed +=
            OnGripStateChanged;

        weaponLoadout.Changed +=
            OnStateChanged;

        characterProfile.AttributesChanged +=
            OnStateChanged;
    }

    public bool SelectHeldItem(
        InventoryItemInstance itemInstance,
        int rotationSteps = 0,
        Vector2Int grabOffset = default)
    {
        if (!IsPlacementCandidate(
            itemInstance))
        {
            return false;
        }

        return cursor.Select(
            itemInstance,
            rotationSteps,
            grabOffset
        );
    }

    public void ClearSelection()
    {
        cursor.ClearSelection();
    }

    public void RotateSelectionCounterClockwise()
    {
        cursor.RotateCounterClockwise();
    }

    public bool CycleHeldSelection()
    {
        gripState.GetHeldItemsInCycleOrder(
            heldItems
        );

        RemoveNonCandidates();

        if (heldItems.Count == 0)
        {
            cursor.ClearSelection();
            return false;
        }

        int currentIndex =
            heldItems.IndexOf(
                cursor.SelectedItem
            );

        int nextIndex =
            currentIndex >= 0
                ? (currentIndex + 1) %
                  heldItems.Count
                : 0;

        InventoryItemInstance nextItem =
            heldItems[nextIndex];

        return cursor.Select(
            nextItem,
            0,
            Vector2Int.zero
        );
    }

    public bool CanAssignSelectedWeapon(
        int setIndex,
        int slotIndex)
    {
        InventoryItemInstance selected =
            cursor.SelectedItem;

        if (!IsPlacementCandidate(
            selected))
        {
            return false;
        }

        if (!IsConventionalWeapon(
            selected))
        {
            return false;
        }

        InventoryItemInstance currentWeapon =
            weaponLoadout.GetWeapon(
                setIndex,
                slotIndex
            );

        if (currentWeapon == null)
            return true;

        return TryFindHoldPlanAfterRelease(
            selected,
            currentWeapon,
            out _,
            out _
        );
    }

    public bool TryAssignSelectedWeapon(
        int setIndex,
        int slotIndex)
    {
        if (!CanAssignSelectedWeapon(
            setIndex,
            slotIndex))
        {
            return false;
        }

        InventoryItemInstance selected =
            cursor.SelectedItem;

        InventoryItemInstance currentWeapon =
            weaponLoadout.GetWeapon(
                setIndex,
                slotIndex
            );

        GripType originalGripType =
            GetHeldGripType(
                selected
            );

        int originalGripCount =
            gripState.GetAssignedGripCount(
                selected
            );

        if (currentWeapon == null)
        {
            return TryAssignToEmptySlot(
                setIndex,
                slotIndex,
                selected
            );
        }

        if (!TryFindHoldPlanAfterRelease(
            selected,
            currentWeapon,
            out GripType replacementGripType,
            out int replacementGripCount))
        {
            return false;
        }

        bool replaced =
            weaponLoadout.TryReplaceWeapon(
                setIndex,
                slotIndex,
                selected,
                out InventoryItemInstance
                    replacedWeapon
            );

        if (!replaced ||
            replacedWeapon == null)
        {
            return false;
        }

        if (!gripState.Release(
            selected))
        {
            RollbackLoadoutReplacement(
                setIndex,
                slotIndex,
                selected,
                replacedWeapon
            );

            return false;
        }

        bool heldReplacement =
            gripState.TryHold(
                replacedWeapon,
                replacementGripType,
                replacementGripCount
            );

        if (!heldReplacement)
        {
            RollbackLoadoutReplacement(
                setIndex,
                slotIndex,
                selected,
                replacedWeapon
            );

            gripState.TryHold(
                selected,
                originalGripType,
                originalGripCount
            );

            return false;
        }

        cursor.Select(
            replacedWeapon,
            0,
            Vector2Int.zero
        );

        return true;
    }

    public bool CanTakeWeaponFromSlot(
        int setIndex,
        int slotIndex)
    {
        InventoryItemInstance weapon =
            weaponLoadout.GetWeapon(
                setIndex,
                slotIndex
            );

        if (weapon == null)
            return false;

        return TryFindHoldPlan(
            weapon,
            out _,
            out _
        );
    }

    public bool TryTakeWeaponFromSlot(
        int setIndex,
        int slotIndex)
    {
        InventoryItemInstance weapon =
            weaponLoadout.GetWeapon(
                setIndex,
                slotIndex
            );

        if (weapon == null)
            return false;

        if (!TryFindHoldPlan(
            weapon,
            out GripType gripType,
            out int gripCount))
        {
            return false;
        }

        InventoryItemInstance removedWeapon =
            weaponLoadout.RemoveWeapon(
                setIndex,
                slotIndex
            );

        if (!ReferenceEquals(
            removedWeapon,
            weapon))
        {
            return false;
        }

        if (!gripState.TryHold(
            weapon,
            gripType,
            gripCount))
        {
            weaponLoadout.TryAssignWeapon(
                setIndex,
                slotIndex,
                weapon
            );

            return false;
        }

        cursor.Select(
            weapon,
            0,
            Vector2Int.zero
        );

        return true;
    }

    public bool SetActiveWeaponSet(
        int setIndex)
    {
        return weaponLoadout
            .SetActiveWeaponSet(
                setIndex
            );
    }

    private bool TryAssignToEmptySlot(
        int setIndex,
        int slotIndex,
        InventoryItemInstance weapon)
    {
        if (!weaponLoadout.TryAssignWeapon(
            setIndex,
            slotIndex,
            weapon))
        {
            return false;
        }

        if (!gripState.Release(
            weapon))
        {
            weaponLoadout.RemoveWeapon(
                setIndex,
                slotIndex
            );

            return false;
        }

        cursor.ClearSelection();

        return true;
    }

    private void RollbackLoadoutReplacement(
        int setIndex,
        int slotIndex,
        InventoryItemInstance newWeapon,
        InventoryItemInstance oldWeapon)
    {
        InventoryItemInstance removed =
            weaponLoadout.RemoveWeapon(
                setIndex,
                slotIndex
            );

        if (!ReferenceEquals(
            removed,
            newWeapon))
        {
            return;
        }

        weaponLoadout.TryAssignWeapon(
            setIndex,
            slotIndex,
            oldWeapon
        );
    }

    private bool TryFindHoldPlan(
        InventoryItemInstance itemInstance,
        out GripType gripType,
        out int gripCount)
    {
        return TryFindHoldPlan(
            itemInstance,
            gripState.GetFreeGripCount(
                GripType.Hand
            ),
            gripState.GetFreeGripCount(
                GripType.Mouth
            ),
            out gripType,
            out gripCount
        );
    }

    private bool TryFindHoldPlanAfterRelease(
        InventoryItemInstance releasedItem,
        InventoryItemInstance itemToHold,
        out GripType gripType,
        out int gripCount)
    {
        int freeHands =
            gripState.GetFreeGripCount(
                GripType.Hand
            );

        int freeMouth =
            gripState.GetFreeGripCount(
                GripType.Mouth
            );

        if (releasedItem != null)
        {
            GripType releasedGripType =
                GetHeldGripType(
                    releasedItem
                );

            int releasedGripCount =
                gripState.GetAssignedGripCount(
                    releasedItem
                );

            if (releasedGripType ==
                GripType.Hand)
            {
                freeHands +=
                    releasedGripCount;
            }
            else if (
                releasedGripType ==
                GripType.Mouth)
            {
                freeMouth +=
                    releasedGripCount;
            }
        }

        return TryFindHoldPlan(
            itemToHold,
            freeHands,
            freeMouth,
            out gripType,
            out gripCount
        );
    }

    private bool TryFindHoldPlan(
        InventoryItemInstance itemInstance,
        int freeHands,
        int freeMouth,
        out GripType gripType,
        out int gripCount)
    {
        gripType =
            GripType.Hand;

        gripCount = 0;

        if (itemInstance == null ||
            itemInstance.Definition == null ||
            characterProfile == null ||
            characterProfile
                .EffectiveHandlingProfile ==
                null)
        {
            return false;
        }

        CharacterHandlingProfile handling =
            characterProfile
                .EffectiveHandlingProfile;

        freeHands =
            Mathf.Clamp(
                freeHands,
                0,
                gripState.HandGripCount
            );

        for (int count = 1;
             count <= freeHands;
             count++)
        {
            ResolvedItemHandling resolved =
                ItemHandlingResolver.Resolve(
                    itemInstance.Definition,
                    handling,
                    GripType.Hand,
                    count
                );

            if (resolved == null ||
                !resolved.canHold)
            {
                continue;
            }

            gripType =
                GripType.Hand;

            gripCount =
                count;

            return true;
        }

        if (freeMouth > 0)
        {
            ResolvedItemHandling resolved =
                ItemHandlingResolver.Resolve(
                    itemInstance.Definition,
                    handling,
                    GripType.Mouth,
                    1
                );

            if (resolved != null &&
                resolved.canHold)
            {
                gripType =
                    GripType.Mouth;

                gripCount = 1;

                return true;
            }
        }

        return false;
    }

    private GripType GetHeldGripType(
        InventoryItemInstance itemInstance)
    {
        if (itemInstance != null &&
            ReferenceEquals(
                gripState.GetItem(
                    GripType.Mouth,
                    0
                ),
                itemInstance))
        {
            return GripType.Mouth;
        }

        return GripType.Hand;
    }

    private bool IsPlacementCandidate(
        InventoryItemInstance itemInstance)
    {
        if (itemInstance == null ||
            itemInstance.IsEmpty ||
            !gripState.IsHolding(
                itemInstance))
        {
            return false;
        }

        // A weapon that is still assigned to a loadout is
        // being treated as a slotted/drawn weapon, not as
        // a loose inventory-placement candidate.
        return !weaponLoadout
            .IsWeaponAssigned(
                itemInstance
            );
    }

    private static bool IsConventionalWeapon(
        InventoryItemInstance itemInstance)
    {
        return itemInstance != null &&
               itemInstance.Definition != null &&
               itemInstance.Definition.itemCategory ==
                   ItemCategory.Weapon;
    }

    private void RemoveNonCandidates()
    {
        for (int i = heldItems.Count - 1;
             i >= 0;
             i--)
        {
            if (IsPlacementCandidate(
                heldItems[i]))
            {
                continue;
            }

            heldItems.RemoveAt(i);
        }
    }

    private void OnGripStateChanged()
    {
        if (cursor.HasSelection &&
            !IsPlacementCandidate(
                cursor.SelectedItem))
        {
            cursor.ClearSelection();
        }

        Changed?.Invoke();
    }

    private void OnStateChanged()
    {
        Changed?.Invoke();
    }

    private void OnDestroy()
    {
        cursor.Changed -=
            OnStateChanged;

        if (gripState != null)
        {
            gripState.Changed -=
                OnGripStateChanged;
        }

        if (weaponLoadout != null)
        {
            weaponLoadout.Changed -=
                OnStateChanged;
        }

        if (characterProfile != null)
        {
            characterProfile.AttributesChanged -=
                OnStateChanged;
        }
    }
}