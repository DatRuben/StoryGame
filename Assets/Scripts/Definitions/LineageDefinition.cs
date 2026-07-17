using System.Collections.Generic;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum LineageType
{
    AnimalSpecies,
    HybridAncestry
}

[CreateAssetMenu(menuName = "Game/Lineage Definition")]
public class LineageDefinition : ScriptableObject
{
    [Header("Identity")]
    [HideInInspector]
    public string lineageId;

    public string displayName;
    public LineageType lineageType;

    [Header("Hybrid Ancestry Source")]
    [Tooltip(
        "For Hybrid Ancestry, this subrace supplies " +
        "the lineage's attribute shape."
    )]
    public SubraceDefinition sourceSubrace;

    [TextArea]
    public string description;

    [Header("Allowed Main Races")]
    public List<RaceDefinition> allowedRaces = new();

    [Header("Animal Species Attribute Modifiers")]
    [Tooltip(
        "Used by Animal Species lineages. " +
        "Hybrid Ancestry uses Source Subrace instead."
    )]
    public CharacterAttributeModifiers modifiers =
        CharacterAttributeModifiers.CreateZero();

    [Header("Skill / Theme")]
    public string skillTheme;

    [TextArea]
    public string skillTreeTheme;

    public bool IsAllowedForRace(
        RaceDefinition race)
    {
        if (race == null)
            return false;

        if (allowedRaces == null ||
            allowedRaces.Count == 0)
        {
            return false;
        }

        return allowedRaces.Contains(race);
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(displayName))
            displayName = name;

        lineageId = MakeId(displayName);
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

#if UNITY_EDITOR
[CustomEditor(typeof(LineageDefinition))]
[CanEditMultipleObjects]
public class LineageDefinitionEditor : Editor
{
    private SerializedProperty displayName;
    private SerializedProperty lineageType;
    private SerializedProperty sourceSubrace;
    private SerializedProperty description;
    private SerializedProperty allowedRaces;
    private SerializedProperty modifiers;
    private SerializedProperty skillTheme;
    private SerializedProperty skillTreeTheme;

    private void OnEnable()
    {
        displayName =
            serializedObject.FindProperty("displayName");

        lineageType =
            serializedObject.FindProperty("lineageType");

        sourceSubrace =
            serializedObject.FindProperty("sourceSubrace");

        description =
            serializedObject.FindProperty("description");

        allowedRaces =
            serializedObject.FindProperty("allowedRaces");

        modifiers =
            serializedObject.FindProperty("modifiers");

        skillTheme =
            serializedObject.FindProperty("skillTheme");

        skillTreeTheme =
            serializedObject.FindProperty("skillTreeTheme");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawIdentity();
        DrawLineageData();
        DrawSharedData();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawIdentity()
    {
        EditorGUILayout.LabelField(
            "Identity",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(displayName);
        EditorGUILayout.PropertyField(lineageType);

        EditorGUILayout.Space();
    }

    private void DrawLineageData()
    {
        LineageType selectedType =
            (LineageType)lineageType.enumValueIndex;

        if (selectedType == LineageType.HybridAncestry)
        {
            DrawHybridAncestry();
            return;
        }

        DrawAnimalSpecies();
    }

    private void DrawHybridAncestry()
    {
        EditorGUILayout.LabelField(
            "Hybrid Ancestry Source",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(sourceSubrace);

        if (sourceSubrace.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox(
                "Assign the subrace that supplies this " +
                "lineage's attributes.",
                MessageType.Warning
            );
        }
        else
        {
            EditorGUILayout.HelpBox(
                "This lineage inherits its attribute shape " +
                "directly from the selected subrace. The " +
                "attributes cannot be edited here.",
                MessageType.Info
            );
        }

        EditorGUILayout.Space();
    }

    private void DrawAnimalSpecies()
    {
        EditorGUILayout.LabelField(
            "Animal Species Attribute Modifiers",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(
            modifiers,
            true
        );

        EditorGUILayout.Space();
    }

    private void DrawSharedData()
    {
        EditorGUILayout.LabelField(
            "Details",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(description);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField(
            "Allowed Main Races",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(
            allowedRaces,
            true
        );

        EditorGUILayout.Space();

        EditorGUILayout.LabelField(
            "Skill / Theme",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(skillTheme);
        EditorGUILayout.PropertyField(skillTreeTheme);
    }
}
#endif