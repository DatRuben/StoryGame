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
                !string.IsNullOrWhiteSpace(Subrace.Id))
            {
                return $"subrace:{Subrace.Id}";
            }

            if (CustomLineage != null &&
                !string.IsNullOrWhiteSpace(CustomLineage.Id))
            {
                return $"lineage:{CustomLineage.Id}";
            }

            return "";
        }
    }

    public string DisplayName
    {
        get
        {
            if (Subrace != null)
                return Subrace.name;

            if (CustomLineage != null)
                return CustomLineage.name;

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
        return first != null &&
               second != null &&
               first == second;
    }

    private static bool SameSubrace(
        SubraceDefinition first,
        SubraceDefinition second)
    {
        return first != null &&
               second != null &&
               first == second;
    }
}