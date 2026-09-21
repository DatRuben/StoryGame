using UnityEngine;

public enum ItemHandlingOperationType
{
    None,
    Store
}

[RequireComponent(typeof(PlayerGripState))]
[RequireComponent(typeof(PlayerHeldItemPresenter))]

public sealed class PlayerItemHandlingController :
    MonoBehaviour
{
    [SerializeField]
    [Min(0.05f)]
    private float defaultStoreDuration = 0.75f;

    private InventoryContainer
        activeTargetContainer;

    private InventoryTransferReservation
        activeTransferReservation;

    private float operationElapsed;
    private float operationDuration;

    public ItemHandlingOperationType
        ActiveOperation
    {
        get;
        private set;
    }

    public float OperationProgress01
    {
        get
        {
            if (!IsBusy ||
                operationDuration <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(
                operationElapsed /
                operationDuration
            );
        }
    }

    private PlayerGripState gripState;

    private PlayerHeldItemPresenter
        heldItemPresenter;

    private WorldItemSpawner
        worldItemSpawner;

    public bool IsBusy
    {
        get;
        private set;
    }

    public InventoryItemInstance ActiveItem
    {
        get;
        private set;
    }

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

        IsBusy = true;
        ActiveItem = worldItemInstance;

        if (!gripState.TryHold(
                worldItemInstance,
                gripType,
                gripCount))
        {
            ClearOperation();
            return false;
        }

        if (!worldItem.ReleaseItem(
                worldItemInstance))
        {
            gripState.Release(
                worldItemInstance
            );

            ClearOperation();
            return false;
        }

        item = worldItemInstance;

        ClearOperation();

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

        IsBusy = true;
        ActiveItem = item;

        bool spawned =
            worldItemSpawner
                .TrySpawnForRelease(
                    item,
                    heldPose,
                    out worldItem
                );

        if (!spawned)
        {
            ClearOperation();
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

            ClearOperation();

            return false;
        }

        if (!gripState.Release(item))
        {
            Destroy(
                worldItem.gameObject
            );

            worldItem = null;

            ClearOperation();
            return false;
        }

        ClearOperation();

        return true;
    }

    private void ClearOperation()
    {
        if (activeTransferReservation != null &&
            activeTransferReservation.IsActive &&
            activeTargetContainer != null)
        {
            activeTargetContainer
                .CancelTransferReservation(
                    activeTransferReservation
                );
        }

        ActiveItem = null;

        ActiveOperation =
            ItemHandlingOperationType.None;

        activeTargetContainer = null;
        activeTransferReservation = null;

        operationElapsed = 0f;
        operationDuration = 0f;

        IsBusy = false;
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

        IsBusy = true;
        ActiveItem = item;

        ActiveOperation =
            ItemHandlingOperationType.Store;

        activeTargetContainer =
            target;

        activeTransferReservation =
            reservation;

        operationElapsed = 0f;

        operationDuration =
            Mathf.Max(
                0.05f,
                defaultStoreDuration
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
        if (ActiveItem == null ||
            ActiveItem.IsEmpty ||
            gripState == null ||
            !gripState.IsHolding(
                ActiveItem) ||
            activeTargetContainer == null ||
            activeTransferReservation == null ||
            !activeTransferReservation.IsActive)
        {
            CancelActiveOperation();
            return;
        }

        operationElapsed +=
            Time.deltaTime;

        if (operationElapsed <
            operationDuration)
        {
            return;
        }

        CompleteStoreOperation();
    }

    private void CompleteStoreOperation()
    {
        InventoryItemInstance item =
            ActiveItem;

        InventoryContainer target =
            activeTargetContainer;

        InventoryTransferReservation
            reservation =
                activeTransferReservation;

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
        if (!IsBusy)
            return false;

        ClearOperation();

        return true;
    }
}