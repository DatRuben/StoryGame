using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStorageContainerInteract : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private float lookInteractRange = 5f;
    [SerializeField] private float autoCloseRange = 4f;
    [SerializeField] private LayerMask containerLayerMask = ~0;

    [Header("Camera Look Targeting")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private bool useLookTargeting = true;

    [Header("UI References")]
    [SerializeField] private StorageContainerGridUI containerGridUI;
    [SerializeField] private GameObject containerPanel;

    [SerializeField] private InventoryContextPanelController contextPanelController;

    private PlayerInputActions inputActions;
    private StorageContainer currentOpenContainer;

    public bool HasOpenContainer => currentOpenContainer != null;
    public StorageContainer CurrentOpenContainer => currentOpenContainer;

    private void Reset()
    {
    }

    private void OnValidate()
    {
        interactRange = Mathf.Max(0f, interactRange);
        lookInteractRange = Mathf.Max(0f, lookInteractRange);
        autoCloseRange = Mathf.Max(0f, autoCloseRange);
    }

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        ValidateReferences(true, true);
    }

    private void OnEnable()
    {
        if (inputActions == null)
            inputActions = new PlayerInputActions();

        inputActions.Enable();

        inputActions.Player.TestKey1.performed += OnInteractPerformed;
        inputActions.Player.TestKey2.performed += OnClosePerformed;
    }

    private void OnDisable()
    {
        if (inputActions == null)
            return;

        inputActions.Player.TestKey1.performed -= OnInteractPerformed;
        inputActions.Player.TestKey2.performed -= OnClosePerformed;

        inputActions.Disable();
    }

    private void Update()
    {
        if (currentOpenContainer == null)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                currentOpenContainer.transform.position
            );

        if (distance > autoCloseRange)
            CloseContainer();
    }

    private void ValidateReferences(
        bool logAutoFilled,
        bool logMissing)
    {
        if (cameraTransform == null &&
            Camera.main != null)
        {
            cameraTransform = Camera.main.transform;

            if (logAutoFilled)
            {
                Debug.Log(
                    "PlayerStorageContainerInteract auto-filled Camera Transform from Camera.main.",
                    this
                );
            }
        }

        if (contextPanelController == null)
        {
            contextPanelController =
                FindSceneComponent<InventoryContextPanelController>();

            if (contextPanelController != null &&
                logAutoFilled)
            {
                Debug.Log(
                    "PlayerStorageContainerInteract auto-filled InventoryContextPanelController.",
                    this
                );
            }
        }

        if (containerGridUI == null)
        {
            containerGridUI =
                FindSceneComponent<StorageContainerGridUI>();

            if (containerGridUI != null &&
                logAutoFilled)
            {
                Debug.Log(
                    "PlayerStorageContainerInteract auto-filled StorageContainerGridUI.",
                    this
                );
            }
        }

        if (containerPanel == null &&
            containerGridUI != null)
        {
            containerPanel =
                containerGridUI.gameObject;

            if (logAutoFilled)
            {
                Debug.Log(
                    "PlayerStorageContainerInteract auto-filled ContainerPanel from StorageContainerGridUI.",
                    this
                );
            }
        }

        if (!logMissing)
            return;

        if (cameraTransform == null &&
            useLookTargeting)
        {
            Debug.LogWarning(
                "PlayerStorageContainerInteract is missing Camera Transform while look targeting is enabled.",
                this
            );
        }

        if (containerGridUI == null)
        {
            Debug.LogWarning(
                "PlayerStorageContainerInteract is missing Container Grid UI.",
                this
            );
        }

        if (containerPanel == null)
        {
            Debug.LogWarning(
                "PlayerStorageContainerInteract is missing Container Panel.",
                this
            );
        }

        if (contextPanelController == null)
        {
            Debug.LogWarning(
                "PlayerStorageContainerInteract is missing InventoryContextPanelController. It can still use ContainerPanel fallback if assigned.",
                this
            );
        }
    }

    private T FindSceneComponent<T>() where T : Component
    {
        T[] matches =
            Resources.FindObjectsOfTypeAll<T>();

        for (int i = 0; i < matches.Length; i++)
        {
            T match = matches[i];

            if (match == null ||
                !match.gameObject.scene.IsValid())
            {
                continue;
            }

            return match;
        }

        return null;
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        StorageContainer targetContainer = null;

        if (useLookTargeting)
            targetContainer = FindLookedAtContainer();

        if (targetContainer == null)
            targetContainer = FindNearestContainer();

        if (targetContainer == null)
        {
            CloseContainer();
            return;
        }

        OpenContainer(targetContainer);
    }

    private void OnClosePerformed(InputAction.CallbackContext context)
    {
        CloseContainer();
    }

    private StorageContainer FindLookedAtContainer()
    {
        if (cameraTransform == null)
            return null;

        Ray ray =
            new Ray(
                cameraTransform.position,
                cameraTransform.forward
            );

        bool hitSomething =
            Physics.Raycast(
                ray,
                out RaycastHit hit,
                lookInteractRange,
                containerLayerMask,
                QueryTriggerInteraction.Collide
            );

        if (!hitSomething)
            return null;

        StorageContainerInteract interact =
            hit.collider.GetComponentInParent<StorageContainerInteract>();

        if (interact == null)
            return null;

        return interact.StorageContainer;
    }

    private StorageContainer FindNearestContainer()
    {
        Collider[] hits =
            Physics.OverlapSphere(
                transform.position,
                interactRange,
                containerLayerMask,
                QueryTriggerInteraction.Collide
            );

        StorageContainer nearestContainer = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            StorageContainerInteract interact =
                hit.GetComponentInParent<StorageContainerInteract>();

            if (interact == null ||
                interact.StorageContainer == null)
            {
                continue;
            }

            float distance =
                Vector3.Distance(
                    transform.position,
                    interact.transform.position
                );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestContainer = interact.StorageContainer;
            }
        }

        return nearestContainer;
    }

    private void OpenContainer(StorageContainer storageContainer)
    {
        ValidateReferences(false, true);

        if (storageContainer == null ||
            containerGridUI == null ||
            containerPanel == null)
        {
            Debug.LogWarning(
                "PlayerStorageContainerInteract cannot open storage because a reference is missing.",
                this
            );

            return;
        }

        currentOpenContainer = storageContainer;

        if (contextPanelController != null)
        {
            contextPanelController.ShowStorageContainer(storageContainer);
        }
        else
        {
            containerPanel.SetActive(true);
            containerGridUI.SetStorageContainer(storageContainer);
        }
    }

    public bool TryQuickTransferPlayerItemToOpenContainer(
        PlayerInventory playerInventory,
        Vector2Int coordinate)
    {
        if (currentOpenContainer == null ||
            playerInventory == null ||
            playerInventory.Grid == null ||
            playerInventory.IsHoldingItem)
        {
            return false;
        }

        PlacedInventoryItem itemAtCell =
            playerInventory.Grid.GetPlacedItem(
                coordinate.x,
                coordinate.y
            );

        if (itemAtCell == null ||
            itemAtCell.ItemData == null)
        {
            return false;
        }

        Vector2Int originalPosition =
            itemAtCell.Position;

        PlacedInventoryItem pickedItem =
            playerInventory.TryPickUpItemAt(
                coordinate.x,
                coordinate.y,
                false
            );

        if (pickedItem == null ||
            pickedItem.ItemData == null)
        {
            return false;
        }

        currentOpenContainer.TryAddItem(
            pickedItem.ItemData,
            pickedItem.RotationSteps,
            pickedItem.Quantity,
            out int remainingQuantity
        );

        if (remainingQuantity <= 0)
        {
            playerInventory.ClearHeldItemAfterExternalMove();
            return true;
        }

        playerInventory.SetHeldItemQuantityAfterExternalMove(
            remainingQuantity
        );

        bool returned =
            playerInventory.TryPlaceHeldItem(
                originalPosition.x,
                originalPosition.y
            );

        if (!returned)
        {
            Debug.LogWarning(
                "Could not return remaining quick-transfer item quantity to the player inventory.",
                this
            );
        }

        return true;
    }

    public bool TryPlaceHeldPlayerItemInOpenContainerAtScreenPosition(
        Vector2 screenPosition)
    {
        if (currentOpenContainer == null ||
            containerGridUI == null)
        {
            return false;
        }

        return containerGridUI.TryPlaceHeldItemAtScreenPosition(
            screenPosition
        );
    }

    private void CloseContainer()
    {
        currentOpenContainer = null;

        if (contextPanelController != null)
        {
            contextPanelController.HideStorageContainer();
        }
        else
        {
            if (containerPanel != null)
                containerPanel.SetActive(false);

            if (containerGridUI != null)
                containerGridUI.SetStorageContainer(null);
        }
    }
}
