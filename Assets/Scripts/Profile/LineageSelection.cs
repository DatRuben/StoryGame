using System;

public sealed class LineageSelection
{
    public SubraceDefinition Subrace { get; }

    public LineageDefinition CustomLineage { get; }

    public bool IsSubrace =>
        Subrace != null;

    public bool IsCustomLineage =>
        CustomLineage != null;

    public bool IsValid =>
        IsSubrace != IsCustomLineage;

    public string SelectionId
    {
        get
        {
            if (Subrace != null &&
                !string.IsNullOrWhiteSpace(
                    Subrace.subraceId))
            {
                return
                    $"subrace:{Subrace.subraceId}";
            }

            if (CustomLineage != null &&
                !string.IsNullOrWhiteSpace(
                    CustomLineage.lineageId))
            {
                return
                    $"lineage:{CustomLineage.lineageId}";
            }

            return "";
        }
    }

    public string DisplayName
    {
        get
        {
            if (Subrace != null)
                return Subrace.displayName;

            if (CustomLineage != null)
                return CustomLineage.displayName;

            return "";
        }
    }

    private LineageSelection(
        SubraceDefinition subrace,
        LineageDefinition customLineage)
    {
        Subrace = subrace;
        CustomLineage = customLineage;
    }

    public static LineageSelection FromSubrace(
        SubraceDefinition subrace)
    {
        if (subrace == null)
            return null;

        return new LineageSelection(
            subrace,
            null
        );
    }

    public static LineageSelection FromCustomLineage(
        LineageDefinition customLineage)
    {
        if (customLineage == null)
            return null;

        return new LineageSelection(
            null,
            customLineage
        );
    }

public bool IsAllowedFor(
    RaceDefinition mainRace,
    SubraceDefinition mainSubrace)
{
    if (!IsValid ||
        mainRace == null)
    {
        return false;
    }

    if (Subrace != null)
    {
        if (mainRace.allowedLineageType !=
            LineageType.HybridAncestry)
        {
            return false;
        }

        if (!SameRace(
            Subrace.race,
            mainRace))
        {
            return false;
        }

        if (SameSubrace(
            Subrace,
            mainSubrace))
        {
            return false;
        }

        return true;
    }

    return CustomLineage != null &&
           CustomLineage.IsAllowedForRace(
               mainRace
           );
}

    public CharacterAttributes GetAttributeShape()
    {
        if (Subrace != null &&
            Subrace.FinalAttributesPreview != null)
        {
            return CharacterAttributes.Copy(
                Subrace.FinalAttributesPreview
            );
        }

        if (CustomLineage != null)
        {
            return CharacterAttributes.AddModifiers(
                CharacterAttributes.CreateDefault(10),
                CustomLineage.modifiers
            );
        }

        return CharacterAttributes.CreateDefault(10);
    }

    public bool MatchesId(
        string selectionId)
    {
        return string.Equals(
            SelectionId,
            selectionId,
            StringComparison.OrdinalIgnoreCase
        );
    }

    private static bool SameRace(
        RaceDefinition first,
        RaceDefinition second)
    {
        if (first == null ||
            second == null)
        {
            return false;
        }

        return first == second ||
                string.Equals(
                    first.raceId,
                    second.raceId,
                    StringComparison.OrdinalIgnoreCase
                );
    }

    private static bool SameSubrace(
        SubraceDefinition first,
        SubraceDefinition second)
    {
        if (first == null ||
            second == null)
        {
            return false;
        }

        return first == second ||
               string.Equals(
                   first.subraceId,
                   second.subraceId,
                   StringComparison.OrdinalIgnoreCase
               );
    }
}