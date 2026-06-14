using UnityEngine;

public class StorageContainerInteract : MonoBehaviour
{
    [SerializeField] private StorageContainer storageContainer;

    public StorageContainer StorageContainer => storageContainer;

    private void Awake()
    {
        if (storageContainer == null)
            storageContainer = GetComponent<StorageContainer>();
    }
}