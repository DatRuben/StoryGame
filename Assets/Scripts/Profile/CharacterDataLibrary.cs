using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Game/Character Data Library")]
public class CharacterDataLibrary : ScriptableObject
{
    private const string SubraceLineagePrefix =
    "subrace:";

    private const string CustomLineagePrefix =
        "lineage:";

    [Header("Race Data")]
    [SerializeField] private List<RaceDefinition> raceDefinitions = new();
    [SerializeField] private RaceDefinition defaultRaceDefinition;
    [SerializeField] private List<SubraceDefinition> subraceDefinitions = new();
    [SerializeField] private List<LineageDefinition> lineageDefinitions = new();
    [SerializeField] private List<BackgroundDefinition> backgroundDefinitions = new();
    [SerializeField] private BackgroundDefinition defaultBackgroundDefinition;
    [SerializeField] private List<TraitDefinition> traitDefinitions = new();

    [Header("Appearance Data")]
    [SerializeField]
    private List<CharacterAppearanceOptionDefinition>
    appearanceOptionDefinitions = new();

    public IReadOnlyList<RaceDefinition> RaceDefinitions => raceDefinitions;
    public IReadOnlyList<SubraceDefinition> SubraceDefinitions => subraceDefinitions;
    public IReadOnlyList<LineageDefinition> LineageDefinitions => lineageDefinitions;
    public IReadOnlyList<BackgroundDefinition> BackgroundDefinitions => backgroundDefinitions;
    public IReadOnlyList<TraitDefinition> TraitDefinitions => traitDefinitions;

    public IReadOnlyList<CharacterAppearanceOptionDefinition>
    AppearanceOptionDefinitions =>
        appearanceOptionDefinitions;


    public bool TryGetRaceDefinition(
        string raceId,
        out RaceDefinition raceDefinition)
    {
        return TryGetUniqueDefinition(
            raceDefinitions,
            raceId,
            out raceDefinition
        );
    }

    public bool TryGetSubraceDefinition(
        string subraceId,
        out SubraceDefinition subraceDefinition)
    {
        return TryGetUniqueDefinition(
            subraceDefinitions,
            subraceId,
            out subraceDefinition
        );
    }

    public bool TryGetLineageDefinition(
        string lineageId,
        out LineageDefinition lineageDefinition)
    {
        return TryGetUniqueDefinition(
            lineageDefinitions,
            lineageId,
            out lineageDefinition
        );
    }

    public bool TryGetBackgroundDefinition(
        string backgroundId,
        out BackgroundDefinition backgroundDefinition)
    {
        return TryGetUniqueDefinition(
            backgroundDefinitions,
            backgroundId,
            out backgroundDefinition
        );
    }

    public bool TryGetTraitDefinition(
        string traitId,
        out TraitDefinition traitDefinition)
    {
        return TryGetUniqueDefinition(
            traitDefinitions,
            traitId,
            out traitDefinition
        );
    }

    public bool TryGetAppearanceOptionDefinition(
        string optionId,
        out CharacterAppearanceOptionDefinition optionDefinition)
    {
        return TryGetUniqueDefinition(
            appearanceOptionDefinitions,
            optionId,
            out optionDefinition
        );
    }

    private static bool TryGetUniqueDefinition<T>(
        IReadOnlyList<T> definitions,
        string id,
        out T definition)
        where T : GameDefinition
    {
        definition = null;

        if (definitions == null ||
            string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        foreach (T candidate in definitions)
        {
            if (candidate == null ||
                !string.Equals(
                    candidate.Id,
                    id,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (definition != null)
            {
                Debug.LogError(
                    $"Duplicate {typeof(T).Name} ID '{id}' " +
                    $"on '{definition.name}' and '{candidate.name}'.",
                    candidate
                );

                definition = null;
                return false;
            }

            definition = candidate;
        }

        return definition != null;
    }

    public List<TraitDefinition> GetTraitDefinitions(
        List<string> traitIds)
    {
        List<TraitDefinition> foundDefinitions = new();

        if (traitIds == null)
            return foundDefinitions;

        foreach (string traitId in traitIds)
        {
            if (TryGetTraitDefinition(
                traitId,
                out TraitDefinition definition))
            {
                foundDefinitions.Add(definition);
            }
        }

        return foundDefinitions;
    }
    public List<SubraceDefinition> GetSubraceDefinitionsForRace(
        RaceDefinition raceDefinition)
    {
        List<SubraceDefinition> found = new();

        if (raceDefinition == null)
            return found;

        foreach (SubraceDefinition definition in subraceDefinitions)
        {
            if (definition != null &&
                definition.race == raceDefinition)
            {
                found.Add(definition);
            }
        }

        return found;
    }

    public bool TryGetLineageSelection(
        string selectionId,
        out LineageSelection selection)
    {
        selection = null;

        if (string.IsNullOrWhiteSpace(selectionId))
            return false;

        if (selectionId.StartsWith(
            SubraceLineagePrefix,
            System.StringComparison.OrdinalIgnoreCase))
        {
            string subraceId =
                selectionId.Substring(
                    SubraceLineagePrefix.Length
                );

            if (!TryGetSubraceDefinition(
                subraceId,
                out SubraceDefinition subrace))
            {
                return false;
            }

            selection =
                LineageSelection.FromSubrace(
                    subrace
                );

            return selection != null &&
                   selection.IsValid;
        }

        if (selectionId.StartsWith(
            CustomLineagePrefix,
            System.StringComparison.OrdinalIgnoreCase))
        {
            string lineageId =
                selectionId.Substring(
                    CustomLineagePrefix.Length
                );

            if (!TryGetLineageDefinition(
                lineageId,
                out LineageDefinition lineage))
            {
                return false;
            }

            selection =
                LineageSelection.FromCustomLineage(
                    lineage
                );

            return selection != null &&
                   selection.IsValid;
        }

        return false;
    }

    public List<LineageSelection> GetLineageSelections(
        List<string> selectionIds)
    {
        List<LineageSelection> selections = new();

        if (selectionIds == null)
            return selections;

        foreach (string selectionId in selectionIds)
        {
            if (TryGetLineageSelection(
                selectionId,
                out LineageSelection selection))
            {
                selections.Add(selection);
            }
        }

        return selections;
    }

    public List<LineageSelection> GetLineageOptionsForRace(
        RaceDefinition raceDefinition,
        SubraceDefinition selectedSubrace)
    {
        List<LineageSelection> options = new();

        HashSet<string> usedSelectionIds =
            new HashSet<string>(
                System.StringComparer.OrdinalIgnoreCase
            );

        if (raceDefinition == null ||
            !raceDefinition.CanUseLineages())
        {
            return options;
        }

        if (raceDefinition.allowedLineageType ==
            LineageType.HybridAncestry)
        {
            List<SubraceDefinition> raceSubraces =
                GetSubraceDefinitionsForRace(
                    raceDefinition
                );

            foreach (SubraceDefinition subrace
                     in raceSubraces)
            {
                AddLineageOption(
                    options,
                    usedSelectionIds,
                    LineageSelection.FromSubrace(
                        subrace
                    ),
                    raceDefinition,
                    selectedSubrace
                );
            }
        }

        foreach (LineageDefinition lineage
                 in lineageDefinitions)
        {
            if (lineage == null)
                continue;

            AddLineageOption(
                options,
                usedSelectionIds,
                LineageSelection.FromCustomLineage(
                    lineage
                ),
                raceDefinition,
                selectedSubrace
            );
        }

        return options;
    }

    private static void AddLineageOption(
        List<LineageSelection> options,
        HashSet<string> usedSelectionIds,
        LineageSelection selection,
        RaceDefinition raceDefinition,
        SubraceDefinition selectedSubrace)
    {
        if (selection == null ||
            !selection.IsValid)
        {
            return;
        }

        if (!selection.IsAllowedFor(
            raceDefinition,
            selectedSubrace))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(
            selection.SelectionId))
        {
            return;
        }

        if (!usedSelectionIds.Add(
            selection.SelectionId))
        {
            return;
        }

        options.Add(selection);
    }

    public RaceDefinition GetDefaultRaceDefinition()
    {
        if (defaultRaceDefinition == null ||
            raceDefinitions == null ||
            !raceDefinitions.Contains(defaultRaceDefinition))
        {
            return null;
        }

        return defaultRaceDefinition;
    }

    public BackgroundDefinition GetDefaultBackgroundDefinition()
    {
        if (defaultBackgroundDefinition != null)
            return defaultBackgroundDefinition;

        foreach (BackgroundDefinition definition in backgroundDefinitions)
        {
            if (definition != null)
                return definition;
        }

        return null;
    }

#if UNITY_EDITOR
    [ContextMenu("Rebuild Library From Project Assets")]
    public void RebuildLibrary()
    {
        RebuildLibraryInternal(true);
    }

    public void RebuildLibraryFromAuto()
    {
        RebuildLibraryInternal(false);
    }

    private void RebuildLibraryInternal(bool saveAssets)
    {
        RebuildRaceDefinitions();
        RebuildSubraceDefinitions();
        RebuildLineageDefinitions();
        RebuildBackgroundDefinitions();
        RebuildTraitDefinitions();
        RebuildAppearanceOptionDefinitions();

        RecalculateDefinitionPreviews();

        EditorUtility.SetDirty(this);

        if (saveAssets)
            AssetDatabase.SaveAssets();

        Debug.Log(
            $"Rebuilt CharacterDataLibrary with " +
            $"{raceDefinitions.Count} race definitions, " +
            $"{subraceDefinitions.Count} subrace definitions, and " +
            $"{lineageDefinitions.Count} lineage definitions.",
            this
        );
    }

    private void RebuildRaceDefinitions()
    {
        raceDefinitions.Clear();

        string[] guids =
            AssetDatabase.FindAssets("t:RaceDefinition");

        foreach (string guid in guids)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(guid);

            RaceDefinition definition =
                AssetDatabase.LoadAssetAtPath<RaceDefinition>(path);

            if (definition != null &&
                !raceDefinitions.Contains(definition))
            {
                raceDefinitions.Add(definition);
            }
        }
    }

    private void RebuildSubraceDefinitions()
    {
        subraceDefinitions.Clear();

        string[] guids =
            AssetDatabase.FindAssets("t:SubraceDefinition");

        foreach (string guid in guids)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(guid);

            SubraceDefinition definition =
                AssetDatabase.LoadAssetAtPath<SubraceDefinition>(path);

            if (definition != null &&
                !subraceDefinitions.Contains(definition))
            {
                subraceDefinitions.Add(definition);
            }
        }
    }

    private void RebuildLineageDefinitions()
    {
        lineageDefinitions.Clear();

        string[] guids =
            AssetDatabase.FindAssets("t:LineageDefinition");

        foreach (string guid in guids)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(guid);

            LineageDefinition definition =
                AssetDatabase.LoadAssetAtPath<LineageDefinition>(path);

            if (definition != null &&
                !lineageDefinitions.Contains(definition))
            {
                lineageDefinitions.Add(definition);
            }
        }
    }

    private void RebuildBackgroundDefinitions()
    {
        backgroundDefinitions.Clear();

        string[] guids =
            AssetDatabase.FindAssets("t:BackgroundDefinition");

        foreach (string guid in guids)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(guid);

            BackgroundDefinition definition =
                AssetDatabase.LoadAssetAtPath<BackgroundDefinition>(path);

            if (definition != null &&
                !backgroundDefinitions.Contains(definition))
            {
                backgroundDefinitions.Add(definition);
            }
        }
    }

    private void RebuildTraitDefinitions()
    {
        traitDefinitions.Clear();

        string[] guids =
            AssetDatabase.FindAssets("t:TraitDefinition");

        foreach (string guid in guids)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(guid);

            TraitDefinition definition =
                AssetDatabase.LoadAssetAtPath<TraitDefinition>(path);

            if (definition != null &&
                !traitDefinitions.Contains(definition))
            {
                traitDefinitions.Add(definition);
            }
        }
    }

    private void RebuildAppearanceOptionDefinitions()
    {
        appearanceOptionDefinitions.Clear();

        string[] guids =
            AssetDatabase.FindAssets(
                "t:CharacterAppearanceOptionDefinition"
            );

        foreach (string guid in guids)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(guid);

            CharacterAppearanceOptionDefinition definition =
                AssetDatabase.LoadAssetAtPath<
                    CharacterAppearanceOptionDefinition
                >(path);

            if (definition != null &&
                !appearanceOptionDefinitions.Contains(definition))
            {
                appearanceOptionDefinitions.Add(definition);
            }
        }
    }

    private void RecalculateDefinitionPreviews()
    {
        foreach (RaceDefinition definition in raceDefinitions)
        {
            if (definition == null)
                continue;

            definition.RecalculatePreview();
            EditorUtility.SetDirty(definition);
        }

        foreach (SubraceDefinition definition
                 in subraceDefinitions)
        {
            if (definition == null)
                continue;

            definition.RecalculatePreview();
            EditorUtility.SetDirty(definition);
        }
    }
#endif
}