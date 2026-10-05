using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputRouter))]
public sealed class PlayerCombatSkillInput :
    MonoBehaviour
{
    private PlayerInputRouter inputRouter;

    [SerializeField]
    private CombatSkillController
        skillController;

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
        RequestSkill(0);
    }

    private void HandleSkill2(
        InputAction.CallbackContext context)
    {
        RequestSkill(1);
    }

    private void HandleSkill3(
        InputAction.CallbackContext context)
    {
        RequestSkill(2);
    }

    private void HandleSkill4(
        InputAction.CallbackContext context)
    {
        RequestSkill(3);
    }

    private void RequestSkill(
        int slotIndex)
    {
        if (InventoryMenuController
            .IsInventoryOpen)
        {
            return;
        }

        if (skillController == null)
            return;

        skillController.TryRequestSkill(
            slotIndex
        );
    }

    private void ResolveReferences()
    {
        if (inputRouter == null)
        {
            inputRouter =
                GetComponent<
                    PlayerInputRouter>();
        }

        if (skillController == null)
        {
            skillController =
                GetComponent<
                    CombatSkillController>();
        }
    }
}