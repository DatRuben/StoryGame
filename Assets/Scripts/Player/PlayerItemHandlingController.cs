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
            item == null ||
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

        immediateActionInProgress = true;

        bool spawned =
            worldItemSpawner
                .TrySpawnForRelease(
                    item,
                    heldPose,
                    out worldItem
                );

        if (!spawned)
        {
            immediateActionInProgress = false;
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

            immediateActionInProgress = false;

            return false;
        }

        if (!gripState.Release(item))
        {
            Destroy(
                worldItem.gameObject
            );

            worldItem = null;

            immediateActionInProgress = false;
            return false;
        }

        immediateActionInProgress = false;

        return true;
    }

    private void ClearOperation()
    {
        CancelOperationReservation(
            activeOperation
        );

        activeOperation = null;

        TryStartNextOperation();
    }

    private void CancelOperationReservation(
        ItemHandlingOperation operation)
    {
        if (operation == null ||
            operation.TransferReservation == null ||
            !operation.TransferReservation.IsActive ||
            operation.TargetContainer == null)
        {
            return;
        }

        operation.TargetContainer
            .CancelTransferReservation(
                operation.TransferReservation
            );
    }

    public bool CancelAllOperations()
    {
        bool cancelledAnything =
            activeOperation != null ||
            queuedOperations.Count > 0;

        CancelOperationReservation(
            activeOperation
        );

        for (int i = 0;
             i < queuedOperations.Count;
             i++)
        {
            CancelOperationReservation(
                queuedOperations[i]
            );
        }

        activeOperation = null;

        queuedOperations.Clear();

        return cancelledAnything;
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

    private void CompleteStoreOperation()
    {
        ItemHandlingOperation operation =
            activeOperation;

        if (operation == null ||
            operation.Type !=
                ItemHandlingOperationType.Store)
        {
            ClearOperation();
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
            ClearOperation();
            return;
        }

        bool movedAnything =
            target.TryCommitTransferReservation(
                reservation,
                out int remainingQuantity
            );

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

        ClearOperation();
    }

    public bool CancelActiveOperation()
    {
        if (activeOperation == null)
            return false;

        ClearOperation();

        return true;
    }
}