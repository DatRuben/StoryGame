using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Game/Character Data Library")]
public class CharacterDataLibrary : ScriptableObject
{
    [Header("Race Profiles")]
    [SerializeField] private List<RaceProfile> raceProfiles = new();

    [Header("Lineage Profiles")]
    [SerializeField] private List<LineageProfile> lineageProfiles = new();

    public IReadOnlyList<RaceProfile> RaceProfiles => raceProfiles;
    public IReadOnlyList<LineageProfile> LineageProfiles => lineageProfiles;

    public bool TryGetRaceProfile(
        string profileId,
        out RaceProfile raceProfile)
    {
        raceProfile = null;

        if (string.IsNullOrWhiteSpace(profileId))
            return false;

        foreach (RaceProfile profile in raceProfiles)
        {
            if (profile == null)
                continue;

            if (string.Equals(
                profile.profileId,
                profileId,
                System.StringComparison.OrdinalIgnoreCase))
            {
                raceProfile = profile;
                return true;
            }
        }

        return false;
    }

    public RaceProfile GetDefaultRaceProfile()
    {
        foreach (RaceProfile profile in raceProfiles)
        {
            if (profile != null)
                return profile;
        }

        return null;
    }

    public List<LineageProfile> GetLineageProfiles(
        List<string> lineageIds)
    {
        List<LineageProfile> foundProfiles = new();

        if (lineageIds == null)
            return foundProfiles;

        foreach (string lineageId in lineageIds)
        {
            if (TryGetLineageProfile(lineageId, out LineageProfile profile))
                foundProfiles.Add(profile);
        }

        return foundProfiles;
    }

    public bool TryGetLineageProfile(
        string lineageId,
        out LineageProfile lineageProfile)
    {
        lineageProfile = null;

        if (string.IsNullOrWhiteSpace(lineageId))
            return false;

        foreach (LineageProfile profile in lineageProfiles)
        {
            if (profile == null)
                continue;

            if (string.Equals(
                profile.LineageName,
                lineageId,
                System.StringComparison.OrdinalIgnoreCase))
            {
                lineageProfile = profile;
                return true;
            }
        }

        return false;
    }

#if UNITY_EDITOR
    [ContextMenu("Rebuild Library From Project Assets")]
    public void RebuildLibrary()
    {
        RebuildRaceProfiles();
        RebuildLineageProfiles();

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();

        Debug.Log(
            $"Rebuilt CharacterDataLibrary with {raceProfiles.Count} race profiles and {lineageProfiles.Count} lineage profiles.",
            this
        );
    }

    private void RebuildRaceProfiles()
    {
        raceProfiles.Clear();

        string[] guids =
            AssetDatabase.FindAssets("t:RaceProfile");

        foreach (string guid in guids)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(guid);

            RaceProfile profile =
                AssetDatabase.LoadAssetAtPath<RaceProfile>(path);

            if (profile != null && !raceProfiles.Contains(profile))
                raceProfiles.Add(profile);
        }
    }

    private void RebuildLineageProfiles()
    {
        lineageProfiles.Clear();

        string[] guids =
            AssetDatabase.FindAssets("t:LineageProfile");

        foreach (string guid in guids)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(guid);

            LineageProfile profile =
                AssetDatabase.LoadAssetAtPath<LineageProfile>(path);

            if (profile != null && !lineageProfiles.Contains(profile))
                lineageProfiles.Add(profile);
        }
    }
#endif
}

#if UNITY_EDITOR
public class CharacterDataLibraryAutoRebuilder : AssetPostprocessor
{
    private static bool rebuildQueued;

    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
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

            if (asset is RaceProfile ||
                asset is LineageProfile)
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

        string[] guids =
            AssetDatabase.FindAssets("t:CharacterDataLibrary");

        foreach (string guid in guids)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(guid);

            CharacterDataLibrary library =
                AssetDatabase.LoadAssetAtPath<CharacterDataLibrary>(path);

            if (library == null)
                continue;

            library.RebuildLibrary();

            Debug.Log(
                $"Auto-rebuilt CharacterDataLibrary: {path}",
                library
            );
        }
    }
}
#endif