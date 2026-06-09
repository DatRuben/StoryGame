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
    [SerializeField] private bool startingRotated = false;

    [Header("Weapon Slot")]
    [SerializeField] private ItemData weaponSlotItem;
    [SerializeField] private bool weaponDrawn = false;

    public InventoryGrid Grid { get; private set; }

    // Cursor/inventory-held item.
    // This is the item that follows the mouse while inventory is open.
    public PlacedInventoryItem HeldItem { get; private set; }

    public bool IsHoldingItem => HeldItem != null;

    // Equipped weapon slot item.
    // If weaponDrawn is true, this weapon is visually in the character's hands.
    public ItemData WeaponSlotItem => weaponSlotItem;
    public bool IsWeaponDrawn => weaponDrawn;
    public bool HasWeaponInSlot => weaponSlotItem != null;

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
                startingRotated
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
                " is assigned to the weapon slot, but it is not marked as a Weapon."
            );

            weaponSlotItem = null;
            weaponDrawn = false;
        }
    }

    public bool CanPlaceItem(
        ItemData item,
        int x,
        int y,
        bool rotated)
    {
        if (Grid == null)
            return false;

        return Grid.CanPlaceItem(
            item,
            x,
            y,
            rotated
        );
    }

    public bool TryPlaceItem(
        ItemData item,
        int x,
        int y,
        bool rotated)
    {
        if (Grid == null)
            return false;

        bool placed =
            Grid.PlaceItem(item, x, y, rotated);

        if (placed)
        {
            OnInventoryChanged?.Invoke();
        }

        return placed;
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

        OnHeldItemChanged?.Invoke();
        OnEquipmentChanged?.Invoke();

        return true;
    }

    public PlacedInventoryItem TryPickUpItemAt(
        int x,
        int y)
    {
        if (Grid == null)
            return null;

        if (HeldItem != null)
            return null;

        PlacedInventoryItem itemAtCell =
            Grid.GetPlacedItem(x, y);

        if (itemAtCell == null)
            return null;

        // If the character is holding their equipped weapon,
        // put it away before letting them cursor-hold an inventory item.
        if (weaponDrawn)
        {
            SheatheWeapon();
        }

        PlacedInventoryItem pickedItem =
            Grid.PickUpItemAt(x, y);

        if (pickedItem == null)
            return null;

        HeldItem = pickedItem;

        OnInventoryChanged?.Invoke();
        OnHeldItemChanged?.Invoke();

        return HeldItem;
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
            HeldItem.Rotated
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

        bool rotated =
            HeldItem.Rotated;

        bool placed =
            Grid.PlaceItem(
                itemData,
                x,
                y,
                rotated
            );

        if (!placed)
            return false;

        HeldItem = null;

        OnInventoryChanged?.Invoke();
        OnHeldItemChanged?.Invoke();

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

        // Do not draw the equipped weapon while managing a cursor-held item.
        if (HeldItem != null)
            return false;

        if (weaponDrawn)
            return true;

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
}