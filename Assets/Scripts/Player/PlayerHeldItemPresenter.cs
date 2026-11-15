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


    private void LateUpdate()
    {
        if (gripState == null)
            return;

        if (leftVisual == null &&
            rightVisual == null &&
            mouthVisual == null)
        {
            return;
        }

        CharacterModelSetup model =
            GetComponentInChildren<CharacterModelSetup>(true);

        UpdateHeldVisualPose(
            leftVisualItem,
            leftVisual,
            model);

        UpdateHeldVisualPose(
            rightVisualItem,
            rightVisual,
            model);

        UpdateHeldVisualPose(
            mouthVisualItem,
            mouthVisual,
            model);
    }

    private void UpdateHeldVisualPose(
        InventoryItemInstance item,
        GameObject visual,
        CharacterModelSetup model)
    {
        if (item == null ||
            item.Definition == null ||
            visual == null ||
            gripState == null)
        {
            return;
        }

        // Deployed weapons use their separate Ready fit.
        if (item.Definition.IsConventionalWeapon &&
            weaponDeployment != null &&
            weaponDeployment.IsDeployed(item))
        {
            return;
        }

        if (!gripState.TryGetGripAssignment(
            item,
            out GripType gripType,
            out int gripCount,
            out int singleHandIndex))
        {
            return;
        }

        HeldItemPresentationBinding presentation = null;

        if (model != null)
        {
            model.TryGetHeldItemPresentation(
                gripType,
                gripCount,
                singleHandIndex,
                out presentation);
        }

        Transform root = visual.transform;
        Transform anchor = root.parent;

        if (anchor == null)
            return;

        ApplyHeldOrientation(
            root,
            gripType,
            anchor,
            presentation);
    }




    private static void ApplyWeaponFit(
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

        Transform socket = visualRoot.parent;

        if (socket == null)
            return;

        Vector3 socketScale = socket.lossyScale;

        if (Mathf.Approximately(socketScale.x, 0f) ||
            Mathf.Approximately(socketScale.y, 0f) ||
            Mathf.Approximately(socketScale.z, 0f))
        {
            Debug.LogError(
                $"Weapon socket '{socket.name}' has zero scale.",
                socket
            );
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

        // Preserve the prefab's world scale independently
        // of the character rig's inherited scale.
        visualRoot.localScale = new Vector3(
            fit.localScaleMultiplier.x / socketScale.x,
            fit.localScaleMultiplier.y / socketScale.y,
            fit.localScaleMultiplier.z / socketScale.z
        );

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



        return variant.TryGetItemFit(
            item.Definition,
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
        HeldItemPresentationBinding heldPresentation = null;

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
                out heldPresentation))
        {
            anchor = heldPresentation.socket;
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
                    anchor,
                    heldPresentation);
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
                anchor,
                heldPresentation);
        }

        visual = visualRoot;
        shownItem = item;

        DisablePhysics(visual);
    }



    //apply the rotation in the presenter


    private void ApplyHeldOrientation(
        Transform root,
        GripType gripType,
        Transform anchor,
        HeldItemPresentationBinding presentation)
    {
        if (root == null || anchor == null)
            return;

        // Resolve upright orientation from the player,
        // independently of the animated hand socket.
        Vector3 forward = Vector3.ProjectOnPlane(
            transform.forward,
            Vector3.up
        );

        Quaternion facingRotation =
            forward.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(
                    forward,
                    Vector3.up)
                : Quaternion.identity;

        // Reset the attachment wrapper before alignment.
        root.localPosition = Vector3.zero;
        root.localRotation = Quaternion.identity;

        // Preserve the item's authored prefab scale.
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

        // The socket supplies the animated position.
        // The offset uses character-facing axes,
        // not the socket's animated rotation.
        Vector3 offset = presentation != null
            ? presentation.localPositionOffset
            : Vector3.zero;

        Vector3 targetPosition =
            anchor.position +
            facingRotation *
            Vector3.Scale(socketScale, offset);

        HeldItemOrientationReference orientation =
    root.GetComponentInChildren<
        HeldItemOrientationReference>(true);

        Transform reference =
            orientation != null
                ? orientation.OrientationReference
                : null;

        // Fallback for items without a valid Held reference.
        if (reference == null)
        {
            root.rotation = facingRotation;
            root.position = targetPosition;
            return;
        }

        // Align the item's Held reference to the
        // character-facing orientation.
        Quaternion rotationDelta =
            facingRotation *
            Quaternion.Inverse(reference.rotation);

        root.rotation =
            rotationDelta * root.rotation;

        // Align the physical Held reference position
        // with the character's target position.
        root.position = targetPosition;
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