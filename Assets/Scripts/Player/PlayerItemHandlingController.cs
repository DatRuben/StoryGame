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
            worldItemSpawner.TrySpawn(
                item,
                heldPose.position,
                heldPose.rotation,
                out worldItem
            );

        if (!spawned)
        {
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