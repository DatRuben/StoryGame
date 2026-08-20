using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public sealed class CharacterRuntimeBinder :
    MonoBehaviour
{
    [Header("Runtime Inventory UI")]
    [SerializeField]
    private InventoryGridUI inventoryGridUI;

    [SerializeField]
    private InventoryGridUI storageInventoryGridUI;

    [SerializeField]
    private InventoryContextPanelController
        contextPanelController;

    [SerializeField]
    private InventoryMenuController
        inventoryMenuController;

    [SerializeField]
    private InventoryFollow inventoryFollow;

    [SerializeField]
    private HeldItemUI heldItemUI;

    [SerializeField]
    private GameObject storageContainerPanel;

    [Header("Runtime World")]
    [SerializeField]
    private WorldItemSpawner worldItemSpawner;

    [Header("Runtime Equipment UI")]
    [SerializeField]
    private Transform inventorySlotUIRoot;

    [SerializeField]
    private HeldItemClosedPreviewUI[]
        closedPreviewUIs =
            new HeldItemClosedPreviewUI[0];

    [Header("Runtime Player UI")]
    [SerializeField]
    private PlayerResourcesUI playerResourcesUI;

    [SerializeField]
    private PlayerCrosshair playerCrosshair;

    [SerializeField]
    private TextMeshProUGUI speedText;

    [Header("Runtime Camera")]
    [SerializeField]
    private CinemachineCamera cinemachineCamera;

    [SerializeField]
    private CameraCollision cameraCollision;

    [SerializeField]
    private Transform cameraTargetOverride;

    [Header("Player Child Names")]
    [SerializeField]
    private string cameraPivotName =
        "CameraPivot";

    [SerializeField]
    private string aimTargetName =
        "AimTarget";

    public void Bind(
        GameObject player)
    {
        if (player == null)
        {
            Debug.LogWarning(
                "CharacterRuntimeBinder cannot bind because player is missing.",
                this
            );

            return;
        }

        PlayerInput playerInput =
            player.GetComponent<PlayerInput>();

        PlayerInputRouter inputRouter =
            player.GetComponent<
                PlayerInputRouter>();

        InventoryContainer playerInventory =
            player.GetComponent<
                InventoryContainer>();

        InventoryInteractionController
            interactionController =
                player.GetComponent<
                    InventoryInteractionController>();

        if (interactionController != null)
        {
            interactionController
                .BindWorldItemSpawner(
                    worldItemSpawner
                );
        }

        PlayerGripState gripState =
            player.GetComponent<
                PlayerGripState>();

        PlayerWeaponLoadout weaponLoadout =
            player.GetComponent<
                PlayerWeaponLoadout>();

        PlayerEquipment playerEquipment =
            player.GetComponent<
                PlayerEquipment>();

        PlayerCharacterProfile
            characterProfile =
                player.GetComponent<
                    PlayerCharacterProfile>();

        PlayerStorageContainerInteract
            storageInteract =
                player.GetComponent<
                    PlayerStorageContainerInteract>();

        EntityResources playerResources =
            player.GetComponent<EntityResources>();

        Camera mainCamera =
            Camera.main;

        if (playerInput != null)
        {
            playerInput
                .SetRuntimeCameraReferences(
                    mainCamera,
                    mainCamera != null
                        ? mainCamera.transform
                        : null,
                    speedText
                );
        }

        Transform cameraTarget =
            cameraTargetOverride != null
                ? cameraTargetOverride
                : FindChildRecursive(
                    player.transform,
                    cameraPivotName
                );

        Transform aimTarget =
            FindChildRecursive(
                player.transform,
                aimTargetName
            );

        if (cameraTarget == null)
        {
            cameraTarget =
                player.transform;
        }

        if (cinemachineCamera != null)
        {
            cinemachineCamera.Follow =
                cameraTarget;
        }

        if (cameraCollision == null &&
            mainCamera != null)
        {
            cameraCollision =
                mainCamera.GetComponent<
                    CameraCollision>();
        }

        if (cameraCollision != null)
        {
            cameraCollision
                .SetCameraPivot(
                    cameraTarget
                );
        }

        if (inventoryGridUI != null)
        {
            inventoryGridUI.BindPlayer(
                playerInventory,
                interactionController,
                true
            );
        }

        if (storageInventoryGridUI != null)
        {
            storageInventoryGridUI.BindPlayer(
                null,
                interactionController,
                false
            );
        }

        if (storageContainerPanel == null &&
            storageInventoryGridUI != null)
        {
            storageContainerPanel =
                storageInventoryGridUI
                    .gameObject;
        }

        if (storageInteract != null)
        {
            storageInteract
                .BindSceneReferences(
                    mainCamera != null
                        ? mainCamera.transform
                        : null,
                    inventoryGridUI,
                    storageInventoryGridUI,
                    storageContainerPanel,
                    contextPanelController,
                    inventoryMenuController
                );
        }

        if (inventoryMenuController != null)
        {
            inventoryMenuController.BindInput(
                inputRouter
            );

            inventoryMenuController
                .BindPlayerStorageInteract(
                    storageInteract
                );

            inventoryMenuController
                .BindInteractionController(
                    interactionController
                );
        }

        if (inventoryFollow != null)
        {
            inventoryFollow.BindPlayer(
                player.transform,
                mainCamera,
                storageInteract
            );
        }

        if (heldItemUI != null)
        {
            heldItemUI.BindPlayer(
                gripState,
                characterProfile
            );
        }

        if (closedPreviewUIs != null)
        {
            for (int i = 0;
                 i < closedPreviewUIs.Length;
                 i++)
            {
                HeldItemClosedPreviewUI preview =
                    closedPreviewUIs[i];

                if (preview == null)
                    continue;

                preview.BindPlayer(
                    interactionController
                );
            }
        }

        Transform slotRoot =
            inventorySlotUIRoot != null
                ? inventorySlotUIRoot
                : inventoryMenuController != null
                    ? inventoryMenuController
                        .transform
                    : null;

        BindSlotUIs(
            slotRoot,
            weaponLoadout,
            playerEquipment,
            interactionController
        );

        if (playerResourcesUI != null)
        {
            playerResourcesUI.BindPlayer(
                playerResources
            );
        }

        if (playerCrosshair != null)
        {
            playerCrosshair.BindPlayer(
                playerInput,
                player.transform,
                mainCamera,
                aimTarget
            );
        }

        Debug.Log(
            "Bound runtime systems to spawned player: " +
            player.name,
            this
        );
    }

    private void BindSlotUIs(
        Transform root,
        PlayerWeaponLoadout weaponLoadout,
        PlayerEquipment playerEquipment,
        InventoryInteractionController
            interactionController)
    {
        if (root == null)
            return;

        WeaponSetSlotUI[] weaponSlots =
            root.GetComponentsInChildren<
                WeaponSetSlotUI>(
                true
            );

        for (int i = 0;
             i < weaponSlots.Length;
             i++)
        {
            if (weaponSlots[i] == null)
                continue;

            weaponSlots[i].BindPlayer(
                weaponLoadout,
                interactionController
            );
        }

        EquipmentSlotUI[] equipmentSlots =
            root.GetComponentsInChildren<
                EquipmentSlotUI>(
                true
            );

        for (int i = 0;
             i < equipmentSlots.Length;
             i++)
        {
            if (equipmentSlots[i] == null)
                continue;

            equipmentSlots[i].BindPlayer(
                playerEquipment,
                interactionController
            );
        }
    }

    private Transform FindChildRecursive(
        Transform parent,
        string childName)
    {
        if (parent == null ||
            string.IsNullOrWhiteSpace(
                childName))
        {
            return null;
        }

        for (int i = 0;
             i < parent.childCount;
             i++)
        {
            Transform child =
                parent.GetChild(i);

            if (child.name ==
                childName)
            {
                return child;
            }

            Transform match =
                FindChildRecursive(
                    child,
                    childName
                );

            if (match != null)
                return match;
        }

        return null;
    }
}