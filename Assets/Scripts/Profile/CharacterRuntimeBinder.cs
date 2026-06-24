using System.Collections;
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
    [SerializeField] private Transform cameraTargetOverride;
    [SerializeField] private string cameraPivotName = "CameraPivot";

    private IEnumerator Start()
    {
        // Wait one frame so CharacterGameBootstrap and UI Start methods
        // have a chance to run first.
        yield return null;

        Bind(CharacterGameBootstrap.CurrentPlayer);
    }

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

        if (cameraTarget == null)
        {
            Debug.LogWarning(
                $"CharacterRuntimeBinder could not find camera pivot named '{cameraPivotName}'. Falling back to player root.",
                this
            );

            cameraTarget = player.transform;
        }

        if (cameraTarget == null)
            cameraTarget = player.transform;

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

        if (inventoryGridUI == null)
            inventoryGridUI = FindSceneComponent<InventoryGridUI>();

        if (storageContainerGridUI == null)
            storageContainerGridUI = FindSceneComponent<StorageContainerGridUI>();

        if (heldItemUI == null)
            heldItemUI = FindSceneComponent<HeldItemUI>();

        if (playerResourcesUI == null)
            playerResourcesUI = FindSceneComponent<PlayerResourcesUI>();

        if (inventoryMenuController == null)
            inventoryMenuController = FindSceneComponent<InventoryMenuController>();

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