using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputRouter))]
[RequireComponent(typeof(CombatSkillLoadout))]
[RequireComponent(typeof(PlayerGameplayState))]
public sealed class CombatSkillController :
    MonoBehaviour
{
    private PlayerInputRouter inputRouter;
    private CombatSkillLoadout skillLoadout;
    private PlayerGameplayState gameplayState;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();

        if (inputRouter == null)
            return;

        inputRouter.Skill1Action.started +=
            HandleSkill1;

        inputRouter.Skill2Action.started +=
            HandleSkill2;

        inputRouter.Skill3Action.started +=
            HandleSkill3;

        inputRouter.Skill4Action.started +=
            HandleSkill4;
    }

    private void OnDisable()
    {
        if (inputRouter == null)
            return;

        inputRouter.Skill1Action.started -=
            HandleSkill1;

        inputRouter.Skill2Action.started -=
            HandleSkill2;

        inputRouter.Skill3Action.started -=
            HandleSkill3;

        inputRouter.Skill4Action.started -=
            HandleSkill4;
    }

    private void HandleSkill1(
        InputAction.CallbackContext context)
    {
        TryRequestSkill(0);
    }

    private void HandleSkill2(
        InputAction.CallbackContext context)
    {
        TryRequestSkill(1);
    }

    private void HandleSkill3(
        InputAction.CallbackContext context)
    {
        TryRequestSkill(2);
    }

    private void HandleSkill4(
        InputAction.CallbackContext context)
    {
        TryRequestSkill(3);
    }

    public bool TryRequestSkill(
        int slotIndex)
    {
        if (InventoryMenuController
            .IsInventoryOpen)
        {
            return false;
        }

        if (gameplayState != null &&
            !gameplayState.Allows(
                PlayerGameplayCapability.Combat))
        {
            return false;
        }

        if (skillLoadout == null)
            return false;

        CombatSkillDefinition skill =
            skillLoadout.GetSkill(
                slotIndex
            );

        if (skill == null)
            return false;

        Debug.Log(
            "Requested skill: " +
            skill.skillName,
            this
        );

        return true;
    }

    private void ResolveReferences()
    {
        if (inputRouter == null)
        {
            inputRouter =
                GetComponent<PlayerInputRouter>();
        }

        if (skillLoadout == null)
        {
            skillLoadout =
                GetComponent<CombatSkillLoadout>();
        }

        if (gameplayState == null)
        {
            gameplayState =
                GetComponent<PlayerGameplayState>();
        }
    }
}