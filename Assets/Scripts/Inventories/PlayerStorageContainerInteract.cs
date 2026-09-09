using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public enum PlayerInteractionType
{
    None,
    PickUpItem,
    OpenContainer,
    CloseContainer
}

[RequireComponent(
    typeof(PlayerInputRouter),
    typeof(InventoryInteractionController),
    typeof(PlayerGameplayState)
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

    private PlayerGameplayState gameplayState;

    private InventoryInteractionController
        interactionController;

    private InventoryContainer currentOpenContainer;

    public bool HasOpenContainer =>
        currentOpenContainer != null;

    public InventoryContainer CurrentOpenContainer =>
        currentOpenContainer;

    private readonly List<WorldItemInteractionOption>
    worldItemOptions =
        new List<WorldItemInteractionOption>();

    private WorldItem worldItemOptionTarget;

    public IReadOnlyList<WorldItemInteractionOption>
        WorldItemOptions =>
            worldItemOptions;

    public int SelectedWorldItemOptionIndex
    {
        get;
        private set;
    } = -1;

    public PlayerInteractionType
    CurrentInteractionType
    {
        get;
        private set;
    }

    public string CurrentInteractionText
    {
        get;
        private set;
    } = "";

    public WorldItem CurrentWorldItem
    {
        get;
        private set;
    }

    public InventoryContainer
        CurrentTargetContainer
    {
        get;
        private set;
    }

    public bool HasInteraction =>
        CurrentInteractionType !=
            PlayerInteractionType.None;

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

        if (interactionController == null)
        {
            interactionController =
                GetComponent<
                    InventoryInteractionController>();
        }

        if (cameraTransform == null &&
            Camera.main != null)
        {
            cameraTransform =
                Camera.main.transform;
        }

        gameplayState =
            GetComponent<PlayerGameplayState>();
    }

    private void OnEnable()
    {
        if (inputRouter == null)
        {
            inputRouter =
                GetComponent<PlayerInputRouter>();
        }

        gameplayState.OnCapabilitiesInterrupted -=
            HandleCapabilitiesInterrupted;

        gameplayState.OnCapabilitiesInterrupted +=
            HandleCapabilitiesInterrupted;

        if (!gameplayState.Allows(
            PlayerGameplayCapability.WorldInteraction))
        {
            if (currentOpenContainer != null)
            {
                CloseContainer();
            }

            RefreshCurrentInteraction();
        }

        if (inputRouter == null)
            return;

        inputRouter.InteractAction.performed -=
            OnInteractPerformed;

        inputRouter.InteractAction.performed +=
            OnInteractPerformed;

        inputRouter.InteractionCycleAction.performed -=
            OnScroll;

        inputRouter.InteractionCycleAction.performed +=
            OnScroll;
    }

    private void OnDisable()
    {
        if (gameplayState != null)
        {
            gameplayState.OnCapabilitiesInterrupted -=
                HandleCapabilitiesInterrupted;
        }

        if (inputRouter == null)
            return;

        inputRouter.InteractAction.performed -=
            OnInteractPerformed;

        inputRouter.InteractionCycleAction.performed -=
            OnScroll;
    }

    private void Update()
    {
        if (currentOpenContainer != null)
        {
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

        RefreshCurrentInteraction();
    }

    private void OnScroll(
        InputAction.CallbackContext context)
    {
        if (CurrentWorldItem == null ||
            worldItemOptions.Count == 0)
        {
            return;
        }

        float scroll =
            context.ReadValue<float>();

        if (Mathf.Approximately(
                scroll,
                0f))
        {
            return;
        }

        CycleWorldItemOption(
            scroll > 0f
                ? -1
                : 1
        );
    }

    private void CycleWorldItemOption(
        int direction)
    {
        if (worldItemOptions.Count == 0)
            return;

        int startIndex =
            SelectedWorldItemOptionIndex;

        if (startIndex < 0)
            startIndex = 0;

        int index =
            startIndex;

        for (int i = 0;
             i < worldItemOptions.Count;
             i++)
        {
            index += direction;

            if (index < 0)
            {
                index =
                    worldItemOptions.Count - 1;
            }
            else if (index >=
                     worldItemOptions.Count)
            {
                index = 0;
            }

            if (!worldItemOptions[index]
                .IsAvailable)
            {
                continue;
            }

            SelectedWorldItemOptionIndex =
                index;

            return;
        }
    }

    private void HandleCapabilitiesInterrupted(
        PlayerGameplayCapability interruptedCapabilities)
    {
        if ((interruptedCapabilities &
             PlayerGameplayCapability.WorldInteraction) == 0)
        {
            return;
        }

        if (currentOpenContainer != null)
        {
            CloseContainer();
        }

        RefreshCurrentInteraction();
    }

    private void RefreshWorldItemOptions(
        WorldItem worldItem)
    {
        bool sameTarget =
            ReferenceEquals(
                worldItemOptionTarget,
                worldItem
            );

        bool hadSelection =
            sameTarget &&
            SelectedWorldItemOptionIndex >= 0 &&
            SelectedWorldItemOptionIndex <
                worldItemOptions.Count;

        WorldItemInteractionAction
            previousAction =
                hadSelection
                    ? worldItemOptions[
                        SelectedWorldItemOptionIndex]
                        .Action
                    : default;

        worldItemOptionTarget =
            worldItem;

        worldItemOptions.Clear();
        SelectedWorldItemOptionIndex = -1;

        if (worldItem == null ||
            interactionController == null)
        {
            return;
        }

        bool canStore =
            interactionController
                .CanStoreWorldItem(
                    worldItem,
                    out string storeReason
                );

        bool canHold =
            interactionController
                .CanHoldWorldItem(
                    worldItem,
                    out string holdReason
                );

        worldItemOptions.Add(
            new WorldItemInteractionOption(
                WorldItemInteractionAction.Store,
                "Store",
                canStore,
                storeReason
            )
        );

        worldItemOptions.Add(
            new WorldItemInteractionOption(
                WorldItemInteractionAction.Hold,
                "Hold",
                canHold,
                holdReason
            )
        );

        worldItemOptions.Add(
            new WorldItemInteractionOption(
                WorldItemInteractionAction.Backpack,
                "Backpack",
                true
            )
        );

        if (hadSelection)
        {
            for (int i = 0;
                 i < worldItemOptions.Count;
                 i++)
            {
                if (worldItemOptions[i].Action ==
                        previousAction &&
                    worldItemOptions[i].IsAvailable)
                {
                    SelectedWorldItemOptionIndex = i;
                    return;
                }
            }
        }

        for (int i = 0;
             i < worldItemOptions.Count;
             i++)
        {
            if (!worldItemOptions[i].IsAvailable)
                continue;

            SelectedWorldItemOptionIndex = i;
            return;
        }
    }

    private void OnInteractPerformed(
        InputAction.CallbackContext context)
    {
        if (!TryResolveInteraction(
                out PlayerInteractionType
                    interactionType,
                out WorldItem worldItem,
                out InventoryContainer container))
        {
            return;
        }

        switch (interactionType)
        {
            case PlayerInteractionType
                .CloseContainer:

                CloseContainer();
                break;

            case PlayerInteractionType
                .PickUpItem:

                ExecuteSelectedWorldItemOption(
                    worldItem
                );

                break;

            case PlayerInteractionType
                .OpenContainer:

                if (container != null)
                {
                    OpenContainer(
                        container
                    );
                }

                break;
        }

        RefreshCurrentInteraction();
    }

    private void ExecuteSelectedWorldItemOption(
        WorldItem worldItem)
    {
        if (worldItem == null ||
            interactionController == null)
        {
            return;
        }

        RefreshWorldItemOptions(
            worldItem
        );

        if (SelectedWorldItemOptionIndex < 0 ||
            SelectedWorldItemOptionIndex >=
                worldItemOptions.Count)
        {
            return;
        }

        WorldItemInteractionOption option =
            worldItemOptions[
                SelectedWorldItemOptionIndex];

        if (!option.IsAvailable)
            return;

        switch (option.Action)
        {
            case WorldItemInteractionAction.Store:

                interactionController
                    .TryStoreWorldItem(
                        worldItem
                    );

                break;

            case WorldItemInteractionAction.Hold:

                interactionController
                    .TryHoldWorldItem(
                        worldItem
                    );

                break;

            case WorldItemInteractionAction.Backpack:

                if (inventoryMenuController != null)
                {
                    inventoryMenuController
                        .SetInventoryOpen(true);
                }

                break;
        }
    }

    private bool TryResolveInteraction(
        out PlayerInteractionType interactionType,
        out WorldItem worldItem,
        out InventoryContainer container)
    {
        interactionType =
            PlayerInteractionType.None;

        worldItem = null;
        container = null;

        if (gameplayState != null &&
            !gameplayState.Allows(
                PlayerGameplayCapability.WorldInteraction))
        {
            return false;
        }

        if (currentOpenContainer != null)
        {
            interactionType =
                PlayerInteractionType
                    .CloseContainer;

            container =
                currentOpenContainer;

            return true;
        }

        if (useLookTargeting)
        {
            worldItem =
                FindLookedAtWorldItem();

            if (worldItem != null)
            {
                interactionType =
                    PlayerInteractionType
                        .PickUpItem;

                return true;
            }

            container =
                FindLookedAtContainer();

            if (container != null)
            {
                interactionType =
                    PlayerInteractionType
                        .OpenContainer;

                return true;
            }
        }

        worldItem =
            FindNearestWorldItem();

        if (worldItem != null)
        {
            interactionType =
                PlayerInteractionType
                    .PickUpItem;

            return true;
        }

        container =
            FindNearestContainer();

        if (container != null)
        {
            interactionType =
                PlayerInteractionType
                    .OpenContainer;

            return true;
        }

        return false;
    }

    private void RefreshCurrentInteraction()
    {
        if (!TryResolveInteraction(
                out PlayerInteractionType
                    interactionType,
                out WorldItem worldItem,
                out InventoryContainer container))
        {
            CurrentInteractionType =
                PlayerInteractionType.None;

            CurrentWorldItem = null;
            CurrentTargetContainer = null;
            CurrentInteractionText = "";

            RefreshWorldItemOptions(
                null
            );

            return;
        }

        CurrentInteractionType =
            interactionType;

        CurrentWorldItem =
            worldItem;

        RefreshWorldItemOptions(
            worldItem
        );

        CurrentTargetContainer =
            container;

        CurrentInteractionText =
            BuildInteractionText(
                interactionType,
                worldItem,
                container
            );
    }

    private string BuildInteractionText(
        PlayerInteractionType interactionType,
        WorldItem worldItem,
        InventoryContainer container)
    {
        switch (interactionType)
        {
            case PlayerInteractionType
                .PickUpItem:

                if (worldItem != null &&
                    worldItem.Item != null &&
                    worldItem.Item.Definition != null &&
                    !string.IsNullOrWhiteSpace(
                        worldItem.Item.Definition
                            .itemName))
                {
                    return
                        "Pick Up " +
                        worldItem.Item.Definition
                            .itemName;
                }

                return "Pick Up Item";

            case PlayerInteractionType
                .OpenContainer:

                return
                    "Open " +
                    GetContainerDisplayName(
                        container
                    );

            case PlayerInteractionType
                .CloseContainer:

                return
                    "Close " +
                    GetContainerDisplayName(
                        container
                    );

            default:
                return "";
        }
    }

    private string GetContainerDisplayName(
        InventoryContainer container)
    {
        if (container == null)
            return "Storage";

        StorageContainerInteract interact =
            container.GetComponent<
                StorageContainerInteract>();

        if (interact != null)
            return interact.DisplayName;

        return container.gameObject.name;
    }

    private WorldItem
        FindLookedAtWorldItem()
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

        return hit.collider
            .GetComponentInParent<
                WorldItem>();
    }

    private WorldItem
        FindNearestWorldItem()
    {
        Collider[] hits =
            Physics.OverlapSphere(
                transform.position,
                interactRange,
                containerLayerMask,
                QueryTriggerInteraction.Collide
            );

        WorldItem nearest =
            null;

        float nearestDistance =
            float.MaxValue;

        for (int i = 0;
             i < hits.Length;
             i++)
        {
            WorldItem worldItem =
                hits[i].GetComponentInParent<
                    WorldItem>();

            if (worldItem == null ||
                worldItem.Item == null ||
                worldItem.Item.IsEmpty)
            {
                continue;
            }

            float distance =
                Vector3.Distance(
                    transform.position,
                    worldItem.transform.position
                );

            if (distance >=
                nearestDistance)
            {
                continue;
            }

            nearestDistance =
                distance;

            nearest =
                worldItem;
        }

        return nearest;
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