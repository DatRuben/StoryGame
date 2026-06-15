using UnityEngine;

public class InventoryContextPanelController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject defaultPanel;
    [SerializeField] private GameObject storagePanel;

    [Header("Storage UI")]
    [SerializeField] private StorageContainerGridUI storageContainerGridUI;

    private void Start()
    {
        ShowDefaultPanel();
    }

    public void ShowDefaultPanel()
    {
        if (defaultPanel != null)
            defaultPanel.SetActive(true);

        if (storagePanel != null)
            storagePanel.SetActive(false);

        if (storageContainerGridUI != null)
            storageContainerGridUI.SetStorageContainer(null);
    }

    public void ShowStorageContainer(StorageContainer storageContainer)
    {
        if (storageContainer == null)
        {
            ShowDefaultPanel();
            return;
        }

        if (defaultPanel != null)
            defaultPanel.SetActive(false);

        if (storagePanel != null)
            storagePanel.SetActive(true);

        if (storageContainerGridUI != null)
            storageContainerGridUI.SetStorageContainer(storageContainer);
    }

    public void HideStorageContainer()
    {
        ShowDefaultPanel();
    }
}