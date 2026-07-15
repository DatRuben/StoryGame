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

    [Header("Racial Passive Details")]
    [SerializeField] private TMP_Text racialPassiveText;

    private readonly List<CharacterOptionButtonUI> backgroundButtons = new();
    private readonly List<string> backgroundButtonIds = new();

    private readonly List<CharacterOptionButtonUI> traitButtons = new();
    private readonly List<string> traitButtonIds = new();

    private void OnEnable()
    {
        if (characterCreator != null)
            characterCreator.SelectionChanged += RefreshUI;

        BuildButtons();
    }

    private void OnDisable()
    {
        if (characterCreator != null)
            characterCreator.SelectionChanged -= RefreshUI;

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

        ShowBackgroundDescription("");
        ShowTraitDescription("");
        RefreshRacialPassiveText();
        RefreshUI();
    }

    private void BuildBackgroundButtons()
    {
        foreach (BackgroundDefinition backgroundDefinition in characterDataLibrary.BackgroundDefinitions)
        {
            if (backgroundDefinition == null)
                continue;

            CharacterOptionButtonUI button =
                Instantiate(optionButtonPrefab, backgroundButtonParent);

            button.name = $"{backgroundDefinition.backgroundId}BackgroundButton";
            button.SetText(GetBackgroundButtonText(backgroundDefinition));
            button.SetSelected(false);
            button.SetInteractable(true);

            BackgroundDefinition capturedBackground = backgroundDefinition;

            button.Button.onClick.RemoveAllListeners();
            button.Button.onClick.AddListener(() =>
                SelectBackground(capturedBackground)
            );

            backgroundButtons.Add(button);
            backgroundButtonIds.Add(backgroundDefinition.backgroundId);
        }

        if (string.IsNullOrWhiteSpace(characterCreator.SelectedBackgroundId))
        {
            BackgroundDefinition startingBackground =
                characterDataLibrary.GetDefaultBackgroundDefinition();

            if (startingBackground != null)
                SelectBackground(startingBackground);
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
            button.SetInteractable(true);

            TraitDefinition capturedTrait = traitDefinition;

            button.Button.onClick.RemoveAllListeners();
            button.Button.onClick.AddListener(() =>
                ToggleTrait(capturedTrait)
            );

            traitButtons.Add(button);
            traitButtonIds.Add(traitDefinition.traitId);
        }
    }

    private void SelectBackground(
        BackgroundDefinition backgroundDefinition)
    {
        if (backgroundDefinition == null ||
            characterCreator == null)
        {
            return;
        }

        if (!characterCreator.SelectBackground(
            backgroundDefinition.backgroundId,
            out _))
        {
            RefreshUI();
            return;
        }

        ShowBackgroundDescription(
            GetBackgroundDescription(backgroundDefinition)
        );

        RefreshUI();
    }

    private void ToggleTrait(
        TraitDefinition traitDefinition)
    {
        if (traitDefinition == null ||
            characterCreator == null)
        {
            return;
        }

        if (!CanUseTrait(traitDefinition.traitId) &&
            !IsTraitSelected(traitDefinition.traitId))
        {
            RefreshUI();
            return;
        }

        if (!characterCreator.ToggleTrait(
            traitDefinition.traitId,
            out _))
        {
            RefreshUI();
            return;
        }

        ShowTraitDescription(
            GetTraitDescription(traitDefinition)
        );

        RefreshUI();
    }

    private void RefreshUI()
    {
        RefreshBackgroundButtons();
        RefreshTraitButtons();
        RefreshRacialPassiveText();
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
            button.SetInteractable(true);
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

            bool selected =
                IsTraitSelected(traitId);

            button.SetSelected(selected);
            button.SetInteractable(selected || CanUseTrait(traitId));
        }
    }

    private bool CanUseTrait(
        string traitId)
    {
        if (characterCreator == null ||
            characterDataLibrary == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(traitId))
            return false;

        if (!characterDataLibrary.TryGetTraitDefinition(
            traitId,
            out TraitDefinition traitDefinition))
        {
            return false;
        }

        foreach (string selectedTraitId in characterCreator.SelectedTraitIds)
        {
            if (!characterDataLibrary.TryGetTraitDefinition(
                selectedTraitId,
                out TraitDefinition selectedTrait))
            {
                continue;
            }

            if (selectedTrait == traitDefinition)
                continue;

            if (traitDefinition.IsMutuallyExclusiveWith(selectedTrait) ||
                selectedTrait.IsMutuallyExclusiveWith(traitDefinition))
            {
                return false;
            }
        }

        return true;
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

        if (string.IsNullOrWhiteSpace(backgroundDefinition.displayName))
            return backgroundDefinition.name;

        return backgroundDefinition.displayName;
    }

    private string GetTraitButtonText(
        TraitDefinition traitDefinition)
    {
        if (traitDefinition == null)
            return "Missing Trait";

        if (string.IsNullOrWhiteSpace(traitDefinition.displayName))
            return traitDefinition.name;

        return traitDefinition.displayName;
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

        List<string> lines = new();

        AddModifierLine(lines, "Strength", modifiers.strength);
        AddModifierLine(lines, "Dexterity", modifiers.dexterity);
        AddModifierLine(lines, "Agility", modifiers.agility);
        AddModifierLine(lines, "Vitality", modifiers.vitality);
        AddModifierLine(lines, "Endurance", modifiers.endurance);
        AddModifierLine(lines, "Intelligence", modifiers.intelligence);
        AddModifierLine(lines, "Willpower", modifiers.willpower);
        AddModifierLine(lines, "Spirit", modifiers.spirit);
        AddModifierLine(lines, "Perception", modifiers.perception);

        if (lines.Count == 0)
            return "None";

        return string.Join("\n", lines);
    }

    private void AddModifierLine(
        List<string> lines,
        string label,
        int value)
    {
        if (value == 0)
            return;

        lines.Add($"{label}: {FormatModifier(value)}");
    }

    private string FormatModifier(
        int value)
    {
        if (value > 0)
            return $"+{value}";

        return value.ToString();
    }

    private void RefreshRacialPassiveText()
    {
        if (racialPassiveText != null)
        {
            racialPassiveText.text =
                "Racial passives will be shown here later.";
        }
    }

    private bool HasRequiredReferences()
    {
        if (characterDataLibrary == null)
            return false;

        if (characterCreator == null)
            return false;

        if (optionButtonPrefab == null)
            return false;

        if (backgroundButtonParent == null)
            return false;

        if (traitButtonParent == null)
            return false;

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