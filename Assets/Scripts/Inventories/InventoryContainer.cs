using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryStartingItem
{
    public ItemDefinition item;

    [Min(1)]
    public int quantity = 1;

    public int x;
    public int y;

    [Range(0, 3)]
    public int rotationSteps;
}

public class InventoryContainer : MonoBehaviour
{
    [Header("Grid Size")]
    [SerializeField]
    [Min(1)]
    private int gridWidth = 8;

    [SerializeField]
    [Min(1)]
    private int gridHeight = 6;

    [Header("Starting Items")]
    [SerializeField]
    private List<InventoryStartingItem> startingItems =
        new List<InventoryStartingItem>();

    private InventoryGrid grid;

    public int Width =>
        grid != null
            ? grid.Width
            : Mathf.Max(1, gridWidth);

    public int Height =>
        grid != null
            ? grid.Height
            : Mathf.Max(1, gridHeight);

    public event Action Changed;

    private void Awake()
    {
        grid =
            new InventoryGrid(
                Mathf.Max(1, gridWidth),
                Mathf.Max(1, gridHeight)
            );
    }

    private void Start()
    {
        SpawnStartingItems();
    }

    private void OnValidate()
    {
        gridWidth =
            Mathf.Max(
                1,
                gridWidth
            );

        gridHeight =
            Mathf.Max(
                1,
                gridHeight
            );
    }

    public PlacedInventoryItem GetItemAt(
        int x,
        int y)
    {
        if (grid == null)
            return null;

        return grid.GetPlacedItem(
            x,
            y
        );
    }

    public bool CanPlace(
        InventoryItemInstance itemInstance,
        int x,
        int y,
        int rotationSteps)
    {
        if (grid == null ||
            itemInstance == null ||
            itemInstance.Definition == null ||
            itemInstance.IsEmpty)
        {
            return false;
        }

        return grid.CanPlaceItem(
            itemInstance.Definition,
            x,
            y,
            rotationSteps
        );
    }

    public bool PlaceInstance(
        InventoryItemInstance itemInstance,
        int x,
        int y,
        int rotationSteps)
    {
        if (grid == null ||
            itemInstance == null ||
            itemInstance.Definition == null ||
            itemInstance.IsEmpty)
        {
            return false;
        }

        bool placed =
            grid.PlaceItem(
                itemInstance,
                x,
                y,
                rotationSteps
            );

        if (!placed)
            return false;

        SubscribeItem(
            itemInstance
        );

        Changed?.Invoke();

        return true;
    }

    public PlacedInventoryItem TakeItemAt(
        int x,
        int y)
    {
        if (grid == null)
            return null;

        PlacedInventoryItem item =
            grid.PickUpItemAt(
                x,
                y
            );

        if (item == null)
            return null;

        UnsubscribeItem(
            item.ItemInstance
        );

        Changed?.Invoke();

        return item;
    }

    public bool TryTransferIn(
        InventoryItemInstance itemInstance,
        int startingRotationSteps,
        out int remainingQuantity)
    {
        remainingQuantity =
            itemInstance != null
                ? itemInstance.Quantity
                : 0;

        if (grid == null ||
            itemInstance == null ||
            itemInstance.Definition == null ||
            itemInstance.IsEmpty)
        {
            return false;
        }

        MergeIntoExistingStacks(
            itemInstance
        );

        if (itemInstance.IsEmpty)
        {
            remainingQuantity = 0;
            return true;
        }

        bool foundSpace =
            grid.TryFindFirstAvailableSpaceTopLeft(
                itemInstance.Definition,
                startingRotationSteps,
                out Vector2Int position,
                out int rotationSteps
            );

        if (!foundSpace)
        {
            remainingQuantity =
                itemInstance.Quantity;

            return false;
        }

        bool placed =
            grid.PlaceItem(
                itemInstance,
                position.x,
                position.y,
                rotationSteps
            );

        if (!placed)
        {
            remainingQuantity =
                itemInstance.Quantity;

            return false;
        }

        SubscribeItem(
            itemInstance
        );

        remainingQuantity = 0;

        Changed?.Invoke();

        return true;
    }

    public bool SpawnAt(
        ItemDefinition itemDefinition,
        int x,
        int y,
        int rotationSteps,
        int quantity = 1)
    {
        if (itemDefinition == null ||
            quantity <= 0 ||
            grid == null)
        {
            return false;
        }

        InventoryItemInstance itemInstance =
            new InventoryItemInstance(
                itemDefinition,
                quantity
            );

        bool placed =
            grid.PlaceItem(
                itemInstance,
                x,
                y,
                rotationSteps
            );

        if (!placed)
            return false;

        SubscribeItem(
            itemInstance
        );

        Changed?.Invoke();

        return true;
    }

    private void MergeIntoExistingStacks(
        InventoryItemInstance source)
    {
        if (grid == null ||
            source == null ||
            !source.IsStackable ||
            source.IsEmpty)
        {
            return;
        }

        HashSet<PlacedInventoryItem> checkedItems =
            new HashSet<PlacedInventoryItem>();

        for (int y = Height - 1;
             y >= 0;
             y--)
        {
            for (int x = 0;
                 x < Width;
                 x++)
            {
                PlacedInventoryItem target =
                    grid.GetPlacedItem(
                        x,
                        y
                    );

                if (target == null ||
                    target.ItemInstance == null ||
                    checkedItems.Contains(target))
                {
                    continue;
                }

                checkedItems.Add(target);

                source.MoveQuantityTo(
                    target.ItemInstance,
                    source.Quantity
                );

                if (source.IsEmpty)
                    return;
            }
        }
    }

    private void SubscribeItem(
        InventoryItemInstance itemInstance)
    {
        if (itemInstance == null)
            return;

        itemInstance.Changed -=
            OnContainedItemChanged;

        itemInstance.Changed +=
            OnContainedItemChanged;
    }

    private void UnsubscribeItem(
        InventoryItemInstance itemInstance)
    {
        if (itemInstance == null)
            return;

        itemInstance.Changed -=
            OnContainedItemChanged;
    }

    private void OnContainedItemChanged()
    {
        Changed?.Invoke();
    }

    private void SpawnStartingItems()
    {
        if (grid == null ||
            startingItems == null)
        {
            return;
        }

        bool placedAny = false;

        for (int i = 0;
             i < startingItems.Count;
             i++)
        {
            InventoryStartingItem startingItem =
                startingItems[i];

            if (startingItem == null ||
                startingItem.item == null)
            {
                continue;
            }

            InventoryItemInstance itemInstance =
                new InventoryItemInstance(
                    startingItem.item,
                    startingItem.quantity
                );

            bool placed =
                grid.PlaceItem(
                    itemInstance,
                    startingItem.x,
                    startingItem.y,
                    startingItem.rotationSteps
                );

            if (!placed)
            {
                Debug.LogWarning(
                    "Could not place starting item: " +
                    startingItem.item.itemName,
                    this
                );

                continue;
            }

            SubscribeItem(
                itemInstance
            );

            placedAny = true;
        }

        if (placedAny)
            Changed?.Invoke();
    }
}