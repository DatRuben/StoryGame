using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public enum CharacterAppearanceCategory
{
    Body,
    Head,
    Hair,
    Eyes,
    Skin
}

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

    [SerializeField]
    private CharacterOptionButtonUI skinButton;

    [Header("Option Category Template")]
    [FormerlySerializedAs("headButton")]
    [SerializeField]
    private CharacterOptionButtonUI
        optionCategoryButtonTemplate;

    private readonly List<CharacterOptionButtonUI>
        optionCategoryButtons = new();

    private readonly List<CharacterAppearanceOptionCategory>
        optionCategories = new();

    private CharacterAppearanceCategory selectedCategory =
        CharacterAppearanceCategory.Body;

    private CharacterAppearanceOptionCategory
        selectedOptionCategory =
            CharacterAppearanceOptionCategory.Head;

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

        if (selectedCategory ==
            CharacterAppearanceCategory.Head)
        {
            selectedCategory =
                CharacterAppearanceCategory.Body;
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

        HookButton(
            skinButton,
            "Skin Color",
            SelectSkinColor
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

        UnhookButton(
            skinButton,
            SelectSkinColor
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

        List<CharacterAppearanceOptionCategory>
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
        List<CharacterAppearanceOptionCategory>
            shownCategories =
                characterCreator != null
                    ? characterCreator
                        .GetShownAppearanceCategories()
                    : new List<
                        CharacterAppearanceOptionCategory>();

        RebuildOptionCategoryButtons(
            shownCategories
        );
    }

    private void RebuildOptionCategoryButtons(
        List<CharacterAppearanceOptionCategory>
            shownCategories)
    {
        ClearOptionCategoryButtons();

        if (optionCategoryButtonTemplate == null)
            return;

        optionCategoryButtonTemplate
            .gameObject.SetActive(false);

        Transform buttonParent =
            optionCategoryButtonTemplate
                .transform.parent;

        if (buttonParent == null)
            return;

        int firstSiblingIndex =
            optionCategoryButtonTemplate
                .transform.GetSiblingIndex();

        foreach (
            CharacterAppearanceOptionCategory category
            in shownCategories)
        {
            CharacterOptionButtonUI button =
                Instantiate(
                    optionCategoryButtonTemplate,
                    buttonParent
                );

            button.gameObject.SetActive(true);

            button.transform.SetSiblingIndex(
                firstSiblingIndex +
                optionCategoryButtons.Count
            );

            button.name =
                $"{category}AppearanceCategoryButton";

            button.SetText(
                GetOptionCategoryLabel(category)
            );

            button.SetImage(null);
            button.SetInteractable(true);
            button.SetSelected(false);

            CharacterAppearanceOptionCategory
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
        List<CharacterAppearanceOptionCategory>
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
        CharacterAppearanceOptionCategory category)
    {
        return ContainsOptionCategory(
            optionCategories,
            category
        );
    }

    private bool ContainsOptionCategory(
        IReadOnlyList<
            CharacterAppearanceOptionCategory> categories,
        CharacterAppearanceOptionCategory category)
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
        CharacterAppearanceOptionCategory category)
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

    private void SelectSkinColor()
    {
        SelectCategory(
            CharacterAppearanceCategory.Skin
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

        SetSelected(
            skinButton,
            category ==
                CharacterAppearanceCategory.Skin
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
        CharacterAppearanceOptionCategory category)
    {
        if (!HasOptionCategory(category))
            return;

        showingOptionCategory = true;
        selectedOptionCategory = category;

        SetSelected(bodyButton, false);
        SetSelected(hairButton, false);
        SetSelected(eyesButton, false);
        SetSelected(skinButton, false);

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
            SetSelected(skinButton, false);
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