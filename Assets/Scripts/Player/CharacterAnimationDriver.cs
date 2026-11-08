using UnityEngine;

[DisallowMultipleComponent]
public sealed class CharacterAnimationDriver : MonoBehaviour
{
    private static readonly int IsMoving =
        Animator.StringToHash("isMoving");

    private static readonly int IsWeaponReady =
    Animator.StringToHash("isWeaponReady");

    private PlayerWeaponDeployment weaponDeployment;

    private Animator animator;

    //movement control
    private Animator GetAnimator()
    {
        if (animator == null)
        {
            CharacterModelSetup model =
                GetComponentInChildren<
                    CharacterModelSetup>(true);

            animator = model != null
                ? model.AnimatorComponent
                : GetComponentInChildren<Animator>(true);
        }

        return animator;
    }

    public void SetMoving(bool isMoving)
    {
        Animator target = GetAnimator();

        if (target == null)
            return;

        target.SetBool(IsMoving, isMoving);
    }

    //weapon control
    private void OnEnable()
    {
        if (weaponDeployment == null)
            weaponDeployment =
                GetComponent<PlayerWeaponDeployment>();

        if (weaponDeployment != null)
            weaponDeployment.Changed +=
                RefreshWeaponReadyAnimation;

        RefreshWeaponReadyAnimation();
    }

    private void OnDisable()
    {
        if (weaponDeployment != null)
            weaponDeployment.Changed -=
                RefreshWeaponReadyAnimation;
    }

    //ready param updater
    private void RefreshWeaponReadyAnimation()
    {
        Animator target = GetAnimator();

        if (target == null)
            return;

        bool isReady =
            weaponDeployment != null &&
            weaponDeployment.TryGetPrimaryDeployedWeapon(
                out _
            );

        target.SetBool(IsWeaponReady, isReady);
    }
}