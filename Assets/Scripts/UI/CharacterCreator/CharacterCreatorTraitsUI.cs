using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterCreatorTraitsUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private CharacterDataLibrary characterDataLibrary;
    [SerializeField] private CharacterCreator characterCreator;

    [Header("Option Prefab")]
    [SerializeField] private CharacterOptionButtonUI optionButtonPrefab;

    [Header("Background UI")]
    [SerializeField] private Transform backgroundButtonParent;
    [SerializeField] private TMP_Text backgroundDescriptionText;

    [Header("Trait UI")]
    [SerializeField] private Transform traitButtonParent;
    [SerializeField] private TMP_Text traitDescriptionText;

    [Header("Messages")]
    [SerializeField] private TMP_Text messageText;

    private readonly List<CharacterOptionButtonUI> backgroundButtons = new();
    private readonly List<string> backgroundButtonIds = new();

    private readonly List<CharacterOptionButtonUI> traitButtons = new();
    private readonly List<string> traitButtonIds = new();

    private void OnEnable()
    {
        if (characterCreator != null)
            characterCreator.SelectionChanged += RefreshSelectedButtons;

        BuildButtons();
    }

    private void OnDisable()
    {
        if (characterCreator != null)
            characterCreator.SelectionChanged -= RefreshSelectedButtons;

        ClearButtons(backgroundButtons);
        ClearButtons(traitButtons);

        backgroundButtonIds.Clear();
        traitButtonIds.Clear();
    }

    public void BuildButtons()
    {
        ClearButtons(backgroundButtons);
        ClearButtons(traitButtons);

        backgroundButtonIds.Clear();
        traitButtonIds.Clear();

        if (!HasRequiredReferences())
            return;

        BuildBackgroundButtons();
        BuildTraitButtons();

        RefreshSelectedButtons();
        ShowMessage("");
    }

    private void BuildBackgroundButtons()
    {
        CharacterOptionButtonUI noneButton =
            Instantiate(optionButtonPrefab, backgroundButtonParent);

        noneButton.name = "NoBackgroundButton";
        noneButton.SetText("No Background");
        noneButton.SetSelected(false);

        noneButton.Button.onClick.RemoveAllListeners();
        noneButton.Button.onClick.AddListener(SelectNoBackground);

        backgroundButtons.Add(noneButton);
        backgroundButtonIds.Add("");

        foreach (BackgroundDefinition backgroundDefinition in characterDataLibrary.BackgroundDefinitions)
        {
            if (backgroundDefinition == null)
                continue;

            CharacterOptionButtonUI button =
                Instantiate(optionButtonPrefab, backgroundButtonParent);

            button.name = $"{backgroundDefinition.backgroundId}BackgroundButton";
            button.SetText(GetBackgroundButtonText(backgroundDefinition));
            button.SetSelected(false);

            BackgroundDefinition capturedBackground = backgroundDefinition;

            button.Button.onClick.RemoveAllListeners();
            button.Button.onClick.AddListener(() =>
                SelectBackground(capturedBackground)
            );

            backgroundButtons.Add(button);
            backgroundButtonIds.Add(backgroundDefinition.backgroundId);
        }
    }

    private void BuildTraitButtons()
    {
        foreach (TraitDefinition traitDefinition in characterDataLibrary.TraitDefinitions)
        {
            if (traitDefinition == null)
                continue;

            CharacterOptionButtonUI button =
                Instantiate(optionButtonPrefab, traitButtonParent);

            button.name = $"{traitDefinition.traitId}TraitButton";
            button.SetText(GetTraitButtonText(traitDefinition));
            button.SetSelected(false);

            TraitDefinition capturedTrait = traitDefinition;

            button.Button.onClick.RemoveAllListeners();
            button.Button.onClick.AddListener(() =>
                ToggleTrait(capturedTrait)
            );

            traitButtons.Add(button);
            traitButtonIds.Add(traitDefinition.traitId);
        }
    }

    private void SelectNoBackground()
    {
        characterCreator.ClearBackground();
        ShowBackgroundDescription("No background selected.");
        ShowMessage("");
        RefreshSelectedButtons();
    }

    private void SelectBackground(
        BackgroundDefinition backgroundDefinition)
    {
        if (backgroundDefinition == null)
            return;

        if (!characterCreator.SelectBackground(
            backgroundDefinition.backgroundId,
            out string errorMessage))
        {
            ShowMessage(errorMessage);
            return;
        }

        ShowBackgroundDescription(
            GetBackgroundDescription(backgroundDefinition)
        );

        ShowMessage("");
        RefreshSelectedButtons();
    }

    private void ToggleTrait(
        TraitDefinition traitDefinition)
    {
        if (traitDefinition == null)
            return;

        if (!characterCreator.ToggleTrait(
            traitDefinition.traitId,
            out string errorMessage))
        {
            ShowMessage(errorMessage);
            return;
        }

        ShowTraitDescription(
            GetTraitDescription(traitDefinition)
        );

        ShowMessage("");
        RefreshSelectedButtons();
    }

    private void RefreshSelectedButtons()
    {
        RefreshBackgroundButtons();
        RefreshTraitButtons();
    }

    private void RefreshBackgroundButtons()
    {
        string selectedBackgroundId =
            characterCreator != null
                ? characterCreator.SelectedBackgroundId
                : "";

        for (int i = 0; i < backgroundButtons.Count; i++)
        {
            CharacterOptionButtonUI button = backgroundButtons[i];

            if (button == null)
                continue;

            string backgroundId =
                i < backgroundButtonIds.Count
                    ? backgroundButtonIds[i]
                    : "";

            button.SetSelected(backgroundId == selectedBackgroundId);
        }
    }

    private void RefreshTraitButtons()
    {
        for (int i = 0; i < traitButtons.Count; i++)
        {
            CharacterOptionButtonUI button = traitButtons[i];

            if (button == null)
                continue;

            string traitId =
                i < traitButtonIds.Count
                    ? traitButtonIds[i]
                    : "";

            button.SetSelected(IsTraitSelected(traitId));
        }
    }

    private bool IsTraitSelected(
        string traitId)
    {
        if (characterCreator == null)
            return false;

        if (string.IsNullOrWhiteSpace(traitId))
            return false;

        foreach (string selectedTraitId in characterCreator.SelectedTraitIds)
        {
            if (selectedTraitId == traitId)
                return true;
        }

        return false;
    }

    private string GetBackgroundButtonText(
        BackgroundDefinition backgroundDefinition)
    {
        if (backgroundDefinition == null)
            return "Missing Background";

        return string.IsNullOrWhiteSpace(backgroundDefinition.displayName)
            ? backgroundDefinition.name
            : backgroundDefinition.displayName;
    }

    private string GetTraitButtonText(
        TraitDefinition traitDefinition)
    {
        if (traitDefinition == null)
            return "Missing Trait";

        return string.IsNullOrWhiteSpace(traitDefinition.displayName)
            ? traitDefinition.name
            : traitDefinition.displayName;
    }

    private string GetBackgroundDescription(
        BackgroundDefinition backgroundDefinition)
    {
        if (backgroundDefinition == null)
            return "";

        string text = GetBackgroundButtonText(backgroundDefinition);

        if (!string.IsNullOrWhiteSpace(backgroundDefinition.description))
            text += $"\n\n{backgroundDefinition.description}";

        text += $"\n\nModifiers:\n{GetModifierText(backgroundDefinition.modifiers)}";

        return text;
    }

    private string GetTraitDescription(
        TraitDefinition traitDefinition)
    {
        if (traitDefinition == null)
            return "";

        string text = GetTraitButtonText(traitDefinition);

        if (!string.IsNullOrWhiteSpace(traitDefinition.description))
            text += $"\n\n{traitDefinition.description}";

        text += $"\n\nModifiers:\n{GetModifierText(traitDefinition.modifiers)}";

        return text;
    }

    private string GetModifierText(
        CharacterAttributeModifiers modifiers)
    {
        if (modifiers == null)
            return "None";

        return
            $"Strength: {FormatModifier(modifiers.strength)}\n" +
            $"Dexterity: {FormatModifier(modifiers.dexterity)}\n" +
            $"Agility: {FormatModifier(modifiers.agility)}\n" +
            $"Vitality: {FormatModifier(modifiers.vitality)}\n" +
            $"Endurance: {FormatModifier(modifiers.endurance)}\n" +
            $"Intelligence: {FormatModifier(modifiers.intelligence)}\n" +
            $"Willpower: {FormatModifier(modifiers.willpower)}\n" +
            $"Spirit: {FormatModifier(modifiers.spirit)}\n" +
            $"Perception: {FormatModifier(modifiers.perception)}";
    }

    private string FormatModifier(
        int value)
    {
        if (value > 0)
            return $"+{value}";

        return value.ToString();
    }

    private bool HasRequiredReferences()
    {
        if (characterDataLibrary == null)
        {
            ShowMessage("CharacterDataLibrary is missing.");
            return false;
        }

        if (characterCreator == null)
        {
            ShowMessage("CharacterCreator is missing.");
            return false;
        }

        if (optionButtonPrefab == null)
        {
            ShowMessage("CharacterOptionButton prefab is missing.");
            return false;
        }

        if (backgroundButtonParent == null)
        {
            ShowMessage("Background button parent is missing.");
            return false;
        }

        if (traitButtonParent == null)
        {
            ShowMessage("Trait button parent is missing.");
            return false;
        }

        return true;
    }

    private void ShowBackgroundDescription(
        string message)
    {
        if (backgroundDescriptionText != null)
            backgroundDescriptionText.text = message;
    }

    private void ShowTraitDescription(
        string message)
    {
        if (traitDescriptionText != null)
            traitDescriptionText.text = message;
    }

    private void ShowMessage(
        string message)
    {
        if (messageText != null)
            messageText.text = message;
    }

    private void ClearButtons(
        List<CharacterOptionButtonUI> buttons)
    {
        foreach (CharacterOptionButtonUI button in buttons)
        {
            if (button != null)
                Destroy(button.gameObject);
        }

        buttons.Clear();
    }
}