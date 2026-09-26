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
    private float defaultStoreDuration = 0.75f;

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
        activeOperation != null;

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

    public bool TryBeginStoreHeldItem(
        InventoryItemInstance item,
        InventoryContainer target)
    {
        if (item == null ||
            item.IsEmpty ||
            gripState == null ||
            !gripState.IsHolding(item) ||
            target == null ||
            HasOperationForItem(item))
        {
            return false;
        }

        if (!target.TryReserveTransferIn(
                item,
                0,
                out InventoryTransferReservation
                    reservation))
        {
            return false;
        }

        ItemHandlingOperation operation =
            new ItemHandlingOperation(
                ItemHandlingOperationType.Store,
                item,
                defaultStoreDuration,
                null,
                target,
                reservation
            );

        queuedOperations.Add(
            operation
        );

        TryStartNextOperation();

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
            immediateActionInProgress ||
            queuedOperations.Count == 0)
        {
            return;
        }

        activeOperation =
            queuedOperations[0];

        queuedOperations.RemoveAt(0);

        OperationStarted?.Invoke(
            activeOperation
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
            case ItemHandlingOperationType.Store:
                UpdateStoreOperation();
                break;
            case ItemHandlingOperationType.Retrieve:
                UpdateRetrieveOperation();
                break;

            case ItemHandlingOperationType.Drop:
                CompleteDropOperation();
                break;
        }
    }

    private void UpdateStoreOperation()
    {
        ItemHandlingOperation operation =
            activeOperation;

        if (operation == null ||
            operation.Type !=
                ItemHandlingOperationType.Store ||
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

        CompleteStoreOperation();
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
            operation.SourceContainer == null ||
            operation.TakeReservation == null ||
            !operation.TakeReservation.IsActive ||
            gripState == null ||
            gripState.IsHolding(
                operation.Item))
        {
            CancelActiveOperation();
            return;
        }

        if (gripState.GetFreeGripCount(
                operation.TargetGripType) <
            operation.TargetGripCount)
        {
            CancelActiveOperation();
            return;
        }

        operation.Advance(
            Time.deltaTime
        );

        if (!operation.IsComplete)
            return;

        CompleteRetrieveOperation();
    }

    private void CompleteRetrieveOperation()
    {
        ItemHandlingOperation operation =
            activeOperation;

        if (operation == null ||
            operation.Type !=
                ItemHandlingOperationType.Retrieve)
        {
            CancelActiveOperation();
            return;
        }

        InventoryContainer source =
            operation.SourceContainer;

        InventoryTakeReservation reservation =
            operation.TakeReservation;

        InventoryItemInstance item =
            operation.Item;

        if (!source.TryCommitTakeReservation(
                reservation,
                out PlacedInventoryItem
                    removedItem))
        {
            CancelActiveOperation();
            return;
        }

        if (removedItem == null ||
            !ReferenceEquals(
                removedItem.ItemInstance,
                item))
        {
            CancelActiveOperation();
            return;
        }

        if (!gripState.TryHold(
                item,
                operation.TargetGripType,
                operation.TargetGripCount))
        {
            bool restored =
                source.PlaceInstance(
                    item,
                    reservation.Position.x,
                    reservation.Position.y,
                    reservation.RotationSteps
                );

            if (!restored)
            {
                Debug.LogError(
                    "Retrieved item could not be held or restored.",
                    this
                );
            }

            CancelActiveOperation();
            return;
        }

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

    private void CompleteStoreOperation()
    {
        ItemHandlingOperation operation =
            activeOperation;

        if (operation == null ||
            operation.Type !=
                ItemHandlingOperationType.Store)
        {
            CancelActiveOperation();
            return;
        }

        InventoryItemInstance item =
            operation.Item;

        InventoryContainer target =
            operation.TargetContainer;

        InventoryTransferReservation
            reservation =
                operation.TransferReservation;

        if (item == null ||
            item.IsEmpty ||
            target == null ||
            reservation == null ||
            !reservation.IsActive)
        {
            CancelActiveOperation();
            return;
        }

        bool movedAnything =
            target.TryCommitTransferReservation(
                reservation,
                out int remainingQuantity
            );

        if (!movedAnything)
        {
            CancelActiveOperation();
            return;
        }

        if (movedAnything &&
            (remainingQuantity <= 0 ||
             item.IsEmpty))
        {
            if (gripState.IsHolding(
                    item))
            {
                gripState.Release(
                    item
                );
            }
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

        ItemHandlingOperation store =
            new ItemHandlingOperation(
                ItemHandlingOperationType.Store,
                item,
                defaultStoreDuration,
                targetContainer: target,
                transferReservation:
                    transferReservation
            );

        queuedOperations.Add(
            retrieve
        );

        queuedOperations.Add(
            store
        );

        TryStartNextOperation();

        return true;
    }
}