using UnityEngine;

[DisallowMultipleComponent]
public sealed class CharacterAnimationDriver : MonoBehaviour
{
    private static readonly int IsMoving =
        Animator.StringToHash("isMoving");

    private Animator animator;

    public void SetMoving(bool isMoving)
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

        if (animator == null)
            return;

        animator.SetBool(IsMoving, isMoving);
    }
}