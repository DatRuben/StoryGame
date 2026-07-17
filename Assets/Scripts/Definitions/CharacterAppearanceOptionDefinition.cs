using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum CharacterAppearanceOptionCategory
{
    Head,
    Ears,
    Horns,
    Tail,
    Hair,
    Eyes,
    Markings
}

public enum CharacterAppearanceOptionAvailability
{
    Hidden,
    Locked,
    Available
}

[CreateAssetMenu(
    menuName = "Game/Character Appearance Option"
)]
public class CharacterAppearanceOptionDefinition :
    ScriptableObject
{
    [Header("Identity")]
    [HideInInspector]
    public string optionId;

    public string displayName;

    public CharacterAppearanceOptionCategory category;

    [TextArea]
    public string description;

    [Header("Creator Display")]
    public Sprite previewImage;

    [Tooltip(
        "Optional visual prefab for the finished model. " +
        "This is not used by the temporary capsule yet."
    )]
    public GameObject visualPrefab;

    [Header("Race Visibility")]
    [Tooltip(
        "The option is hidden when the selected race " +
        "is not in this list."
    )]
    public List<RaceDefinition> shownForRaces = new();

    [Header("Subrace Or Lineage Requirement")]
    [Tooltip(
        "Leave both requirement lists empty to make " +
        "the option available to every subrace and " +
        "lineage of the shown races."
    )]
    public List<SubraceDefinition> allowedSubraces = new();

    public List<LineageDefinition> allowedLineages = new();

    [Header("Defaults")]
    [Tooltip(
        "Used as a fallback when the previously selected " +
        "option is no longer available."
    )]
    public bool isDefaultOption;

    public CharacterAppearanceOptionAvailability GetAvailability(
        RaceDefinition selectedRace,
        SubraceDefinition selectedSubrace,
        List<LineageDefinition> selectedLineages)
    {
        if (!IsShownForRace(selectedRace))
        {
            return CharacterAppearanceOptionAvailability.Hidden;
        }

        if (!HasAncestryRequirements())
        {
            return CharacterAppearanceOptionAvailability.Available;
        }

        if (IsAllowedForSubrace(selectedSubrace))
        {
            return CharacterAppearanceOptionAvailability.Available;
        }

        if (IsAllowedForAnyLineage(selectedLineages))
        {
            return CharacterAppearanceOptionAvailability.Available;
        }

        return CharacterAppearanceOptionAvailability.Locked;
    }

    private bool IsShownForRace(
        RaceDefinition selectedRace)
    {
        if (selectedRace == null ||
            shownForRaces == null ||
            shownForRaces.Count == 0)
        {
            return false;
        }

        foreach (RaceDefinition raceDefinition
                 in shownForRaces)
        {
            if (raceDefinition == null)
                continue;

            if (raceDefinition == selectedRace ||
                raceDefinition.raceId ==
                selectedRace.raceId)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasAncestryRequirements()
    {
        bool hasSubraceRequirements =
            allowedSubraces != null &&
            allowedSubraces.Count > 0;

        bool hasLineageRequirements =
            allowedLineages != null &&
            allowedLineages.Count > 0;

        return hasSubraceRequirements ||
               hasLineageRequirements;
    }

    private bool IsAllowedForSubrace(
        SubraceDefinition selectedSubrace)
    {
        if (selectedSubrace == null ||
            allowedSubraces == null)
        {
            return false;
        }

        foreach (SubraceDefinition subraceDefinition
                 in allowedSubraces)
        {
            if (subraceDefinition == null)
                continue;

            if (subraceDefinition == selectedSubrace ||
                subraceDefinition.subraceId ==
                selectedSubrace.subraceId)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsAllowedForAnyLineage(
        List<LineageDefinition> selectedLineages)
    {
        if (selectedLineages == null ||
            allowedLineages == null)
        {
            return false;
        }

        foreach (LineageDefinition selectedLineage
                 in selectedLineages)
        {
            if (selectedLineage == null)
                continue;

            foreach (LineageDefinition allowedLineage
                     in allowedLineages)
            {
                if (allowedLineage == null)
                    continue;

                if (allowedLineage == selectedLineage ||
                    allowedLineage.lineageId ==
                    selectedLineage.lineageId)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(displayName))
            displayName = name;

        optionId = MakeId(displayName);
    }

    private string MakeId(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        StringBuilder builder = new();

        foreach (char character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(
                    char.ToLowerInvariant(character)
                );
            }
            else if (builder.Length > 0 &&
                     builder[^1] != '_')
            {
                builder.Append('_');
            }
        }

        return builder.ToString().Trim('_');
    }
}