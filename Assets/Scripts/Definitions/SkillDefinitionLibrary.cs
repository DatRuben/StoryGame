using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Game/Skill Definition Library")]
public sealed class SkillDefinitionLibrary : ScriptableObject
{
    [SerializeField]
    private List<CombatSkillDefinition> skillDefinitions = new();

    public IReadOnlyList<CombatSkillDefinition> SkillDefinitions =>
        skillDefinitions;

    public bool TryGetSkill(
        string skillId,
        out CombatSkillDefinition definition)
    {
        definition = null;

        if (string.IsNullOrWhiteSpace(skillId))
            return false;

        foreach (CombatSkillDefinition skill in skillDefinitions)
        {
            if (skill == null ||
                !string.Equals(
                    skill.Id,
                    skillId,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Do not resolve ambiguous IDs.
            if (definition != null)
            {
                Debug.LogError(
                    $"Cannot resolve Skill ID '{skillId}': " +
                    $"both '{definition.name}' and " +
                    $"'{skill.name}' use this ID."
                );

                definition = null;
                return false;
            }

            definition = skill;
        }

        return definition != null;
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
        string[] guids =
            AssetDatabase.FindAssets("t:CombatSkillDefinition");

        List<CombatSkillDefinition> discovered = new();

        Dictionary<string, CombatSkillDefinition> ids =
            new(StringComparer.OrdinalIgnoreCase);

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            CombatSkillDefinition skill =
                AssetDatabase.LoadAssetAtPath<CombatSkillDefinition>(
                    path
                );

            if (skill == null)
                continue;

            discovered.Add(skill);

            if (string.IsNullOrWhiteSpace(skill.Id))
            {
                Debug.LogError(
                    $"Skill '{skill.name}' generates an empty ID. " +
                    "Rename the asset to a valid name.",
                    skill
                );

                continue;
            }

            if (ids.TryGetValue(skill.Id, out var existing))
            {
                Debug.LogError(
                    $"Duplicate Skill ID '{skill.Id}' on " +
                    $"'{existing.name}' and '{skill.name}'.",
                    skill
                );
            }
            else
            {
                ids.Add(skill.Id, skill);
            }
        }

        discovered.Sort((a, b) =>
        {
            int comparison = string.Compare(
                a.Id,
                b.Id,
                StringComparison.OrdinalIgnoreCase
            );

            return comparison != 0
                ? comparison
                : string.CompareOrdinal(a.name, b.name);
        });

        bool changed =
            skillDefinitions == null ||
            skillDefinitions.Count != discovered.Count;

        if (!changed)
        {
            for (int i = 0; i < discovered.Count; i++)
            {
                if (skillDefinitions[i] != discovered[i])
                {
                    changed = true;
                    break;
                }
            }
        }

        if (changed)
        {
            skillDefinitions = discovered;
            EditorUtility.SetDirty(this);
        }

        if (saveAssets)
            AssetDatabase.SaveAssets();
    }
#endif
}
