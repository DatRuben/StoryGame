using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Race Definition")]
public class RaceDefinition : ScriptableObject
{
    [Header("Identity")]
    [HideInInspector]
    public string raceId;

    public string displayName;
    public BaseRace baseRace;

    [TextArea]
    public string description;

    [Header("Standard Subrace")]
    public SubraceDefinition standardSubrace;

    [Header("Base Combat Stats")]
    public CharacterBaseStats baseStats =
        CharacterBaseStats.CreateHumanDefault();

    [Header("Compared To Human")]
    public CharacterAttributeModifiers modifiersFromHuman =
        CharacterAttributeModifiers.CreateZero();

    [Header("Calculated Preview")]
    [SerializeField]
    private CharacterAttributes finalAttributesPreview =
        CharacterAttributes.CreateDefault(10);

    [SerializeField]
    private int totalAttributePointsPreview;

    public CharacterAttributes FinalAttributesPreview =>
        finalAttributesPreview;

    public int TotalAttributePointsPreview =>
        totalAttributePointsPreview;

    [Header("Lineage Rules")]
    public LineageType allowedLineageType =
        LineageType.HybridAncestry;

    [Min(0)]
    public int minLineages = 0;

    [Min(0)]
    public int maxLineages = 0;

    public bool CanUseLineages()
    {
        return maxLineages > 0;
    }

    public bool IsLineageAllowed(
        LineageSelection lineage,
        SubraceDefinition selectedSubrace)
    {
        if (lineage == null ||
            !CanUseLineages())
        {
            return false;
        }

        return lineage.IsAllowedFor(
            this,
            selectedSubrace,
            allowedLineageType
        );
    }

    public bool AreLineageSelectionsValid(
        SubraceDefinition subraceDefinition,
        List<LineageSelection> lineages,
        out string errorMessage)
    {
        int count =
            lineages == null
                ? 0
                : lineages.Count;

        if (!IsLineageCountValid(
            count,
            out errorMessage))
        {
            return false;
        }

        if (lineages == null)
            return true;

        HashSet<string> usedSelectionIds =
            new HashSet<string>(
                System.StringComparer.OrdinalIgnoreCase
            );

        foreach (LineageSelection lineage
                 in lineages)
        {
            if (lineage == null ||
                !lineage.IsValid)
            {
                errorMessage =
                    "Missing or invalid lineage selection.";

                return false;
            }

            if (string.IsNullOrWhiteSpace(
                lineage.SelectionId))
            {
                errorMessage =
                    $"{lineage.DisplayName} has no lineage selection ID.";

                return false;
            }

            if (!usedSelectionIds.Add(
                lineage.SelectionId))
            {
                errorMessage =
                    $"{lineage.DisplayName} was selected more than once.";

                return false;
            }

            if (!IsLineageAllowed(
                lineage,
                subraceDefinition))
            {
                errorMessage =
                    $"{displayName} cannot use lineage " +
                    $"{lineage.DisplayName}.";

                return false;
            }
        }

        errorMessage = "";
        return true;
    }

    private bool IsLineageCountValid(
        int count,
        out string errorMessage)
    {
        errorMessage = "";

        if (!CanUseLineages())
        {
            if (count > 0)
            {
                errorMessage =
                    $"{displayName} cannot use lineages.";

                return false;
            }

            return true;
        }

        if (count < minLineages)
        {
            errorMessage =
                $"{displayName} requires at least " +
                $"{minLineages} lineage.";

            return false;
        }

        if (count > maxLineages)
        {
            errorMessage =
                $"{displayName} can only use up to " +
                $"{maxLineages} lineages.";

            return false;
        }

        return true;
    }

    public void RecalculatePreview()
    {
        finalAttributesPreview =
            CharacterAttributes.AddModifiers(
                CharacterAttributes.CreateDefault(10),
                modifiersFromHuman
            );

        totalAttributePointsPreview =
            finalAttributesPreview.BasePoints();
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(displayName))
            displayName = name;

        raceId = MakeId(displayName);

        RecalculatePreview();

        if (maxLineages < minLineages)
            maxLineages = minLineages;
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