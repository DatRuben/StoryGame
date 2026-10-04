using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerGripState))]
[RequireComponent(typeof(PlayerHeldItemPresenter))]

public sealed class PlayerItemHandlingController :
    MonoBehaviour
{
    [SerializeField]
    [Min(0.05f)]
    private float defaultRetrieveDuration = 0.75f;

    private ItemHandlingOperation activeOperation;

    private readonly List<ItemHandlingOperation>
        queuedOperations =
            new List<ItemHandlingOperation>();

    private bool immediateActionInProgress;

    private PlayerGripState gripState;

    private PlayerHeldItemPresenter
        heldItemPresenter;

    private WorldItemSpawner
        worldItemSpawner;

    public ItemHandlingOperation
        CurrentOperation =>
            activeOperation;

    public IReadOnlyList<ItemHandlingOperation>
    QueuedOperations =>
        queuedOperations;

    public int QueuedOperationCount =>
        queuedOperations.Count;

    public ItemHandlingOperationType
        ActiveOperation =>
            activeOperation != null
                ? activeOperation.Type
                : ItemHandlingOperationType.None;

    public float OperationProgress01 =>
        activeOperation != null
            ? activeOperation.Progress01
            : 0f;

    public bool IsBusy =>
        immediateActionInProgress ||
        activeOperation != null ||
        queuedOperations.Count > 0;

    public InventoryItemInstance ActiveItem =>
        activeOperation != null
            ? activeOperation.Item
            : null;

    public event Action<ItemHandlingOperation>
        OperationStarted;

    public event Action<ItemHandlingOperation>
        OperationCompleted;

    public event Action<ItemHandlingOperation>
        OperationCancelled;

    private void Awake()
    {
        gripState =
            GetComponent<PlayerGripState>();

        heldItemPresenter =
            GetComponent<PlayerHeldItemPresenter>();
    }

    public void BindWorldItemSpawner(
        WorldItemSpawner spawner)
    {
        worldItemSpawner = spawner;
    }

    public bool TryAcquireWorldItem(
        WorldItem worldItem,
        GripType gripType,
        int gripCount,
        out InventoryItemInstance item)
    {
        item = null;

        if (IsBusy ||
            worldItem == null ||
            gripState == null)
        {
            return false;
        }

        InventoryItemInstance worldItemInstance =
            worldItem.Item;

        if (worldItemInstance == null ||
            worldItemInstance.IsEmpty ||
            worldItemInstance.Definition == null)
        {
            return false;
        }

        immediateActionInProgress = true;

        if (!gripState.TryHold(
                worldItemInstance,
                gripType,
                gripCount))
        {
            immediateActionInProgress = false;
            return false;
        }

        if (!worldItem.ReleaseItem(
                worldItemInstance))
        {
            gripState.Release(
                worldItemInstance
            );

            immediateActionInProgress = false;
            return false;
        }

        item = worldItemInstance;

        immediateActionInProgress = false;

        return true;
    }

    public bool TryDropHeldItem(
        InventoryItemInstance item,
        out WorldItem worldItem)
    {
        worldItem = null;

        if (IsBusy ||
            item == null)
        {
            return false;
        }

        immediateActionInProgress = true;

        bool dropped =
            TryReleaseHeldItemToWorld(
                item,
                out worldItem
            );

        immediateActionInProgress = false;

        return dropped;
    }

    private bool TryReleaseHeldItemToWorld(
        InventoryItemInstance item,
        out WorldItem worldItem)
    {
        worldItem = null;

        if (item == null ||
            item.IsEmpty ||
            gripState == null ||
            !gripState.IsHolding(item) ||
            heldItemPresenter == null ||
            worldItemSpawner == null)
        {
            return false;
        }

        if (!heldItemPresenter
            .TryGetHeldItemPose(
                item,
                out Pose heldPose))
        {
            return false;
        }

        bool spawned =
            worldItemSpawner
                .TrySpawnForRelease(
                    item,
                    heldPose,
                    out worldItem
                );

        if (!spawned)
        {
            return false;
        }

        WorldItemReleaseGuard
            releaseGuard =
                worldItem.gameObject
                    .AddComponent<
                        WorldItemReleaseGuard>();

        if (!releaseGuard.Begin(
                transform))
        {
            Destroy(
                worldItem.gameObject
            );

            worldItem = null;

            return false;
        }

        if (!gripState.Release(item))
        {
            Destroy(
                worldItem.gameObject
            );

            worldItem = null;

            return false;
        }

        return true;
    }

    private void CompleteActiveOperation()
    {
        ItemHandlingOperation operation =
            activeOperation;

        if (operation == null)
            return;

        activeOperation = null;

        OperationCompleted?.Invoke(
            operation
        );

        TryStartNextOperation();
    }

    private void CancelOperationReservation(
        ItemHandlingOperation operation)
    {
        if (operation == null)
            return;

        if (operation.TransferReservation != null &&
            operation.TransferReservation.IsActive &&
            operation.TargetContainer != null)
        {
            operation.TargetContainer
                .CancelTransferReservation(
                    operation.TransferReservation
                );
        }

        if (operation.TakeReservation != null &&
            operation.TakeReservation.IsActive &&
            operation.SourceContainer != null)
        {
            operation.SourceContainer
                .CancelTakeReservation(
                    operation.TakeReservation
                );
        }
    }

    public bool CancelAllOperations()
    {
        bool cancelledAnything =
            activeOperation != null ||
            queuedOperations.Count > 0;

        ItemHandlingOperation current =
            activeOperation;

        if (current != null)
        {
            ResolveInterruptedOperation(
                current
            );

            CancelOperationReservation(
                current
            );
        }

        activeOperation = null;

        for (int i =
                 queuedOperations.Count - 1;
             i >= 0;
             i--)
        {
            ItemHandlingOperation operation =
                queuedOperations[i];

            CancelOperationReservation(
                operation
            );

            queuedOperations.RemoveAt(
                i
            );

            OperationCancelled?.Invoke(
                operation
            );
        }

        if (current != null)
        {
            OperationCancelled?.Invoke(
                current
            );
        }

        return cancelledAnything;
    }

    private void CancelQueuedOperationsForItem(
        InventoryItemInstance item)
    {
        if (item == null)
            return;

        for (int i =
                 queuedOperations.Count - 1;
             i >= 0;
             i--)
        {
            ItemHandlingOperation operation =
                queuedOperations[i];

            if (operation == null ||
                !ReferenceEquals(
                    operation.Item,
                    item))
            {
                continue;
            }

            CancelOperationReservation(
                operation
            );

            queuedOperations.RemoveAt(
                i
            );

            OperationCancelled?.Invoke(
                operation
            );
        }
    }

    public bool TryStoreHeldItem(
        InventoryItemInstance item,
        InventoryContainer target)
    {
        if (IsBusy ||
            item == null ||
            item.IsEmpty ||
            gripState == null ||
            !gripState.IsHolding(item) ||
            target == null)
        {
            return false;
        }

        immediateActionInProgress = true;

        try
        {
            if (!target.TryReserveTransferIn(
                    item,
                    0,
                    out InventoryTransferReservation
                        reservation))
            {
                return false;
            }

            return TryCommitStore(
                item,
                target,
                reservation,
                out _
            );
        }
        finally
        {
            immediateActionInProgress = false;
        }
    }

    private bool TryCommitStore(
        InventoryItemInstance item,
        InventoryContainer target,
        InventoryTransferReservation reservation,
        out int remainingQuantity)
    {
        remainingQuantity =
            item != null
                ? item.Quantity
                : 0;

        if (item == null ||
            item.IsEmpty ||
            target == null ||
            reservation == null ||
            !reservation.IsActive ||
            gripState == null ||
            !gripState.IsHolding(item))
        {
            if (reservation != null &&
                reservation.IsActive &&
                target != null)
            {
                target.CancelTransferReservation(
                    reservation
                );
            }

            return false;
        }

        bool movedAnything =
            target.TryCommitTransferReservation(
                reservation,
                out remainingQuantity
            );

        if (!movedAnything)
        {
            if (reservation.IsActive)
            {
                target.CancelTransferReservation(
                    reservation
                );
            }

            return false;
        }

        if (remainingQuantity <= 0 ||
            item.IsEmpty)
        {
            if (!gripState.Release(item))
            {
                Debug.LogError(
                    "Stored item successfully but could not release it from PlayerGripState.",
                    this
                );

                return false;
            }
        }

        return true;
    }

    public bool TryDropActiveHeldItemImmediately()
    {
        ItemHandlingOperation operation =
            activeOperation;

        if (operation == null ||
            operation.Item == null ||
            operation.Item.IsEmpty ||
            gripState == null ||
            !gripState.IsHolding(
                operation.Item))
        {
            return false;
        }

        InventoryItemInstance item =
            operation.Item;

        immediateActionInProgress = true;

        bool dropped =
            TryReleaseHeldItemToWorld(
                item,
                out _
            );

        immediateActionInProgress = false;

        if (!dropped)
        {
            return false;
        }

        CancelActiveOperation();

        return true;
    }

    private bool HasOperationForItem(
        InventoryItemInstance item)
    {
        if (item == null)
            return false;

        if (activeOperation != null &&
            ReferenceEquals(
                activeOperation.Item,
                item))
        {
            return true;
        }

        for (int i = 0;
             i < queuedOperations.Count;
             i++)
        {
            ItemHandlingOperation operation =
                queuedOperations[i];

            if (operation != null &&
                ReferenceEquals(
                    operation.Item,
                    item))
            {
                return true;
            }
        }

        return false;
    }

    private void TryStartNextOperation()
    {
        if (activeOperation != null ||
            immediateActionInProgress)
        {
            return;
        }

        while (queuedOperations.Count > 0)
        {
            activeOperation =
                queuedOperations[0];

            queuedOperations.RemoveAt(0);

            if (!TryBeginActiveOperation(
                    activeOperation))
            {
                ItemHandlingOperation failed =
                    activeOperation;

                CancelOperationReservation(
                    failed
                );

                activeOperation = null;

                OperationCancelled?.Invoke(
                    failed
                );

                continue;
            }

            OperationStarted?.Invoke(
                activeOperation
            );

            return;
        }
    }

    private bool TryBeginActiveOperation(
        ItemHandlingOperation operation)
    {
        if (operation == null)
            return false;

        switch (operation.Type)
        {
            case ItemHandlingOperationType.Retrieve:
            case ItemHandlingOperationType.Transfer:
                return TryBeginContainerRetrieve(
                    operation
                );

            default:
                return true;
        }
    }

    private bool TryBeginContainerRetrieve(
        ItemHandlingOperation operation)
    {
        if (operation == null ||
            operation.Item == null ||
            operation.Item.IsEmpty ||
            operation.SourceContainer == null ||
            operation.TakeReservation == null ||
            !operation.TakeReservation.IsActive ||
            gripState == null)
        {
            return false;
        }

        if (operation.Type !=
                ItemHandlingOperationType.Retrieve &&
            operation.Type !=
                ItemHandlingOperationType.Transfer)
        {
            return false;
        }

        if (operation.Type ==
            ItemHandlingOperationType.Transfer)
        {
            if (operation.TargetContainer == null ||
                operation.TransferReservation == null ||
                !operation.TransferReservation.IsActive)
            {
                return false;
            }
        }

        if (gripState.GetFreeGripCount(
                operation.TargetGripType) <
            operation.TargetGripCount)
        {
            return false;
        }

        return TryAcquireOperationItem(
            operation
        );
    }
    private void UpdateTransferOperation()
    {
        ItemHandlingOperation operation =
            activeOperation;

        if (operation == null ||
            operation.Type !=
                ItemHandlingOperationType.Transfer ||
            operation.Item == null ||
            operation.Item.IsEmpty ||
            gripState == null ||
            !gripState.IsHolding(
                operation.Item) ||
            operation.TargetContainer == null ||
            operation.TransferReservation == null ||
            !operation.TransferReservation.IsActive)
        {
            CancelActiveOperation();
            return;
        }

        operation.Advance(
            Time.deltaTime
        );

        if (!operation.IsComplete)
            return;

        CompleteTransferOperation();
    }

    private bool TryAcquireOperationItem(
        ItemHandlingOperation operation)
    {
        if (operation == null ||
            operation.SourceContainer == null ||
            operation.TakeReservation == null ||
            !operation.TakeReservation.IsActive ||
            gripState == null)
        {
            return false;
        }

        if (!operation.SourceContainer
            .TryCommitTakeReservation(
                operation.TakeReservation,
                out PlacedInventoryItem removedItem))
        {
            return false;
        }

        if (removedItem == null ||
            !ReferenceEquals(
                removedItem.ItemInstance,
                operation.Item))
        {
            return false;
        }

        if (gripState.TryHold(
                operation.Item,
                operation.TargetGripType,
                operation.TargetGripCount))
        {
            return true;
        }

        bool restored =
            operation.SourceContainer
                .PlaceInstance(
                    operation.Item,
                    operation.TakeReservation.Position.x,
                    operation.TakeReservation.Position.y,
                    operation.TakeReservation.RotationSteps
                );

        if (!restored)
        {
            Debug.LogError(
                "Retrieved item could not be held or restored to its source container.",
                this
            );
        }

        return false;
    }

    private void CompleteTransferOperation()
    {
        ItemHandlingOperation operation =
            activeOperation;

        if (operation == null ||
            operation.Type !=
                ItemHandlingOperationType.Transfer ||
            operation.Item == null ||
            operation.Item.IsEmpty ||
            gripState == null ||
            !gripState.IsHolding(
                operation.Item) ||
            operation.TargetContainer == null ||
            operation.TransferReservation == null ||
            !operation.TransferReservation.IsActive)
        {
            CancelActiveOperation();
            return;
        }

        if (!TryCommitStore(
                operation.Item,
                operation.TargetContainer,
                operation.TransferReservation,
                out _))
        {
            CancelActiveOperation();
            return;
        }

        CompleteActiveOperation();
    }

    private void ResolveInterruptedOperation(
        ItemHandlingOperation operation)
    {
        if (operation == null)
            return;

        switch (operation.Type)
        {
            case ItemHandlingOperationType.Retrieve:
            case ItemHandlingOperationType.Transfer:
                ResolveInterruptedRetrievedItem(
                    operation
                );
                break;
        }
    }

    private void ResolveInterruptedRetrievedItem(
        ItemHandlingOperation operation)
    {
        if (operation == null)
            return;

        InventoryItemInstance item =
            operation.Item;

        if (item == null ||
            item.IsEmpty ||
            gripState == null ||
            !gripState.IsHolding(item))
        {
            return;
        }

        bool returnedToSource = false;

        if (operation.SourceContainer != null &&
            operation.TakeReservation != null)
        {
            returnedToSource =
                operation.SourceContainer
                    .PlaceInstance(
                        item,
                        operation.TakeReservation.Position.x,
                        operation.TakeReservation.Position.y,
                        operation.TakeReservation.RotationSteps
                    );
        }

        if (returnedToSource)
        {
            gripState.Release(item);
            return;
        }

        if (TryReleaseHeldItemToWorld(
                item,
                out _))
        {
            return;
        }

        Debug.LogError(
            "Interrupted item retrieval could not return or drop the held item. The item remains in PlayerGripState to preserve ownership.",
            this
        );
    }

    private void Update()
    {
        if (activeOperation == null)
        {
            TryStartNextOperation();

            if (activeOperation == null)
                return;
        }

        switch (ActiveOperation)
        {
            case ItemHandlingOperationType.Retrieve:
                UpdateRetrieveOperation();
                break;

            case ItemHandlingOperationType.Transfer:
                UpdateTransferOperation();
                break;

            case ItemHandlingOperationType.Drop:
                CompleteDropOperation();
                break;
        }
    }

    private void UpdateRetrieveOperation()
    {
        ItemHandlingOperation operation =
            activeOperation;

        if (operation == null ||
            operation.Type !=
                ItemHandlingOperationType.Retrieve ||
            operation.Item == null ||
            operation.Item.IsEmpty ||
            gripState == null ||
            !gripState.IsHolding(
                operation.Item))
        {
            CancelActiveOperation();
            return;
        }

        operation.Advance(
            Time.deltaTime
        );

        if (!operation.IsComplete)
            return;

        CompleteActiveOperation();
    }

    private void CompleteDropOperation()
    {
        ItemHandlingOperation operation =
            activeOperation;

        if (operation == null ||
            operation.Type !=
                ItemHandlingOperationType.Drop ||
            operation.Item == null ||
            !gripState.IsHolding(
                operation.Item))
        {
            CancelActiveOperation();
            return;
        }

        if (!TryReleaseHeldItemToWorld(
                operation.Item,
                out _))
        {
            CancelActiveOperation();
            return;
        }

        CompleteActiveOperation();
    }
    public bool CancelActiveOperation()
    {
        ItemHandlingOperation operation =
            activeOperation;

        if (operation == null)
            return false;

        CancelQueuedOperationsForItem(
            operation.Item
        );

        ResolveInterruptedOperation(
            operation
        );

        CancelOperationReservation(
            operation
        );

        activeOperation = null;

        OperationCancelled?.Invoke(
            operation
        );

        TryStartNextOperation();

        return true;
    }

    public bool TryCancelOperation(
        ItemHandlingOperation operation)
    {
        if (operation == null)
            return false;

        if (ReferenceEquals(
                activeOperation,
                operation))
        {
            return CancelActiveOperation();
        }

        for (int i = 0;
             i < queuedOperations.Count;
             i++)
        {
            if (!ReferenceEquals(
                    queuedOperations[i],
                    operation))
            {
                continue;
            }

            InventoryItemInstance item =
                operation.Item;

            if (item != null)
            {
                CancelQueuedOperationsForItem(
                    item
                );
            }
            else
            {
                CancelOperationReservation(
                    operation
                );

                queuedOperations.RemoveAt(
                    i
                );

                OperationCancelled?.Invoke(
                    operation
                );
            }

            return true;
        }

        return false;
    }

    public bool TryBeginRetrieveThenDrop(
        InventoryContainer source,
        Vector2Int coordinate,
        GripType gripType,
        int gripCount)
    {
        if (source == null ||
            gripState == null ||
            gripCount <= 0)
        {
            return false;
        }

        PlacedInventoryItem placed =
            source.GetItemAt(
                coordinate.x,
                coordinate.y
            );

        if (placed == null ||
            placed.ItemInstance == null ||
            placed.ItemInstance.IsEmpty)
        {
            return false;
        }

        InventoryItemInstance item =
            placed.ItemInstance;

        if (HasOperationForItem(
                item))
        {
            return false;
        }

        if (gripState.GetFreeGripCount(
                gripType) < gripCount)
        {
            return false;
        }

        if (!source.TryReserveTakeAt(
                coordinate.x,
                coordinate.y,
                out InventoryTakeReservation
                    takeReservation))
        {
            return false;
        }

        ItemHandlingOperation retrieve =
            new ItemHandlingOperation(
                ItemHandlingOperationType.Retrieve,
                item,
                defaultRetrieveDuration,
                sourceContainer: source,
                takeReservation:
                    takeReservation,
                targetGripType: gripType,
                targetGripCount: gripCount
            );

        ItemHandlingOperation drop =
            new ItemHandlingOperation(
                ItemHandlingOperationType.Drop,
                item,
                0f
            );

        queuedOperations.Add(
            retrieve
        );

        queuedOperations.Add(
            drop
        );

        TryStartNextOperation();

        return true;
    }

    public bool TryBeginTransferFromContainer(
        InventoryContainer source,
        InventoryContainer target,
        Vector2Int coordinate,
        GripType gripType,
        int gripCount)
    {
        if (source == null ||
            target == null ||
            ReferenceEquals(
                source,
                target) ||
            gripState == null ||
            gripCount <= 0)
        {
            return false;
        }

        PlacedInventoryItem placed =
            source.GetItemAt(
                coordinate.x,
                coordinate.y
            );

        if (placed == null ||
            placed.ItemInstance == null ||
            placed.ItemInstance.IsEmpty)
        {
            return false;
        }

        InventoryItemInstance item =
            placed.ItemInstance;

        if (HasOperationForItem(item))
            return false;

        if (!source.TryReserveTakeAt(
                coordinate.x,
                coordinate.y,
                out InventoryTakeReservation
                    takeReservation))
        {
            return false;
        }

        if (!target.TryReserveTransferIn(
                item,
                placed.RotationSteps,
                out InventoryTransferReservation
                    transferReservation))
        {
            source.CancelTakeReservation(
                takeReservation
            );

            return false;
        }

        ItemHandlingOperation operation =
            new ItemHandlingOperation(
                ItemHandlingOperationType.Transfer,
                item,
                defaultRetrieveDuration,
                sourceContainer: source,
                targetContainer: target,
                transferReservation:
                    transferReservation,
                takeReservation:
                    takeReservation,
                targetGripType: gripType,
                targetGripCount: gripCount
            );

        queuedOperations.Add(
            operation
        );

        TryStartNextOperation();

        return true;
    }

    public bool TryBeginRetrieveFromContainer(
        InventoryContainer source,
        Vector2Int coordinate,
        GripType gripType,
        int gripCount,
        out ItemHandlingOperation operation)
    {
        operation = null;

        if (source == null ||
            gripState == null ||
            gripCount <= 0)
        {
            return false;
        }

        PlacedInventoryItem placed =
            source.GetItemAt(
                coordinate.x,
                coordinate.y
            );

        if (placed == null ||
            placed.ItemInstance == null ||
            placed.ItemInstance.IsEmpty)
        {
            return false;
        }

        InventoryItemInstance item =
            placed.ItemInstance;

        if (HasOperationForItem(
                item))
        {
            return false;
        }

        if (gripState.GetFreeGripCount(
                gripType) < gripCount)
        {
            return false;
        }

        if (!source.TryReserveTakeAt(
                coordinate.x,
                coordinate.y,
                out InventoryTakeReservation
                    reservation))
        {
            return false;
        }

        operation =
            new ItemHandlingOperation(
                ItemHandlingOperationType.Retrieve,
                item,
                defaultRetrieveDuration,
                sourceContainer: source,
                takeReservation: reservation,
                targetGripType: gripType,
                targetGripCount: gripCount
            );

        queuedOperations.Add(
            operation
        );

        TryStartNextOperation();

        return true;
    }

    public bool TryStoreHeldItemAt(
        InventoryItemInstance item,
        InventoryContainer target,
        Vector2Int stackCoordinate,
        Vector2Int placementOrigin,
        int rotationSteps,
        out int remainingQuantity)
    {
        remainingQuantity =
            item != null
                ? item.Quantity
                : 0;

        if (IsBusy ||
            item == null ||
            item.IsEmpty ||
            gripState == null ||
            !gripState.IsHolding(item) ||
            target == null)
        {
            return false;
        }

        immediateActionInProgress = true;

        try
        {
            InventoryTransferReservation
                reservation;

            bool reserved =
                target.TryReserveStackTransferAt(
                    item,
                    stackCoordinate.x,
                    stackCoordinate.y,
                    out reservation
                );

            if (!reserved)
            {
                reserved =
                    target.TryReservePlacementAt(
                        item,
                        placementOrigin.x,
                        placementOrigin.y,
                        rotationSteps,
                        out reservation
                    );
            }

            if (!reserved)
                return false;

            return TryCommitStore(
                item,
                target,
                reservation,
                out remainingQuantity
            );
        }
        finally
        {
            immediateActionInProgress = false;
        }
    }

    public bool IsItemReadyForUse(
        InventoryItemInstance item)
    {
        if (item == null ||
            item.IsEmpty ||
            gripState == null ||
            !gripState.IsHolding(item))
        {
            return false;
        }

        return !HasOperationForItem(
            item
        );
    }
}