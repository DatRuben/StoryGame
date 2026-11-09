using UnityEditor;
using UnityEngine;

public sealed class DefinitionLibraryAutoRebuilder : AssetPostprocessor
{
    private static bool rebuildQueued;
    private static bool isRebuilding;
    private static bool rebuildCharacters;
    private static bool rebuildSkills;

    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        if (isRebuilding)
            return;

        bool removedAsset =
            HasRemovedAsset(deletedAssets) ||
            HasRemovedAsset(movedFromAssetPaths);

        rebuildCharacters |= removedAsset ||
            HasCharacterAsset(importedAssets) ||
            HasCharacterAsset(movedAssets);

        rebuildSkills |= removedAsset ||
            HasSkillAsset(importedAssets) ||
            HasSkillAsset(movedAssets);

        if (rebuildQueued ||
            (!rebuildCharacters && !rebuildSkills))
        {
            return;
        }

        rebuildQueued = true;
        EditorApplication.delayCall += RebuildLibraries;
    }

    private static bool HasRemovedAsset(string[] paths)
    {
        if (paths == null)
            return false;

        foreach (string path in paths)
        {
            if (path.EndsWith(".asset",
                System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasCharacterAsset(string[] paths)
    {
        if (paths == null)
            return false;

        foreach (string path in paths)
        {
            Object asset = AssetDatabase.LoadMainAssetAtPath(path);

            if (asset is RaceDefinition ||
                asset is SubraceDefinition ||
                asset is LineageDefinition ||
                asset is BackgroundDefinition ||
                asset is TraitDefinition ||
                asset is CharacterAppearanceOptionDefinition)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasSkillAsset(string[] paths)
    {
        if (paths == null)
            return false;

        foreach (string path in paths)
        {
            Object asset = AssetDatabase.LoadMainAssetAtPath(path);

            if (asset is CombatSkillDefinition ||
                asset is SkillDefinitionLibrary)
            {
                return true;
            }
        }

        return false;
    }

    private static void RebuildLibraries()
    {
        rebuildQueued = false;

        if (isRebuilding)
            return;

        isRebuilding = true;

        bool updateCharacters = rebuildCharacters;
        bool updateSkills = rebuildSkills;

        rebuildCharacters = false;
        rebuildSkills = false;

        try
        {
            if (updateCharacters)
                RebuildCharacterLibraries();

            if (updateSkills)
                RebuildSkillLibraries();
        }
        finally
        {
            isRebuilding = false;
        }
    }

    private static void RebuildCharacterLibraries()
    {
        string[] guids =
            AssetDatabase.FindAssets("t:CharacterDataLibrary");

        foreach (string guid in guids)
        {
            CharacterDataLibrary library =
                AssetDatabase.LoadAssetAtPath<CharacterDataLibrary>(
                    AssetDatabase.GUIDToAssetPath(guid));

            if (library != null)
                library.RebuildLibraryFromAuto();
        }
    }

    private static void RebuildSkillLibraries()
    {
        string[] guids =
            AssetDatabase.FindAssets("t:SkillDefinitionLibrary");

        foreach (string guid in guids)
        {
            SkillDefinitionLibrary library =
                AssetDatabase.LoadAssetAtPath<SkillDefinitionLibrary>(
                    AssetDatabase.GUIDToAssetPath(guid));

            if (library != null)
                library.RebuildLibraryFromAuto();
        }
    }
}