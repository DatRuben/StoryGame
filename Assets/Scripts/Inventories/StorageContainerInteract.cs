using UnityEngine;

[RequireComponent(
    typeof(InventoryContainer)
)]
public sealed class StorageContainerInteract :
    MonoBehaviour
{
    [SerializeField]
    private InventoryContainer storageContainer;

    [Header("Display")]
    [SerializeField]
    private string displayName = "Storage";

    public InventoryContainer Container =>
        storageContainer;

    public string DisplayName
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(
                displayName))
            {
                return displayName;
            }

            return gameObject.name;
        }
    }

    private void Reset()
    {
        ResolveContainer();
    }

    private void OnValidate()
    {
        ResolveContainer();
    }

    private void Awake()
    {
        ResolveContainer();

        if (storageContainer == null)
        {
            Debug.LogError(
                "StorageContainerInteract requires InventoryContainer.",
                this
            );
        }
    }

    private void ResolveContainer()
    {
        if (storageContainer != null)
            return;

        storageContainer =
            GetComponent<InventoryContainer>();
    }
}