using System;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Grid Size")]
    [SerializeField] private int gridWidth = 8;
    [SerializeField] private int gridHeight = 6;

    [Header("Test Starting Item")]
    [SerializeField] private ItemData startingItem;
    [SerializeField] private int startingX = 1;
    [SerializeField] private int startingY = 1;

    [Tooltip("0 = 0°, 1 = 90°, 2 = 180°, 3 = 270°")]
    [SerializeField] private int startingRotationSteps = 0;

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
    }

    private void Start()
    {
        ValidateWeaponSlot();

        if (startingItem != null)
        {
            TryPlaceItem(
                startingItem,
                startingX,
                startingY,
                startingRotationSteps
            );
        }
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
        int rotationSteps)
    {
        if (Grid == null)
            return false;

        bool placed =
            Grid.PlaceItem(
                item,
                x,
                y,
                rotationSteps
            );

        if (placed)
            OnInventoryChanged?.Invoke();

        return placed;
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

        if (countsAsHeld &&
            weaponDrawn &&
            WeaponConflictsWithHeldItem(
                weaponSlotItem,
                itemAtCell.ItemData
            ))
        {
            SheatheWeapon();
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

    public bool CanPlaceHeldItem(int x, int y)
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

    public bool TryPlaceHeldItem(int x, int y)
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

        bool placed =
            Grid.PlaceItem(
                itemData,
                x,
                y,
                rotationSteps
            );

        if (!placed)
            return false;

        HeldItem = null;
        MouseHeldItemCountsAsHeld = false;

        OnInventoryChanged?.Invoke();
        OnHeldItemChanged?.Invoke();

        return true;
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

    public void SetMouseHeldItemFromExternal(
        ItemData item,
        int rotationSteps = 0,
        bool countsAsHeld = true)
    {
        if (item == null)
        {
            HeldItem = null;
            MouseHeldItemCountsAsHeld = false;
            CenterHeldItemOnCursorRequested = false;

            OnHeldItemChanged?.Invoke();
            return;
        }

        HeldItem =
            new PlacedInventoryItem(
                item,
                Vector2Int.zero,
                rotationSteps
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

    public bool TryEquipWeaponToSlot(ItemData weaponItem)
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
            MouseHeldItemCountsAsHeld &&
            WeaponConflictsWithHeldItem(
                weaponSlotItem,
                HeldItem.ItemData
            ))
        {
            TryStoreMouseHeldItemInInventoryOrDrop();
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

    public bool IsWeapon(ItemData item)
    {
        return item != null &&
               item.itemCategory == ItemCategory.Weapon;
    }

    private bool WeaponConflictsWithHeldItem(
        ItemData weapon,
        ItemData heldItem)
    {
        if (weapon == null ||
            heldItem == null)
        {
            return false;
        }

        bool weaponIsTwoHanded =
            weapon.handUsage == ItemHandUsage.TwoHanded;

        bool heldItemIsTwoHanded =
            heldItem.handUsage == ItemHandUsage.TwoHanded;

        return weaponIsTwoHanded ||
               heldItemIsTwoHanded;
    }

    private bool TryStoreMouseHeldItemInInventoryOrDrop()
    {
        if (HeldItem == null ||
            HeldItem.ItemData == null)
        {
            MouseHeldItemCountsAsHeld = false;
            return true;
        }

        ItemData itemData =
            HeldItem.ItemData;

        int rotationSteps =
            HeldItem.RotationSteps;

        for (int y = 0; y < Grid.Height; y++)
        {
            for (int x = 0; x < Grid.Width; x++)
            {
                bool canPlace =
                    Grid.CanPlaceItem(
                        itemData,
                        x,
                        y,
                        rotationSteps
                    );

                if (!canPlace)
                    continue;

                Grid.PlaceItem(
                    itemData,
                    x,
                    y,
                    rotationSteps
                );

                HeldItem = null;
                MouseHeldItemCountsAsHeld = false;

                OnInventoryChanged?.Invoke();
                OnHeldItemChanged?.Invoke();

                return true;
            }
        }

        Debug.Log(
            "Dropped held item because there was no room in the inventory. Temporary behavior: item disappears."
        );

        HeldItem = null;
        MouseHeldItemCountsAsHeld = false;

        OnHeldItemChanged?.Invoke();

        return false;
    }
}