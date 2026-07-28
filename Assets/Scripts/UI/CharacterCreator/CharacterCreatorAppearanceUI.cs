using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

public class CharacterCreatorAppearanceUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField]
    private CharacterCreator characterCreator;

    [Header("Details")]
    [SerializeField]
    private CharacterCreatorAppearanceDetailsUI
        appearanceDetailsUI;

    [Header("Fixed Category Buttons")]
    [SerializeField]
    private CharacterOptionButtonUI bodyButton;

    [SerializeField]
    private CharacterOptionButtonUI hairButton;

    [SerializeField]
    private CharacterOptionButtonUI eyesButton;

    [Header("Option Category Buttons")]
    [SerializeField]
    private CharacterOptionButtonUI
        optionCategoryButtonPrefab;

    [SerializeField]
    private Transform optionCategoryButtonParent;

    private readonly List<CharacterOptionButtonUI>
        optionCategoryButtons = new();

    private readonly List<CharacterAppearanceCategory>
        optionCategories = new();

    private CharacterAppearanceCategory selectedCategory =
        CharacterAppearanceCategory.Body;

    private CharacterAppearanceCategory
        selectedOptionCategory =
            CharacterAppearanceCategory.Head;

    private bool showingOptionCategory;

    private void OnEnable()
    {
        HookButtons();
        SubscribeToCreator();
        RebuildOptionCategoryButtons();

        if (showingOptionCategory &&
            HasOptionCategory(selectedOptionCategory))
        {
            SelectOptionCategory(
                selectedOptionCategory
            );

            return;
        }

        SelectCategory(selectedCategory);
    }

    private void OnDisable()
    {
        UnhookButtons();
        UnsubscribeFromCreator();
        ClearOptionCategoryButtons();
    }

    private void HookButtons()
    {
        HookButton(
            bodyButton,
            "Body",
            SelectBody
        );

        HookButton(
            hairButton,
            "Hair Color",
            SelectHairColor
        );

        HookButton(
            eyesButton,
            "Eye Color",
            SelectEyeColor
        );
    }

    private void UnhookButtons()
    {
        UnhookButton(
            bodyButton,
            SelectBody
        );

        UnhookButton(
            hairButton,
            SelectHairColor
        );

        UnhookButton(
            eyesButton,
            SelectEyeColor
        );
    }

    private void SubscribeToCreator()
    {
        if (characterCreator == null)
            return;

        characterCreator.SelectionChanged -=
            RefreshOptionCategories;

        characterCreator.SelectionChanged +=
            RefreshOptionCategories;
    }

    private void UnsubscribeFromCreator()
    {
        if (characterCreator == null)
            return;

        characterCreator.SelectionChanged -=
            RefreshOptionCategories;
    }

    private void RefreshOptionCategories()
    {
        if (characterCreator == null)
            return;

        List<CharacterAppearanceCategory>
            shownCategories =
                characterCreator
                    .GetShownAppearanceCategories();

        if (SameOptionCategories(shownCategories))
        {
            RefreshCategorySelection();
            return;
        }

        bool selectedCategoryStillShown =
            !showingOptionCategory ||
            ContainsOptionCategory(
                shownCategories,
                selectedOptionCategory
            );

        RebuildOptionCategoryButtons(
            shownCategories
        );

        if (showingOptionCategory &&
            !selectedCategoryStillShown)
        {
            SelectCategory(
                CharacterAppearanceCategory.Body
            );

            return;
        }

        RefreshCategorySelection();
    }

    private void RebuildOptionCategoryButtons()
    {
        List<CharacterAppearanceCategory>
            shownCategories =
                characterCreator != null
                    ? characterCreator
                        .GetShownAppearanceCategories()
                    : new List<
                        CharacterAppearanceCategory>();

        RebuildOptionCategoryButtons(
            shownCategories
        );
    }

    private void RebuildOptionCategoryButtons(
        List<CharacterAppearanceCategory>
            shownCategories)
    {
        ClearOptionCategoryButtons();

        if (optionCategoryButtonPrefab == null ||
            optionCategoryButtonParent == null)
        {
            return;
        }

        foreach (
            CharacterAppearanceCategory category
            in shownCategories)
        {
            CharacterOptionButtonUI button =
                Instantiate(
                    optionCategoryButtonPrefab,
                    optionCategoryButtonParent
                );

            button.gameObject.SetActive(true);

            button.name =
                $"{category}AppearanceCategoryButton";

            button.SetText(
                GetOptionCategoryLabel(category)
            );

            button.SetImage(null);
            button.SetInteractable(true);
            button.SetSelected(false);

            CharacterAppearanceCategory
                capturedCategory = category;

            if (button.Button != null)
            {
                button.Button.onClick
                    .RemoveAllListeners();

                button.Button.onClick.AddListener(
                    () => SelectOptionCategory(
                        capturedCategory
                    )
                );
            }

            optionCategoryButtons.Add(button);
            optionCategories.Add(category);
        }
    }

    private void ClearOptionCategoryButtons()
    {
        foreach (CharacterOptionButtonUI button
                 in optionCategoryButtons)
        {
            if (button != null)
            {
                button.gameObject.SetActive(false);
                Destroy(button.gameObject);
            }
        }

        optionCategoryButtons.Clear();
        optionCategories.Clear();
    }

    private bool SameOptionCategories(
        List<CharacterAppearanceCategory>
            shownCategories)
    {
        if (shownCategories == null)
            return optionCategories.Count == 0;

        if (optionCategories.Count !=
            shownCategories.Count)
        {
            return false;
        }

        for (int i = 0;
             i < shownCategories.Count;
             i++)
        {
            if (optionCategories[i] !=
                shownCategories[i])
            {
                return false;
            }
        }

        return true;
    }

    private bool HasOptionCategory(
        CharacterAppearanceCategory category)
    {
        return ContainsOptionCategory(
            optionCategories,
            category
        );
    }

    private bool ContainsOptionCategory(
        IReadOnlyList<
            CharacterAppearanceCategory> categories,
        CharacterAppearanceCategory category)
    {
        if (categories == null)
            return false;

        for (int i = 0;
             i < categories.Count;
             i++)
        {
            if (categories[i] == category)
                return true;
        }

        return false;
    }

    private string GetOptionCategoryLabel(
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

    private void HookButton(
        CharacterOptionButtonUI button,
        string label,
        UnityAction action)
    {
        if (button == null)
            return;

        button.SetText(label);
        button.SetInteractable(true);

        if (button.Button == null)
            return;

        button.Button.onClick
            .RemoveListener(action);

        button.Button.onClick
            .AddListener(action);
    }

    private void UnhookButton(
        CharacterOptionButtonUI button,
        UnityAction action)
    {
        if (button == null ||
            button.Button == null)
        {
            return;
        }

        button.Button.onClick
            .RemoveListener(action);
    }

    private void SelectBody()
    {
        SelectCategory(
            CharacterAppearanceCategory.Body
        );
    }

    private void SelectHairColor()
    {
        SelectCategory(
            CharacterAppearanceCategory.Hair
        );
    }

    private void SelectEyeColor()
    {
        SelectCategory(
            CharacterAppearanceCategory.Eyes
        );
    }

    private void SelectCategory(
        CharacterAppearanceCategory category)
    {
        showingOptionCategory = false;
        selectedCategory = category;

        SetSelected(
            bodyButton,
            category ==
                CharacterAppearanceCategory.Body
        );

        SetSelected(
            hairButton,
            category ==
                CharacterAppearanceCategory.Hair
        );

        SetSelected(
            eyesButton,
            category ==
                CharacterAppearanceCategory.Eyes
        );

        RefreshOptionCategorySelection();

        if (appearanceDetailsUI != null)
        {
            appearanceDetailsUI.ShowCategory(
                category
            );
        }
    }

    private void SelectOptionCategory(
        CharacterAppearanceCategory category)
    {
        if (!HasOptionCategory(category))
            return;

        showingOptionCategory = true;
        selectedOptionCategory = category;

        SetSelected(bodyButton, false);
        SetSelected(hairButton, false);
        SetSelected(eyesButton, false);

        RefreshOptionCategorySelection();

        if (appearanceDetailsUI != null)
        {
            appearanceDetailsUI.ShowOptionCategory(
                category
            );
        }
    }

    private void RefreshCategorySelection()
    {
        if (showingOptionCategory)
        {
            SetSelected(bodyButton, false);
            SetSelected(hairButton, false);
            SetSelected(eyesButton, false);
        }

        RefreshOptionCategorySelection();
    }

    private void RefreshOptionCategorySelection()
    {
        for (int i = 0;
             i < optionCategoryButtons.Count;
             i++)
        {
            CharacterOptionButtonUI button =
                optionCategoryButtons[i];

            bool selected =
                showingOptionCategory &&
                i < optionCategories.Count &&
                optionCategories[i] ==
                    selectedOptionCategory;

            if (button != null)
                button.SetSelected(selected);
        }
    }

    private void SetSelected(
        CharacterOptionButtonUI button,
        bool selected)
    {
        if (button != null)
            button.SetSelected(selected);
    }
}