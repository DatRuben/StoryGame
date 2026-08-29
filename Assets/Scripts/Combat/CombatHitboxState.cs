using UnityEngine;

public sealed class CombatHitboxStateCleanup :
    StateMachineBehaviour
{
    public override void OnStateEnter(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        DeactivateAllHitboxes(animator);
    }

    public override void OnStateExit(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        DeactivateAllHitboxes(animator);
    }

    private static void DeactivateAllHitboxes(
        Animator animator)
    {
        if (animator == null)
            return;

        CombatHitbox[] hitboxes =
            animator.GetComponentsInChildren<
                CombatHitbox>(
                    true
                );

        for (int i = 0;
             i < hitboxes.Length;
             i++)
        {
            if (hitboxes[i] != null)
            {
                hitboxes[i]
                    .DeactivateHitbox();
            }
        }
    }
}