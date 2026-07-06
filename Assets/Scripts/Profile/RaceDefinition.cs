using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Race Definition")]
public class RaceDefinition : ScriptableObject
{
    [Header("Identity")]
    public string raceId;
    public string displayName;
    public BaseRace baseRace;

    [TextArea]
    public string description;

    [Header("Standard Subrace")]
    public SubraceDefinition standardSubrace;

    [Header("Compared To Human")]
    public CharacterAttributeModifiers modifiersFromHuman =
        CharacterAttributeModifiers.CreateZero();

    [Header("Lineage Rules")]
    public LineageType allowedLineageType = LineageType.HybridAncestry;

    [Min(0)]
    public int minLineages = 0;

    [Min(0)]
    public int maxLineages = 0;

    public bool CanUseLineages()
    {
        return maxLineages > 0;
    }

    public bool IsLineageAllowed(LineageDefinition lineage)
    {
        if (lineage == null)
            return false;

        if (!CanUseLineages())
            return false;

        if (lineage.lineageType != allowedLineageType)
            return false;

        return lineage.IsAllowedForRace(this);
    }

    public bool AreLineagesValid(
        List<LineageDefinition> lineages,
        out string errorMessage)
    {
        errorMessage = "";

        int count =
            lineages == null
                ? 0
                : lineages.Count;

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
                $"{displayName} requires at least {minLineages} lineage.";

            return false;
        }

        if (count > maxLineages)
        {
            errorMessage =
                $"{displayName} can only use up to {maxLineages} lineages.";

            return false;
        }

        if (lineages == null)
            return true;

        HashSet<string> usedLineageIds = new();

        foreach (LineageDefinition lineage in lineages)
        {
            if (lineage == null)
            {
                errorMessage =
                    "Missing lineage definition.";

                return false;
            }

            if (string.IsNullOrWhiteSpace(lineage.lineageId))
            {
                errorMessage =
                    $"{lineage.displayName} has no lineage ID.";

                return false;
            }

            if (!usedLineageIds.Add(lineage.lineageId))
            {
                errorMessage =
                    $"{lineage.displayName} was selected more than once.";

                return false;
            }

            if (!IsLineageAllowed(lineage))
            {
                errorMessage =
                    $"{displayName} cannot use lineage {lineage.displayName}.";

                return false;
            }
        }

        return true;
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(displayName))
            displayName = name;

        raceId = MakeId(displayName);

        if (maxLineages < minLineages)
            maxLineages = minLineages;
    }

    private string MakeId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        StringBuilder builder = new();

        foreach (char character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToLowerInvariant(character));
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