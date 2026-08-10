using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(
    typeof(PlayerGripState),
    typeof(PlayerWeaponLoadout),
    typeof(PlayerEquipment)
)]
[RequireComponent(
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
    private PlayerEquipment equipment;
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

        equipment =
            GetComponent<PlayerEquipment>();

        characterProfile =
            GetComponent<PlayerCharacterProfile>();

        cursor.Changed +=
            OnStateChanged;

        gripState.Changed +=
            OnGripStateChanged;

        weaponLoadout.Changed +=
            OnStateChanged;

        equipment.OnEquipmentChanged +=
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

    public bool TryPickUpItemFromContainer(
        InventoryContainer source,
        Vector2Int coordinate)
    {
        if (source == null ||
            cursor.HasSelection)
        {
            return false;
        }

        PlacedInventoryItem placedItem =
            source.GetItemAt(
                coordinate.x,
                coordinate.y
            );

        if (placedItem == null ||
            placedItem.ItemInstance == null ||
            placedItem.ItemDefinition == null)
        {
            return false;
        }

        InventoryItemInstance itemInstance =
            placedItem.ItemInstance;

        if (!TryFindHoldPlan(
            itemInstance,
            out GripType gripType,
            out int gripCount))
        {
            return false;
        }

        Vector2Int originalPosition =
            placedItem.Position;

        int originalRotation =
            placedItem.RotationSteps;

        Vector2Int grabOffset =
            coordinate -
            originalPosition;

        PlacedInventoryItem removedItem =
            source.TakeItemAt(
                coordinate.x,
                coordinate.y
            );

        if (removedItem == null ||
            !ReferenceEquals(
                removedItem.ItemInstance,
                itemInstance))
        {
            return false;
        }

        if (!gripState.TryHold(
            itemInstance,
            gripType,
            gripCount))
        {
            source.PlaceInstance(
                itemInstance,
                originalPosition.x,
                originalPosition.y,
                originalRotation
            );

            return false;
        }

        if (!cursor.Select(
            itemInstance,
            originalRotation,
            grabOffset))
        {
            gripState.Release(
                itemInstance
            );

            source.PlaceInstance(
                itemInstance,
                originalPosition.x,
                originalPosition.y,
                originalRotation
            );

            return false;
        }

        return true;
    }

    public bool CanPlaceSelection(
        InventoryContainer target,
        Vector2Int origin)
    {
        InventoryItemInstance selected =
            cursor.SelectedItem;

        if (target == null ||
            !IsPlacementCandidate(
                selected))
        {
            return false;
        }

        return target.CanPlace(
            selected,
            origin.x,
            origin.y,
            cursor.RotationSteps
        );
    }

    public bool TryPlaceSelection(
        InventoryContainer target,
        Vector2Int origin)
    {
        InventoryItemInstance selected =
            cursor.SelectedItem;

        if (!CanPlaceSelection(
            target,
            origin))
        {
            return false;
        }

        bool placed =
            target.PlaceInstance(
                selected,
                origin.x,
                origin.y,
                cursor.RotationSteps
            );

        if (!placed)
            return false;

        gripState.Release(
            selected
        );

        cursor.ClearSelection();

        return true;
    }

    public bool CanMergeSelectionIntoStackAt(
        InventoryContainer target,
        Vector2Int coordinate,
        out bool fullyFits)
    {
        fullyFits = false;

        InventoryItemInstance selected =
            cursor.SelectedItem;

        if (target == null ||
            !IsPlacementCandidate(
                selected))
        {
            return false;
        }

        PlacedInventoryItem placedTarget =
            target.GetItemAt(
                coordinate.x,
                coordinate.y
            );

        if (placedTarget == null ||
            placedTarget.ItemInstance == null)
        {
            return false;
        }

        InventoryItemInstance targetInstance =
            placedTarget.ItemInstance;

        if (!selected.CanStackWith(
            targetInstance))
        {
            return false;
        }

        int availableSpace =
            targetInstance.MaxStackSize -
            targetInstance.Quantity;

        if (availableSpace <= 0)
            return false;

        fullyFits =
            selected.Quantity <=
            availableSpace;

        return true;
    }

    public bool TryMergeSelectionIntoStackAt(
        InventoryContainer target,
        Vector2Int coordinate)
    {
        if (!CanMergeSelectionIntoStackAt(
            target,
            coordinate,
            out _))
        {
            return false;
        }

        InventoryItemInstance selected =
            cursor.SelectedItem;

        PlacedInventoryItem placedTarget =
            target.GetItemAt(
                coordinate.x,
                coordinate.y
            );

        if (placedTarget == null ||
            placedTarget.ItemInstance == null)
        {
            return false;
        }

        int moved =
            selected.MoveQuantityTo(
                placedTarget.ItemInstance,
                selected.Quantity
            );

        return moved > 0;
    }

    public bool TryPlaceOneSelection(
        InventoryContainer target,
        Vector2Int origin)
    {
        InventoryItemInstance selected =
            cursor.SelectedItem;

        if (target == null ||
            !IsPlacementCandidate(
                selected) ||
            !selected.IsStackable)
        {
            return false;
        }

        PlacedInventoryItem targetStack =
            target.GetItemAt(
                origin.x,
                origin.y
            );

        if (targetStack != null)
        {
            if (targetStack.ItemInstance == null)
                return false;

            int moved =
                selected.MoveQuantityTo(
                    targetStack.ItemInstance,
                    1
                );

            return moved > 0;
        }

        if (selected.Quantity <= 1)
        {
            return TryPlaceSelection(
                target,
                origin
            );
        }

        if (!selected.TrySplit(
            1,
            out InventoryItemInstance
                singleItem))
        {
            return false;
        }

        bool placed =
            target.PlaceInstance(
                singleItem,
                origin.x,
                origin.y,
                cursor.RotationSteps
            );

        if (placed)
            return true;

        singleItem.MoveQuantityTo(
            selected,
            singleItem.Quantity
        );

        return false;
    }

    public bool TrySplitStackFromContainer(
        InventoryContainer source,
        Vector2Int coordinate)
    {
        if (source == null ||
            cursor.HasSelection)
        {
            return false;
        }

        PlacedInventoryItem placedItem =
            source.GetItemAt(
                coordinate.x,
                coordinate.y
            );

        if (placedItem == null ||
            placedItem.ItemInstance == null ||
            !placedItem.ItemInstance.IsStackable ||
            placedItem.ItemInstance.Quantity <= 1)
        {
            return false;
        }

        InventoryItemInstance sourceInstance =
            placedItem.ItemInstance;

        int splitQuantity =
            Mathf.CeilToInt(
                sourceInstance.Quantity *
                0.5f
            );

        if (!sourceInstance.TrySplit(
            splitQuantity,
            out InventoryItemInstance
                splitInstance))
        {
            return false;
        }

        if (!TryFindHoldPlan(
            splitInstance,
            out GripType gripType,
            out int gripCount))
        {
            splitInstance.MoveQuantityTo(
                sourceInstance,
                splitInstance.Quantity
            );

            return false;
        }

        if (!gripState.TryHold(
            splitInstance,
            gripType,
            gripCount))
        {
            splitInstance.MoveQuantityTo(
                sourceInstance,
                splitInstance.Quantity
            );

            return false;
        }

        int rotation =
            placedItem.RotationSteps;

        Vector2Int grabOffset =
            new Vector2Int(
                placedItem.ItemDefinition
                    .GetWidth(rotation) / 2,
                placedItem.ItemDefinition
                    .GetHeight(rotation) / 2
            );

        cursor.Select(
            splitInstance,
            rotation,
            grabOffset
        );

        return true;
    }

    public bool TryTransferSelectionIntoContainer(
        InventoryContainer target)
    {
        InventoryItemInstance selected =
            cursor.SelectedItem;

        if (target == null ||
            !IsPlacementCandidate(
                selected))
        {
            return false;
        }

        int quantityBefore =
            selected.Quantity;

        target.TryTransferIn(
            selected,
            cursor.RotationSteps,
            out int remainingQuantity
        );

        int quantityAfter =
            selected.IsEmpty
                ? 0
                : selected.Quantity;

        bool movedAnything =
            quantityAfter <
                quantityBefore ||
            remainingQuantity <= 0;

        if (!movedAnything)
            return false;

        if (remainingQuantity <= 0)
        {
            if (gripState.IsHolding(
                selected))
            {
                gripState.Release(
                    selected
                );
            }

            if (ReferenceEquals(
                cursor.SelectedItem,
                selected))
            {
                cursor.ClearSelection();
            }
        }

        return true;
    }

    public bool TryQuickTransfer(
        InventoryContainer source,
        InventoryContainer target,
        Vector2Int coordinate)
    {
        if (source == null ||
            target == null ||
            ReferenceEquals(
                source,
                target) ||
            cursor.HasSelection)
        {
            return false;
        }

        if (!TryPickUpItemFromContainer(
            source,
            coordinate))
        {
            return false;
        }

        return TryTransferSelectionIntoContainer(
            target
        );
    }

    public bool CanAssignSelectedWeapon(
        int setIndex,
        int slotIndex)
    {
        InventoryItemInstance selected =
            cursor.SelectedItem;

        if (!IsPlacementCandidate(
            selected) ||
            !IsConventionalWeapon(
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
            return TryAssignToEmptyWeaponSlot(
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
            RestoreWeaponSlot(
                setIndex,
                slotIndex,
                selected,
                replacedWeapon
            );

            return false;
        }

        if (!gripState.TryHold(
            replacedWeapon,
            replacementGripType,
            replacementGripCount))
        {
            RestoreWeaponSlot(
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
        if (cursor.HasSelection)
            return false;

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
        if (!CanTakeWeaponFromSlot(
            setIndex,
            slotIndex))
        {
            return false;
        }

        InventoryItemInstance weapon =
            weaponLoadout.GetWeapon(
                setIndex,
                slotIndex
            );

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

    public bool CanEquipSelectedItem(
        EquipmentSlotType slotType,
        int slotIndex = 0)
    {
        InventoryItemInstance selected =
            cursor.SelectedItem;

        if (!IsPlacementCandidate(
            selected))
        {
            return false;
        }

        if (!equipment.CanEquipItemToSlot(
            selected,
            slotType,
            slotIndex))
        {
            return false;
        }

        InventoryItemInstance currentItem =
            equipment.GetEquippedItem(
                slotType,
                slotIndex
            );

        if (currentItem == null ||
            ReferenceEquals(
                currentItem,
                selected))
        {
            return true;
        }

        return TryFindHoldPlanAfterRelease(
            selected,
            currentItem,
            out _,
            out _
        );
    }

    public bool TryEquipSelectedItem(
        EquipmentSlotType slotType,
        int slotIndex = 0)
    {
        if (!CanEquipSelectedItem(
            slotType,
            slotIndex))
        {
            return false;
        }

        InventoryItemInstance selected =
            cursor.SelectedItem;

        InventoryItemInstance currentItem =
            equipment.GetEquippedItem(
                slotType,
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

        if (currentItem == null)
        {
            bool equipped =
                equipment.TryEquipItemToSlot(
                    selected,
                    slotType,
                    slotIndex,
                    out _
                );

            if (!equipped)
                return false;

            if (!gripState.Release(
                selected))
            {
                equipment.UnequipSlot(
                    slotType,
                    slotIndex
                );

                return false;
            }

            cursor.ClearSelection();

            return true;
        }

        if (!TryFindHoldPlanAfterRelease(
            selected,
            currentItem,
            out GripType replacementGripType,
            out int replacementGripCount))
        {
            return false;
        }

        bool replaced =
            equipment.TryEquipItemToSlot(
                selected,
                slotType,
                slotIndex,
                out InventoryItemInstance
                    replacedItem
            );

        if (!replaced ||
            replacedItem == null)
        {
            return false;
        }

        if (!gripState.Release(
            selected))
        {
            RestoreEquipmentSlot(
                slotType,
                slotIndex,
                selected,
                replacedItem
            );

            return false;
        }

        if (!gripState.TryHold(
            replacedItem,
            replacementGripType,
            replacementGripCount))
        {
            RestoreEquipmentSlot(
                slotType,
                slotIndex,
                selected,
                replacedItem
            );

            gripState.TryHold(
                selected,
                originalGripType,
                originalGripCount
            );

            return false;
        }

        cursor.Select(
            replacedItem,
            0,
            Vector2Int.zero
        );

        return true;
    }

    public bool CanTakeEquipmentFromSlot(
        EquipmentSlotType slotType,
        int slotIndex = 0)
    {
        if (cursor.HasSelection)
            return false;

        InventoryItemInstance item =
            equipment.GetEquippedItem(
                slotType,
                slotIndex
            );

        if (item == null)
            return false;

        return TryFindHoldPlan(
            item,
            out _,
            out _
        );
    }

    public bool TryTakeEquipmentFromSlot(
        EquipmentSlotType slotType,
        int slotIndex = 0)
    {
        if (!CanTakeEquipmentFromSlot(
            slotType,
            slotIndex))
        {
            return false;
        }

        InventoryItemInstance item =
            equipment.GetEquippedItem(
                slotType,
                slotIndex
            );

        if (!TryFindHoldPlan(
            item,
            out GripType gripType,
            out int gripCount))
        {
            return false;
        }

        InventoryItemInstance removedItem =
            equipment.UnequipSlot(
                slotType,
                slotIndex
            );

        if (!ReferenceEquals(
            removedItem,
            item))
        {
            return false;
        }

        if (!gripState.TryHold(
            item,
            gripType,
            gripCount))
        {
            equipment.TryEquipItemToSlot(
                item,
                slotType,
                slotIndex,
                out _
            );

            return false;
        }

        cursor.Select(
            item,
            0,
            Vector2Int.zero
        );

        return true;
    }

    private bool TryAssignToEmptyWeaponSlot(
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

    private void RestoreWeaponSlot(
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

    private void RestoreEquipmentSlot(
        EquipmentSlotType slotType,
        int slotIndex,
        InventoryItemInstance newItem,
        InventoryItemInstance oldItem)
    {
        InventoryItemInstance removed =
            equipment.UnequipSlot(
                slotType,
                slotIndex
            );

        if (!ReferenceEquals(
            removed,
            newItem))
        {
            return;
        }

        equipment.TryEquipItemToSlot(
            oldItem,
            slotType,
            slotIndex,
            out _
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

        if (releasedItem != null &&
            gripState.IsHolding(
                releasedItem))
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
            else
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

        freeMouth =
            Mathf.Clamp(
                freeMouth,
                0,
                gripState.MouthGripCount
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
               itemInstance.Definition
                   .itemCategory ==
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

        if (equipment != null)
        {
            equipment.OnEquipmentChanged -=
                OnStateChanged;
        }

        if (characterProfile != null)
        {
            characterProfile.AttributesChanged -=
                OnStateChanged;
        }
    }
}