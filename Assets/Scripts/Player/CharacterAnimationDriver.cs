
using UnityEngine;

[DisallowMultipleComponent]
public sealed class CharacterAnimationDriver : MonoBehaviour
{
    private static readonly int IsMoving =
        Animator.StringToHash("isMoving");

    private static readonly int IsItemPoseActive =
        Animator.StringToHash("isItemPoseActive");

    private PlayerWeaponDeployment weaponDeployment;
    private PlayerGripState gripState;

    private Animator animator;

    // The controller originally assigned to this model.
    private RuntimeAnimatorController baseController;

    private Animator GetAnimator()
    {
        if (animator == null)
        {
            CharacterModelSetup model =
                GetComponentInChildren<CharacterModelSetup>(
                    true
                );

            animator = model != null
                ? model.AnimatorComponent
                : GetComponentInChildren<Animator>(true);

            baseController = animator != null
                ? animator.runtimeAnimatorController
                : null;
        }

        return animator;
    }

    public void SetMoving(bool isMoving)
    {
        Animator target = GetAnimator();

        if (target != null)
            target.SetBool(IsMoving, isMoving);
    }

    private void OnEnable()
    {
        weaponDeployment =
            GetComponent<PlayerWeaponDeployment>();

        gripState =
            GetComponent<PlayerGripState>();

        if (weaponDeployment != null)
        {
            weaponDeployment.Changed +=
                RefreshWeaponReadyAnimation;
        }

        if (gripState != null)
        {
            gripState.Changed +=
                RefreshWeaponReadyAnimation;
        }

        RefreshWeaponReadyAnimation();
    }

    private void OnDisable()
    {
        if (weaponDeployment != null)
        {
            weaponDeployment.Changed -=
                RefreshWeaponReadyAnimation;
        }

        if (gripState != null)
        {
            gripState.Changed -=
                RefreshWeaponReadyAnimation;
        }
    }

    private void RefreshWeaponReadyAnimation()
    {
        Animator target = GetAnimator();

        if (target == null)
            return;

        CharacterModelSetup model =
            GetComponentInChildren<CharacterModelSetup>(
                true
            );

        InventoryItemInstance weapon = null;

        bool isReady =
            weaponDeployment != null &&
            weaponDeployment.TryGetPrimaryDeployedWeapon(
                out weapon
            );

        RuntimeAnimatorController selectedController =
            baseController;

        // Only conventional weapons use grip-based
        // wield configurations in this step.
        if (isReady &&
            weapon != null &&
            weapon.Definition != null &&
            weapon.Definition.IsConventionalWeapon &&
            model != null &&
            model.WieldConfiguration != null)
        {
            // A specialized ready animation requires
            // a matching actual grip assignment.
            isReady = false;

            if (gripState != null &&
                gripState.TryGetGripAssignment(
                    weapon,
                    out GripType gripType,
                    out int gripCount,
                    out int singleHandIndex))
            {
                WeaponAttachmentSocket? requestedSocket =
                    null;

                if (gripType == GripType.Mouth)
                {
                    requestedSocket =
                        WeaponAttachmentSocket.Mouth;
                }
                else if (singleHandIndex == 0)
                {
                    requestedSocket =
                        WeaponAttachmentSocket.LeftHand;
                }
                else if (singleHandIndex == 1)
                {
                    requestedSocket =
                        WeaponAttachmentSocket.RightHand;
                }

                if (model.WieldConfiguration.TryGetVariant(
                    weapon.Definition.wieldStyle,
                    gripType,
                    gripCount,
                    requestedSocket,
                    out WeaponWieldRigVariant variant) &&
                    variant.readyController != null)
                {
                    selectedController =
                        variant.readyController;

                    isReady = true;
                }
            }
        }



        bool itemPoseActive = isReady;

        // Only use ordinary holding-out animations
        // while weapons are not deployed.
        if ((weaponDeployment == null ||
             !weaponDeployment.WeaponsDrawn) &&
            TryGetHeldItemController(
                model,
                out AnimatorOverrideController heldController))
        {
            selectedController = heldController;
            itemPoseActive = true;
        }



        if (target.runtimeAnimatorController !=
            selectedController)
        {
            target.runtimeAnimatorController =
                selectedController;
        }

        target.SetBool(IsItemPoseActive, itemPoseActive);
    }


    private bool TryGetHeldItemController(
        CharacterModelSetup model,
        out AnimatorOverrideController controller)
    {
        controller = null;

        if (model == null || gripState == null)
            return false;

        InventoryItemInstance leftItem =
            gripState.GetItem(GripType.Hand, 0);

        InventoryItemInstance rightItem =
            gripState.GetItem(GripType.Hand, 1);

        InventoryItemInstance mouthItem =
            gripState.GetItem(GripType.Mouth, 0);

        InventoryItemInstance item = null;

        if (leftItem != null &&
            ReferenceEquals(leftItem, rightItem) &&
            mouthItem == null)
        {
            item = leftItem;
        }
        else if (leftItem != null &&
                 rightItem == null &&
                 mouthItem == null)
        {
            item = leftItem;
        }
        else if (rightItem != null &&
                 leftItem == null &&
                 mouthItem == null)
        {
            item = rightItem;
        }
        else if (mouthItem != null &&
                 leftItem == null &&
                 rightItem == null)
        {
            item = mouthItem;
        }

        // No held item, or multiple distinct items
        // requiring independent animations.
        if (item == null)
            return false;

        if (!gripState.TryGetGripAssignment(
                item,
                out GripType gripType,
                out int gripCount,
                out int singleHandIndex))
        {
            return false;
        }

        if (!model.TryGetHeldItemPresentation(
                gripType,
                gripCount,
                singleHandIndex,
                out HeldItemPresentationBinding presentation))
        {
            return false;
        }

        controller = presentation.controller;

        return controller != null;
    }

}
