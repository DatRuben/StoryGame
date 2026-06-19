using TMPro;
using UnityEngine;

public class InventoryContextPanelController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject defaultPanel;
    [SerializeField] private GameObject storagePanel;

    [Header("Storage UI")]
    [SerializeField] private StorageContainerGridUI storageContainerGridUI;

    [Header("Storage Title")]
    [SerializeField] private TextMeshProUGUI storageTitleText;
    [SerializeField] private string defaultStorageTitle = "Storage";
    [SerializeField] private bool hideTitleWhenNoStorageOpen = true;
    [SerializeField] private bool warnIfStorageTitleTextMissing = true;

    private void Reset()
    {
        ValidateReferences(true, false);
    }

    private void OnValidate()
    {
        ValidateReferences(true, false);
        UpdateStorageTitle(null);
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

        if (storageTitleText == null)
        {
            TextMeshProUGUI foundTitleText =
                FindStorageTitleText();

            if (foundTitleText != null)
            {
                storageTitleText = foundTitleText;

                if (logAutoFilled)
                {
                    Debug.Log(
                        "InventoryContextPanelController auto-filled Storage Title Text.",
                        this
                    );
                }
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

        if (warnIfStorageTitleTextMissing &&
            storageTitleText == null)
        {
            Debug.LogWarning(
                "InventoryContextPanelController is missing Storage Title Text. Container names will not be shown.",
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

    private TextMeshProUGUI FindStorageTitleText()
    {
        if (storagePanel == null)
            return null;

        TextMeshProUGUI[] texts =
            storagePanel.GetComponentsInChildren<TextMeshProUGUI>(true);

        if (texts == null ||
            texts.Length == 0)
        {
            return null;
        }

        for (int i = 0; i < texts.Length; i++)
        {
            TextMeshProUGUI text =
                texts[i];

            if (text == null)
                continue;

            string lowerName =
                text.gameObject.name.ToLowerInvariant();

            if (lowerName.Contains("title") ||
                lowerName.Contains("name") ||
                lowerName.Contains("header"))
            {
                return text;
            }
        }

        return texts[0];
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

        UpdateStorageTitle(null);
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

        UpdateStorageTitle(storageContainer);
    }

    public void HideStorageContainer()
    {
        ShowDefaultPanel();
    }

    private void UpdateStorageTitle(StorageContainer storageContainer)
    {
        if (storageTitleText == null)
            return;

        if (storageContainer == null)
        {
            storageTitleText.text =
                defaultStorageTitle;

            storageTitleText.gameObject.SetActive(
                !hideTitleWhenNoStorageOpen
            );

            return;
        }

        storageTitleText.gameObject.SetActive(true);

        storageTitleText.text =
            GetStorageContainerTitle(storageContainer);
    }

    private string GetStorageContainerTitle(
        StorageContainer storageContainer)
    {
        if (storageContainer == null)
            return defaultStorageTitle;

        StorageContainerInteract interact =
            storageContainer.GetComponent<StorageContainerInteract>();

        if (interact != null &&
            !string.IsNullOrWhiteSpace(interact.DisplayName))
        {
            return interact.DisplayName;
        }

        if (!string.IsNullOrWhiteSpace(storageContainer.gameObject.name))
            return storageContainer.gameObject.name;

        return defaultStorageTitle;
    }
}