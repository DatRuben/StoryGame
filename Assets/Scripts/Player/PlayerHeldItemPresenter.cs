using UnityEngine;

[RequireComponent(typeof(PlayerGripState))]
public sealed class PlayerHeldItemPresenter :
    MonoBehaviour
{
    private ItemVisualResolver visualResolver;

    public void BindItemVisualResolver(
        ItemVisualResolver newVisualResolver)
    {
        visualResolver =
            newVisualResolver;

        if (isActiveAndEnabled)
        {
            Refresh();
        }
    }

    private PlayerGripState gripState;
    private PlayerWeaponDeployment weaponDeployment; //for holding weapon to attack instead of holding it out

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
        weaponDeployment =
            GetComponent<PlayerWeaponDeployment>();
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

        //weapon deployemnt status
        if (weaponDeployment == null)
        {
            weaponDeployment =
                GetComponent<PlayerWeaponDeployment>();
        }

        if (weaponDeployment != null)
        {
            weaponDeployment.Changed += Refresh;
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
        //weapon deployment status
        if (weaponDeployment != null)
        {
            weaponDeployment.Changed -= Refresh;
        }

        ClearVisuals();
    }



    private static void ApplyWeaponFit( //align weapon to the virtual marker thats parented to the socket
        Transform visualRoot,
        Transform weaponVisual,
        WeaponWieldItemFit fit)
    {
        if (visualRoot == null ||
            weaponVisual == null ||
            fit == null)
        {
            return;
        }

        Vector3 gripPosition =
            weaponVisual.localPosition +
            weaponVisual.localRotation *
            Vector3.Scale(
                weaponVisual.localScale,
                fit.localPosition
            );

        Quaternion gripRotation =
            weaponVisual.localRotation *
            fit.LocalRotation;


        visualRoot.localScale =
            fit.localScaleMultiplier;

        visualRoot.localRotation =
            Quaternion.Inverse(gripRotation);

        visualRoot.localPosition =
            -(
                visualRoot.localRotation *
                Vector3.Scale(
                    visualRoot.localScale,
                    gripPosition
                )
            );
    }


    private bool TryResolveWeaponFit(
        InventoryItemInstance item,
        CharacterModelSetup model,
        HeldItemAnchors anchors,
        out Transform anchor,
        out WeaponWieldItemFit fit)
    {
        anchor = null;
        fit = null;

        if (item == null ||
            item.Definition == null ||
            model == null ||
            model.WieldConfiguration == null ||
            anchors == null ||
            gripState == null)
        {
            return false;
        }

        if (!gripState.TryGetGripAssignment(
            item,
            out GripType gripType,
            out int gripCount,
            out int singleHandIndex))
        {
            return false;
        }

        WeaponAttachmentSocket? requestedSocket = null;

        if (gripType == GripType.Mouth)
        {
            requestedSocket = WeaponAttachmentSocket.Mouth;
        }
        else if (singleHandIndex == 0)
        {
            requestedSocket = WeaponAttachmentSocket.LeftHand;
        }
        else if (singleHandIndex == 1)
        {
            requestedSocket = WeaponAttachmentSocket.RightHand;
        }

        if (!model.WieldConfiguration.TryGetVariant(
            item.Definition.wieldStyle,
            gripType,
            gripCount,
            requestedSocket,
            out WeaponWieldRigVariant variant))
        {
            return false;
        }

        switch (variant.primarySocket)
        {
            case WeaponAttachmentSocket.LeftHand:
                anchor = anchors.LeftHand;
                break;

            case WeaponAttachmentSocket.RightHand:
                anchor = anchors.RightHand;
                break;

            case WeaponAttachmentSocket.Mouth:
                anchor = anchors.Mouth;
                break;

            default:
                return false;
        }

        if (anchor == null)
            return false;

        HeldItemGripContext context =
            weaponDeployment != null
                ? weaponDeployment.GetGripContext(item)
                : HeldItemGripContext.Held;

        return variant.TryGetItemFit(
            item.Definition,
            context,
            out fit);
    }



    private void Refresh()
    {
        if (gripState == null)
            return;

        HeldItemAnchors anchors =
            GetComponentInChildren<HeldItemAnchors>(true);

        if (anchors == null)
        {
            ClearVisuals();
            return;
        }

        CharacterModelSetup model =
            GetComponentInChildren<CharacterModelSetup>(true);

        InventoryItemInstance leftItem =
            gripState.GetItem(GripType.Hand, 0);

        InventoryItemInstance rightItem =
            gripState.GetItem(GripType.Hand, 1);

        InventoryItemInstance mouthItem =
            gripState.GetItem(GripType.Mouth, 0);

        bool sameTwoHandItem =
            leftItem != null &&
            ReferenceEquals(leftItem, rightItem);

        if (sameTwoHandItem)
        {
            RefreshVisual(
                leftItem,
                anchors.RightHand,
                GripType.Hand,
                model,
                anchors,
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
                model,
                anchors,
                ref leftVisualItem,
                ref leftVisual
            );

            RefreshVisual(
                rightItem,
                anchors.RightHand,
                GripType.Hand,
                model,
                anchors,
                ref rightVisualItem,
                ref rightVisual
            );
        }

        RefreshVisual(
            mouthItem,
            anchors.Mouth,
            GripType.Mouth,
            model,
            anchors,
            ref mouthVisualItem,
            ref mouthVisual
        );
    }




    private void RefreshVisual(
        InventoryItemInstance item,
        Transform fallbackAnchor,
        GripType gripType,
        CharacterModelSetup model,
        HeldItemAnchors anchors,
        ref InventoryItemInstance shownItem,
        ref GameObject visual)
    {
        if (item == null || item.Definition == null)
        {
            ClearVisual(ref shownItem, ref visual);
            return;
        }

        Transform anchor = fallbackAnchor;
        WeaponWieldItemFit fit = null;

        // Specialized weapon fitting is only needed
        // when the weapon is deployed.
        bool usesWeaponFit =
            item.Definition.IsConventionalWeapon &&
            weaponDeployment != null &&
            weaponDeployment.IsDeployed(item);

        if (usesWeaponFit &&
            !TryResolveWeaponFit(
                item,
                model,
                anchors,
                out anchor,
                out fit))
        {
            Debug.LogWarning(
                $"Missing weapon fitting for " +
                $"{item.Definition.name}.",
                this);

            ClearVisual(ref shownItem, ref visual);
            return;
        }


        if (!usesWeaponFit &&
            model != null &&
            gripState != null &&
            gripState.TryGetGripAssignment(
                item,
                out GripType assignedType,
                out int assignedCount,
                out int singleHandIndex) &&
            model.TryGetHeldItemPresentation(
                assignedType,
                assignedCount,
                singleHandIndex,
                out HeldItemPresentationBinding presentation))
        {
            anchor = presentation.socket;
        }


        if (anchor == null)
        {
            ClearVisual(ref shownItem, ref visual);
            return;
        }

        // Reuse the visual if this is still
        // the same item instance.
        if (ReferenceEquals(item, shownItem) &&
            visual != null)
        {
            Transform root = visual.transform;

            if (root.parent != anchor)
                root.SetParent(anchor, false);

            if (usesWeaponFit)
            {
                if (root.childCount == 0)
                {
                    ClearVisual(ref shownItem, ref visual);
                    return;
                }

                ApplyWeaponFit(
                    root,
                    root.GetChild(0),
                    fit);
            }
            else
            {
                ApplyHeldOrientation(
                    root,
                    gripType,
                    anchor);
            }

            return;
        }

        ClearVisual(ref shownItem, ref visual);

        GameObject visualPrefab =
            visualResolver != null
                ? visualResolver.ResolveWorldPrefab(
                    item.Definition)
                : item.Definition.worldPrefab;

        if (visualPrefab == null)
            return;

        GameObject visualRoot =
            new GameObject("HeldItemVisual");

        Transform rootTransform =
            visualRoot.transform;

        rootTransform.SetParent(anchor, false);
        rootTransform.localPosition = Vector3.zero;
        rootTransform.localRotation = Quaternion.identity;
        rootTransform.localScale = Vector3.one;

        GameObject itemVisual =
            Instantiate(
                visualPrefab,
                rootTransform,
                false);

        if (itemVisual == null)
        {
            Destroy(visualRoot);
            return;
        }

        if (usesWeaponFit)
        {
            ApplyWeaponFit(
                rootTransform,
                itemVisual.transform,
                fit);
        }
        else
        {
            ApplyHeldOrientation(
                rootTransform,
                gripType,
                anchor);
        }

        visual = visualRoot;
        shownItem = item;

        DisablePhysics(visual);
    }



    //apply the transform in the presenter

    private static void ApplyHeldOrientation(
        Transform root,
        GripType gripType,
        Transform anchor)
    {
        if (root == null || anchor == null)
            return;

        // The character socket determines
        // where the item appears.

        root.localPosition = Vector3.zero;
        root.localRotation = Quaternion.identity;

        // Preserve the prefab's authored world scale
        // instead of multiplying it by the character's scale.
        Vector3 socketScale = anchor.lossyScale;

        if (Mathf.Approximately(socketScale.x, 0f) ||
            Mathf.Approximately(socketScale.y, 0f) ||
            Mathf.Approximately(socketScale.z, 0f))
        {
            Debug.LogError(
                $"Holding socket '{anchor.name}' has a zero scale.",
                anchor);
            return;
        }

        root.localScale = new Vector3(
            1f / socketScale.x,
            1f / socketScale.y,
            1f / socketScale.z
        );


        HeldItemGripPoints points =
            root.GetComponentInChildren<
                HeldItemGripPoints>(true);

        // The prefab's original orientation is
        // used if there is no Held reference.
        if (points == null)
            return;

        Transform reference = null;

        if (points.HasGripSets)
        {
            points.TryGetGripPoint(
                HeldItemGripContext.Held,
                out reference);
        }
        else
        {
            reference =
                points.GetPrimaryGrip(gripType);
        }

        if (reference == null)
            return;

        // Correct orientation only.
        // Item-side reference position is ignored.
        Quaternion rotationDelta =
            anchor.rotation *
            Quaternion.Inverse(reference.rotation);

        root.rotation =
            rotationDelta * root.rotation;
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

    public bool TryGetHeldItemCastPoint(
        InventoryItemInstance item,
        out Transform castPoint)
    {
        castPoint = null;

        if (item == null)
            return false;

        GameObject heldVisual = null;

        if (ReferenceEquals(
                item,
                leftVisualItem))
        {
            heldVisual =
                leftVisual;
        }
        else if (ReferenceEquals(
                     item,
                     rightVisualItem))
        {
            heldVisual =
                rightVisual;
        }
        else if (ReferenceEquals(
                     item,
                     mouthVisualItem))
        {
            heldVisual =
                mouthVisual;
        }

        if (heldVisual == null)
            return false;

        ItemActionPoints actionPoints =
            heldVisual.GetComponentInChildren<
                ItemActionPoints>(true);

        if (actionPoints == null ||
            actionPoints.CastPoint == null)
        {
            return false;
        }

        castPoint =
            actionPoints.CastPoint;

        return true;
    }

  
}