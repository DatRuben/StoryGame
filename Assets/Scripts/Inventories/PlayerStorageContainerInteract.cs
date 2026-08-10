using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(
    typeof(PlayerInputRouter)
)]
public sealed class PlayerStorageContainerInteract :
    MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField]
    private float interactRange = 3f;

    [SerializeField]
    private float lookInteractRange = 5f;

    [SerializeField]
    private float autoCloseRange = 4f;

    [SerializeField]
    private LayerMask containerLayerMask = ~0;

    [Header("Camera")]
    [SerializeField]
    private Transform cameraTransform;

    [SerializeField]
    private bool useLookTargeting = true;

    [Header("UI")]
    [SerializeField]
    private InventoryGridUI playerInventoryGridUI;

    [SerializeField]
    private InventoryGridUI containerGridUI;

    [SerializeField]
    private GameObject containerPanel;

    [SerializeField]
    private InventoryContextPanelController
        contextPanelController;

    [SerializeField]
    private InventoryMenuController
        inventoryMenuController;

    [SerializeField]
    private PlayerInputRouter inputRouter;

    private InventoryContainer currentOpenContainer;

    public bool HasOpenContainer =>
        currentOpenContainer != null;

    public InventoryContainer CurrentOpenContainer =>
        currentOpenContainer;

    private void OnValidate()
    {
        interactRange =
            Mathf.Max(
                0f,
                interactRange
            );

        lookInteractRange =
            Mathf.Max(
                0f,
                lookInteractRange
            );

        autoCloseRange =
            Mathf.Max(
                0f,
                autoCloseRange
            );
    }

    private void Awake()
    {
        if (inputRouter == null)
        {
            inputRouter =
                GetComponent<PlayerInputRouter>();
        }

        if (cameraTransform == null &&
            Camera.main != null)
        {
            cameraTransform =
                Camera.main.transform;
        }
    }

    private void OnEnable()
    {
        if (inputRouter == null)
        {
            inputRouter =
                GetComponent<PlayerInputRouter>();
        }

        if (inputRouter == null)
            return;

        inputRouter.InteractAction.performed -=
            OnInteractPerformed;

        inputRouter.InteractAction.performed +=
            OnInteractPerformed;
    }

    private void OnDisable()
    {
        if (inputRouter != null)
        {
            inputRouter.InteractAction.performed -=
                OnInteractPerformed;
        }
    }

    private void Update()
    {
        if (currentOpenContainer == null)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                currentOpenContainer
                    .transform.position
            );

        if (distance >
            autoCloseRange)
        {
            CloseContainer();
        }
    }

    private void OnInteractPerformed(
        InputAction.CallbackContext context)
    {
        if (currentOpenContainer != null)
        {
            CloseContainer();
            return;
        }

        InventoryContainer target =
            useLookTargeting
                ? FindLookedAtContainer()
                : null;

        if (target == null)
        {
            target =
                FindNearestContainer();
        }

        if (target != null)
        {
            OpenContainer(
                target
            );
        }
    }

    private InventoryContainer
        FindLookedAtContainer()
    {
        if (cameraTransform == null)
            return null;

        Ray ray =
            new Ray(
                cameraTransform.position,
                cameraTransform.forward
            );

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            lookInteractRange,
            containerLayerMask,
            QueryTriggerInteraction.Collide))
        {
            return null;
        }

        StorageContainerInteract interact =
            hit.collider.GetComponentInParent<
                StorageContainerInteract>();

        return interact != null
            ? interact.Container
            : null;
    }

    private InventoryContainer
        FindNearestContainer()
    {
        Collider[] hits =
            Physics.OverlapSphere(
                transform.position,
                interactRange,
                containerLayerMask,
                QueryTriggerInteraction.Collide
            );

        InventoryContainer nearest =
            null;

        float nearestDistance =
            float.MaxValue;

        for (int i = 0;
             i < hits.Length;
             i++)
        {
            StorageContainerInteract interact =
                hits[i].GetComponentInParent<
                    StorageContainerInteract>();

            if (interact == null ||
                interact.Container == null)
            {
                continue;
            }

            float distance =
                Vector3.Distance(
                    transform.position,
                    interact.transform.position
                );

            if (distance >=
                nearestDistance)
            {
                continue;
            }

            nearestDistance =
                distance;

            nearest =
                interact.Container;
        }

        return nearest;
    }

    private void OpenContainer(
        InventoryContainer storageContainer)
    {
        if (storageContainer == null)
            return;

        currentOpenContainer =
            storageContainer;

        if (containerGridUI != null)
        {
            containerGridUI.BindContainer(
                storageContainer
            );
        }

        LinkInventoryGrids();

        if (inventoryMenuController != null)
        {
            inventoryMenuController
                .SetInventoryOpen(true);
        }

        if (contextPanelController != null)
        {
            contextPanelController
                .ShowStorageContainer(
                    storageContainer
                );
        }
        else if (containerPanel != null)
        {
            containerPanel.SetActive(true);
        }
    }

    public void CloseOpenContainer()
    {
        CloseContainer();
    }

    private void CloseContainer()
    {
        currentOpenContainer =
            null;

        UnlinkInventoryGrids();

        if (containerGridUI != null)
        {
            containerGridUI.BindContainer(
                null
            );
        }

        if (contextPanelController != null)
        {
            contextPanelController
                .HideStorageContainer();
        }
        else if (containerPanel != null)
        {
            containerPanel.SetActive(false);
        }
    }

    private void LinkInventoryGrids()
    {
        if (playerInventoryGridUI != null)
        {
            playerInventoryGridUI
                .SetQuickTransferTarget(
                    containerGridUI
                );
        }

        if (containerGridUI != null)
        {
            containerGridUI
                .SetQuickTransferTarget(
                    playerInventoryGridUI
                );
        }
    }

    private void UnlinkInventoryGrids()
    {
        if (playerInventoryGridUI != null)
        {
            playerInventoryGridUI
                .SetQuickTransferTarget(
                    null
                );
        }

        if (containerGridUI != null)
        {
            containerGridUI
                .SetQuickTransferTarget(
                    null
                );
        }
    }

    public void BindSceneReferences(
        Transform newCameraTransform,
        InventoryGridUI newPlayerInventoryGridUI,
        InventoryGridUI newContainerGridUI,
        GameObject newContainerPanel,
        InventoryContextPanelController
            newContextPanelController,
        InventoryMenuController
            newInventoryMenuController)
    {
        cameraTransform =
            newCameraTransform;

        playerInventoryGridUI =
            newPlayerInventoryGridUI;

        containerGridUI =
            newContainerGridUI;

        containerPanel =
            newContainerPanel;

        contextPanelController =
            newContextPanelController;

        inventoryMenuController =
            newInventoryMenuController;
    }
}