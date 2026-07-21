using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterCreatorRaceUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private CharacterDataLibrary characterDataLibrary;
    [SerializeField] private CharacterCreator characterCreator;

    [Header("Option Prefab")]
    [SerializeField] private CharacterOptionButtonUI optionButtonPrefab;

    [Header("Base Race UI")]
    [SerializeField] private Transform baseRaceButtonParent;
    [SerializeField] private TMP_Text baseRaceDescriptionText;

    [Header("Subrace UI")]
    [SerializeField] private Transform subraceButtonParent;
    [SerializeField] private TMP_Text subraceDescriptionText;

    [Header("Lineage UI")]
    [SerializeField] private Transform lineageButtonParent;
    [SerializeField]
    private TMP_Text lineageDescriptionText;

    private readonly List<CharacterOptionButtonUI> raceButtons = new();
    private readonly List<CharacterOptionButtonUI> subraceButtons = new();
    private readonly List<CharacterOptionButtonUI> lineageButtons = new();
    private readonly List<string> lineageButtonIds = new();

    private string selectedRaceId;
    private string selectedSubraceId;

    private static readonly BaseRace[] BaseRaceOrder =
    {
        BaseRace.Human,
        BaseRace.Animali,
        BaseRace.Eastern,
        BaseRace.WesternDragon,
        BaseRace.Drakken,
        BaseRace.Griffin,
        BaseRace.Canispar,
        BaseRace.SoulChip
    };

    private void OnEnable()
    {
        BuildRaceButtons();
    }

    private void OnDisable()
    {
        ClearButtons(raceButtons);
        ClearButtons(subraceButtons);
        ClearButtons(lineageButtons);

        lineageButtonIds.Clear();
    }

    public void BuildRaceButtons()
    {
        ClearButtons(raceButtons);
        ClearButtons(subraceButtons);
        ClearButtons(lineageButtons);
        lineageButtonIds.Clear();

        if (!HasRequiredReferences())
            return;

        List<RaceDefinition> raceDefinitions =
            GetOrderedRaceDefinitions();

        foreach (RaceDefinition raceDefinition in raceDefinitions)
        {
            CharacterOptionButtonUI button =
                Instantiate(optionButtonPrefab, baseRaceButtonParent);

            button.name = $"{raceDefinition.raceId}RaceButton";
            button.SetText(GetRaceButtonText(raceDefinition));
            button.SetSelected(false);

            RaceDefinition capturedRace = raceDefinition;
            CharacterOptionButtonUI capturedButton = button;

            button.Button.onClick.RemoveAllListeners();
            button.Button.onClick.AddListener(() =>
                SelectRaceDefinition(capturedRace, capturedButton)
            );

            raceButtons.Add(button);
        }

        if (raceDefinitions.Count > 0)
        {
            RaceDefinition startingRace =
                GetStartingRaceDefinition(raceDefinitions);

            CharacterOptionButtonUI startingButton =
                GetRaceButtonFor(startingRace);

            SelectRaceDefinition(startingRace, startingButton);
        }
        else
        {
            ShowBaseRaceDescription("No race definitions found.");
            ShowSubraceDescription("");
            ShowLineageDescription("");
        }
    }

    private void SelectRaceDefinition(
        RaceDefinition raceDefinition,
        CharacterOptionButtonUI selectedButton)
    {
        if (raceDefinition == null)
            return;

        bool alreadySelected =
            characterCreator != null &&
            string.Equals(
                characterCreator.SelectedRaceId,
                raceDefinition.raceId,
                System.StringComparison.OrdinalIgnoreCase
            );

        if (!alreadySelected)
        {
            if (!characterCreator.SelectRace(
                raceDefinition.raceId,
                out string errorMessage))
            {
                ShowBaseRaceDescription(errorMessage);
                return;
            }
        }

        selectedRaceId = raceDefinition.raceId;

        RefreshSelectedRace(selectedButton);
        ShowBaseRaceDescription(
            GetRaceDescription(raceDefinition)
        );

        BuildSubraceButtons(raceDefinition);
    }

    private void BuildSubraceButtons(
        RaceDefinition raceDefinition)
    {
        ClearButtons(subraceButtons);

        if (raceDefinition == null)
        {
            ShowSubraceDescription("");
            return;
        }

        List<SubraceDefinition> subraceDefinitions =
            characterDataLibrary.GetSubraceDefinitionsForRace(raceDefinition);

        foreach (SubraceDefinition subraceDefinition in subraceDefinitions)
        {
            CharacterOptionButtonUI button =
                Instantiate(optionButtonPrefab, subraceButtonParent);

            button.name = $"{subraceDefinition.subraceId}SubraceButton";
            button.SetText(GetSubraceButtonText(subraceDefinition));
            button.SetSelected(false);

            SubraceDefinition capturedSubrace = subraceDefinition;
            CharacterOptionButtonUI capturedButton = button;

            button.Button.onClick.RemoveAllListeners();
            button.Button.onClick.AddListener(() =>
                SelectSubraceDefinition(capturedSubrace, capturedButton)
            );

            subraceButtons.Add(button);
        }

        if (subraceDefinitions.Count == 0)
        {
            ShowSubraceDescription("No subraces available.");
            return;
        }

        SubraceDefinition startingSubrace =
            GetStartingSubraceDefinition(
                raceDefinition,
                subraceDefinitions
            );

        CharacterOptionButtonUI startingButton =
            GetSubraceButtonFor(startingSubrace);

        SelectSubraceDefinition(startingSubrace, startingButton);
    }

    private void SelectSubraceDefinition(
        SubraceDefinition subraceDefinition,
        CharacterOptionButtonUI selectedButton)
    {
        if (subraceDefinition == null)
            return;

        if (!characterCreator.SelectSubrace(
            subraceDefinition.subraceId,
            out string errorMessage))
        {
            ShowSubraceDescription(errorMessage);
            return;
        }

        selectedSubraceId = subraceDefinition.subraceId;

        RefreshSelectedSubrace(selectedButton);
        ShowSubraceDescription(GetSubraceDescription(subraceDefinition));
        BuildLineageButtons(
            subraceDefinition.race,
            subraceDefinition
        );
    }

    private void BuildLineageButtons(
    RaceDefinition raceDefinition,
    SubraceDefinition selectedSubrace)
    {
        ClearButtons(lineageButtons);
        lineageButtonIds.Clear();

        if (raceDefinition == null ||
            selectedSubrace == null)
        {
            ShowLineageDescription("");
            return;
        }

        if (!raceDefinition.CanUseLineages())
        {
            ShowLineageDescription(
                "This race does not use lineages."
            );

            return;
        }

        List<LineageSelection> lineageOptions =
            characterDataLibrary.GetLineageOptionsForRace(
                raceDefinition,
                selectedSubrace
            );

        foreach (LineageSelection lineage
                 in lineageOptions)
        {
            if (lineage == null ||
                !lineage.IsValid)
            {
                continue;
            }

            CharacterOptionButtonUI button =
                Instantiate(
                    optionButtonPrefab,
                    lineageButtonParent
                );

            string selectionId =
                lineage.SelectionId;

            button.name =
                $"{selectionId}LineageButton";

            button.SetText(
                lineage.DisplayName
            );

            button.SetSelected(false);
            button.SetInteractable(true);

            LineageSelection capturedLineage =
                lineage;

            button.Button.onClick.RemoveAllListeners();

            button.Button.onClick.AddListener(() =>
                ToggleLineage(capturedLineage)
            );

            lineageButtons.Add(button);
            lineageButtonIds.Add(selectionId);
        }

        if (lineageOptions.Count == 0)
        {
            ShowLineageDescription(
                "No lineage options are available."
            );
        }
        else
        {
            ShowLineageDescription(
                "Select a lineage."
            );
        }

        RefreshLineageButtons();
    }

    private void ToggleLineage(
        LineageSelection lineage)
    {
        if (lineage == null ||
            !lineage.IsValid ||
            characterCreator == null)
        {
            return;
        }

        if (!characterCreator.ToggleLineage(
            lineage.SelectionId,
            out string errorMessage))
        {
            ShowLineageDescription(
                errorMessage
            );

            RefreshLineageButtons();
            return;
        }

        ShowLineageDescription(
            GetLineageDescription(lineage)
        );

        RefreshLineageButtons();
    }

    private void RefreshLineageButtons()
    {
        for (int i = 0;
             i < lineageButtons.Count;
             i++)
        {
            CharacterOptionButtonUI button =
                lineageButtons[i];

            if (button == null)
                continue;

            string selectionId =
                i < lineageButtonIds.Count
                    ? lineageButtonIds[i]
                    : "";

            bool selected =
                IsLineageSelected(selectionId);

            button.SetSelected(selected);

            button.SetInteractable(
                selected ||
                CanSelectAnotherLineage()
            );
        }
    }

    private bool IsLineageSelected(
        string selectionId)
    {
        if (characterCreator == null ||
            string.IsNullOrWhiteSpace(selectionId))
        {
            return false;
        }

        foreach (string selectedId
                 in characterCreator.SelectedLineageIds)
        {
            if (string.Equals(
                selectedId,
                selectionId,
                System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private bool CanSelectAnotherLineage()
    {
        if (characterCreator == null ||
            characterDataLibrary == null)
        {
            return false;
        }

        if (!characterDataLibrary.TryGetRaceDefinition(
            characterCreator.SelectedRaceId,
            out RaceDefinition raceDefinition))
        {
            return false;
        }

        int selectedCount =
            characterCreator.SelectedLineageIds.Count;

        if (selectedCount <
            raceDefinition.maxLineages)
        {
            return true;
        }

        return raceDefinition.maxLineages == 1 &&
               selectedCount == 1;
    }

    private string GetLineageDescription(
        LineageSelection lineage)
    {
        if (lineage == null)
            return "";

        string text =
            lineage.DisplayName;

        if (lineage.IsSubrace &&
            lineage.Subrace != null &&
            !string.IsNullOrWhiteSpace(
                lineage.Subrace.description))
        {
            text +=
                $"\n\n{lineage.Subrace.description}";
        }

        if (lineage.IsCustomLineage &&
            lineage.CustomLineage != null &&
            !string.IsNullOrWhiteSpace(
                lineage.CustomLineage.description))
        {
            text +=
                $"\n\n{lineage.CustomLineage.description}";
        }

        return text;
    }

    private bool HasRequiredReferences()
    {
        if (characterDataLibrary == null)
        {
            ShowBaseRaceDescription("CharacterDataLibrary is missing.");
            return false;
        }

        if (characterCreator == null)
        {
            ShowBaseRaceDescription("CharacterCreator is missing.");
            return false;
        }

        if (optionButtonPrefab == null)
        {
            ShowBaseRaceDescription("CharacterOptionButton prefab is missing.");
            return false;
        }

        if (baseRaceButtonParent == null)
        {
            ShowBaseRaceDescription("BaseRaceSelection is missing.");
            return false;
        }

        if (subraceButtonParent == null)
        {
            ShowSubraceDescription("SubraceSelection is missing.");
            return false;
        }

        if (lineageButtonParent == null)
        {
            ShowLineageDescription(
                "LineageSelection is missing."
            );

            return false;
        }

        return true;
    }

    private List<RaceDefinition> GetOrderedRaceDefinitions()
    {
        List<RaceDefinition> ordered = new();

        foreach (BaseRace baseRace in BaseRaceOrder)
        {
            foreach (RaceDefinition raceDefinition in characterDataLibrary.RaceDefinitions)
            {
                if (raceDefinition == null)
                    continue;

                if (ordered.Contains(raceDefinition))
                    continue;

                if (raceDefinition.baseRace == baseRace)
                    ordered.Add(raceDefinition);
            }
        }

        foreach (RaceDefinition raceDefinition in characterDataLibrary.RaceDefinitions)
        {
            if (raceDefinition == null)
                continue;

            if (!ordered.Contains(raceDefinition))
                ordered.Add(raceDefinition);
        }

        return ordered;
    }

    private List<SubraceDefinition> GetSubraceDefinitionsForRace(
        RaceDefinition raceDefinition)
    {
        List<SubraceDefinition> found = new();

        if (raceDefinition == null)
            return found;

        foreach (SubraceDefinition subraceDefinition in characterDataLibrary.SubraceDefinitions)
        {
            if (subraceDefinition == null)
                continue;

            if (subraceDefinition.race == null)
                continue;

            if (subraceDefinition.race == raceDefinition ||
                subraceDefinition.race.raceId == raceDefinition.raceId)
            {
                found.Add(subraceDefinition);
            }
        }

        return found;
    }

    private RaceDefinition GetStartingRaceDefinition(
        List<RaceDefinition> raceDefinitions)
    {
        if (characterCreator != null &&
            !string.IsNullOrWhiteSpace(
                characterCreator.SelectedRaceId))
        {
            foreach (RaceDefinition raceDefinition
                     in raceDefinitions)
            {
                if (raceDefinition == null)
                    continue;

                if (string.Equals(
                    raceDefinition.raceId,
                    characterCreator.SelectedRaceId,
                    System.StringComparison.OrdinalIgnoreCase))
                {
                    return raceDefinition;
                }
            }
        }

        foreach (RaceDefinition raceDefinition
                 in raceDefinitions)
        {
            if (raceDefinition != null &&
                raceDefinition.baseRace == BaseRace.Human)
            {
                return raceDefinition;
            }
        }

        return raceDefinitions[0];
    }

    private SubraceDefinition GetStartingSubraceDefinition(
        RaceDefinition raceDefinition,
        List<SubraceDefinition> subraceDefinitions)
    {
        if (characterCreator != null &&
            !string.IsNullOrWhiteSpace(
                characterCreator.SelectedSubraceId))
        {
            foreach (SubraceDefinition subraceDefinition
                     in subraceDefinitions)
            {
                if (subraceDefinition == null)
                    continue;

                if (string.Equals(
                    subraceDefinition.subraceId,
                    characterCreator.SelectedSubraceId,
                    System.StringComparison.OrdinalIgnoreCase))
                {
                    return subraceDefinition;
                }
            }
        }

        if (raceDefinition != null &&
            raceDefinition.standardSubrace != null)
        {
            foreach (SubraceDefinition subraceDefinition
                     in subraceDefinitions)
            {
                if (subraceDefinition == null)
                    continue;

                if (subraceDefinition ==
                        raceDefinition.standardSubrace ||
                    string.Equals(
                        subraceDefinition.subraceId,
                        raceDefinition.standardSubrace.subraceId,
                        System.StringComparison.OrdinalIgnoreCase))
                {
                    return subraceDefinition;
                }
            }
        }

        foreach (SubraceDefinition subraceDefinition
                 in subraceDefinitions)
        {
            if (subraceDefinition != null)
                return subraceDefinition;
        }

        return null;
    }

    private CharacterOptionButtonUI GetRaceButtonFor(
        RaceDefinition raceDefinition)
    {
        foreach (CharacterOptionButtonUI button in raceButtons)
        {
            if (button == null ||
                raceDefinition == null)
            {
                continue;
            }

            if (button.name == $"{raceDefinition.raceId}RaceButton")
                return button;
        }

        return null;
    }

    private CharacterOptionButtonUI GetSubraceButtonFor(
        SubraceDefinition subraceDefinition)
    {
        foreach (CharacterOptionButtonUI button in subraceButtons)
        {
            if (button == null ||
                subraceDefinition == null)
            {
                continue;
            }

            if (button.name == $"{subraceDefinition.subraceId}SubraceButton")
                return button;
        }

        return null;
    }

    private void RefreshSelectedRace(
        CharacterOptionButtonUI selectedButton)
    {
        foreach (CharacterOptionButtonUI button in raceButtons)
        {
            if (button == null)
                continue;

            button.SetSelected(button == selectedButton);
        }
    }

    private void RefreshSelectedSubrace(
        CharacterOptionButtonUI selectedButton)
    {
        foreach (CharacterOptionButtonUI button in subraceButtons)
        {
            if (button == null)
                continue;

            button.SetSelected(button == selectedButton);
        }
    }

    private void ClearButtons(
        List<CharacterOptionButtonUI> buttons)
    {
        for (int i = buttons.Count - 1; i >= 0; i--)
        {
            if (buttons[i] != null)
                Destroy(buttons[i].gameObject);
        }

        buttons.Clear();
    }

    private void ShowBaseRaceDescription(string message)
    {
        if (baseRaceDescriptionText != null)
            baseRaceDescriptionText.text = message;
    }

    private void ShowSubraceDescription(string message)
    {
        if (subraceDescriptionText != null)
            subraceDescriptionText.text = message;
    }

    private void ShowLineageDescription(
    string message)
    {
        if (lineageDescriptionText != null)
            lineageDescriptionText.text = message;
    }

    private string GetRaceButtonText(
        RaceDefinition raceDefinition)
    {
        if (raceDefinition == null)
            return "";

        if (!string.IsNullOrWhiteSpace(raceDefinition.displayName))
            return raceDefinition.displayName;

        return raceDefinition.baseRace.ToString();
    }

    private string GetSubraceButtonText(
        SubraceDefinition subraceDefinition)
    {
        if (subraceDefinition == null)
            return "";

        if (!string.IsNullOrWhiteSpace(subraceDefinition.displayName))
            return subraceDefinition.displayName;

        return subraceDefinition.subraceId;
    }

    private string GetRaceDescription(
        RaceDefinition raceDefinition)
    {
        if (raceDefinition == null)
            return "";

        if (!string.IsNullOrWhiteSpace(raceDefinition.description))
            return raceDefinition.description;

        return GetRaceButtonText(raceDefinition);
    }

    private string GetSubraceDescription(
        SubraceDefinition subraceDefinition)
    {
        if (subraceDefinition == null)
            return "";

        string text =
            !string.IsNullOrWhiteSpace(
                subraceDefinition.description)
                    ? subraceDefinition.description
                    : GetSubraceButtonText(
                        subraceDefinition
                    );

        string comparisonName =
            GetComparisonName(
                subraceDefinition
            );

        List<string> differences = new();

        AddAttributeDifferences(
            differences,
            subraceDefinition.modifiersFromComparison
        );

        AddBodyDifferences(
            differences,
            subraceDefinition
        );

        text +=
            $"\n\nCompared to {comparisonName}:";

        if (differences.Count == 0)
        {
            text += "\nNo differences.";
        }
        else
        {
            text +=
                $"\n{string.Join("\n", differences)}";
        }

        return text;
    }

    private string GetComparisonName(
    SubraceDefinition subraceDefinition)
    {
        if (subraceDefinition.compareToSubrace != null)
        {
            return GetSubraceButtonText(
                subraceDefinition.compareToSubrace
            );
        }

        if (subraceDefinition.race != null)
        {
            return
                $"{GetRaceButtonText(subraceDefinition.race)} " +
                "race attributes";
        }

        return "race attributes";
    }

    private void AddAttributeDifferences(
        List<string> differences,
        CharacterAttributeModifiers modifiers)
    {
        if (differences == null ||
            modifiers == null)
        {
            return;
        }

        AddDifference(
            differences,
            "Strength",
            modifiers.strength
        );

        AddDifference(
            differences,
            "Dexterity",
            modifiers.dexterity
        );

        AddDifference(
            differences,
            "Agility",
            modifiers.agility
        );

        AddDifference(
            differences,
            "Vitality",
            modifiers.vitality
        );

        AddDifference(
            differences,
            "Endurance",
            modifiers.endurance
        );

        AddDifference(
            differences,
            "Intelligence",
            modifiers.intelligence
        );

        AddDifference(
            differences,
            "Willpower",
            modifiers.willpower
        );

        AddDifference(
            differences,
            "Spirit",
            modifiers.spirit
        );

        AddDifference(
            differences,
            "Perception",
            modifiers.perception
        );
    }

    private void AddDifference(
        List<string> differences,
        string label,
        int value)
    {
        if (differences == null ||
            value == 0)
        {
            return;
        }

        string sign =
            value > 0
                ? "+"
                : "";

        differences.Add(
            $"{label}: {sign}{value}"
        );
    }

    private void AddBodyDifferences(
        List<string> differences,
        SubraceDefinition subraceDefinition)
    {
        if (differences == null ||
            subraceDefinition == null ||
            subraceDefinition.compareToSubrace == null)
        {
            return;
        }

        SubraceDefinition comparison =
            subraceDefinition.compareToSubrace;

        if (comparison.size != subraceDefinition.size)
        {
            differences.Add(
                $"Size: {GetSizeName(comparison.size)} → " +
                $"{GetSizeName(subraceDefinition.size)}"
            );
        }

        if (comparison.bodyType !=
            subraceDefinition.bodyType)
        {
            differences.Add(
                $"Body: {GetBodyName(comparison.bodyType)} → " +
                $"{GetBodyName(subraceDefinition.bodyType)}"
            );
        }
    }

    private string GetSizeName(
        RaceSize size)
    {
        return size switch
        {
            RaceSize.Size1 => "Size 1",
            RaceSize.Size2 => "Size 2",
            RaceSize.TallerSize2 => "Taller Size 2",
            RaceSize.Size3 => "Size 3",
            RaceSize.Size1Feral => "Size 1 Feral",
            RaceSize.Size2Feral => "Size 2 Feral",
            RaceSize.Size3Feral => "Size 3 Feral",
            RaceSize.Dragon => "Dragon",
            RaceSize.BigDragon => "Big Dragon",
            _ => size.ToString()
        };
    }

    private string GetBodyName(
        BodyType bodyType)
    {
        return bodyType switch
        {
            BodyType.Humanoid => "Humanoid",
            BodyType.Quadruped => "Quadruped",
            BodyType.StanceSwitching =>
                "Stance Switching",

            _ => bodyType.ToString()
        };
    }
}