using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterCreatorFinalizeSummaryUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField]
    private CharacterDataLibrary characterDataLibrary;

    [SerializeField]
    private CharacterCreator characterCreator;

    [Header("Character Name")]
    [SerializeField]
    private TMP_InputField characterNameInput;

    [Header("Selected Options")]
    [SerializeField]
    private TMP_Text genderText;

    [SerializeField]
    private TMP_Text raceText;

    [SerializeField]
    private TMP_Text subraceText;

    [SerializeField]
    private TMP_Text lineageText;

    [SerializeField]
    private TMP_Text backgroundText;

    [SerializeField]
    private TMP_Text traitText;

    private void OnEnable()
    {
        SubscribeToCreator();
        HookControls();

        RefreshNameInput();
        RefreshSummary();
    }

    private void OnDisable()
    {
        UnhookControls();
        UnsubscribeFromCreator();
    }

    private void SubscribeToCreator()
    {
        if (characterCreator == null)
            return;

        characterCreator.SelectionChanged -= RefreshSummary;
        characterCreator.SelectionChanged += RefreshSummary;
    }

    private void UnsubscribeFromCreator()
    {
        if (characterCreator == null)
            return;

        characterCreator.SelectionChanged -= RefreshSummary;
    }

    private void HookControls()
    {
        if (characterNameInput == null)
            return;

        characterNameInput.onValueChanged.RemoveListener(
            OnNameChanged
        );

        characterNameInput.onValueChanged.AddListener(
            OnNameChanged
        );
    }

    private void UnhookControls()
    {
        if (characterNameInput == null)
            return;

        characterNameInput.onValueChanged.RemoveListener(
            OnNameChanged
        );
    }

    private void OnNameChanged(
        string characterName)
    {
        if (characterCreator == null)
            return;

        characterCreator.SetCharacterName(
            characterName
        );
    }

    private void RefreshNameInput()
    {
        if (characterNameInput == null ||
            characterCreator == null)
        {
            return;
        }

        characterNameInput.SetTextWithoutNotify(
            characterCreator.SelectedCharacterName
        );
    }

    private void RefreshSummary()
    {
        if (characterCreator == null ||
            characterDataLibrary == null)
        {
            ShowUnavailable();
            return;
        }

        SetText(
            genderText,
            characterCreator.SelectedGender.ToString()
        );

        SetText(
            raceText,
            GetRaceName()
        );

        SetText(
            subraceText,
            GetSubraceName()
        );

        SetText(
            lineageText,
            GetLineageNames()
        );

        SetText(
            backgroundText,
            GetBackgroundName()
        );

        SetText(
            traitText,
            GetTraitNames()
        );
    }

    private string GetRaceName()
    {
        if (!characterDataLibrary.TryGetRaceDefinition(
            characterCreator.SelectedRaceId,
            out RaceDefinition raceDefinition) ||
            raceDefinition == null)
        {
            return "None";
        }

        return raceDefinition.displayName;
    }

    private string GetSubraceName()
    {
        if (!characterDataLibrary.TryGetSubraceDefinition(
            characterCreator.SelectedSubraceId,
            out SubraceDefinition subraceDefinition) ||
            subraceDefinition == null)
        {
            return "None";
        }

        return subraceDefinition.displayName;
    }

    private string GetLineageNames()
    {
        List<string> lineageIds =
            new List<string>(
                characterCreator.SelectedLineageIds
            );

        List<LineageSelection> lineages =
            characterDataLibrary.GetLineageSelections(
                lineageIds
            );

        if (lineages == null ||
            lineages.Count == 0)
        {
            return "None";
        }

        List<string> names =
            new List<string>();

        foreach (LineageSelection lineage in lineages)
        {
            if (lineage == null ||
                string.IsNullOrWhiteSpace(
                    lineage.DisplayName))
            {
                continue;
            }

            names.Add(
                lineage.DisplayName
            );
        }

        return names.Count > 0
            ? string.Join(", ", names)
            : "None";
    }

    private string GetBackgroundName()
    {
        if (!characterDataLibrary
            .TryGetBackgroundDefinition(
                characterCreator.SelectedBackgroundId,
                out BackgroundDefinition backgroundDefinition) ||
            backgroundDefinition == null)
        {
            return "None";
        }

        return backgroundDefinition.displayName;
    }

    private string GetTraitNames()
    {
        List<string> traitIds =
            new List<string>(
                characterCreator.SelectedTraitIds
            );

        List<TraitDefinition> traits =
            characterDataLibrary.GetTraitDefinitions(
                traitIds
            );

        if (traits == null ||
            traits.Count == 0)
        {
            return "None";
        }

        List<string> names =
            new List<string>();

        foreach (TraitDefinition trait in traits)
        {
            if (trait == null ||
                string.IsNullOrWhiteSpace(
                    trait.displayName))
            {
                continue;
            }

            names.Add(
                trait.displayName
            );
        }

        return names.Count > 0
            ? string.Join(", ", names)
            : "None";
    }

    private void SetText(
        TMP_Text target,
        string value)
    {
        if (target == null)
            return;

        target.text =
            string.IsNullOrWhiteSpace(value)
                ? "None"
                : value;
    }

    private void ShowUnavailable()
    {
        SetText(
            genderText,
            "Unavailable"
        );

        SetText(
            raceText,
            "Unavailable"
        );

        SetText(
            subraceText,
            "Unavailable"
        );

        SetText(
            lineageText,
            "Unavailable"
        );

        SetText(
            backgroundText,
            "Unavailable"
        );

        SetText(
            traitText,
            "Unavailable"
        );
    }
}