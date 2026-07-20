using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class CharacterCreatorFinalizeUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private CharacterDataLibrary characterDataLibrary;
    [SerializeField] private CharacterCreator characterCreator;

    [Header("Right Panel")]
    [SerializeField] private TMP_Text finalAttributesText;
    [SerializeField] private TMP_Text finalStatsText;

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
        if (!TryResolveStats(
            out ResolvedCharacterStats resolvedStats))
        {
            ShowMissingData();
            return;
        }

        ShowFinalAttributes(resolvedStats.finalAttributes);
        ShowFinalStats(resolvedStats);
    }

    private bool TryResolveStats(
        out ResolvedCharacterStats resolvedStats)
    {
        resolvedStats = null;

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

        if (!characterDataLibrary.TryGetSubraceDefinition(
            characterCreator.SelectedSubraceId,
            out SubraceDefinition subraceDefinition))
        {
            return false;
        }

        List<string> lineageIds =
            new(characterCreator.SelectedLineageIds);

        List<LineageSelection> lineages =
            characterDataLibrary.GetLineageSelections(
                lineageIds
            );

        characterDataLibrary.TryGetBackgroundDefinition(
            characterCreator.SelectedBackgroundId,
            out BackgroundDefinition backgroundDefinition
        );

        List<string> traitIds =
            new(characterCreator.SelectedTraitIds);

        List<TraitDefinition> traits =
            characterDataLibrary.GetTraitDefinitions(traitIds);

        resolvedStats =
            CharacterStatsResolver.ResolveCharacter(
                raceDefinition,
                subraceDefinition,
                lineages,
                backgroundDefinition,
                traits
            );

        return resolvedStats != null;
    }

    private void ShowFinalAttributes(
        CharacterAttributes attributes)
    {
        if (finalAttributesText == null)
            return;

        if (attributes == null)
        {
            finalAttributesText.text =
                "FINAL ATTRIBUTES\n\nUnavailable";

            return;
        }

        StringBuilder text = new();

        text.AppendLine("FINAL ATTRIBUTES");
        text.AppendLine();

        AddValue(text, "Strength", attributes.strength);
        AddValue(text, "Dexterity", attributes.dexterity);
        AddValue(text, "Agility", attributes.agility);
        AddValue(text, "Vitality", attributes.vitality);
        AddValue(text, "Endurance", attributes.endurance);
        AddValue(text, "Intelligence", attributes.intelligence);
        AddValue(text, "Willpower", attributes.willpower);
        AddValue(text, "Spirit", attributes.spirit);
        AddValue(text, "Perception", attributes.perception);

        finalAttributesText.text = text.ToString();
    }

    private void ShowFinalStats(
        ResolvedCharacterStats resolvedStats)
    {
        if (finalStatsText == null)
            return;

        FinalCharacterStats stats =
            resolvedStats.finalStats;

        CharacterBaseStats baseStats =
            resolvedStats.totalBaseStats;

        if (stats == null ||
            baseStats == null)
        {
            finalStatsText.text =
                "FINAL STATS\n\nUnavailable";

            return;
        }

        StringBuilder text = new();

        text.AppendLine("FINAL STATS");
        text.AppendLine();

        AddValue(text, "Health", stats.maxHealth);
        AddValue(text, "Soul Barrier", stats.maxSoulBarrier);
        AddValue(text, "Stamina", stats.maxStamina);
        AddValue(text, "Aether", stats.maxAether);
        AddValue(text, "Mass", stats.mass);
        AddValue(text, "Poise", stats.poise);
        AddValue(text, "Carry Weight", baseStats.carryWeight);

        finalStatsText.text = text.ToString();
    }

    private void AddValue(
        StringBuilder text,
        string label,
        int value)
    {
        text.AppendLine($"{label}: {value}");
    }

    private void AddValue(
        StringBuilder text,
        string label,
        float value)
    {
        text.AppendLine($"{label}: {value:0.##}");
    }

    private void ShowMissingData()
    {
        if (finalAttributesText != null)
        {
            finalAttributesText.text =
                "FINAL ATTRIBUTES\n\nUnavailable";
        }

        if (finalStatsText != null)
        {
            finalStatsText.text =
                "FINAL STATS\n\nUnavailable";
        }
    }
}