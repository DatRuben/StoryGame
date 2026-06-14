using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStorageContainerInteract : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask containerLayerMask = ~0;

    [Header("UI References")]
    [SerializeField] private StorageContainerGridUI containerGridUI;
    [SerializeField] private GameObject containerPanel;

    private PlayerInputActions inputActions;
    private StorageContainer currentOpenContainer;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();

        inputActions.Player.TestKey1.performed += OnInteractPerformed;
        inputActions.Player.TestKey2.performed += OnClosePerformed;
    }

    private void OnDisable()
    {
        inputActions.Player.TestKey1.performed -= OnInteractPerformed;
        inputActions.Player.TestKey2.performed -= OnClosePerformed;

        inputActions.Disable();
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        StorageContainer nearestContainer = FindNearestContainer();

        if (nearestContainer == null)
            return;

        OpenContainer(nearestContainer);
    }

    private void OnClosePerformed(InputAction.CallbackContext context)
    {
        CloseContainer();
    }

    private StorageContainer FindNearestContainer()
    {
        Collider[] hits =
            Physics.OverlapSphere(
                transform.position,
                interactRange,
                containerLayerMask
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
        if (storageContainer == null ||
            containerGridUI == null ||
            containerPanel == null)
        {
            Debug.LogWarning(
                "PlayerStorageContainerInteract is missing a reference.",
                this
            );

            return;
        }

        currentOpenContainer = storageContainer;

        containerPanel.SetActive(true);
        containerGridUI.SetStorageContainer(storageContainer);
    }

    private void CloseContainer()
    {
        currentOpenContainer = null;

        if (containerPanel != null)
            containerPanel.SetActive(false);

        if (containerGridUI != null)
            containerGridUI.SetStorageContainer(null);
    }
}