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
                GripType.Hand,
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
                GripType.Hand,
                ref leftVisualItem,
                ref leftVisual
            );

            RefreshVisual(
                rightItem,
                anchors.RightHand,
                GripType.Hand,
                ref rightVisualItem,
                ref rightVisual
            );
        }

        RefreshVisual(
            mouthItem,
            anchors.Mouth,
            GripType.Mouth,
            ref mouthVisualItem,
            ref mouthVisual
        );
    }

    private void RefreshVisual(
        InventoryItemInstance item,
        Transform anchor,
        GripType gripType,
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

        GameObject visualRoot =
            new GameObject(
                "HeldItemVisual"
            );

        Transform rootTransform =
            visualRoot.transform;

        rootTransform.position =
            anchor.position;

        rootTransform.rotation =
            anchor.rotation;

        rootTransform.localScale =
            Vector3.one;

        rootTransform.SetParent(
            anchor,
            true
        );

        GameObject itemVisual =
            Instantiate(
                item.Definition.worldPrefab,
                rootTransform,
                false
            );

        if (itemVisual == null)
        {
            Destroy(
                visualRoot
            );

            return;
        }

        HeldItemGripPoints gripPoints =
            itemVisual.GetComponentInChildren<
                HeldItemGripPoints>(true);

        Transform gripPoint =
            gripPoints != null
                ? gripPoints.GetPrimaryGrip(
                    gripType
                )
                : null;

        if (gripPoint != null)
        {
            AlignGripPointToAnchor(
                rootTransform,
                gripPoint,
                anchor
            );
        }

        visual =
            visualRoot;

        shownItem =
            item;

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

    public bool TryGetHeldItemPose(
        InventoryItemInstance item,
        out Pose pose)
    {
        pose = default;

        if (item == null)
            return false;

        if (ReferenceEquals(
                item,
                leftVisualItem) &&
            leftVisual != null)
        {
            Transform releaseFrame =
                leftVisual.transform;

            pose = new Pose(
                releaseFrame.position,
                releaseFrame.rotation
            );

            return true;
        }

        if (ReferenceEquals(
                item,
                rightVisualItem) &&
            rightVisual != null)
        {
            Transform releaseFrame =
                rightVisual.transform;

            pose = new Pose(
                releaseFrame.position,
                releaseFrame.rotation
            );

            return true;
        }

        if (ReferenceEquals(
                item,
                mouthVisualItem) &&
            mouthVisual != null)
        {
            Transform releaseFrame =
                mouthVisual.transform;

            pose = new Pose(
                releaseFrame.position,
                releaseFrame.rotation
            );

            return true;
        }

        return false;
    }

    private static void AlignGripPointToAnchor(
        Transform itemRoot,
        Transform gripPoint,
        Transform anchor)
    {
        if (itemRoot == null ||
            gripPoint == null ||
            anchor == null)
        {
            return;
        }

        Quaternion rotationDelta =
            anchor.rotation *
            Quaternion.Inverse(
                gripPoint.rotation
            );

        itemRoot.rotation =
            rotationDelta *
            itemRoot.rotation;

        Vector3 positionDelta =
            anchor.position -
            gripPoint.position;

        itemRoot.position +=
            positionDelta;
    }
}