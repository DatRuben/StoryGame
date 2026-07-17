using UnityEngine;
using UnityEngine.Events;

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
    [Header("Details")]
    [SerializeField]
    private CharacterCreatorAppearanceDetailsUI appearanceDetailsUI;

    [Header("Category Buttons")]
    [SerializeField] private CharacterOptionButtonUI bodyButton;
    [SerializeField] private CharacterOptionButtonUI headButton;
    [SerializeField] private CharacterOptionButtonUI hairButton;
    [SerializeField] private CharacterOptionButtonUI eyesButton;
    [SerializeField] private CharacterOptionButtonUI skinButton;

    private CharacterAppearanceCategory selectedCategory =
        CharacterAppearanceCategory.Body;

    private void OnEnable()
    {
        HookButtons();
        SelectCategory(selectedCategory);
    }

    private void OnDisable()
    {
        UnhookButtons();
    }

    private void HookButtons()
    {
        HookButton(
            bodyButton,
            "Body",
            SelectBody
        );

        HookButton(
            headButton,
            "Head",
            SelectHead
        );

        HookButton(
            hairButton,
            "Hair",
            SelectHair
        );

        HookButton(
            eyesButton,
            "Eyes",
            SelectEyes
        );

        HookButton(
            skinButton,
            "Skin",
            SelectSkin
        );
    }

    private void UnhookButtons()
    {
        UnhookButton(bodyButton, SelectBody);
        UnhookButton(headButton, SelectHead);
        UnhookButton(hairButton, SelectHair);
        UnhookButton(eyesButton, SelectEyes);
        UnhookButton(skinButton, SelectSkin);
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

        button.Button.onClick.RemoveListener(action);
        button.Button.onClick.AddListener(action);
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

        button.Button.onClick.RemoveListener(action);
    }

    private void SelectBody()
    {
        SelectCategory(
            CharacterAppearanceCategory.Body
        );
    }

    private void SelectHead()
    {
        SelectCategory(
            CharacterAppearanceCategory.Head
        );
    }

    private void SelectHair()
    {
        SelectCategory(
            CharacterAppearanceCategory.Hair
        );
    }

    private void SelectEyes()
    {
        SelectCategory(
            CharacterAppearanceCategory.Eyes
        );
    }

    private void SelectSkin()
    {
        SelectCategory(
            CharacterAppearanceCategory.Skin
        );
    }

    private void SelectCategory(
        CharacterAppearanceCategory category)
    {
        selectedCategory = category;

        SetSelected(
            bodyButton,
            category == CharacterAppearanceCategory.Body
        );

        SetSelected(
            headButton,
            category == CharacterAppearanceCategory.Head
        );

        SetSelected(
            hairButton,
            category == CharacterAppearanceCategory.Hair
        );

        SetSelected(
            eyesButton,
            category == CharacterAppearanceCategory.Eyes
        );

        SetSelected(
            skinButton,
            category == CharacterAppearanceCategory.Skin
        );

        if (appearanceDetailsUI != null)
        {
            appearanceDetailsUI.ShowCategory(
                category
            );
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