using UnityEngine;

[RequireComponent(typeof(PlayerGripState))]
public sealed class PlayerHeldItemPresenter :
    MonoBehaviour
{
    private PlayerGripState gripState;

    private GameObject leftVisual;
    private GameObject rightVisual;
    private GameObject mouthVisual;

    private InventoryItemInstance
        leftVisualItem;

    private InventoryItemInstance
        rightVisualItem;

    private InventoryItemInstance
        mouthVisualItem;

    private void Awake()
    {
        gripState =
            GetComponent<PlayerGripState>();
    }

    private void OnEnable()
    {
        if (gripState == null)
        {
            gripState =
                GetComponent<PlayerGripState>();
        }

        if (gripState != null)
        {
            gripState.Changed +=
                Refresh;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (gripState != null)
        {
            gripState.Changed -=
                Refresh;
        }

        ClearVisuals();
    }

    private void Refresh()
    {
        if (gripState == null)
            return;

        HeldItemAnchors anchors =
            GetComponentInChildren<
                HeldItemAnchors>(true);

        if (anchors == null)
        {
            ClearVisuals();
            return;
        }

        InventoryItemInstance leftItem =
            gripState.GetItem(
                GripType.Hand,
                0
            );

        InventoryItemInstance rightItem =
            gripState.GetItem(
                GripType.Hand,
                1
            );

        InventoryItemInstance mouthItem =
            gripState.GetItem(
                GripType.Mouth,
                0
            );

        bool sameTwoHandItem =
            leftItem != null &&
            ReferenceEquals(
                leftItem,
                rightItem
            );

        if (sameTwoHandItem)
        {
            RefreshVisual(
                leftItem,
                anchors.RightHand,
                ref rightVisualItem,
                ref rightVisual
            );

            ClearVisual(
                ref leftVisualItem,
                ref leftVisual
            );
        }
        else
        {
            RefreshVisual(
                leftItem,
                anchors.LeftHand,
                ref leftVisualItem,
                ref leftVisual
            );

            RefreshVisual(
                rightItem,
                anchors.RightHand,
                ref rightVisualItem,
                ref rightVisual
            );
        }

        RefreshVisual(
            mouthItem,
            anchors.Mouth,
            ref mouthVisualItem,
            ref mouthVisual
        );
    }

    private void RefreshVisual(
        InventoryItemInstance item,
        Transform anchor,
        ref InventoryItemInstance shownItem,
        ref GameObject visual)
    {
        if (ReferenceEquals(
                item,
                shownItem) &&
            visual != null)
        {
            return;
        }

        ClearVisual(
            ref shownItem,
            ref visual
        );

        if (item == null ||
            item.Definition == null ||
            item.Definition.worldPrefab == null ||
            anchor == null)
        {
            return;
        }

        visual =
            Instantiate(
                item.Definition.worldPrefab
            );

        Transform visualTransform =
            visual.transform;

        Vector3 authoredLocalPosition =
            visualTransform.localPosition;

        Quaternion authoredLocalRotation =
            visualTransform.localRotation;

        visualTransform.SetParent(
            anchor,
            true
        );

        visualTransform.localPosition =
            authoredLocalPosition;

        visualTransform.localRotation =
            authoredLocalRotation;

        shownItem = item;

        DisablePhysics(
            visual
        );
    }

    private void DisablePhysics(
        GameObject visual)
    {
        Collider[] colliders =
            visual.GetComponentsInChildren<
                Collider>(true);

        for (int i = 0;
             i < colliders.Length;
             i++)
        {
            colliders[i].enabled = false;
        }

        Rigidbody[] rigidbodies =
            visual.GetComponentsInChildren<
                Rigidbody>(true);

        for (int i = 0;
             i < rigidbodies.Length;
             i++)
        {
            rigidbodies[i].isKinematic =
                true;
        }
    }

    private void ClearVisual(
        ref InventoryItemInstance shownItem,
        ref GameObject visual)
    {
        shownItem = null;

        if (visual != null)
        {
            Destroy(
                visual
            );
        }

        visual = null;
    }

    private void ClearVisuals()
    {
        ClearVisual(
            ref leftVisualItem,
            ref leftVisual
        );

        ClearVisual(
            ref rightVisualItem,
            ref rightVisual
        );

        ClearVisual(
            ref mouthVisualItem,
            ref mouthVisual
        );
    }
}