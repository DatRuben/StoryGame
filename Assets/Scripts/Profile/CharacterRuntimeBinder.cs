using TMPro;
using UnityEngine;
using Unity.Cinemachine;

public class CharacterRuntimeBinder : MonoBehaviour
{
    [Header("Runtime Inventory UI")]
    [SerializeField] private InventoryGridUI inventoryGridUI;
    [SerializeField] private StorageContainerGridUI storageContainerGridUI;
    [SerializeField] private InventoryContextPanelController contextPanelController;
    [SerializeField] private InventoryMenuController inventoryMenuController;
    [SerializeField] private InventoryFollow inventoryFollow;
    [SerializeField] private HeldItemUI heldItemUI;
    [SerializeField] private GameObject storageContainerPanel;

    [Header("Runtime Equipment UI")]
    [SerializeField]
    private WeaponSetSlotUI[] weaponSetSlotUIs =
        new WeaponSetSlotUI[0];

    [SerializeField]
    private EquipmentSlotUI[] equipmentSlotUIs =
        new EquipmentSlotUI[0];

    [SerializeField]
    private HeldItemClosedPreviewUI[] closedPreviewUIs =
        new HeldItemClosedPreviewUI[0];

    [Header("Runtime Player UI")]
    [SerializeField] private PlayerResourcesUI playerResourcesUI;
    [SerializeField] private PlayerCrosshair playerCrosshair;
    [SerializeField] private TextMeshProUGUI speedText;

    [Header("Runtime Camera")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private CameraCollision cameraCollision;
    [SerializeField] private Transform cameraTargetOverride;

    [Header("Player Child Names")]
    [SerializeField] private string cameraPivotName = "CameraPivot";
    [SerializeField] private string aimTargetName = "AimTarget";

    public void Bind(GameObject player)
    {
        if (player == null)
        {
            Debug.LogWarning(
                "CharacterRuntimeBinder could not bind systems because no player exists.",
                this
            );

            return;
        }

        PlayerInput playerInput =
            player.GetComponent<PlayerInput>();

        PlayerInputRouter inputRouter =
            player.GetComponent<PlayerInputRouter>();

        PlayerInventory playerInventory =
            player.GetComponent<PlayerInventory>();

        PlayerStorageContainerInteract storageInteract =
            player.GetComponent<PlayerStorageContainerInteract>();

        PlayerHolding playerHolding =
            player.GetComponent<PlayerHolding>();

        PlayerWeaponSlots playerWeaponSlots =
            player.GetComponent<PlayerWeaponSlots>();

        PlayerEquipment playerEquipment =
            player.GetComponent<PlayerEquipment>();

        PlayerResources playerResources =
            player.GetComponent<PlayerResources>();

        Camera mainCamera =
            Camera.main;

        if (playerInput != null)
        {
            playerInput.SetRuntimeCameraReferences(
                mainCamera,
                mainCamera != null ? mainCamera.transform : null,
                speedText
            );
        }

        if (inputRouter == null)
        {
            Debug.LogError(
                "CharacterRuntimeBinder could not find PlayerInputRouter on the runtime player.",
                player
            );
        }

        Transform cameraTarget =
            cameraTargetOverride != null
                ? cameraTargetOverride
                : FindChildRecursive(player.transform, cameraPivotName);

        Transform aimTarget =
            FindChildRecursive(
                player.transform,
                aimTargetName
            );

        if (cameraTarget == null)
        {
            Debug.LogWarning(
                $"CharacterRuntimeBinder could not find camera pivot named '{cameraPivotName}'. Falling back to player root.",
                this
            );

            cameraTarget = player.transform;
        }

        if (cinemachineCamera != null)
        {
            cinemachineCamera.Follow = cameraTarget;

            Debug.Log(
                $"Camera bound to target: {cameraTarget.name}",
                cameraTarget
            );
        }
        else
        {
            Debug.LogWarning(
                "CharacterRuntimeBinder could not find a CinemachineCamera to bind.",
                this
            );
        }

        if (cameraCollision == null &&
            mainCamera != null)
        {
            cameraCollision = mainCamera.GetComponent<CameraCollision>();
        }

        if (cameraCollision != null)
        {
            cameraCollision.SetCameraPivot(cameraTarget);
        }
        else
        {
            Debug.LogWarning(
                "CharacterRuntimeBinder could not find CameraCollision to bind.",
                this
            );
        }

        if (storageContainerPanel == null &&
            storageContainerGridUI != null)
        {
            storageContainerPanel = storageContainerGridUI.gameObject;
        }

        if (inventoryFollow != null)
        {
            inventoryFollow.BindPlayer(
                player.transform,
                mainCamera,
                storageInteract
            );
        }
        else
        {
            Debug.LogWarning(
                "CharacterRuntimeBinder could not find InventoryFollow to bind.",
                this
            );
        }

        if (playerCrosshair != null)
        {
            if (aimTarget == null)
            {
                Debug.LogWarning(
                    $"CharacterRuntimeBinder could not find aim target named '{aimTargetName}'.",
                    this
                );
            }

            playerCrosshair.BindPlayer(
                playerInput,
                player.transform,
                mainCamera,
                aimTarget
            );
        }
        else
        {
            Debug.LogWarning(
                "CharacterRuntimeBinder could not find PlayerCrosshair to bind.",
                this
            );
        }

        if (storageInteract != null)
        {
            storageInteract.BindSceneReferences(
                mainCamera != null ? mainCamera.transform : null,
                storageContainerGridUI,
                storageContainerPanel,
                contextPanelController,
                inventoryMenuController
            );
        }

        if (inventoryMenuController != null)
        {
            inventoryMenuController.BindPlayerStorageInteract(
                storageInteract
            );
        }

        if (inventoryGridUI != null)
        {
            inventoryGridUI.BindInput(
                inputRouter
            );

            inventoryGridUI.BindPlayer(
                playerInventory,
                storageInteract
            );
        }

        if (storageContainerGridUI != null)
        {
            storageContainerGridUI.BindPlayer(
                playerInventory,
                inventoryGridUI
            );
        }

        if (heldItemUI != null)
        {
            heldItemUI.BindPlayer(
                playerInventory,
                playerHolding,
                playerWeaponSlots
            );
        }

        if (weaponSetSlotUIs != null)
        {
            for (int i = 0;
                 i < weaponSetSlotUIs.Length;
                 i++)
            {
                WeaponSetSlotUI weaponSetSlotUI =
                    weaponSetSlotUIs[i];

                if (weaponSetSlotUI == null)
                    continue;

                weaponSetSlotUI.BindPlayer(
                    playerInventory,
                    playerWeaponSlots
                );
            }
        }

        if (equipmentSlotUIs != null)
        {
            for (int i = 0;
                 i < equipmentSlotUIs.Length;
                 i++)
            {
                EquipmentSlotUI equipmentSlotUI =
                    equipmentSlotUIs[i];

                if (equipmentSlotUI == null)
                    continue;

                equipmentSlotUI.BindPlayer(
                    playerInventory,
                    playerEquipment
                );
            }
        }

        if (closedPreviewUIs != null)
        {
            for (int i = 0;
                 i < closedPreviewUIs.Length;
                 i++)
            {
                HeldItemClosedPreviewUI closedPreviewUI =
                    closedPreviewUIs[i];

                if (closedPreviewUI == null)
                    continue;

                closedPreviewUI.BindPlayer(
                    playerInventory
                );
            }
        }

        if (playerResourcesUI != null)
            playerResourcesUI.BindPlayer(playerResources);

        Debug.Log(
            $"Bound runtime systems to spawned player: {player.name}",
            this
        );
    }

    private Transform FindChildRecursive(
        Transform parent,
        string childName)
    {
        if (parent == null ||
            string.IsNullOrWhiteSpace(childName))
        {
            return null;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child =
                parent.GetChild(i);

            if (child.name == childName)
                return child;

            Transform match =
                FindChildRecursive(child, childName);

            if (match != null)
                return match;
        }

        return null;
    }
}