using TMPro;
using UnityEngine;
using Unity.Cinemachine;

public class CharacterRuntimeBinder : MonoBehaviour
{
    [Header("Optional Scene References")]
    [SerializeField] private InventoryGridUI inventoryGridUI;
    [SerializeField] private StorageContainerGridUI storageContainerGridUI;
    [SerializeField] private HeldItemUI heldItemUI;
    [SerializeField] private PlayerResourcesUI playerResourcesUI;
    [SerializeField] private InventoryMenuController inventoryMenuController;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private CameraCollision cameraCollision;
    [SerializeField] private Transform cameraTargetOverride;
    [SerializeField] private string cameraPivotName = "CameraPivot";
    [SerializeField] private InventoryContextPanelController contextPanelController;
    [SerializeField] private GameObject storageContainerPanel;
    [SerializeField] private InventoryFollow inventoryFollow;
    [SerializeField] private PlayerCrosshair playerCrosshair;
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

        if (cinemachineCamera == null)
            cinemachineCamera = FindSceneComponent<CinemachineCamera>();

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

        if (cameraCollision == null)
            cameraCollision = FindSceneComponent<CameraCollision>();

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

        if (inventoryGridUI == null)
            inventoryGridUI = FindSceneComponent<InventoryGridUI>();

        if (storageContainerGridUI == null)
            storageContainerGridUI = FindSceneComponent<StorageContainerGridUI>();

        if (contextPanelController == null)
            contextPanelController = FindSceneComponent<InventoryContextPanelController>();

        if (storageContainerPanel == null &&
            storageContainerGridUI != null)
        {
            storageContainerPanel = storageContainerGridUI.gameObject;
        }

        if (heldItemUI == null)
            heldItemUI = FindSceneComponent<HeldItemUI>();

        if (playerResourcesUI == null)
            playerResourcesUI = FindSceneComponent<PlayerResourcesUI>();

        if (inventoryMenuController == null)
            inventoryMenuController = FindSceneComponent<InventoryMenuController>();

        if (inventoryFollow != null)
        {
            inventoryFollow.BindPlayer(
                player.transform,
                mainCamera,
                storageInteract
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

        if (inventoryFollow == null)
            inventoryFollow = FindSceneComponent<InventoryFollow>();

        if (playerCrosshair == null)
            playerCrosshair = FindSceneComponent<PlayerCrosshair>();

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

        WeaponSetSlotUI[] weaponSetSlotUIs =
            Resources.FindObjectsOfTypeAll<
                WeaponSetSlotUI>();

        for (int i = 0;
             i < weaponSetSlotUIs.Length;
             i++)
        {
            WeaponSetSlotUI weaponSetSlotUI =
                weaponSetSlotUIs[i];

            if (weaponSetSlotUI == null ||
                !weaponSetSlotUI.gameObject.scene.IsValid())
            {
                continue;
            }

            weaponSetSlotUI.BindPlayer(
                playerInventory,
                playerWeaponSlots
            );
        }

        EquipmentSlotUI[] equipmentSlotUIs =
            Resources.FindObjectsOfTypeAll<
                EquipmentSlotUI>();

        for (int i = 0;
             i < equipmentSlotUIs.Length;
             i++)
        {
            EquipmentSlotUI equipmentSlotUI =
                equipmentSlotUIs[i];

            if (equipmentSlotUI == null ||
                !equipmentSlotUI.gameObject.scene.IsValid())
            {
                continue;
            }

            equipmentSlotUI.BindPlayer(
                playerInventory,
                playerEquipment
            );
        }

        HeldItemClosedPreviewUI[] closedPreviewUIs =
            Resources.FindObjectsOfTypeAll<
                HeldItemClosedPreviewUI>();

        for (int i = 0;
             i < closedPreviewUIs.Length;
             i++)
        {
            HeldItemClosedPreviewUI closedPreviewUI =
                closedPreviewUIs[i];

            if (closedPreviewUI == null ||
                !closedPreviewUI.gameObject.scene.IsValid())
            {
                continue;
            }

            closedPreviewUI.BindPlayer(
                playerInventory
            );
        }

        if (playerResourcesUI != null)
            playerResourcesUI.BindPlayer(playerResources);

        Debug.Log(
            $"Bound runtime systems to spawned player: {player.name}",
            this
        );
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