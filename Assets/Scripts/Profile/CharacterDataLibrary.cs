using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Game/Character Data Library")]
public class CharacterDataLibrary : ScriptableObject
{
    [Header("Race Data")]
    [SerializeField] private List<RaceDefinition> raceDefinitions = new();
    [SerializeField] private List<SubraceDefinition> subraceDefinitions = new();
    [SerializeField] private List<LineageDefinition> lineageDefinitions = new();
    [SerializeField] private List<BackgroundDefinition> backgroundDefinitions = new();
    [SerializeField] private List<TraitDefinition> traitDefinitions = new();

    public IReadOnlyList<RaceDefinition> RaceDefinitions => raceDefinitions;
    public IReadOnlyList<SubraceDefinition> SubraceDefinitions => subraceDefinitions;
    public IReadOnlyList<LineageDefinition> LineageDefinitions => lineageDefinitions;
    public IReadOnlyList<BackgroundDefinition> BackgroundDefinitions => backgroundDefinitions;
    public IReadOnlyList<TraitDefinition> TraitDefinitions => traitDefinitions;

    public bool TryGetRaceDefinition(
        string raceId,
        out RaceDefinition raceDefinition)
    {
        raceDefinition = null;

        if (string.IsNullOrWhiteSpace(raceId))
            return false;

        foreach (RaceDefinition definition in raceDefinitions)
        {
            if (definition == null)
                continue;

            if (string.Equals(
                definition.raceId,
                raceId,
                System.StringComparison.OrdinalIgnoreCase))
            {
                raceDefinition = definition;
                return true;
            }
        }

        return false;
    }

    public bool TryGetSubraceDefinition(
        string subraceId,
        out SubraceDefinition subraceDefinition)
    {
        subraceDefinition = null;

        if (string.IsNullOrWhiteSpace(subraceId))
            return false;

        foreach (SubraceDefinition definition in subraceDefinitions)
        {
            if (definition == null)
                continue;

            if (string.Equals(
                definition.subraceId,
                subraceId,
                System.StringComparison.OrdinalIgnoreCase))
            {
                subraceDefinition = definition;
                return true;
            }
        }

        return false;
    }

    public bool TryGetLineageDefinition(
        string lineageId,
        out LineageDefinition lineageDefinition)
    {
        lineageDefinition = null;

        if (string.IsNullOrWhiteSpace(lineageId))
            return false;

        foreach (LineageDefinition definition in lineageDefinitions)
        {
            if (definition == null)
                continue;

            if (string.Equals(
                definition.lineageId,
                lineageId,
                System.StringComparison.OrdinalIgnoreCase))
            {
                lineageDefinition = definition;
                return true;
            }
        }

        return false;
    }

    public bool TryGetBackgroundDefinition(
    string backgroundId,
    out BackgroundDefinition backgroundDefinition)
    {
        backgroundDefinition = null;

        if (string.IsNullOrWhiteSpace(backgroundId))
            return false;

        foreach (BackgroundDefinition definition in backgroundDefinitions)
        {
            if (definition == null)
                continue;

            if (string.Equals(
                definition.backgroundId,
                backgroundId,
                System.StringComparison.OrdinalIgnoreCase))
            {
                backgroundDefinition = definition;
                return true;
            }
        }

        return false;
    }

    public bool TryGetTraitDefinition(
        string traitId,
        out TraitDefinition traitDefinition)
    {
        traitDefinition = null;

        if (string.IsNullOrWhiteSpace(traitId))
            return false;

        foreach (TraitDefinition definition in traitDefinitions)
        {
            if (definition == null)
                continue;

            if (string.Equals(
                definition.traitId,
                traitId,
                System.StringComparison.OrdinalIgnoreCase))
            {
                traitDefinition = definition;
                return true;
            }
        }

        return false;
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
            if (definition == null)
                continue;

            if (definition.race == null)
                continue;

            if (definition.race == raceDefinition ||
                definition.race.raceId == raceDefinition.raceId)
            {
                found.Add(definition);
            }
        }

        return found;
    }

    public List<LineageDefinition> GetLineageDefinitions(
        List<string> lineageIds)
    {
        List<LineageDefinition> foundDefinitions = new();

        if (lineageIds == null)
            return foundDefinitions;

        foreach (string lineageId in lineageIds)
        {
            if (TryGetLineageDefinition(
                lineageId,
                out LineageDefinition definition))
            {
                foundDefinitions.Add(definition);
            }
        }

        return foundDefinitions;
    }

    public RaceDefinition GetDefaultRaceDefinition()
    {
        foreach (RaceDefinition definition in raceDefinitions)
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

    private void RecalculateDefinitionPreviews()
    {
        foreach (RaceDefinition definition in raceDefinitions)
        {
            if (definition == null)
                continue;

            definition.RecalculatePreview();
            EditorUtility.SetDirty(definition);
        }

        for (int pass = 0; pass < subraceDefinitions.Count; pass++)
        {
            foreach (SubraceDefinition definition in subraceDefinitions)
            {
                if (definition == null)
                    continue;

                definition.RecalculatePreview();
                EditorUtility.SetDirty(definition);
            }
        }
    }
#endif
}

#if UNITY_EDITOR
public class CharacterDataLibraryAutoRebuilder : AssetPostprocessor
{
    private static bool rebuildQueued;
    private static bool isRebuilding;

    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        if (isRebuilding)
            return;

        if (!ShouldQueueRebuild(
            importedAssets,
            deletedAssets,
            movedAssets,
            movedFromAssetPaths))
        {
            return;
        }

        if (rebuildQueued)
            return;

        rebuildQueued = true;
        EditorApplication.delayCall += RebuildAllLibraries;
    }

    private static bool ShouldQueueRebuild(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        return HasRelevantImportedAsset(importedAssets) ||
               HasRelevantImportedAsset(movedAssets) ||
               HasPossibleDeletedDefinitionAsset(deletedAssets) ||
               HasPossibleDeletedDefinitionAsset(movedFromAssetPaths);
    }

    private static bool HasRelevantImportedAsset(string[] paths)
    {
        if (paths == null)
            return false;

        foreach (string path in paths)
        {
            if (!path.EndsWith(".asset"))
                continue;

            Object asset =
                AssetDatabase.LoadMainAssetAtPath(path);

            if (asset is RaceDefinition ||
                asset is SubraceDefinition ||
                asset is LineageDefinition ||
                asset is BackgroundDefinition ||
                asset is TraitDefinition)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasPossibleDeletedDefinitionAsset(string[] paths)
    {
        if (paths == null)
            return false;

        foreach (string path in paths)
        {
            if (path.EndsWith(".asset"))
                return true;
        }

        return false;
    }

    private static void RebuildAllLibraries()
    {
        rebuildQueued = false;

        if (isRebuilding)
            return;

        isRebuilding = true;

        try
        {
            string[] libraryGuids =
                AssetDatabase.FindAssets("t:CharacterDataLibrary");

            foreach (string guid in libraryGuids)
            {
                string path =
                    AssetDatabase.GUIDToAssetPath(guid);

                CharacterDataLibrary library =
                    AssetDatabase.LoadAssetAtPath<CharacterDataLibrary>(path);

                if (library == null)
                    continue;

                library.RebuildLibraryFromAuto();
            }
        }
        finally
        {
            isRebuilding = false;
        }
    }
}
#endif