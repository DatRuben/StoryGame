using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CharacterCreatorAppearanceUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField]
    private CharacterCreator characterCreator;

    [Header("Details")]
    [SerializeField]
    private CharacterCreatorAppearanceDetailsUI
        appearanceDetailsUI;

    [Header("Category")]
    [SerializeField]
    private CharacterAppearanceCategoryUI
        categoryPrefab;

    [SerializeField]
    private Transform categoryParent;

    [Header("Option Button")]
    [SerializeField]
    private CharacterOptionButtonUI
        optionButtonPrefab;

    private readonly List<CharacterAppearanceCategoryUI>
        categoryUIs = new();

    private readonly List<CharacterAppearanceCategory>
        categories = new();

    private readonly List<CharacterOptionButtonUI>
        optionButtons = new();

    private readonly List<CharacterAppearanceOptionDefinition>
        optionDefinitions = new();

    private CharacterAppearanceCategory selectedCategory =
        CharacterAppearanceCategory.Body;

    private void OnEnable()
    {
        SubscribeToCreator();
        RebuildCategories();
    }

    private void OnDisable()
    {
        UnsubscribeFromCreator();
        ClearCategories();
    }

    private void SubscribeToCreator()
    {
        if (characterCreator == null)
            return;

        characterCreator.SelectionChanged -=
            RebuildCategories;

        characterCreator.SelectionChanged +=
            RebuildCategories;
    }

    private void UnsubscribeFromCreator()
    {
        if (characterCreator == null)
            return;

        characterCreator.SelectionChanged -=
            RebuildCategories;
    }

    private void RebuildCategories()
    {
        ClearCategories();

        if (categoryPrefab == null ||
            categoryParent == null)
        {
            return;
        }

        List<CharacterAppearanceCategory>
            shownCategories = GetShownCategories();

        if (!ContainsCategory(
                shownCategories,
                selectedCategory))
        {
            selectedCategory =
                CharacterAppearanceCategory.Body;
        }

        foreach (CharacterAppearanceCategory category
                 in shownCategories)
        {
            BuildCategory(category);
        }

        RefreshCategories();
        ShowDetails();
    }

    private List<CharacterAppearanceCategory>
        GetShownCategories()
    {
        List<CharacterAppearanceCategory> shown =
            new List<CharacterAppearanceCategory>
            {
                CharacterAppearanceCategory.Body
            };

        if (characterCreator == null)
            return shown;

        List<CharacterAppearanceCategory> optionCategories =
            characterCreator.GetShownAppearanceCategories();

        foreach (CharacterAppearanceCategory category
                 in optionCategories)
        {
            if (!ContainsCategory(shown, category))
                shown.Add(category);
        }

        return shown;
    }

    private void BuildCategory(
        CharacterAppearanceCategory category)
    {
        List<CharacterAppearanceOptionDefinition> options =
            characterCreator != null
                ? characterCreator.GetShownAppearanceOptions(
                    category
                )
                : new List<
                    CharacterAppearanceOptionDefinition>();

        CharacterAppearanceCategory capturedCategory =
            category;

        CharacterAppearanceCategoryUI categoryUI =
            Instantiate(
                categoryPrefab,
                categoryParent
            );

        categoryUI.name =
            $"{category}AppearanceCategory";

        categoryUI.gameObject.SetActive(true);

        categoryUI.Setup(
            GetCategoryLabel(category),
            options.Count > 0,
            () => SelectCategory(capturedCategory)
        );

        categoryUIs.Add(categoryUI);
        categories.Add(category);

        foreach (CharacterAppearanceOptionDefinition option
                 in options)
        {
            BuildOptionButton(
                categoryUI,
                option
            );
        }
    }

    private void BuildOptionButton(
        CharacterAppearanceCategoryUI categoryUI,
        CharacterAppearanceOptionDefinition option)
    {
        if (categoryUI == null ||
            option == null ||
            optionButtonPrefab == null ||
            categoryUI.OptionButtonParent == null)
        {
            return;
        }

        CharacterOptionButtonUI button =
            Instantiate(
                optionButtonPrefab,
                categoryUI.OptionButtonParent
            );

        button.name =
            $"{option.optionId}AppearanceOptionButton";

        button.gameObject.SetActive(true);
        button.SetImage(option.optionImage);
        button.SetSelected(false);

        CharacterAppearanceOptionDefinition capturedOption =
            option;

        if (button.Button != null)
        {
            button.Button.onClick.RemoveAllListeners();

            button.Button.onClick.AddListener(
                () => SelectOption(capturedOption)
            );
        }

        optionButtons.Add(button);
        optionDefinitions.Add(option);
    }

    private void SelectCategory(
        CharacterAppearanceCategory category)
    {
        selectedCategory = category;

        RefreshCategories();
        ShowDetails();
    }

    private void SelectOption(
        CharacterAppearanceOptionDefinition option)
    {
        if (characterCreator == null ||
            option == null)
        {
            return;
        }

        if (!characterCreator.SelectAppearanceOption(
                option.optionId,
                out string errorMessage))
        {
            Debug.LogWarning(
                errorMessage,
                this
            );
        }
    }

    private void RefreshCategories()
    {
        for (int i = 0;
             i < categoryUIs.Count;
             i++)
        {
            CharacterAppearanceCategoryUI categoryUI =
                categoryUIs[i];

            if (categoryUI == null ||
                i >= categories.Count)
            {
                continue;
            }

            bool selected =
                categories[i] == selectedCategory;

            categoryUI.SetSelected(selected);
            categoryUI.SetExpanded(selected);
        }

        RefreshOptionButtons();
    }

    private void RefreshOptionButtons()
    {
        if (characterCreator == null)
            return;

        CharacterAppearanceData appearance =
            characterCreator.SelectedAppearance;

        if (appearance == null)
            return;

        for (int i = 0;
             i < optionButtons.Count;
             i++)
        {
            CharacterOptionButtonUI button =
                optionButtons[i];

            CharacterAppearanceOptionDefinition option =
                i < optionDefinitions.Count
                    ? optionDefinitions[i]
                    : null;

            if (button == null ||
                option == null)
            {
                continue;
            }

            CharacterAppearanceOptionAvailability availability =
                characterCreator.GetAppearanceOptionAvailability(
                    option
                );

            bool shown =
                availability !=
                CharacterAppearanceOptionAvailability.Hidden;

            button.gameObject.SetActive(shown);

            if (!shown)
                continue;

            bool available =
                availability ==
                CharacterAppearanceOptionAvailability.Available;

            string selectedOptionId =
                appearance.GetSingleOptionId(
                    option.category
                );

            bool selected =
                available &&
                string.Equals(
                    selectedOptionId,
                    option.optionId,
                    System.StringComparison.OrdinalIgnoreCase
                );

            button.SetInteractable(available);
            button.SetSelected(selected);
        }
    }

    private void ShowDetails()
    {
        if (appearanceDetailsUI != null)
        {
            appearanceDetailsUI.ShowCategory(
                selectedCategory
            );
        }
    }

    private void ClearCategories()
    {
        foreach (CharacterAppearanceCategoryUI categoryUI
                 in categoryUIs)
        {
            if (categoryUI == null)
                continue;

            categoryUI.gameObject.SetActive(false);
            Destroy(categoryUI.gameObject);
        }

        categoryUIs.Clear();
        categories.Clear();
        optionButtons.Clear();
        optionDefinitions.Clear();
    }

    private bool ContainsCategory(
        IReadOnlyList<CharacterAppearanceCategory> list,
        CharacterAppearanceCategory category)
    {
        if (list == null)
            return false;

        for (int i = 0;
             i < list.Count;
             i++)
        {
            if (list[i] == category)
                return true;
        }

        return false;
    }

    private string GetCategoryLabel(
        CharacterAppearanceCategory category)
    {
        string categoryName =
            category.ToString();

        StringBuilder label =
            new StringBuilder();

        for (int i = 0;
             i < categoryName.Length;
             i++)
        {
            char character =
                categoryName[i];

            if (i > 0 &&
                char.IsUpper(character))
            {
                label.Append(' ');
            }

            label.Append(character);
        }

        return label.ToString();
    }
}