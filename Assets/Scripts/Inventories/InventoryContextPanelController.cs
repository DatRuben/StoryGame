using UnityEngine;

public class InventoryContextPanelController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject defaultPanel;
    [SerializeField] private GameObject storagePanel;

    [Header("Storage UI")]
    [SerializeField] private StorageContainerGridUI storageContainerGridUI;

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

    private void Start()
    {
        ShowDefaultPanel();
    }

    private void ValidateReferences(
        bool logAutoFilled,
        bool logMissing)
    {
        if (storageContainerGridUI == null)
        {
            StorageContainerGridUI foundStorageUI =
                GetComponentInChildren<StorageContainerGridUI>(true);

            if (foundStorageUI == null)
                foundStorageUI = FindSceneComponent<StorageContainerGridUI>();

            if (foundStorageUI != null)
            {
                storageContainerGridUI = foundStorageUI;

                if (logAutoFilled)
                {
                    Debug.Log(
                        "InventoryContextPanelController auto-filled StorageContainerGridUI.",
                        this
                    );
                }
            }
        }

        if (storagePanel == null &&
            storageContainerGridUI != null)
        {
            storagePanel =
                storageContainerGridUI.gameObject;

            if (logAutoFilled)
            {
                Debug.Log(
                    "InventoryContextPanelController auto-filled StoragePanel from StorageContainerGridUI.",
                    this
                );
            }
        }

        if (defaultPanel == null)
        {
            defaultPanel =
                FindDirectChildByName("CharacterInfoPanel");

            if (defaultPanel == null)
                defaultPanel = FindDirectChildByName("DefaultPanel");

            if (defaultPanel == null)
                defaultPanel = FindDirectChildByName("StatsPanel");

            if (defaultPanel != null &&
                logAutoFilled)
            {
                Debug.Log(
                    "InventoryContextPanelController auto-filled DefaultPanel.",
                    this
                );
            }
        }

        if (!logMissing)
            return;

        if (defaultPanel == null)
        {
            Debug.LogWarning(
                "InventoryContextPanelController is missing DefaultPanel.",
                this
            );
        }

        if (storagePanel == null)
        {
            Debug.LogWarning(
                "InventoryContextPanelController is missing StoragePanel.",
                this
            );
        }

        if (storageContainerGridUI == null)
        {
            Debug.LogWarning(
                "InventoryContextPanelController is missing StorageContainerGridUI.",
                this
            );
        }
    }

    private GameObject FindDirectChildByName(string childName)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            if (child != null &&
                child.name == childName)
            {
                return child.gameObject;
            }
        }

        return null;
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

    public void ShowDefaultPanel()
    {
        ValidateReferences(false, true);

        if (defaultPanel != null)
            defaultPanel.SetActive(true);

        if (storagePanel != null)
            storagePanel.SetActive(false);

        if (storageContainerGridUI != null)
            storageContainerGridUI.SetStorageContainer(null);
    }

    public void ShowStorageContainer(StorageContainer storageContainer)
    {
        ValidateReferences(false, true);

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
        {
            storageContainerGridUI.SetStorageContainer(storageContainer);
        }
        else
        {
            Debug.LogWarning(
                "Cannot show storage container because StorageContainerGridUI is missing.",
                this
            );
        }
    }

    public void HideStorageContainer()
    {
        ShowDefaultPanel();
    }
}
