using System.Collections.Generic;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Game/Subrace Definition")]
public class SubraceDefinition : ScriptableObject
{
    [Header("Identity")]
    [HideInInspector] public string subraceId;
    public string displayName;
    public RaceDefinition race;

    [TextArea]
    public string description;

    [Header("Base Combat Stat Modifiers")]
    public CharacterBaseStats baseStatModifiers =
        CharacterBaseStats.CreateZero();

    [Header("Comparison")]
    public SubraceDefinition compareToSubrace;

    public CharacterAttributeModifiers modifiersFromComparison =
        CharacterAttributeModifiers.CreateZero();

    [Header("Calculated Preview")]
    [SerializeField]
    private CharacterAttributes finalAttributesPreview =
        CharacterAttributes.CreateDefault(10);

    [SerializeField] private int totalAttributePointsPreview;

    public CharacterAttributes FinalAttributesPreview =>
        finalAttributesPreview;

    public int TotalAttributePointsPreview =>
        totalAttributePointsPreview;

    [Header("Body")]
    public RaceSize size;
    public BodyType bodyType;

    [Header("Character Preview")]
    public GameObject previewPrefab;

    [Header("Equipment / Holding Rules")]
    public bool canHoldItemInMouth;
    public bool canUseMouthWeapons;
    public bool canEquipSaddles;

    public void RecalculatePreview()
    {
        if (finalAttributesPreview == null)
        {
            finalAttributesPreview =
                CharacterAttributes.CreateDefault(10);
        }

        CharacterAttributes comparisonAttributes =
            race != null &&
            race.FinalAttributesPreview != null
                ? race.FinalAttributesPreview
                : CharacterAttributes.CreateDefault(10);

        if (compareToSubrace != null &&
            compareToSubrace != this &&
            compareToSubrace.FinalAttributesPreview != null)
        {
            comparisonAttributes =
                compareToSubrace.FinalAttributesPreview;
        }

        modifiersFromComparison =
            CharacterAttributeModifiers.FromDifference(
                finalAttributesPreview,
                comparisonAttributes
            );

        totalAttributePointsPreview =
            finalAttributesPreview.BasePoints();
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(displayName))
            displayName = name;

        subraceId = MakeId(displayName);

        RecalculatePreview();
    }

    private string MakeId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        StringBuilder builder = new();

        foreach (char character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToLowerInvariant(character));
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
[CustomEditor(typeof(SubraceDefinition))]
public class SubraceDefinitionEditor : Editor
{
    private SerializedProperty displayName;
    private SerializedProperty race;
    private SerializedProperty description;
    private SerializedProperty baseStatModifiers;
    private SerializedProperty compareToSubrace;
    private SerializedProperty modifiersFromComparison;
    private SerializedProperty finalAttributesPreview;
    private SerializedProperty totalAttributePointsPreview;
    private SerializedProperty size;
    private SerializedProperty bodyType;
    private SerializedProperty previewPrefab;
    private SerializedProperty canHoldItemInMouth;
    private SerializedProperty canUseMouthWeapons;
    private SerializedProperty canEquipSaddles;

    private void OnEnable()
    {
        displayName =
            serializedObject.FindProperty(
                "displayName"
            );

        race =
            serializedObject.FindProperty(
                "race"
            );

        description =
            serializedObject.FindProperty(
                "description"
            );

        baseStatModifiers =
            serializedObject.FindProperty(
                "baseStatModifiers"
            );

        compareToSubrace =
            serializedObject.FindProperty(
                "compareToSubrace"
            );

        modifiersFromComparison =
            serializedObject.FindProperty(
                "modifiersFromComparison"
            );

        finalAttributesPreview =
            serializedObject.FindProperty(
                "finalAttributesPreview"
            );

        totalAttributePointsPreview =
            serializedObject.FindProperty(
                "totalAttributePointsPreview"
            );

        size =
            serializedObject.FindProperty(
                "size"
            );

        bodyType =
            serializedObject.FindProperty(
                "bodyType"
            );

        previewPrefab =
            serializedObject.FindProperty(
                "previewPrefab"
            );

        canHoldItemInMouth =
            serializedObject.FindProperty(
                "canHoldItemInMouth"
            );

        canUseMouthWeapons =
            serializedObject.FindProperty(
                "canUseMouthWeapons"
            );

        canEquipSaddles =
            serializedObject.FindProperty(
                "canEquipSaddles"
            );
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space(6f);

        DrawRace();
        DrawIdentity();
        DrawCombatStats();
        DrawAttributes();
        DrawComparison();
        DrawBody();
        DrawEquipmentRules();

        bool changed =
            serializedObject.ApplyModifiedProperties();

        if (changed)
        {
            SubraceDefinition subraceDefinition =
                target as SubraceDefinition;

            if (subraceDefinition != null)
            {
                subraceDefinition.RecalculatePreview();
                EditorUtility.SetDirty(subraceDefinition);

                serializedObject.Update();
                Repaint();
            }
        }
    }

    private void DrawRace()
    {
        EditorGUILayout.LabelField(
            "Race",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(
            race,
            new GUIContent("Race")
        );

        EditorGUILayout.Space();
    }

    private void DrawIdentity()
    {
        EditorGUILayout.LabelField(
            "Identity",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(
            displayName
        );

        EditorGUILayout.PropertyField(
            description
        );

        EditorGUILayout.Space();
    }

    private void DrawCombatStats()
    {
        EditorGUILayout.LabelField(
            "Base Combat Stat Modifiers",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(
            baseStatModifiers,
            true
        );

        EditorGUILayout.Space();
    }

    private void DrawComparison()
    {
        EditorGUILayout.LabelField(
            "Attribute Comparison",
            EditorStyles.boldLabel
        );

        DrawComparisonDropdown();

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.PropertyField(
                modifiersFromComparison,
                new GUIContent("Differences From Target"),
                true
            );
        }

        EditorGUILayout.Space();
    }

    private void DrawComparisonDropdown()
    {
        List<SubraceDefinition> options =
            GetComparisonOptions();

        string[] labels =
            new string[options.Count + 1];

        labels[0] =
            "Race Base (No Comparison)";

        for (int i = 0; i < options.Count; i++)
        {
            labels[i + 1] =
                options[i].displayName;
        }

        SubraceDefinition current =
            compareToSubrace.objectReferenceValue
            as SubraceDefinition;

        int currentIndex = 0;

        for (int i = 0; i < options.Count; i++)
        {
            if (options[i] == current)
            {
                currentIndex = i + 1;
                break;
            }
        }

        int nextIndex =
            EditorGUILayout.Popup(
                "Compare To",
                currentIndex,
                labels
            );

        compareToSubrace.objectReferenceValue =
            nextIndex == 0
                ? null
                : options[nextIndex - 1];
    }

    private List<SubraceDefinition> GetComparisonOptions()
    {
        List<SubraceDefinition> options = new();

        RaceDefinition selectedRace =
            race.objectReferenceValue
            as RaceDefinition;

        SubraceDefinition editedSubrace =
            target as SubraceDefinition;

        if (selectedRace == null)
            return options;

        string[] guids =
            AssetDatabase.FindAssets(
                "t:SubraceDefinition"
            );

        foreach (string guid in guids)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(
                    guid
                );

            SubraceDefinition definition =
                AssetDatabase.LoadAssetAtPath<
                    SubraceDefinition
                >(path);

            if (definition == null ||
                definition == editedSubrace ||
                definition.race == null)
            {
                continue;
            }

            bool sameRace =
                definition.race == selectedRace ||
                string.Equals(
                    definition.race.raceId,
                    selectedRace.raceId,
                    System.StringComparison.OrdinalIgnoreCase
                );

            if (sameRace)
            {
                options.Add(definition);
            }
        }

        options.Sort(
            (first, second) =>
                string.Compare(
                    first.displayName,
                    second.displayName,
                    System.StringComparison.OrdinalIgnoreCase
                )
        );

        return options;
    }

    private void DrawAttributes()
    {
        EditorGUILayout.LabelField(
            "Final Ancestry Attributes",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(
            finalAttributesPreview,
            new GUIContent("Attributes"),
            true
        );

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.PropertyField(
                totalAttributePointsPreview,
                new GUIContent("Total Attribute Points")
            );
        }

        if (totalAttributePointsPreview.intValue != 90)
        {
            EditorGUILayout.HelpBox(
                $"Attribute total is " +
                $"{totalAttributePointsPreview.intValue}. " +
                "Expected 90.",
                MessageType.Warning
            );
        }

        EditorGUILayout.Space();
    }

    private void DrawBody()
    {
        EditorGUILayout.LabelField(
            "Body",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(size);
        EditorGUILayout.PropertyField(bodyType);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField(
            "Character Preview",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(
            previewPrefab
        );

        EditorGUILayout.Space();
    }

    private void DrawEquipmentRules()
    {
        EditorGUILayout.LabelField(
            "Equipment / Holding Rules",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(
            canHoldItemInMouth
        );

        EditorGUILayout.PropertyField(
            canUseMouthWeapons
        );

        EditorGUILayout.PropertyField(
            canEquipSaddles
        );
    }
}
#endif