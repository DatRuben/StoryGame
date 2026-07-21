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
        "Complete ancestry attribute shape used when " +
        "the owning race uses Hybrid Ancestry."
    )]
    public CharacterAttributes hybridAttributeShape =
        CharacterAttributes.CreateDefault(10);

    [Tooltip(
        "Attribute changes used when the owning race " +
        "uses Animal Species."
    )]
    public CharacterAttributeModifiers animalSpeciesModifiers =
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
    private SerializedProperty hybridAttributeShape;
    private SerializedProperty animalSpeciesModifiers;
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

        hybridAttributeShape =
            serializedObject.FindProperty(
                "hybridAttributeShape"
            );

        animalSpeciesModifiers =
            serializedObject.FindProperty(
                "animalSpeciesModifiers"
            );

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

        bool changed =
            serializedObject.ApplyModifiedProperties();

        if (changed)
        {
            EditorUtility.SetDirty(target);

            serializedObject.Update();
            Repaint();
        }
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

        if (selectedRace == null)
            return;

        LineageDefinition definition =
            target as LineageDefinition;

        if (selectedRace.allowedLineageType ==
            LineageType.HybridAncestry)
        {
            EditorGUILayout.HelpBox(
                "This lineage contributes a complete " +
                "attribute shape to ancestry blending.",
                MessageType.Info
            );

            EditorGUILayout.LabelField(
                "Hybrid Ancestry Shape",
                EditorStyles.boldLabel
            );

            EditorGUILayout.PropertyField(
                hybridAttributeShape,
                new GUIContent("Attributes"),
                true
            );

            int total =
                definition?.hybridAttributeShape != null
                    ? definition
                        .hybridAttributeShape
                        .BasePoints()
                    : 0;

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.IntField(
                    "Total Attribute Points",
                    total
                );
            }

            if (total != 90)
            {
                EditorGUILayout.HelpBox(
                    $"Attribute total is {total}. " +
                    "Expected 90.",
                    MessageType.Warning
                );
            }
        }
        else
        {
            EditorGUILayout.HelpBox(
                "This lineage modifies the selected " +
                "race and subrace attribute baseline.",
                MessageType.Info
            );

            EditorGUILayout.LabelField(
                "Animal Species Modifiers",
                EditorStyles.boldLabel
            );

            EditorGUILayout.PropertyField(
                animalSpeciesModifiers,
                new GUIContent("Attribute Modifiers"),
                true
            );

            int total =
                definition?.animalSpeciesModifiers != null
                    ? definition
                        .animalSpeciesModifiers
                        .Total()
                    : 0;

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.IntField(
                    "Modifier Total",
                    total
                );
            }

            if (total != 0)
            {
                EditorGUILayout.HelpBox(
                    $"Modifier total is {total}. " +
                    "Animal Species modifiers should " +
                    "normally total 0.",
                    MessageType.Warning
                );
            }
        }

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