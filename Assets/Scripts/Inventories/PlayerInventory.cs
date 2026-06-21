using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StartingInventoryItem
{
    public ItemData item;

    [Min(1)]
    public int quantity = 1;

    public int x;
    public int y;

    [Tooltip("0 = 0 degrees, 1 = 90 degrees, 2 = 180 degrees, 3 = 270 degrees")]
    [Range(0, 3)]
    public int rotationSteps;
}

public class PlayerInventory : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerWeaponSlots playerWeaponSlots;

    [Header("Grid Size")]
    [SerializeField] private int gridWidth = 8;
    [SerializeField] private int gridHeight = 6;

    [Header("Test Starting Items")]
    [SerializeField]
    private List<StartingInventoryItem> startingItems =
        new List<StartingInventoryItem>();

    [Header("Old Weapon Slot - Temporary")]
    [SerializeField] private ItemData weaponSlotItem;
    [SerializeField] private bool weaponDrawn = false;

    public InventoryGrid Grid { get; private set; }

    public PlacedInventoryItem HeldItem { get; private set; }
    public bool IsHoldingItem => HeldItem != null;

    public ItemData WeaponSlotItem => weaponSlotItem;
    public bool IsWeaponDrawn => weaponDrawn;
    public bool HasWeaponInSlot => weaponSlotItem != null;

    public bool CenterHeldItemOnCursorRequested { get; private set; }
    public bool MouseHeldItemCountsAsHeld { get; private set; }

    public int HeldItemRotationSteps
    {
        get
        {
            if (HeldItem == null)
                return 0;

            return HeldItem.RotationSteps;
        }
    }

    public event Action OnInventoryChanged;
    public event Action OnHeldItemChanged;
    public event Action OnEquipmentChanged;

    private void Awake()
    {
        Grid = new InventoryGrid(gridWidth, gridHeight);

        if (playerWeaponSlots == null)
            playerWeaponSlots = GetComponent<PlayerWeaponSlots>();
    }

    private void OnValidate()
    {
        ClampStartingItemQuantities();
    }

    private void Start()
    {
        ValidateWeaponSlot();
        ClampStartingItemQuantities();
        PlaceStartingItems();
    }

    private void ClampStartingItemQuantities()
    {
        if (startingItems == null)
            return;

        for (int i = 0; i < startingItems.Count; i++)
        {
            StartingInventoryItem startingItem =
                startingItems[i];

            if (startingItem == null)
                continue;

            startingItem.quantity =
                GetSafePlacedQuantityForItem(
                    startingItem.item,
                    startingItem.quantity
                );
        }
    }

    private int GetSafePlacedQuantityForItem(
        ItemData item,
        int quantity)
    {
        quantity = Mathf.Max(1, quantity);

        if (item == null)
            return quantity;

        if (!item.isStackable)
            return 1;

        return Mathf.Clamp(
            quantity,
            1,
            Mathf.Max(1, item.maxStackSize)
        );
    }

    private int GetSafeTransferQuantityForItem(
        ItemData item,
        int quantity)
    {
        quantity = Mathf.Max(1, quantity);

        if (item == null)
            return quantity;

        if (!item.isStackable)
            return 1;

        return quantity;
    }

    private void ValidateWeaponSlot()
    {
        if (weaponSlotItem == null)
        {
            weaponDrawn = false;
            return;
        }

        if (!IsWeapon(weaponSlotItem))
        {
            Debug.LogWarning(
                weaponSlotItem.itemName +
                " is assigned to the old weapon slot, but it is not marked as a Weapon."
            );

            weaponSlotItem = null;
            weaponDrawn = false;
        }
    }

    private void PlaceStartingItems()
    {
        for (int i = 0; i < startingItems.Count; i++)
        {
            StartingInventoryItem startingItem =
                startingItems[i];

            if (startingItem == null ||
                startingItem.item == null)
            {
                continue;
            }

            bool placed =
                TryPlaceItem(
                    startingItem.item,
                    startingItem.x,
                    startingItem.y,
                    startingItem.rotationSteps,
                    startingItem.quantity
                );

            if (!placed)
            {
                Debug.LogWarning(
                    "Could not place starting item: " +
                    startingItem.item.itemName +
                    " at " +
                    startingItem.x +
                    ", " +
                    startingItem.y
                );
            }
        }
    }

    public bool ConsumeCenterHeldItemOnCursorRequest()
    {
        if (!CenterHeldItemOnCursorRequested)
            return false;

        CenterHeldItemOnCursorRequested = false;
        return true;
    }

    public bool CanPlaceItem(
        ItemData item,
        int x,
        int y,
        int rotationSteps)
    {
        if (Grid == null)
            return false;

        return Grid.CanPlaceItem(
            item,
            x,
            y,
            rotationSteps
        );
    }

    public bool TryPlaceItem(
        ItemData item,
        int x,
        int y,
        int rotationSteps,
        int quantity = 1)
    {
        if (Grid == null)
            return false;

        int safeQuantity =
            GetSafePlacedQuantityForItem(
                item,
                quantity
            );

        bool placed =
            Grid.PlaceItem(
                item,
                x,
                y,
                rotationSteps,
                safeQuantity
            );

        if (placed)
            OnInventoryChanged?.Invoke();

        return placed;
    }

    public bool TryMergeHeldItemIntoStackAt(
        int x,
        int y)
    {
        if (Grid == null)
            return false;

        if (HeldItem == null ||
            HeldItem.ItemData == null)
        {
            return false;
        }

        PlacedInventoryItem targetStack =
            Grid.GetPlacedItem(x, y);

        if (targetStack == null ||
            targetStack.ItemData == null)
        {
            return false;
        }

        if (targetStack == HeldItem)
            return false;

        if (targetStack.ItemData != HeldItem.ItemData)
            return false;

        if (!targetStack.ItemData.isStackable)
            return false;

        if (!targetStack.HasRoomInStack)
            return false;

        int heldQuantity =
            Mathf.Max(1, HeldItem.Quantity);

        int addedQuantity =
            targetStack.AddQuantity(heldQuantity);

        if (addedQuantity <= 0)
            return false;

        int remainingQuantity =
            heldQuantity - addedQuantity;

        if (remainingQuantity <= 0)
        {
            HeldItem = null;
            MouseHeldItemCountsAsHeld = false;
            CenterHeldItemOnCursorRequested = false;
        }
        else
        {
            HeldItem.SetQuantity(remainingQuantity);
            CenterHeldItemOnCursorRequested = true;
        }

        OnInventoryChanged?.Invoke();
        OnHeldItemChanged?.Invoke();

        return true;
    }

    public bool TryAddItemToFirstAvailableSpace(
        ItemData item,
        int rotationSteps = 0,
        int quantity = 1)
    {
        return TryAddItemToFirstAvailableSpace(
            item,
            rotationSteps,
            quantity,
            out int remainingQuantity
        );
    }

    public bool TryAddItemToFirstAvailableSpace(
        ItemData item,
        int rotationSteps,
        int quantity,
        out int remainingQuantity)
    {
        remainingQuantity = quantity;

        if (Grid == null ||
            item == null ||
            quantity <= 0)
        {
            return false;
        }

        int safeQuantity =
            GetSafeTransferQuantityForItem(
                item,
                quantity
            );

        bool fullyAdded =
            Grid.TryAddItemTopLeft(
                item,
                rotationSteps,
                safeQuantity,
                out remainingQuantity
            );

        int addedQuantity =
            safeQuantity - remainingQuantity;

        if (addedQuantity > 0)
            OnInventoryChanged?.Invoke();

        return fullyAdded;
    }

    public PlacedInventoryItem TryPickUpItemAt(
        int x,
        int y,
        bool countsAsHeld = true)
    {
        if (Grid == null)
            return null;

        if (HeldItem != null)
            return null;

        PlacedInventoryItem itemAtCell =
            Grid.GetPlacedItem(x, y);

        if (itemAtCell == null)
            return null;

        if (countsAsHeld)
        {
            if (playerWeaponSlots != null &&
                playerWeaponSlots.WeaponsDrawn)
            {
                bool canKeepWeaponsDrawn =
                    playerWeaponSlots.ActiveSetCanCoexistWithHeldItem(
                        itemAtCell.ItemData
                    );

                if (!canKeepWeaponsDrawn)
                    playerWeaponSlots.SheatheWeapons();
            }

            if (weaponDrawn)
            {
                bool oldWeaponCanStayDrawn =
                    weaponSlotItem != null &&
                    weaponSlotItem.handUsage == ItemHandUsage.OneHanded &&
                    itemAtCell.ItemData.handUsage == ItemHandUsage.OneHanded;

                if (!oldWeaponCanStayDrawn)
                    SheatheWeapon();
            }
        }

        PlacedInventoryItem pickedItem =
            Grid.PickUpItemAt(x, y);

        if (pickedItem == null)
            return null;

        HeldItem = pickedItem;
        MouseHeldItemCountsAsHeld = countsAsHeld;

        OnInventoryChanged?.Invoke();
        OnHeldItemChanged?.Invoke();

        return HeldItem;
    }

    public bool TrySplitStackAt(
    int x,
    int y,
    bool countsAsHeld = true)
    {
        if (Grid == null)
            return false;

        if (HeldItem != null)
            return false;

        PlacedInventoryItem sourceStack =
            Grid.GetPlacedItem(x, y);

        if (sourceStack == null ||
            sourceStack.ItemData == null)
        {
            return false;
        }

        ItemData itemData =
            sourceStack.ItemData;

        if (!itemData.isStackable)
            return false;

        int sourceQuantity =
            Mathf.Max(1, sourceStack.Quantity);

        if (sourceQuantity <= 1)
            return false;

        int splitQuantity =
            Mathf.CeilToInt(sourceQuantity * 0.5f);

        int remainingQuantity =
            sourceQuantity - splitQuantity;

        if (remainingQuantity <= 0)
            return false;

        if (countsAsHeld)
        {
            if (playerWeaponSlots != null &&
                playerWeaponSlots.WeaponsDrawn)
            {
                bool canKeepWeaponsDrawn =
                    playerWeaponSlots.ActiveSetCanCoexistWithHeldItem(
                        itemData
                    );

                if (!canKeepWeaponsDrawn)
                    playerWeaponSlots.SheatheWeapons();
            }

            if (weaponDrawn)
            {
                bool oldWeaponCanStayDrawn =
                    weaponSlotItem != null &&
                    weaponSlotItem.handUsage == ItemHandUsage.OneHanded &&
                    itemData.handUsage == ItemHandUsage.OneHanded;

                if (!oldWeaponCanStayDrawn)
                    SheatheWeapon();
            }
        }

        sourceStack.SetQuantity(remainingQuantity);

        HeldItem =
            new PlacedInventoryItem(
                itemData,
                Vector2Int.zero,
                sourceStack.RotationSteps,
                splitQuantity
            );

        MouseHeldItemCountsAsHeld = countsAsHeld;
        CenterHeldItemOnCursorRequested = true;

        OnInventoryChanged?.Invoke();
        OnHeldItemChanged?.Invoke();

        return true;
    }

    public bool CanPlaceHeldItem(
        int x,
        int y)
    {
        if (Grid == null)
            return false;

        if (HeldItem == null ||
            HeldItem.ItemData == null)
        {
            return false;
        }

        return Grid.CanPlaceItem(
            HeldItem.ItemData,
            x,
            y,
            HeldItem.RotationSteps
        );
    }

    public bool TryPlaceHeldItem(
        int x,
        int y)
    {
        if (Grid == null)
            return false;

        if (HeldItem == null ||
            HeldItem.ItemData == null)
        {
            return false;
        }

        ItemData itemData =
            HeldItem.ItemData;

        int rotationSteps =
            HeldItem.RotationSteps;

        int quantity =
            Mathf.Max(1, HeldItem.Quantity);

        bool placed =
            Grid.PlaceItem(
                itemData,
                x,
                y,
                rotationSteps,
                quantity
            );

        if (!placed)
            return false;

        HeldItem = null;
        MouseHeldItemCountsAsHeld = false;

        OnInventoryChanged?.Invoke();
        OnHeldItemChanged?.Invoke();

        return true;
    }

    public bool TryPlaceOneHeldItem(
    int x,
    int y)
    {
        if (Grid == null)
            return false;

        if (HeldItem == null ||
            HeldItem.ItemData == null)
        {
            return false;
        }

        ItemData itemData =
            HeldItem.ItemData;

        if (!itemData.isStackable)
            return false;

        int heldQuantity =
            Mathf.Max(1, HeldItem.Quantity);

        if (heldQuantity <= 0)
            return false;

        PlacedInventoryItem targetStack =
            Grid.GetPlacedItem(x, y);

        if (targetStack != null)
        {
            if (targetStack.ItemData != itemData)
                return false;

            if (!targetStack.ItemData.isStackable)
                return false;

            if (!targetStack.HasRoomInStack)
                return false;

            int addedQuantity =
                targetStack.AddQuantity(1);

            if (addedQuantity <= 0)
                return false;

            ReduceHeldStackAfterPlacingOne();

            OnInventoryChanged?.Invoke();
            OnHeldItemChanged?.Invoke();

            return true;
        }

        bool placed =
            Grid.PlaceItem(
                itemData,
                x,
                y,
                HeldItem.RotationSteps,
                1
            );

        if (!placed)
            return false;

        ReduceHeldStackAfterPlacingOne();

        OnInventoryChanged?.Invoke();
        OnHeldItemChanged?.Invoke();

        return true;
    }

    private void ReduceHeldStackAfterPlacingOne()
    {
        if (HeldItem == null)
            return;

        int heldQuantity =
            Mathf.Max(1, HeldItem.Quantity);

        int remainingQuantity =
            heldQuantity - 1;

        if (remainingQuantity <= 0)
        {
            HeldItem = null;
            MouseHeldItemCountsAsHeld = false;
            CenterHeldItemOnCursorRequested = false;
            return;
        }

        HeldItem.SetQuantity(remainingQuantity);
        CenterHeldItemOnCursorRequested = true;
    }

    public bool RotateHeldItemCounterClockwise()
    {
        if (HeldItem == null ||
            HeldItem.ItemData == null)
        {
            return false;
        }

        HeldItem.RotateCounterClockwise();

        OnHeldItemChanged?.Invoke();

        return true;
    }

    public void ClearHeldItemAfterExternalMove()
    {
        if (HeldItem == null)
            return;

        HeldItem = null;
        MouseHeldItemCountsAsHeld = false;
        CenterHeldItemOnCursorRequested = false;

        OnHeldItemChanged?.Invoke();
    }

    public void SetHeldItemQuantityAfterExternalMove(
        int quantity)
    {
        if (HeldItem == null ||
            HeldItem.ItemData == null)
        {
            return;
        }

        int safeQuantity =
            GetSafePlacedQuantityForItem(
                HeldItem.ItemData,
                quantity
            );

        HeldItem.SetQuantity(safeQuantity);
        CenterHeldItemOnCursorRequested = true;

        OnHeldItemChanged?.Invoke();
    }

    public void SetMouseHeldItemFromExternal(
        ItemData item,
        int rotationSteps = 0,
        bool countsAsHeld = true,
        int quantity = 1)
    {
        if (item == null)
        {
            HeldItem = null;
            MouseHeldItemCountsAsHeld = false;
            CenterHeldItemOnCursorRequested = false;

            OnHeldItemChanged?.Invoke();
            return;
        }

        if (countsAsHeld)
        {
            if (playerWeaponSlots != null &&
                playerWeaponSlots.WeaponsDrawn)
            {
                bool canKeepWeaponsDrawn =
                    playerWeaponSlots.ActiveSetCanCoexistWithHeldItem(
                        item
                    );

                if (!canKeepWeaponsDrawn)
                    playerWeaponSlots.SheatheWeapons();
            }

            if (weaponDrawn)
            {
                bool oldWeaponCanStayDrawn =
                    weaponSlotItem != null &&
                    weaponSlotItem.handUsage == ItemHandUsage.OneHanded &&
                    item.handUsage == ItemHandUsage.OneHanded;

                if (!oldWeaponCanStayDrawn)
                    SheatheWeapon();
            }
        }

        int safeQuantity =
            GetSafePlacedQuantityForItem(
                item,
                quantity
            );

        HeldItem =
            new PlacedInventoryItem(
                item,
                Vector2Int.zero,
                rotationSteps,
                safeQuantity
            );

        MouseHeldItemCountsAsHeld = countsAsHeld;
        CenterHeldItemOnCursorRequested = true;

        OnHeldItemChanged?.Invoke();
    }

    public bool HasUsableMouseHeldWeapon()
    {
        return GetUsableMouseHeldWeapon() != null;
    }

    public ItemData GetUsableMouseHeldWeapon()
    {
        if (!MouseHeldItemCountsAsHeld)
            return null;

        if (HeldItem == null ||
            HeldItem.ItemData == null)
        {
            return null;
        }

        ItemData item =
            HeldItem.ItemData;

        if (!IsWeapon(item))
            return null;

        if (item.weaponUseType != WeaponUseType.HandWeapon)
            return null;

        return item;
    }

    public bool TryStoreHeldItemInInventoryOrDrop()
    {
        if (HeldItem == null ||
            HeldItem.ItemData == null)
        {
            MouseHeldItemCountsAsHeld = false;
            return true;
        }

        if (Grid == null)
            return false;

        ItemData itemData =
            HeldItem.ItemData;

        int rotationSteps =
            HeldItem.RotationSteps;

        int quantity =
            Mathf.Max(1, HeldItem.Quantity);

        bool fullyStored =
            Grid.TryAddItemTopLeft(
                itemData,
                rotationSteps,
                quantity,
                out int remainingQuantity
            );

        int storedQuantity =
            quantity - remainingQuantity;

        if (storedQuantity > 0)
            OnInventoryChanged?.Invoke();

        HeldItem = null;
        MouseHeldItemCountsAsHeld = false;

        if (!fullyStored)
        {
            Debug.Log(
                "Dropped " +
                remainingQuantity +
                " of " +
                itemData.itemName +
                " because there was no room in the inventory. Temporary behavior: item disappears."
            );
        }

        OnHeldItemChanged?.Invoke();

        return fullyStored;
    }

    public bool CanEquipHeldItemToWeaponSlot()
    {
        if (HeldItem == null ||
            HeldItem.ItemData == null)
        {
            return false;
        }

        if (weaponSlotItem != null)
            return false;

        return IsWeapon(HeldItem.ItemData);
    }

    public bool TryEquipHeldItemToWeaponSlot()
    {
        if (!CanEquipHeldItemToWeaponSlot())
            return false;

        weaponSlotItem = HeldItem.ItemData;
        weaponDrawn = false;

        HeldItem = null;
        MouseHeldItemCountsAsHeld = false;

        OnHeldItemChanged?.Invoke();
        OnEquipmentChanged?.Invoke();

        return true;
    }

    public bool TryPickUpWeaponSlotItem()
    {
        if (weaponSlotItem == null)
            return false;

        if (HeldItem != null)
            return false;

        if (weaponDrawn)
            SheatheWeapon();

        HeldItem =
            new PlacedInventoryItem(
                weaponSlotItem,
                Vector2Int.zero,
                0
            );

        CenterHeldItemOnCursorRequested = true;
        MouseHeldItemCountsAsHeld = true;

        weaponSlotItem = null;
        weaponDrawn = false;

        OnHeldItemChanged?.Invoke();
        OnEquipmentChanged?.Invoke();

        return true;
    }

    public bool TrySwapHeldWeaponWithWeaponSlot()
    {
        if (HeldItem == null ||
            HeldItem.ItemData == null)
        {
            return false;
        }

        if (!IsWeapon(HeldItem.ItemData))
            return false;

        if (weaponSlotItem == null)
            return TryEquipHeldItemToWeaponSlot();

        if (weaponDrawn)
            SheatheWeapon();

        ItemData oldWeapon =
            weaponSlotItem;

        weaponSlotItem =
            HeldItem.ItemData;

        HeldItem =
            new PlacedInventoryItem(
                oldWeapon,
                Vector2Int.zero,
                0
            );

        CenterHeldItemOnCursorRequested = true;
        MouseHeldItemCountsAsHeld = true;

        weaponDrawn = false;

        OnHeldItemChanged?.Invoke();
        OnEquipmentChanged?.Invoke();

        return true;
    }

    public bool TryEquipWeaponToSlot(
        ItemData weaponItem)
    {
        if (!IsWeapon(weaponItem))
            return false;

        if (weaponSlotItem != null)
            return false;

        weaponSlotItem = weaponItem;
        weaponDrawn = false;

        OnEquipmentChanged?.Invoke();

        return true;
    }

    public bool DrawWeapon()
    {
        if (weaponSlotItem == null)
            return false;

        if (weaponDrawn)
            return true;

        if (HeldItem != null &&
            MouseHeldItemCountsAsHeld)
        {
            bool canKeepHeldItem =
                weaponSlotItem.handUsage == ItemHandUsage.OneHanded &&
                HeldItem.ItemData.handUsage == ItemHandUsage.OneHanded;

            if (!canKeepHeldItem)
                TryStoreHeldItemInInventoryOrDrop();
        }

        weaponDrawn = true;

        OnEquipmentChanged?.Invoke();

        return true;
    }

    public bool SheatheWeapon()
    {
        if (!weaponDrawn)
            return false;

        weaponDrawn = false;

        OnEquipmentChanged?.Invoke();

        return true;
    }

    public bool ToggleWeaponDrawn()
    {
        if (weaponDrawn)
            return SheatheWeapon();

        return DrawWeapon();
    }

    public bool IsWeapon(
        ItemData item)
    {
        return item != null &&
               item.itemCategory == ItemCategory.Weapon;
    }
}