using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StartingStorageItem
{
    public ItemData item;

    [Min(1)]
    public int quantity = 1;

    public int x;
    public int y;

    [Tooltip("0 = 0°, 1 = 90°, 2 = 180°, 3 = 270°")]
    [Range(0, 3)]
    public int rotationSteps;
}

public class StorageContainer : MonoBehaviour
{
    [Header("Grid Size")]
    [SerializeField] private int gridWidth = 8;
    [SerializeField] private int gridHeight = 6;

    [Header("Starting Items")]
    [SerializeField]
    private List<StartingStorageItem> startingItems =
        new List<StartingStorageItem>();

    public InventoryGrid Grid { get; private set; }

    public event Action OnContainerChanged;

    private void OnValidate()
    {
        ClampStartingItemQuantities();
    }

    private void Awake()
    {
        Grid =
            new InventoryGrid(
                gridWidth,
                gridHeight
            );

        ClampStartingItemQuantities();
        PlaceStartingItems();
    }

    private void ClampStartingItemQuantities()
    {
        if (startingItems == null)
            return;

        for (int i = 0; i < startingItems.Count; i++)
        {
            StartingStorageItem startingItem =
                startingItems[i];

            if (startingItem == null)
                continue;

            startingItem.quantity =
                GetSafeQuantityForItem(
                    startingItem.item,
                    startingItem.quantity
                );
        }
    }

    private int GetSafeQuantityForItem(
        ItemData item,
        int quantity)
    {
        quantity =
            Mathf.Max(1, quantity);

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

    private void PlaceStartingItems()
    {
        if (Grid == null ||
            startingItems == null)
        {
            return;
        }

        for (int i = 0; i < startingItems.Count; i++)
        {
            StartingStorageItem startingItem =
                startingItems[i];

            if (startingItem == null ||
                startingItem.item == null)
            {
                continue;
            }

            bool placed =
                Grid.PlaceItem(
                    startingItem.item,
                    startingItem.x,
                    startingItem.y,
                    startingItem.rotationSteps,
                    startingItem.quantity
                );

            if (!placed)
            {
                Debug.LogWarning(
                    "Could not place storage item: " +
                    startingItem.item.itemName +
                    " at " +
                    startingItem.x +
                    ", " +
                    startingItem.y,
                    this
                );
            }
        }

        OnContainerChanged?.Invoke();
    }

    public bool TryAddItem(
        ItemData item,
        int rotationSteps,
        int quantity,
        out int remainingQuantity)
    {
        remainingQuantity =
            Mathf.Max(0, quantity);

        if (Grid == null ||
            item == null ||
            quantity <= 0)
        {
            return false;
        }

        int safeQuantity =
            GetSafeQuantityForItem(
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

        if (remainingQuantity < safeQuantity)
            OnContainerChanged?.Invoke();

        return fullyAdded;
    }

    public PlacedInventoryItem PickUpItemAt(
        int x,
        int y)
    {
        if (Grid == null)
            return null;

        PlacedInventoryItem pickedItem =
            Grid.PickUpItemAt(x, y);

        if (pickedItem != null)
            OnContainerChanged?.Invoke();

        return pickedItem;
    }

    public void NotifyChanged()
    {
        OnContainerChanged?.Invoke();
    }

    public bool PlaceItem(
        ItemData item,
        int x,
        int y,
        int rotationSteps,
        int quantity)
    {
        if (Grid == null)
            return false;

        int safeQuantity =
            GetSafeQuantityForItem(
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
            OnContainerChanged?.Invoke();

        return placed;
    }
}