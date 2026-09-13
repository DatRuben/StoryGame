using UnityEngine;

[RequireComponent(
    typeof(PlayerGameplayState)
)]
[RequireComponent(
    typeof(PlayerCombatController)
)]
[RequireComponent(
    typeof(InventoryInteractionController)
)]
public sealed class PlayerPrimaryActionController :
    MonoBehaviour
{
    private PlayerGameplayState gameplayState;

    private PlayerCombatController
        combatController;

    private InventoryInteractionController
        inventoryInteractionController;

    private Animator animator;

    private void Awake()
    {
        gameplayState =
            GetComponent<PlayerGameplayState>();

        combatController =
            GetComponent<PlayerCombatController>();

        inventoryInteractionController =
            GetComponent<
                InventoryInteractionController>();

        animator =
            GetComponent<Animator>();
    }

    public void HandlePrimaryAction()
    {
        if (InventoryMenuController
            .IsInventoryOpen)
        {
            return;
        }

        if (inventoryInteractionController !=
                null &&
            inventoryInteractionController
                .HasUsableSelectedHeldItem)
        {
            inventoryInteractionController
                .TryUseSelectedHeldItem();

            return;
        }

        if (gameplayState != null &&
            !gameplayState.Allows(
                PlayerGameplayCapability.Combat))
        {
            return;
        }

        if (animator != null)
        {
            animator.SetTrigger(
                "attack"
            );
        }

        if (combatController != null)
        {
            combatController
                .TryPrimaryAttack();
        }
    }
}