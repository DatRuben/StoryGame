using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public sealed class DefinitionLibraryAutoRebuilder : AssetPostprocessor
{
    private static bool rebuildQueued;
    private static bool isRebuilding;
    private static bool rebuildCharacters;
    private static bool rebuildSkills;
    private static bool validateDefinitionIds;

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

        validateDefinitionIds |= removedAsset ||
            HasDefinitionAsset(importedAssets) ||
            HasDefinitionAsset(movedAssets);

        if (rebuildQueued ||
            (!rebuildCharacters &&
             !rebuildSkills &&
             !validateDefinitionIds))
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

    private static bool HasDefinitionAsset(string[] paths)
    {
        if (paths == null)
            return false;

        foreach (string path in paths)
        {
            if (AssetDatabase.LoadMainAssetAtPath(path)
                is GameDefinition)
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
        bool checkDefinitionIds = validateDefinitionIds;

        rebuildCharacters = false;
        rebuildSkills = false;

        validateDefinitionIds = false;

        try
        {
            if (updateCharacters)
                RebuildCharacterLibraries();

            if (updateSkills)
                RebuildSkillLibraries();

            if (checkDefinitionIds)
                ValidateDefinitionIds();
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


    [MenuItem("Tools/StoryGame/Validate Definition IDs")]
    private static void ValidateDefinitionIds()
    {
        Dictionary<string, GameDefinition> knownIds =
            new Dictionary<string, GameDefinition>(
                System.StringComparer.OrdinalIgnoreCase
            );

        string[] guids = AssetDatabase.FindAssets(
            "t:ScriptableObject",
            new[] { "Assets" }
        );

        int checkedCount = 0;
        int errorCount = 0;

        foreach (string guid in guids)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(guid);

            GameDefinition definition =
                AssetDatabase.LoadAssetAtPath<GameDefinition>(
                    path
                );

            if (definition == null)
                continue;

            checkedCount++;

            string id = definition.Id;

            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogError(
                    $"Definition '{definition.name}' generates an empty ID.",
                    definition
                );

                errorCount++;
                continue;
            }

            System.Type identityType =
                definition.GetType();

            if (definition is CombatSkillActionDefinition)
            {
                identityType =
                    typeof(CombatSkillActionDefinition);
            }
            else if (definition is CombatSkillHitEffectDefinition)
            {
                identityType =
                    typeof(CombatSkillHitEffectDefinition);
            }

            string key = identityType.FullName + ":" + id;

            if (knownIds.TryGetValue(
                key,
                out GameDefinition existing))
            {
                Debug.LogError(
                    $"Duplicate {identityType.Name} ID '{id}' " +
                    $"on '{existing.name}' and '{definition.name}'.",
                    definition
                );

                errorCount++;
            }
            else
            {
                knownIds.Add(key, definition);
            }
        }

        if (checkedCount == 0)
        {
            Debug.LogWarning(
                "Definition ID validation found no GameDefinition assets."
            );
        }
        else if (errorCount == 0)
        {
            Debug.Log(
                $"Definition ID validation passed for {checkedCount} assets."
            );
        }
    }

}