using System.Text;
using TMPro;
using UnityEngine;

public class CharacterCreatorCharacteristicsUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private CharacterCreator characterCreator;

    [Header("Identity UI")]
    [SerializeField] private TMP_InputField characterNameInput;
    [SerializeField] private CharacterOptionButtonUI maleGenderButton;
    [SerializeField] private CharacterOptionButtonUI femaleGenderButton;

    [Header("Preview UI")]
    [SerializeField] private TMP_Text attributePreviewText;
    [SerializeField] private TMP_Text derivedStatsPreviewText;

    private void OnEnable()
    {
        HookUI();
        SubscribeToCreator();
        PushCurrentInputToCreator();
        Refresh();
    }

    private void OnDisable()
    {
        UnhookUI();
        UnsubscribeFromCreator();
    }

    private void HookUI()
    {
        if (characterNameInput != null)
        {
            characterNameInput.onValueChanged.RemoveListener(
                OnCharacterNameChanged
            );

            characterNameInput.onValueChanged.AddListener(
                OnCharacterNameChanged
            );
        }

        HookGenderButton(
            maleGenderButton,
            "Male",
            CharacterGender.Male
        );

        HookGenderButton(
            femaleGenderButton,
            "Female",
            CharacterGender.Female
        );
    }

    private void UnhookUI()
    {
        if (characterNameInput != null)
        {
            characterNameInput.onValueChanged.RemoveListener(
                OnCharacterNameChanged
            );
        }
    }

    private void HookGenderButton(
        CharacterOptionButtonUI button,
        string label,
        CharacterGender gender)
    {
        if (button == null)
            return;

        button.SetText(label);

        if (button.Button == null)
            return;

        button.Button.onClick.RemoveAllListeners();
        button.Button.onClick.AddListener(() =>
            SelectGender(gender)
        );
    }

    private void SubscribeToCreator()
    {
        if (characterCreator == null)
            return;

        characterCreator.SelectionChanged -= Refresh;
        characterCreator.SelectionChanged += Refresh;
    }

    private void UnsubscribeFromCreator()
    {
        if (characterCreator == null)
            return;

        characterCreator.SelectionChanged -= Refresh;
    }

    private void PushCurrentInputToCreator()
    {
        if (characterCreator == null ||
            characterNameInput == null)
        {
            return;
        }

        characterCreator.SetCharacterName(
            characterNameInput.text
        );
    }

    private void OnCharacterNameChanged(
        string characterName)
    {
        if (characterCreator != null)
            characterCreator.SetCharacterName(characterName);
    }

    private void SelectGender(
        CharacterGender gender)
    {
        if (characterCreator == null)
            return;

        characterCreator.SelectGender(gender);
    }

    private void Refresh()
    {
        RefreshName();
        RefreshGenderButtons();
        RefreshPreview();
    }

    private void RefreshName()
    {
        if (characterNameInput == null ||
            characterCreator == null)
        {
            return;
        }

        if (characterNameInput.text != characterCreator.SelectedCharacterName)
        {
            characterNameInput.SetTextWithoutNotify(
                characterCreator.SelectedCharacterName
            );
        }
    }

    private void RefreshGenderButtons()
    {
        CharacterGender selectedGender =
            characterCreator != null
                ? characterCreator.SelectedGender
                : CharacterGender.Male;

        if (maleGenderButton != null)
        {
            maleGenderButton.SetSelected(
                selectedGender == CharacterGender.Male
            );
        }

        if (femaleGenderButton != null)
        {
            femaleGenderButton.SetSelected(
                selectedGender == CharacterGender.Female
            );
        }
    }

    private void RefreshPreview()
    {
        if (characterCreator == null)
        {
            ShowAttributePreview("CharacterCreator is missing.");
            ShowDerivedStatsPreview("");
            return;
        }

        if (!characterCreator.TryGetAttributePreview(
            out CharacterAttributePreview attributePreview,
            out string errorMessage))
        {
            ShowAttributePreview(errorMessage);
            ShowDerivedStatsPreview("");
            return;
        }

        ShowAttributePreview(
            GetAttributePreviewText(attributePreview)
        );

        if (!characterCreator.TryGetStatPreview(
            out CharacterStatPreview statPreview,
            out errorMessage))
        {
            ShowDerivedStatsPreview(errorMessage);
            return;
        }

        ShowDerivedStatsPreview(
            GetDerivedStatsPreviewText(statPreview)
        );
    }

    private void ShowAttributePreview(
        string message)
    {
        if (attributePreviewText != null)
            attributePreviewText.text = message;
    }

    private void ShowDerivedStatsPreview(
        string message)
    {
        if (derivedStatsPreviewText != null)
            derivedStatsPreviewText.text = message;
    }

    private string GetAttributePreviewText(
        CharacterAttributePreview attributePreview)
    {
        if (attributePreview == null)
            return "";

        StringBuilder builder =
            new StringBuilder();

        builder.AppendLine(
            $"Ancestry Total: {attributePreview.AncestryTotal}"
        );

        builder.AppendLine(
            $"Background: {GetSignedNumber(attributePreview.BackgroundModifierTotal)}"
        );

        builder.AppendLine(
            $"Traits: {GetSignedNumber(attributePreview.TraitModifierTotal)}"
        );

        builder.AppendLine(
            $"Racial Passive: {GetSignedNumber(attributePreview.RacialPassiveModifierTotal)}"
        );

        builder.AppendLine(
            $"Level 1 Total: {attributePreview.LevelOneTotal}"
        );

        builder.AppendLine();

        AppendAttributes(
            builder,
            attributePreview.levelOneAttributes
        );

        return builder.ToString();
    }

    private void AppendAttributes(
        StringBuilder builder,
        CharacterAttributes attributes)
    {
        if (builder == null ||
            attributes == null)
        {
            return;
        }

        builder.AppendLine($"Strength: {attributes.strength}");
        builder.AppendLine($"Dexterity: {attributes.dexterity}");
        builder.AppendLine($"Agility: {attributes.agility}");
        builder.AppendLine($"Vitality: {attributes.vitality}");
        builder.AppendLine($"Endurance: {attributes.endurance}");
        builder.AppendLine($"Intelligence: {attributes.intelligence}");
        builder.AppendLine($"Willpower: {attributes.willpower}");
        builder.AppendLine($"Spirit: {attributes.spirit}");
        builder.AppendLine($"Perception: {attributes.perception}");
    }

    private string GetDerivedStatsPreviewText(
        CharacterStatPreview statPreview)
    {
        if (statPreview == null ||
            statPreview.baseStats == null ||
            statPreview.attributeBonuses == null ||
            statPreview.finalStats == null)
        {
            return "";
        }

        StringBuilder builder =
            new StringBuilder();

        AppendBaseStatLine(
            builder,
            "Health",
            statPreview.baseStats.health,
            statPreview.finalStats.health,
            statPreview.attributeBonuses.health
        );

        AppendBaseStatLine(
            builder,
            "Stamina",
            statPreview.baseStats.stamina,
            statPreview.finalStats.stamina,
            statPreview.attributeBonuses.stamina
        );

        AppendBaseStatLine(
            builder,
            "Mana",
            statPreview.baseStats.mana,
            statPreview.finalStats.mana,
            statPreview.attributeBonuses.mana
        );

        AppendBaseStatLine(
            builder,
            "Stagger Resist",
            statPreview.baseStats.staggerResist,
            statPreview.finalStats.staggerResist,
            statPreview.attributeBonuses.staggerResist
        );

        AppendBaseStatLine(
            builder,
            "Carry Weight",
            statPreview.baseStats.carryWeight,
            statPreview.finalStats.carryWeight,
            statPreview.attributeBonuses.carryWeight
        );

        return builder.ToString();
    }

    private void AppendBaseStatLine(
        StringBuilder builder,
        string label,
        int baseValue,
        int finalValue,
        int attributeBonus)
    {
        builder.AppendLine(
            $"{label}: {baseValue} -> {finalValue} ({GetSignedNumber(attributeBonus)})"
        );
    }

    private string GetSignedNumber(
        int value)
    {
        if (value > 0)
            return $"+{value}";

        return value.ToString();
    }
}