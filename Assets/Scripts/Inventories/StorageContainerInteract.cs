using UnityEngine;

public class StorageContainerInteract : MonoBehaviour
{
    [SerializeField] private StorageContainer storageContainer;

    [Header("Display")]
    [SerializeField] private string displayName = "Storage";

    public StorageContainer StorageContainer => storageContainer;

    public string DisplayName
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(displayName))
                return displayName;

            return gameObject.name;
        }
    }

    private void Reset()
    {
        ValidateReferences(true, false);
    }

    private void OnValidate()
    {
        ValidateReferences(true, false);
    }

    private void Awake()
    {
        ValidateReferences(true, true);
    }

    private void ValidateReferences(
        bool logAutoFilled,
        bool logMissing)
    {
        if (storageContainer == null)
        {
            storageContainer =
                GetComponent<StorageContainer>();

            if (storageContainer != null &&
                logAutoFilled)
            {
                Debug.Log(
                    "StorageContainerInteract auto-filled StorageContainer.",
                    this
                );
            }
        }

        if (logMissing &&
            storageContainer == null)
        {
            Debug.LogWarning(
                "StorageContainerInteract is missing StorageContainer.",
                this
            );
        }
    }
}