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

    [Tooltip(
        "For Hybrid Ancestry, this subrace supplies " +
        "the lineage's attribute shape."
    )]
    public SubraceDefinition sourceSubrace;

    [TextArea]
    public string description;

    [Tooltip(
        "The main race that is allowed to select this lineage."
    )]
    public RaceDefinition allowedRace;

    [Tooltip(
        "Used by Animal Species lineages. " +
        "Hybrid Ancestry uses Source Subrace instead."
    )]
    public CharacterAttributeModifiers modifiers =
        CharacterAttributeModifiers.CreateZero();

    public string skillTheme;
    public string skillTreeTheme;

    public bool IsAllowedForRace(
        RaceDefinition race)
    {
        if (race == null ||
            allowedRace == null)
        {
            return false;
        }

        return allowedRace == race ||
               allowedRace.raceId == race.raceId;
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
    private SerializedProperty allowedRace;
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

        allowedRace =
            serializedObject.FindProperty("allowedRace");

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

        EditorGUILayout.Space(6f);

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
        EditorGUILayout.PropertyField(
            sourceSubrace,
            new GUIContent("Source Subrace")
        );

        EditorGUILayout.Space();
    }

    private void DrawAnimalSpecies()
    {
        EditorGUILayout.PropertyField(
            modifiers,
            new GUIContent("Attribute Modifiers"),
            true
        );

        EditorGUILayout.Space();
    }

    private void DrawSharedData()
    {
        EditorGUILayout.PropertyField(description);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(
            allowedRace,
            new GUIContent("Main Race")
        );

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(
            skillTheme,
            new GUIContent("Skill Theme")
        );

        EditorGUILayout.LabelField("Skill Tree Theme");

        skillTreeTheme.stringValue =
            EditorGUILayout.TextArea(
                skillTreeTheme.stringValue,
                GUILayout.MinHeight(48f)
            );
    }
}
#endif