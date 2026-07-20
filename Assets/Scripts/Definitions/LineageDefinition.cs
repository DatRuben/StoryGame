using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Game/Lineage Definition")]
public class LineageDefinition : ScriptableObject
{
    [Header("Identity")]
    [HideInInspector]
    public string lineageId;

    public string displayName;

    [TextArea]
    public string description;

    [Tooltip(
        "The main race that is allowed to select this lineage."
    )]
    public RaceDefinition allowedRace;

    [Tooltip(
        "The attribute shape for this custom lineage. " +
        "Playable subrace ancestry uses its SubraceDefinition instead."
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
               string.Equals(
                   allowedRace.raceId,
                   race.raceId,
                   System.StringComparison.OrdinalIgnoreCase
               );
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
    private SerializedProperty description;
    private SerializedProperty allowedRace;
    private SerializedProperty modifiers;
    private SerializedProperty skillTheme;
    private SerializedProperty skillTreeTheme;

    private void OnEnable()
    {
        displayName =
            serializedObject.FindProperty("displayName");

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

        DrawRace();
        DrawIdentity();
        DrawLineageData();
        DrawSharedData();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawRace()
    {
        EditorGUILayout.LabelField(
            "Race",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(
            allowedRace,
            new GUIContent("Race")
        );

        if (allowedRace.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox(
                "Select the race that owns this lineage.",
                MessageType.Warning
            );
        }

        EditorGUILayout.Space();
    }

    private void DrawIdentity()
    {
        EditorGUILayout.LabelField(
            "Identity",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(displayName);
        EditorGUILayout.PropertyField(description);

        EditorGUILayout.Space();
    }

    private void DrawLineageData()
    {
        RaceDefinition selectedRace =
            allowedRace.objectReferenceValue
            as RaceDefinition;

        if (selectedRace != null &&
            selectedRace.allowedLineageType ==
            LineageType.HybridAncestry)
        {
            EditorGUILayout.HelpBox(
                "Playable subraces are included automatically. " +
                "Create a custom lineage asset only for ancestry " +
                "without a playable subrace, such as Giant.",
                MessageType.Info
            );

            EditorGUILayout.Space();
        }

        EditorGUILayout.PropertyField(
            modifiers,
            new GUIContent(
                "Custom Attribute Modifiers"
            ),
            true
        );

        EditorGUILayout.Space();
    }

    private void DrawSharedData()
    {
        EditorGUILayout.PropertyField(
            skillTheme,
            new GUIContent("Skill Theme")
        );

        EditorGUILayout.LabelField(
            "Skill Tree Theme"
        );

        skillTreeTheme.stringValue =
            EditorGUILayout.TextArea(
                skillTreeTheme.stringValue,
                GUILayout.MinHeight(48f)
            );
    }
}
#endif