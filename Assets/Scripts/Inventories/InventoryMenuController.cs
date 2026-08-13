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
    }

    private void OnDisable()
    {
        UnsubscribeInput();
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