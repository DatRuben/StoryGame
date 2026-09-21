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

        return CanPlaceWithReservations(
            itemInstance.Definition,
            x,
            y,
            rotationSteps,
            null
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

        if (!CanPlaceWithReservations(
            itemInstance.Definition,
            x,
            y,
            rotationSteps,
            null))
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

    public bool CanTransferIn(
        InventoryItemInstance itemInstance,
        int startingRotationSteps = 0)
    {
        if (grid == null ||
            itemInstance == null ||
            itemInstance.Definition == null ||
            itemInstance.IsEmpty)
        {
            return false;
        }

        if (itemInstance.IsStackable)
        {
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
                    PlacedInventoryItem placed =
                        grid.GetPlacedItem(
                            x,
                            y
                        );

                    if (placed == null ||
                        placed.ItemInstance == null ||
                        checkedItems.Contains(
                            placed))
                    {
                        continue;
                    }

                    checkedItems.Add(
                        placed
                    );

                    if (itemInstance.CanStackWith(
                            placed.ItemInstance) &&
                        placed.ItemInstance
                            .HasRoomInStack)
                    {
                        return true;
                    }
                }
            }
        }

        return TryFindAvailableSpace(
            itemInstance.Definition,
            startingRotationSteps,
            null,
            out _,
            out _
        );
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
            TryFindAvailableSpace(
                itemInstance.Definition,
                startingRotationSteps,
                null,
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

                int reservedQuantity =
                    GetReservedStackQuantity(
                        target.ItemInstance,
                        null
                    );

                int available =
                    target.ItemInstance
                        .MaxStackSize -
                    target.ItemInstance.Quantity -
                    reservedQuantity;

                if (available <= 0)
                    continue;

                source.MoveQuantityTo(
                    target.ItemInstance,
                    Mathf.Min(
                        source.Quantity,
                        available
                    )
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

    private readonly List<
        InventoryTransferReservation>
        transferReservations =
            new List<
                InventoryTransferReservation>();

    public bool TryReserveTransferIn(
        InventoryItemInstance itemInstance,
        int startingRotationSteps,
        out InventoryTransferReservation
            reservation)
    {
        reservation = null;

        if (grid == null ||
            itemInstance == null ||
            itemInstance.IsEmpty ||
            itemInstance.Definition == null)
        {
            return false;
        }

        InventoryTransferReservation
            newReservation =
                new InventoryTransferReservation(
                    this,
                    itemInstance
                );

        int remainingQuantity =
            itemInstance.Quantity;

        if (itemInstance.IsStackable)
        {
            HashSet<PlacedInventoryItem>
                checkedItems =
                    new HashSet<
                        PlacedInventoryItem>();

            for (int y = Height - 1;
                 y >= 0 &&
                 remainingQuantity > 0;
                 y--)
            {
                for (int x = 0;
                     x < Width &&
                     remainingQuantity > 0;
                     x++)
                {
                    PlacedInventoryItem placed =
                        grid.GetPlacedItem(
                            x,
                            y
                        );

                    if (placed == null ||
                        placed.ItemInstance == null ||
                        checkedItems.Contains(
                            placed))
                    {
                        continue;
                    }

                    checkedItems.Add(
                        placed
                    );

                    InventoryItemInstance
                        target =
                            placed.ItemInstance;

                    if (!itemInstance
                        .CanStackWith(target))
                    {
                        continue;
                    }

                    int reservedAlready =
                        GetReservedStackQuantity(
                            target,
                            null
                        );

                    int available =
                        target.MaxStackSize -
                        target.Quantity -
                        reservedAlready;

                    if (available <= 0)
                        continue;

                    int quantityToReserve =
                        Mathf.Min(
                            remainingQuantity,
                            available
                        );

                    newReservation
                        .AddStackTransfer(
                            target,
                            quantityToReserve
                        );

                    remainingQuantity -=
                        quantityToReserve;
                }
            }
        }

        if (remainingQuantity > 0 &&
            TryFindAvailableSpace(
                itemInstance.Definition,
                startingRotationSteps,
                null,
                out Vector2Int position,
                out int rotationSteps))
        {
            newReservation
                .ReservePlacement(
                    position,
                    rotationSteps,
                    remainingQuantity
                );

            remainingQuantity = 0;
        }

        if (newReservation
            .ReservedQuantity <= 0)
        {
            return false;
        }

        transferReservations.Add(
            newReservation
        );

        reservation =
            newReservation;

        return true;
    }

    public bool TryCommitTransferReservation(
        InventoryTransferReservation reservation,
        out int remainingQuantity)
    {
        remainingQuantity = 0;

        if (!OwnsActiveReservation(
                reservation))
        {
            return false;
        }

        InventoryItemInstance source =
            reservation.SourceItem;

        if (source == null ||
            source.IsEmpty ||
            source.Definition == null)
        {
            CancelTransferReservation(
                reservation
            );

            return false;
        }

        remainingQuantity =
            source.Quantity;

        bool movedAnything = false;

        IReadOnlyList<
            InventoryStackTransferReservation>
            stackTransfers =
                reservation.StackTransfers;

        for (int i = 0;
             i < stackTransfers.Count &&
             !source.IsEmpty;
             i++)
        {
            InventoryStackTransferReservation
                stackTransfer =
                    stackTransfers[i];

            InventoryItemInstance target =
                stackTransfer.Target;

            if (target == null ||
                !ContainsItemInstance(
                    target) ||
                !source.CanStackWith(
                    target))
            {
                continue;
            }

            int reservedByOthers =
                GetReservedStackQuantity(
                    target,
                    reservation
                );

            int available =
                target.MaxStackSize -
                target.Quantity -
                reservedByOthers;

            if (available <= 0)
                continue;

            int quantityToMove =
                Mathf.Min(
                    stackTransfer.Quantity,
                    available
                );

            int moved =
                source.MoveQuantityTo(
                    target,
                    quantityToMove
                );

            if (moved > 0)
            {
                movedAnything = true;
            }
        }

        if (!source.IsEmpty &&
            reservation.HasPlacement &&
            CanPlaceWithReservations(
                source.Definition,
                reservation.PlacementPosition.x,
                reservation.PlacementPosition.y,
                reservation.PlacementRotationSteps,
                reservation))
        {
            bool placed =
                grid.PlaceItem(
                    source,
                    reservation
                        .PlacementPosition.x,
                    reservation
                        .PlacementPosition.y,
                    reservation
                        .PlacementRotationSteps
                );

            if (placed)
            {
                SubscribeItem(
                    source
                );

                movedAnything = true;
            }
        }

        remainingQuantity =
            source.IsEmpty
                ? 0
                : ContainsItemInstance(source)
                    ? 0
                    : source.Quantity;

        transferReservations.Remove(
            reservation
        );

        reservation.IsActive = false;

        if (movedAnything)
        {
            Changed?.Invoke();
        }

        return movedAnything;
    }

    private bool OwnsActiveReservation(
    InventoryTransferReservation
        reservation)
    {
        return
            reservation != null &&
            reservation.IsActive &&
            ReferenceEquals(
                reservation.Owner,
                this
            ) &&
            transferReservations.Contains(
                reservation
            );
    }

    private int GetReservedStackQuantity(
        InventoryItemInstance target,
        InventoryTransferReservation
            excludedReservation)
    {
        if (target == null)
            return 0;

        int reservedQuantity = 0;

        for (int i = 0;
             i < transferReservations.Count;
             i++)
        {
            InventoryTransferReservation
                reservation =
                    transferReservations[i];

            if (reservation == null ||
                !reservation.IsActive ||
                ReferenceEquals(
                    reservation,
                    excludedReservation))
            {
                continue;
            }

            IReadOnlyList<
                InventoryStackTransferReservation>
                transfers =
                    reservation.StackTransfers;

            for (int j = 0;
                 j < transfers.Count;
                 j++)
            {
                if (ReferenceEquals(
                        transfers[j].Target,
                        target))
                {
                    reservedQuantity +=
                        transfers[j].Quantity;
                }
            }
        }

        return reservedQuantity;
    }

    private bool ContainsItemInstance(
        InventoryItemInstance itemInstance)
    {
        if (grid == null ||
            itemInstance == null)
        {
            return false;
        }

        for (int y = 0;
             y < Height;
             y++)
        {
            for (int x = 0;
                 x < Width;
                 x++)
            {
                PlacedInventoryItem placed =
                    grid.GetPlacedItem(
                        x,
                        y
                    );

                if (placed != null &&
                    ReferenceEquals(
                        placed.ItemInstance,
                        itemInstance))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool TryFindAvailableSpace(
        ItemDefinition definition,
        int startingRotationSteps,
        InventoryTransferReservation excludedReservation,
        out Vector2Int position,
        out int rotationSteps)
    {
        position =
            new Vector2Int(-1, -1);

        rotationSteps = 0;

        if (definition == null)
            return false;

        for (int y = Height - 1;
             y >= 0;
             y--)
        {
            for (int x = 0;
                 x < Width;
                 x++)
            {
                for (int rotationOffset = 0;
                     rotationOffset < 4;
                     rotationOffset++)
                {
                    int testRotation =
                        ItemDefinition
                            .NormalizeRotationSteps(
                                startingRotationSteps +
                                rotationOffset
                            );

                    if (!CanPlaceWithReservations(
                            definition,
                            x,
                            y,
                            testRotation,
                            excludedReservation))
                    {
                        continue;
                    }

                    position =
                        new Vector2Int(
                            x,
                            y
                        );

                    rotationSteps =
                        testRotation;

                    return true;
                }
            }
        }

        return false;
    }

    private bool CanPlaceWithReservations(
        ItemDefinition definition,
        int startX,
        int startY,
        int rotationSteps,
        InventoryTransferReservation
            excludedReservation)
    {
        if (grid == null ||
            definition == null ||
            !grid.CanPlaceItem(
                definition,
                startX,
                startY,
                rotationSteps))
        {
            return false;
        }

        rotationSteps =
            ItemDefinition
                .NormalizeRotationSteps(
                    rotationSteps
                );

        int itemWidth =
            definition.GetWidth(
                rotationSteps
            );

        int itemHeight =
            definition.GetHeight(
                rotationSteps
            );

        for (int y = 0;
             y < itemHeight;
             y++)
        {
            for (int x = 0;
                 x < itemWidth;
                 x++)
            {
                if (!definition
                    .IsCellOccupied(
                        x,
                        y,
                        rotationSteps))
                {
                    continue;
                }

                if (IsCellReserved(
                        startX + x,
                        startY + y,
                        excludedReservation))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private bool IsCellReserved(
        int gridX,
        int gridY,
        InventoryTransferReservation
            excludedReservation)
    {
        for (int i = 0;
             i < transferReservations.Count;
             i++)
        {
            InventoryTransferReservation
                reservation =
                    transferReservations[i];

            if (reservation == null ||
                !reservation.IsActive ||
                !reservation.HasPlacement ||
                ReferenceEquals(
                    reservation,
                    excludedReservation))
            {
                continue;
            }

            InventoryItemInstance source =
                reservation.SourceItem;

            if (source == null ||
                source.Definition == null)
            {
                continue;
            }

            ItemDefinition definition =
                source.Definition;

            int rotation =
                reservation
                    .PlacementRotationSteps;

            int width =
                definition.GetWidth(
                    rotation
                );

            int height =
                definition.GetHeight(
                    rotation
                );

            for (int y = 0;
                 y < height;
                 y++)
            {
                for (int x = 0;
                     x < width;
                     x++)
                {
                    if (!definition
                        .IsCellOccupied(
                            x,
                            y,
                            rotation))
                    {
                        continue;
                    }

                    Vector2Int cell =
                        reservation
                            .PlacementPosition +
                        new Vector2Int(
                            x,
                            y
                        );

                    if (cell.x == gridX &&
                        cell.y == gridY)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}