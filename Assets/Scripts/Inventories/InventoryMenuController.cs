using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup inventoryCanvasGroup;
    [SerializeField] private Behaviour[] componentsDisabledWhileOpen;

    [Header("Settings")]
    [SerializeField] private bool startsOpen = false;

    private PlayerInputActions playerInput;
    private bool isOpen;

    public static bool IsInventoryOpen { get; private set; }

    private void Awake()
    {
        if (inventoryCanvasGroup == null)
            inventoryCanvasGroup = GetComponent<CanvasGroup>();

        playerInput = new PlayerInputActions();
    }

    private void OnEnable()
    {
        playerInput.Player.Inventory.started += ToggleInventory;
        playerInput.Player.Enable();
    }

    private void OnDisable()
    {
        playerInput.Player.Inventory.started -= ToggleInventory;
        playerInput.Player.Disable();
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

    private void SetInventoryOpen(bool open)
    {
        isOpen = open;
        IsInventoryOpen = open;

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
}