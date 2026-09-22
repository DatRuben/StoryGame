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

    private bool immediateActionInProgress;

    private PlayerGripState gripState;

    private PlayerHeldItemPresenter
        heldItemPresenter;

    private WorldItemSpawner
        worldItemSpawner;

    public ItemHandlingOperation
        CurrentOperation =>
            activeOperation;

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
        if (activeOperation != null &&
            activeOperation
                .TransferReservation != null &&
            activeOperation
                .TransferReservation.IsActive &&
            activeOperation.TargetContainer != null)
        {
            activeOperation
                .TargetContainer
                .CancelTransferReservation(
                    activeOperation
                        .TransferReservation
                );
        }

        activeOperation = null;
    }

    public bool TryBeginStoreHeldItem(
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

        if (!target.TryReserveTransferIn(
                item,
                0,
                out InventoryTransferReservation
                    reservation))
        {
            return false;
        }

        activeOperation =
            new ItemHandlingOperation(
                ItemHandlingOperationType.Store,
                item,
                defaultStoreDuration,
                null,
                target,
                reservation
            );

        return true;
    }

    private void Update()
    {
        if (!IsBusy)
            return;

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