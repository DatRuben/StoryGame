using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterCreatorTraitsDetailsUI :
    MonoBehaviour
{
    [Header("Data")]
    [SerializeField]
    private CharacterDataLibrary characterDataLibrary;

    [SerializeField]
    private CharacterCreator characterCreator;

    [Header("Display")]
    [SerializeField]
    private TMP_Text backgroundDescriptionText;

    [SerializeField]
    private TMP_Text traitDescriptionText;

    [SerializeField]
    private TMP_Text racialPassiveText;

    private string shownTraitId = "";

    private void OnEnable()
    {
        SubscribeToCreator();
        Refresh();
    }

    private void OnDisable()
    {
        UnsubscribeFromCreator();
        shownTraitId = "";
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

    public void ShowBackground(
        BackgroundDefinition backgroundDefinition)
    {
        ShowBackgroundDescription(
            GetBackgroundDescription(
                backgroundDefinition
            )
        );
    }

    public void ShowTrait(
        TraitDefinition traitDefinition)
    {
        shownTraitId =
            traitDefinition != null
                ? traitDefinition.traitId
                : "";

        RefreshTraitDescription();
    }

    private void Refresh()
    {
        RefreshBackgroundDescription();
        RefreshTraitDescription();
        RefreshRacialPassiveText();
    }

    private void RefreshBackgroundDescription()
    {
        if (characterCreator == null ||
            characterDataLibrary == null)
        {
            ShowBackgroundDescription("");
            return;
        }

        string backgroundId =
            characterCreator.SelectedBackgroundId;

        if (string.IsNullOrWhiteSpace(
                backgroundId) ||
            !characterDataLibrary
                .TryGetBackgroundDefinition(
                    backgroundId,
                    out BackgroundDefinition
                        backgroundDefinition))
        {
            ShowBackgroundDescription("");
            return;
        }

        ShowBackground(
            backgroundDefinition
        );
    }

    private void RefreshTraitDescription()
    {
        if (characterCreator == null ||
            characterDataLibrary == null)
        {
            ShowTraitDescription("");
            return;
        }

        string traitId =
            shownTraitId;

        if (string.IsNullOrWhiteSpace(traitId) &&
            characterCreator.SelectedTraitIds.Count > 0)
        {
            traitId =
                characterCreator.SelectedTraitIds[
                    characterCreator
                        .SelectedTraitIds.Count - 1
                ];
        }

        if (string.IsNullOrWhiteSpace(traitId) ||
            !characterDataLibrary
                .TryGetTraitDefinition(
                    traitId,
                    out TraitDefinition
                        traitDefinition))
        {
            ShowTraitDescription("");
            return;
        }

        ShowTraitDescription(
            GetTraitDescription(
                traitDefinition
            )
        );
    }

    private string GetBackgroundDescription(
        BackgroundDefinition backgroundDefinition)
    {
        if (backgroundDefinition == null)
            return "";

        string text =
            GetBackgroundName(
                backgroundDefinition
            );

        if (!string.IsNullOrWhiteSpace(
                backgroundDefinition.description))
        {
            text +=
                $"\n\n{backgroundDefinition.description}";
        }

        text +=
            "\n\nModifiers:\n" +
            GetModifierText(
                backgroundDefinition.modifiers
            );

        return text;
    }

    private string GetTraitDescription(
        TraitDefinition traitDefinition)
    {
        if (traitDefinition == null)
            return "";

        string text =
            GetTraitName(
                traitDefinition
            );

        if (!string.IsNullOrWhiteSpace(
                traitDefinition.description))
        {
            text +=
                $"\n\n{traitDefinition.description}";
        }

        text +=
            "\n\nModifiers:\n" +
            GetModifierText(
                traitDefinition.modifiers
            );

        return text;
    }

    private string GetBackgroundName(
        BackgroundDefinition backgroundDefinition)
    {
        if (backgroundDefinition == null)
            return "Missing Background";

        return string.IsNullOrWhiteSpace(
            backgroundDefinition.displayName)
                ? backgroundDefinition.name
                : backgroundDefinition.displayName;
    }

    private string GetTraitName(
        TraitDefinition traitDefinition)
    {
        if (traitDefinition == null)
            return "Missing Trait";

        return string.IsNullOrWhiteSpace(
            traitDefinition.displayName)
                ? traitDefinition.name
                : traitDefinition.displayName;
    }

    private string GetModifierText(
        CharacterAttributeModifiers modifiers)
    {
        if (modifiers == null)
            return "None";

        List<string> lines = new();

        AddModifierLine(
            lines,
            "Strength",
            modifiers.strength
        );

        AddModifierLine(
            lines,
            "Dexterity",
            modifiers.dexterity
        );

        AddModifierLine(
            lines,
            "Agility",
            modifiers.agility
        );

        AddModifierLine(
            lines,
            "Vitality",
            modifiers.vitality
        );

        AddModifierLine(
            lines,
            "Endurance",
            modifiers.endurance
        );

        AddModifierLine(
            lines,
            "Intelligence",
            modifiers.intelligence
        );

        AddModifierLine(
            lines,
            "Willpower",
            modifiers.willpower
        );

        AddModifierLine(
            lines,
            "Spirit",
            modifiers.spirit
        );

        AddModifierLine(
            lines,
            "Perception",
            modifiers.perception
        );

        return lines.Count > 0
            ? string.Join("\n", lines)
            : "None";
    }

    private void AddModifierLine(
        List<string> lines,
        string label,
        int value)
    {
        if (value == 0)
            return;

        lines.Add(
            $"{label}: {FormatModifier(value)}"
        );
    }

    private string FormatModifier(
        int value)
    {
        return value > 0
            ? $"+{value}"
            : value.ToString();
    }

    private void RefreshRacialPassiveText()
    {
        if (racialPassiveText != null)
        {
            racialPassiveText.text =
                "Racial passives will be shown here later.";
        }
    }

    private void ShowBackgroundDescription(
        string message)
    {
        if (backgroundDescriptionText != null)
        {
            backgroundDescriptionText.text =
                message ?? "";
        }
    }

    private void ShowTraitDescription(
        string message)
    {
        if (traitDescriptionText != null)
        {
            traitDescriptionText.text =
                message ?? "";
        }
    }
}