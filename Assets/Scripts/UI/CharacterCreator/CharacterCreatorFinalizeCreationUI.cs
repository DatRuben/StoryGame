using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCreatorFinalizeCreationUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField]
    private CharacterDataLibrary characterDataLibrary;

    [SerializeField]
    private CharacterCreator characterCreator;

    [Header("Final Values")]
    [SerializeField]
    private TMP_Text finalAttributesText;

    [SerializeField]
    private TMP_Text finalStatsText;

    [Header("Creation")]
    [SerializeField]
    private TMP_Text creationMessageText;

    [SerializeField]
    private Button createCharacterButton;

    [Header("Navigation")]
    [SerializeField]
    private Menus menus;

    private void OnEnable()
    {
        SubscribeToCreator();
        HookControls();

        ShowCreationMessage("");
        Refresh();
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

        characterCreator.SelectionChanged -= Refresh;
        characterCreator.SelectionChanged += Refresh;
    }

    private void UnsubscribeFromCreator()
    {
        if (characterCreator == null)
            return;

        characterCreator.SelectionChanged -= Refresh;
    }

    private void HookControls()
    {
        if (createCharacterButton == null)
            return;

        createCharacterButton.onClick.RemoveListener(
            CreateCharacter
        );

        createCharacterButton.onClick.AddListener(
            CreateCharacter
        );
    }

    private void UnhookControls()
    {
        if (createCharacterButton == null)
            return;

        createCharacterButton.onClick.RemoveListener(
            CreateCharacter
        );
    }

    private void CreateCharacter()
    {
        if (characterCreator == null)
        {
            ShowCreationMessage(
                "Character Creator is missing."
            );

            return;
        }

        bool created =
            characterCreator.TryCreateCharacter(
                characterCreator.SelectedCharacterName,
                out CharacterProfileData _,
                out string errorMessage
            );

        if (!created)
        {
            ShowCreationMessage(
                errorMessage
            );

            RefreshCreateButton();
            return;
        }

        ShowCreationMessage("");

        characterCreator.ResetCreator();

        if (menus != null)
        {
            menus.ShowCharacterSelect();
        }
        else
        {
            Debug.LogWarning(
                "Character was saved, but Menus is missing.",
                this
            );
        }
    }

    private void Refresh()
    {
        RefreshCreateButton();

        if (!TryResolveStats(
            out ResolvedCharacterStats resolvedStats))
        {
            ShowMissingData();
            return;
        }

        ShowFinalAttributes(
            resolvedStats.finalAttributes
        );

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
            new List<string>(
                characterCreator.SelectedLineageIds
            );

        List<LineageSelection> lineages =
            characterDataLibrary.GetLineageSelections(
                lineageIds
            );

        characterDataLibrary.TryGetBackgroundDefinition(
            characterCreator.SelectedBackgroundId,
            out BackgroundDefinition backgroundDefinition
        );

        List<string> traitIds =
            new List<string>(
                characterCreator.SelectedTraitIds
            );

        List<TraitDefinition> traits =
            characterDataLibrary.GetTraitDefinitions(
                traitIds
            );

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

        StringBuilder text = new StringBuilder();

        text.AppendLine("FINAL ATTRIBUTES");
        text.AppendLine();

        AddValue(
            text,
            "Strength",
            attributes.strength
        );

        AddValue(
            text,
            "Dexterity",
            attributes.dexterity
        );

        AddValue(
            text,
            "Agility",
            attributes.agility
        );

        AddValue(
            text,
            "Vitality",
            attributes.vitality
        );

        AddValue(
            text,
            "Endurance",
            attributes.endurance
        );

        AddValue(
            text,
            "Intelligence",
            attributes.intelligence
        );

        AddValue(
            text,
            "Willpower",
            attributes.willpower
        );

        AddValue(
            text,
            "Spirit",
            attributes.spirit
        );

        AddValue(
            text,
            "Perception",
            attributes.perception
        );

        finalAttributesText.text =
            text.ToString();
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

        StringBuilder text = new StringBuilder();

        text.AppendLine("FINAL STATS");
        text.AppendLine();

        AddValue(
            text,
            "Health",
            stats.maxHealth
        );

        AddValue(
            text,
            "Soul Barrier",
            stats.maxSoulBarrier
        );

        AddValue(
            text,
            "Stamina",
            stats.maxStamina
        );

        AddValue(
            text,
            "Aether",
            stats.maxAether
        );

        AddValue(
            text,
            "Mass",
            stats.mass
        );

        AddValue(
            text,
            "Poise",
            stats.poise
        );

        AddValue(
            text,
            "Carry Weight",
            baseStats.carryWeight
        );

        finalStatsText.text =
            text.ToString();
    }

    private void AddValue(
        StringBuilder text,
        string label,
        int value)
    {
        text.AppendLine(
            $"{label}: {value}"
        );
    }

    private void AddValue(
        StringBuilder text,
        string label,
        float value)
    {
        text.AppendLine(
            $"{label}: {value:0.##}"
        );
    }

    private void RefreshCreateButton()
    {
        if (createCharacterButton == null)
            return;

        bool hasName =
            characterCreator != null &&
            !string.IsNullOrWhiteSpace(
                characterCreator.SelectedCharacterName
            );

        bool hasValidSelections =
            characterCreator != null &&
            characterCreator.CanCreateCharacter(
                out string _
            );

        createCharacterButton.interactable =
            hasName &&
            hasValidSelections;
    }

    private void ShowCreationMessage(
        string message)
    {
        if (creationMessageText == null)
            return;

        creationMessageText.text =
            message ?? "";
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