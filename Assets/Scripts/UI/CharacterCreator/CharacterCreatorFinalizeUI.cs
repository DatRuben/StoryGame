using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class CharacterCreatorFinalizeUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private CharacterDataLibrary characterDataLibrary;
    [SerializeField] private CharacterCreator characterCreator;

    [Header("Output")]
    [SerializeField] private TMP_Text summaryText;

    private void OnEnable()
    {
        SubscribeToCreator();
        Refresh();
    }

    private void OnDisable()
    {
        UnsubscribeFromCreator();
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

    private void Refresh()
    {
        if (summaryText == null)
            return;

        if (characterCreator == null ||
            characterDataLibrary == null)
        {
            summaryText.text =
                "Finalize\n\nCharacterCreator or CharacterDataLibrary is missing.";

            return;
        }

        StringBuilder summary = new();

        summary.AppendLine("Finalize");
        summary.AppendLine();
        summary.AppendLine("This page does not create the character yet.");
        summary.AppendLine();

        summary.AppendLine($"Name: {GetNameText()}");
        summary.AppendLine($"Gender: {characterCreator.SelectedGender}");
        summary.AppendLine();

        summary.AppendLine($"Race: {GetRaceText()}");
        summary.AppendLine($"Subrace: {GetSubraceText()}");
        summary.AppendLine($"Lineage: {GetLineageText()}");
        summary.AppendLine();

        summary.AppendLine($"Background: {GetBackgroundText()}");
        summary.AppendLine($"Traits: {GetTraitText()}");
        summary.AppendLine();

        CharacterAppearanceData appearance =
            characterCreator.SelectedAppearance;

        summary.AppendLine("Appearance");
        summary.AppendLine($"Body Scale: {appearance.bodyScale:0.00}");
        summary.AppendLine($"Hue: {appearance.hue:0.00}");
        summary.AppendLine($"Saturation: {appearance.saturation:0.00}");
        summary.AppendLine($"Value: {appearance.value:0.00}");

        summaryText.text = summary.ToString();
    }

    private string GetNameText()
    {
        if (string.IsNullOrWhiteSpace(characterCreator.SelectedCharacterName))
            return "Unnamed";

        return characterCreator.SelectedCharacterName;
    }

    private string GetRaceText()
    {
        if (characterDataLibrary.TryGetRaceDefinition(
            characterCreator.SelectedRaceId,
            out RaceDefinition raceDefinition))
        {
            return GetDisplayName(
                raceDefinition.displayName,
                raceDefinition.raceId
            );
        }

        return "None selected";
    }

    private string GetSubraceText()
    {
        if (characterDataLibrary.TryGetSubraceDefinition(
            characterCreator.SelectedSubraceId,
            out SubraceDefinition subraceDefinition))
        {
            return GetDisplayName(
                subraceDefinition.displayName,
                subraceDefinition.subraceId
            );
        }

        return "None selected";
    }

    private string GetLineageText()
    {
        IReadOnlyList<string> lineageIds =
            characterCreator.SelectedLineageIds;

        if (lineageIds.Count == 0)
            return "None";

        List<string> names = new();

        foreach (string lineageId in lineageIds)
        {
            if (!characterDataLibrary.TryGetLineageDefinition(
                lineageId,
                out LineageDefinition lineageDefinition))
            {
                continue;
            }

            names.Add(
                GetDisplayName(
                    lineageDefinition.displayName,
                    lineageDefinition.lineageId
                )
            );
        }

        if (names.Count == 0)
            return "None";

        return string.Join(", ", names);
    }

    private string GetBackgroundText()
    {
        if (characterDataLibrary.TryGetBackgroundDefinition(
            characterCreator.SelectedBackgroundId,
            out BackgroundDefinition backgroundDefinition))
        {
            return GetDisplayName(
                backgroundDefinition.displayName,
                backgroundDefinition.backgroundId
            );
        }

        return "None selected";
    }

    private string GetTraitText()
    {
        IReadOnlyList<string> traitIds =
            characterCreator.SelectedTraitIds;

        if (traitIds.Count == 0)
            return "None";

        List<string> names = new();

        foreach (string traitId in traitIds)
        {
            if (!characterDataLibrary.TryGetTraitDefinition(
                traitId,
                out TraitDefinition traitDefinition))
            {
                continue;
            }

            names.Add(
                GetDisplayName(
                    traitDefinition.displayName,
                    traitDefinition.traitId
                )
            );
        }

        if (names.Count == 0)
            return "None";

        return string.Join(", ", names);
    }

    private string GetDisplayName(
        string displayName,
        string fallbackId)
    {
        if (!string.IsNullOrWhiteSpace(displayName))
            return displayName;

        if (!string.IsNullOrWhiteSpace(fallbackId))
            return fallbackId;

        return "Unknown";
    }
}