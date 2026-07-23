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

    [Header("Attributes")]
    public CharacterAttributeModifiers modifiersFromDefaultSubrace =
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

    public SubraceDefinition GetDefaultSubrace()
    {
        return race != null
            ? race.standardSubrace
            : null;
    }

    public bool IsDefaultSubrace()
    {
        return GetDefaultSubrace() == this;
    }

    public void RecalculatePreview()
    {
        CharacterAttributes baseAttributes =
            race != null &&
            race.FinalAttributesPreview != null
                ? race.FinalAttributesPreview
                : CharacterAttributes.CreateDefault(10);

        if (modifiersFromDefaultSubrace == null)
        {
            modifiersFromDefaultSubrace =
                CharacterAttributeModifiers.CreateZero();
        }

        if (IsDefaultSubrace())
        {
            modifiersFromDefaultSubrace =
                CharacterAttributeModifiers.CreateZero();
        }

        finalAttributesPreview =
            CharacterAttributes.AddModifiers(
                baseAttributes,
                modifiersFromDefaultSubrace
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
    private SerializedProperty modifiersFromDefaultSubrace;
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

        modifiersFromDefaultSubrace =
            serializedObject.FindProperty(
                "modifiersFromDefaultSubrace"
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
        DrawAttributeInheritance();
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

    private void DrawAttributeInheritance()
    {
        EditorGUILayout.LabelField(
            "Attribute Inheritance",
            EditorStyles.boldLabel
        );

        SubraceDefinition definition =
            target as SubraceDefinition;

        RaceDefinition selectedRace =
            race.objectReferenceValue
            as RaceDefinition;

        if (selectedRace == null)
        {
            EditorGUILayout.HelpBox(
                "Assign a Race before configuring attributes.",
                MessageType.Warning
            );

            EditorGUILayout.Space();
            return;
        }

        SubraceDefinition defaultSubrace =
            selectedRace.standardSubrace;

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.ObjectField(
                "Default Subrace",
                defaultSubrace,
                typeof(SubraceDefinition),
                false
            );
        }

        if (defaultSubrace == null)
        {
            EditorGUILayout.HelpBox(
                $"{selectedRace.displayName} has no default " +
                "Subrace assigned.",
                MessageType.Warning
            );
        }

        bool isDefault =
            defaultSubrace != null &&
            defaultSubrace == definition;

        if (isDefault)
        {
            EditorGUILayout.HelpBox(
                "This is the Race's default Subrace. " +
                "Its attribute differences are fixed at zero.",
                MessageType.Info
            );
        }
        else
        {
            string defaultName =
                defaultSubrace != null
                    ? defaultSubrace.displayName
                    : selectedRace.displayName;

            EditorGUILayout.HelpBox(
                $"These values are the differences from " +
                $"{defaultName}.",
                MessageType.Info
            );
        }

        using (new EditorGUI.DisabledScope(isDefault))
        {
            EditorGUILayout.PropertyField(
                modifiersFromDefaultSubrace,
                new GUIContent(
                    "Differences From Default Subrace"
                ),
                true
            );
        }

        SubraceDefinition currentDefinition =
            target as SubraceDefinition;

        int modifierTotal =
            currentDefinition
                ?.modifiersFromDefaultSubrace
                ?.Total() ?? 0;

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.IntField(
                "Difference Total",
                modifierTotal
            );

            EditorGUILayout.PropertyField(
                finalAttributesPreview,
                new GUIContent(
                    "Final Ancestry Attributes"
                ),
                true
            );

            EditorGUILayout.PropertyField(
                totalAttributePointsPreview,
                new GUIContent(
                    "Total Attribute Points"
                )
            );
        }

        if (!isDefault && modifierTotal != 0)
        {
            EditorGUILayout.HelpBox(
                $"Difference total is {modifierTotal}. " +
                "Expected 0.",
                MessageType.Warning
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