using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class InventoryContextPanelController :
    MonoBehaviour
{
    [Header("Panels")]
    [SerializeField]
    private GameObject defaultPanel;

    [SerializeField]
    private GameObject skillLibraryPanel;

    [SerializeField]
    private GameObject navigationButtons;

    [SerializeField]
    private Button characterStatsButton;

    [SerializeField]
    private Button skillLibraryButton;

    [SerializeField]
    private GameObject storagePanel;

    [Header("Storage UI")]
    [SerializeField]
    private InventoryGridUI storageInventoryGridUI;

    [Header("Storage Title")]
    [SerializeField]
    private TextMeshProUGUI storageTitleText;

    [SerializeField]
    private string defaultStorageTitle = "Storage";

    [SerializeField]
    private bool hideTitleWhenNoStorageOpen = true;
    private enum ContextTab
    {
        CharacterStats,
        Skills
    }

    private ContextTab selectedTab = ContextTab.CharacterStats;
    private bool showingStorage;

    private void Awake()
    {
        ResolveReferences();
    }
    private void OnEnable()
    {
        if (characterStatsButton != null)
            characterStatsButton.onClick.AddListener(ShowCharacterStats);

        if (skillLibraryButton != null)
            skillLibraryButton.onClick.AddListener(ShowSkillLibrary);
    }

    private void OnDisable()
    {
        if (characterStatsButton != null)
            characterStatsButton.onClick.RemoveListener(ShowCharacterStats);

        if (skillLibraryButton != null)
            skillLibraryButton.onClick.RemoveListener(ShowSkillLibrary);
    }

    private void Start()
    {
        ShowDefaultPanel();
    }

    private void OnValidate()
    {
        ResolveReferences();
    }


    public void ShowCharacterStats()
    {
        if (showingStorage)
            return;

        selectedTab = ContextTab.CharacterStats;
        RefreshPanels();
    }

    public void ShowSkillLibrary()
    {
        if (showingStorage)
            return;

        selectedTab = ContextTab.Skills;
        RefreshPanels();
    }

    public void ShowDefaultPanel()
    {
        showingStorage = false;

        if (storageInventoryGridUI != null)
            storageInventoryGridUI.BindContainer(null);

        UpdateStorageTitle(null);
        RefreshPanels();
    }
    public void ShowStorageContainer(InventoryContainer storageContainer)
    {
        if (storageContainer == null)
        {
            ShowDefaultPanel();
            return;
        }

        showingStorage = true;

        if (storageInventoryGridUI != null)
            storageInventoryGridUI.BindContainer(storageContainer);

        UpdateStorageTitle(storageContainer);
        RefreshPanels();
    }

    public void HideStorageContainer()
    {
        ShowDefaultPanel();
    }

    private void RefreshPanels()
    {
        if (navigationButtons != null)
            navigationButtons.SetActive(!showingStorage);

        if (defaultPanel != null)
            defaultPanel.SetActive(!showingStorage && selectedTab == ContextTab.CharacterStats);

        if (skillLibraryPanel != null)
            skillLibraryPanel.SetActive(!showingStorage && selectedTab == ContextTab.Skills);

        if (storagePanel != null)
            storagePanel.SetActive(showingStorage);
    }

    private void ResolveReferences()
    {
        if (defaultPanel == null)
        {
            defaultPanel =
                FindDirectChild(
                    "CharacterInfoPanel"
                );

            if (defaultPanel == null)
            {
                defaultPanel =
                    FindDirectChild(
                        "DefaultPanel"
                    );
            }
        }

        if (storagePanel == null)
        {
            storagePanel =
                FindDirectChild(
                    "StoragePanel"
                );

            if (storagePanel == null)
            {
                storagePanel =
                    FindDirectChild(
                        "ContainerPanel"
                    );
            }
        }

        if (storageInventoryGridUI == null &&
            storagePanel != null)
        {
            storageInventoryGridUI =
                storagePanel
                    .GetComponentInChildren<
                        InventoryGridUI>(
                        true
                    );
        }

        if (storageTitleText == null &&
            storagePanel != null)
        {
            storageTitleText =
                FindStorageTitle();
        }
    }

    private GameObject FindDirectChild(
        string childName)
    {
        for (int i = 0;
             i < transform.childCount;
             i++)
        {
            Transform child =
                transform.GetChild(i);

            if (child != null &&
                child.name ==
                    childName)
            {
                return child.gameObject;
            }
        }

        return null;
    }

    private TextMeshProUGUI FindStorageTitle()
    {
        TextMeshProUGUI[] texts =
            storagePanel.GetComponentsInChildren<
                TextMeshProUGUI>(
                true
            );

        for (int i = 0;
             i < texts.Length;
             i++)
        {
            TextMeshProUGUI text =
                texts[i];

            if (text == null)
                continue;

            string lowerName =
                text.gameObject.name
                    .ToLowerInvariant();

            if (lowerName.Contains("title") ||
                lowerName.Contains("name") ||
                lowerName.Contains("header"))
            {
                return text;
            }
        }

        return texts.Length > 0
            ? texts[0]
            : null;
    }

    private void UpdateStorageTitle(
        InventoryContainer storageContainer)
    {
        if (storageTitleText == null)
            return;

        if (storageContainer == null)
        {
            storageTitleText.text =
                defaultStorageTitle;

            storageTitleText.gameObject
                .SetActive(
                    !hideTitleWhenNoStorageOpen
                );

            return;
        }

        storageTitleText.gameObject
            .SetActive(true);

        StorageContainerInteract interact =
            storageContainer.GetComponent<
                StorageContainerInteract>();

        if (interact != null &&
            !string.IsNullOrWhiteSpace(
                interact.DisplayName))
        {
            storageTitleText.text =
                interact.DisplayName;

            return;
        }

        storageTitleText.text =
            string.IsNullOrWhiteSpace(
                storageContainer.gameObject.name)
                ? defaultStorageTitle
                : storageContainer.gameObject.name;
    }
}