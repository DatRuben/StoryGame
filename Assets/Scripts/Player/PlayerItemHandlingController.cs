using UnityEngine;

[RequireComponent(typeof(PlayerGripState))]
[RequireComponent(typeof(PlayerHeldItemPresenter))]
public sealed class PlayerItemHandlingController :
    MonoBehaviour
{
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
        ActiveItem = null;
        IsBusy = false;
    }
}