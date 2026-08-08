using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StartingInventoryItem
{
    public ItemDefinition item;

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

    public InventoryGrid Grid { get; private set; }

    public PlacedInventoryItem HeldItem { get; private set; }
    public bool IsHoldingItem => HeldItem != null;

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
        ItemDefinition item,
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
        ItemDefinition item,
        int quantity)
    {
        quantity = Mathf.Max(1, quantity);

        if (item == null)
            return quantity;

        if (!item.isStackable)
            return 1;

        return quantity;
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
        ItemDefinition item,
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
        ItemDefinition item,
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
            Grid.SpawnItem(
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
        if (Grid == null ||
            HeldItem == null ||
            HeldItem.ItemInstance == null)
        {
            return false;
        }

        PlacedInventoryItem targetStack =
            Grid.GetPlacedItem(
                x,
                y
            );

        if (targetStack == null ||
            targetStack.ItemInstance == null)
        {
            return false;
        }

        int moved =
            HeldItem.ItemInstance.MoveQuantityTo(
                targetStack.ItemInstance,
                HeldItem.Quantity
            );

        if (moved <= 0)
            return false;

        if (HeldItem.ItemInstance.IsEmpty)
        {
            HeldItem = null;
            MouseHeldItemCountsAsHeld = false;
            CenterHeldItemOnCursorRequested = false;
        }
        else
        {
            CenterHeldItemOnCursorRequested = true;
        }

        OnInventoryChanged?.Invoke();
        OnHeldItemChanged?.Invoke();

        return true;
    }

    public bool TryAddItemToFirstAvailableSpace(
        ItemDefinition item,
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
        ItemDefinition item,
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

    public bool TryAddItemToFirstAvailableSpace(
        InventoryItemInstance itemInstance,
        int rotationSteps,
        out int remainingQuantity)
    {
        remainingQuantity =
            itemInstance != null
                ? Mathf.Max(
                    0,
                    itemInstance.Quantity
                )
                : 0;

        if (Grid == null ||
            itemInstance == null ||
            itemInstance.Definition == null ||
            itemInstance.IsEmpty)
        {
            return false;
        }

        int originalQuantity =
            itemInstance.Quantity;

        bool fullyAdded =
            Grid.TryAddItemTopLeft(
                itemInstance,
                rotationSteps,
                out remainingQuantity
            );

        if (remainingQuantity <
            originalQuantity)
        {
            OnInventoryChanged?.Invoke();
        }

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
                        itemAtCell.ItemDefinition
                    );

                if (!canKeepWeaponsDrawn)
                    playerWeaponSlots.SheatheWeapons();
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
            sourceStack.ItemInstance == null)
        {
            return false;
        }

        ItemDefinition itemDefinition =
            sourceStack.ItemDefinition;

        if (!itemDefinition.isStackable)
            return false;

        int sourceQuantity =
            Mathf.Max(1, sourceStack.Quantity);

        if (sourceQuantity <= 1)
            return false;

        int splitQuantity =
            Mathf.CeilToInt(sourceQuantity * 0.5f);

        if (countsAsHeld)
        {
            if (playerWeaponSlots != null &&
                playerWeaponSlots.WeaponsDrawn)
            {
                bool canKeepWeaponsDrawn =
                    playerWeaponSlots.ActiveSetCanCoexistWithHeldItem(
                        itemDefinition
                    );

                if (!canKeepWeaponsDrawn)
                    playerWeaponSlots.SheatheWeapons();
            }
        }

        bool split =
            sourceStack.ItemInstance.TrySplit(
                splitQuantity,
                out InventoryItemInstance splitInstance
            );

        if (!split)
            return false;

        HeldItem =
            new PlacedInventoryItem(
                splitInstance,
                Vector2Int.zero,
                sourceStack.RotationSteps
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
            HeldItem.ItemDefinition == null)
        {
            return false;
        }

        return Grid.CanPlaceItem(
            HeldItem.ItemDefinition,
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
            HeldItem.ItemInstance == null ||
            HeldItem.ItemDefinition == null)
        {
            return false;
        }

        bool placed =
            Grid.PlaceItem(
                HeldItem.ItemInstance,
                x,
                y,
                HeldItem.RotationSteps
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
        if (Grid == null ||
            HeldItem == null ||
            HeldItem.ItemInstance == null ||
            !HeldItem.ItemInstance.IsStackable)
        {
            return false;
        }

        PlacedInventoryItem targetStack =
            Grid.GetPlacedItem(
                x,
                y
            );

        if (targetStack != null)
        {
            if (targetStack.ItemInstance == null)
                return false;

            int moved =
                HeldItem.ItemInstance.MoveQuantityTo(
                    targetStack.ItemInstance,
                    1
                );

            if (moved != 1)
                return false;

            if (HeldItem.ItemInstance.IsEmpty)
            {
                HeldItem = null;
                MouseHeldItemCountsAsHeld = false;
                CenterHeldItemOnCursorRequested = false;
            }
            else
            {
                CenterHeldItemOnCursorRequested = true;
            }

            OnInventoryChanged?.Invoke();
            OnHeldItemChanged?.Invoke();

            return true;
        }

        bool movingWholeInstance =
            HeldItem.Quantity == 1;

        InventoryItemInstance instanceToPlace;

        if (movingWholeInstance)
        {
            instanceToPlace =
                HeldItem.ItemInstance;
        }
        else
        {
            bool split =
                HeldItem.ItemInstance.TrySplit(
                    1,
                    out instanceToPlace
                );

            if (!split)
                return false;
        }

        bool placed =
            Grid.PlaceItem(
                instanceToPlace,
                x,
                y,
                HeldItem.RotationSteps
            );

        if (!placed)
        {
            if (!movingWholeInstance)
            {
                instanceToPlace.MoveQuantityTo(
                    HeldItem.ItemInstance,
                    instanceToPlace.Quantity
                );
            }

            return false;
        }

        if (movingWholeInstance)
        {
            HeldItem = null;
            MouseHeldItemCountsAsHeld = false;
            CenterHeldItemOnCursorRequested = false;
        }
        else
        {
            CenterHeldItemOnCursorRequested = true;
        }

        OnInventoryChanged?.Invoke();
        OnHeldItemChanged?.Invoke();

        return true;
    }

    public bool RotateHeldItemCounterClockwise()
    {
        if (HeldItem == null ||
            HeldItem.ItemDefinition == null)
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
            HeldItem.ItemInstance == null)
        {
            return;
        }

        int safeQuantity =
            GetSafePlacedQuantityForItem(
                HeldItem.ItemDefinition,
                quantity
            );

        HeldItem.ItemInstance.SetQuantity(
            safeQuantity
        );
        CenterHeldItemOnCursorRequested = true;

        OnHeldItemChanged?.Invoke();
    }

    public void SetMouseHeldItemFromExternal(
        ItemDefinition item,
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

    public void SetMouseHeldItemFromExternal(
        InventoryItemInstance itemInstance,
        int rotationSteps = 0,
        bool countsAsHeld = true)
    {
        if (itemInstance == null ||
            itemInstance.Definition == null ||
            itemInstance.IsEmpty)
        {
            HeldItem = null;
            MouseHeldItemCountsAsHeld = false;
            CenterHeldItemOnCursorRequested = false;

            OnHeldItemChanged?.Invoke();
            return;
        }

        ItemDefinition item =
            itemInstance.Definition;

        if (countsAsHeld)
        {
            if (playerWeaponSlots != null &&
                playerWeaponSlots.WeaponsDrawn)
            {
                bool canKeepWeaponsDrawn =
                    playerWeaponSlots
                        .ActiveSetCanCoexistWithHeldItem(
                            item
                        );

                if (!canKeepWeaponsDrawn)
                    playerWeaponSlots.SheatheWeapons();
            }
        }

        HeldItem =
            new PlacedInventoryItem(
                itemInstance,
                Vector2Int.zero,
                rotationSteps
            );

        MouseHeldItemCountsAsHeld =
            countsAsHeld;

        CenterHeldItemOnCursorRequested =
            true;

        OnHeldItemChanged?.Invoke();
    }

    public bool HasUsableMouseHeldWeapon()
    {
        return GetUsableMouseHeldWeapon() != null;
    }

    public ItemDefinition GetUsableMouseHeldWeapon()
    {
        if (!MouseHeldItemCountsAsHeld)
            return null;

        if (HeldItem == null ||
            HeldItem.ItemDefinition == null)
        {
            return null;
        }

        ItemDefinition item =
            HeldItem.ItemDefinition;

        if (!IsWeapon(item))
            return null;

        if (item.weaponUseType != WeaponUseType.HandWeapon)
            return null;

        return item;
    }

    public bool TryStoreHeldItemInInventoryOrDrop()
    {
        if (HeldItem == null ||
            HeldItem.ItemDefinition == null)
        {
            MouseHeldItemCountsAsHeld = false;
            return true;
        }

        if (Grid == null)
            return false;

        ItemDefinition itemDefinition =
            HeldItem.ItemDefinition;

        int rotationSteps =
            HeldItem.RotationSteps;

        int quantity =
            Mathf.Max(1, HeldItem.Quantity);

        bool fullyStored =
            Grid.TryAddItemTopLeft(
                itemDefinition,
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
                itemDefinition.itemName +
                " because there was no room in the inventory. Temporary behavior: item disappears."
            );
        }

        OnHeldItemChanged?.Invoke();

        return fullyStored;
    }

    public bool IsWeapon(
        ItemDefinition item)
    {
        return item != null &&
               item.itemCategory == ItemCategory.Weapon;
    }
}