using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup inventoryCanvasGroup;
    [SerializeField] private Behaviour[] componentsDisabledWhileOpen;

    [Header("Settings")]
    [SerializeField] private bool startsOpen = false;

    private PlayerInputRouter inputRouter;
    private bool isOpen;

    private PlayerStorageContainerInteract storageInteract;

    private PlayerGameplayState gameplayState;

    private InventoryInteractionController
    interactionController;

    public static bool IsInventoryOpen { get; private set; }

    private void Awake()
    {
        if (inventoryCanvasGroup == null)
            inventoryCanvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        SubscribeInput();
        SubscribeGameplayState();
    }

    private void OnDisable()
    {
        UnsubscribeInput();
        UnsubscribeGameplayState();
    }

    private void Start()
    {
        SetInventoryOpen(startsOpen);
    }

    private void ToggleInventory(InputAction.CallbackContext context)
    {
        SetInventoryOpen(!isOpen);
    }

    public void SetComponentsDisabledWhileOpen(
    Behaviour[] components)
    {
        componentsDisabledWhileOpen = components;
        SetInventoryOpen(isOpen);
    }

    public void BindPlayerStorageInteract(
        PlayerStorageContainerInteract newStorageInteract)
    {
        storageInteract = newStorageInteract;

        if (!isOpen && storageInteract != null)
            storageInteract.CloseOpenContainer();
    }

    public void BindInput(
        PlayerInputRouter newInputRouter)
    {
        UnsubscribeInput();

        inputRouter = newInputRouter;

        if (isActiveAndEnabled)
            SubscribeInput();
    }

    private void SubscribeInput()
    {
        if (inputRouter == null)
            return;

        inputRouter.InventoryAction.started -=
            ToggleInventory;

        inputRouter.InventoryAction.started +=
            ToggleInventory;
    }

    private void UnsubscribeInput()
    {
        if (inputRouter == null)
            return;

        inputRouter.InventoryAction.started -=
            ToggleInventory;
    }

    public void SetInventoryOpen(bool open)
    {
        if (open &&
            gameplayState != null &&
            !gameplayState.Allows(
                PlayerGameplayCapability.Inventory))
        {
            open = false;
        }

        isOpen = open;
        IsInventoryOpen = open;

        if (!open)
        {
            if (storageInteract != null)
            {
                storageInteract
                    .CloseOpenContainer();
            }

            if (interactionController != null)
            {
                interactionController
                    .CancelLoadoutAssignment();
            }
        }

        if (inventoryCanvasGroup != null)
        {
            inventoryCanvasGroup.alpha = open ? 1f : 0f;
            inventoryCanvasGroup.interactable = open;
            inventoryCanvasGroup.blocksRaycasts = open;
        }

        foreach (Behaviour component in componentsDisabledWhileOpen)
        {
            if (component != null)
            {
                component.enabled = !open;
            }
        }

        if (open)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void BindPlayerGameplayState(
        PlayerGameplayState newGameplayState)
    {
        UnsubscribeGameplayState();

        gameplayState =
            newGameplayState;

        if (isActiveAndEnabled)
        {
            SubscribeGameplayState();
        }

        if (gameplayState != null &&
            !gameplayState.Allows(
                PlayerGameplayCapability.Inventory))
        {
            SetInventoryOpen(false);
        }
    }

    private void SubscribeGameplayState()
    {
        if (gameplayState == null)
            return;

        gameplayState.OnCapabilitiesInterrupted -=
            HandleCapabilitiesInterrupted;

        gameplayState.OnCapabilitiesInterrupted +=
            HandleCapabilitiesInterrupted;
    }

    private void UnsubscribeGameplayState()
    {
        if (gameplayState == null)
            return;

        gameplayState.OnCapabilitiesInterrupted -=
            HandleCapabilitiesInterrupted;
    }

    private void HandleCapabilitiesInterrupted(
        PlayerGameplayCapability interruptedCapabilities)
    {
        if ((interruptedCapabilities &
             PlayerGameplayCapability.Inventory) == 0)
        {
            return;
        }

        SetInventoryOpen(false);
    }

    public void BindInteractionController(
        InventoryInteractionController
        newInteractionController)
    {
        interactionController =
            newInteractionController;

        if (!isOpen &&
            interactionController != null)
        {
            interactionController
                .CancelLoadoutAssignment();
        }
    }
}