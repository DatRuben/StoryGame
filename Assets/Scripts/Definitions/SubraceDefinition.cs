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

    [Header("Inheritance")]
    [Tooltip(
        "Optional Subrace inheritance override. " +
        "Leave empty to inherit from the Race's standard Subrace. " +
        "The standard Subrace always inherits directly from the Race."
    )]
    public SubraceDefinition baseSubrace;

    [Header("Base Combat Stat Differences")]
    public CharacterBaseStats baseStatModifiers =
        CharacterBaseStats.CreateZero();

    [Header("Attribute Differences")]
    public CharacterAttributeModifiers attributeDifferences =
        CharacterAttributeModifiers.CreateZero();

    [Header("Calculated Preview")]
    [SerializeField]
    private CharacterBaseStats finalBaseStatsPreview =
        CharacterBaseStats.CreateHumanDefault();

    [SerializeField]
    private CharacterAttributes finalAttributesPreview =
        CharacterAttributes.CreateDefault(10);

    [SerializeField]
    private int totalAttributePointsPreview;

    public CharacterBaseStats FinalBaseStatsPreview =>
        finalBaseStatsPreview;

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

    public SubraceDefinition GetBaseSubrace()
    {
        if (IsDefaultSubrace())
            return null;

        if (baseSubrace != null &&
            baseSubrace != this &&
            IsSameRace(baseSubrace))
        {
            return baseSubrace;
        }

        return GetDefaultSubrace();
    }

    private bool IsSameRace(
        SubraceDefinition other)
    {
        if (other == null ||
            race == null ||
            other.race == null)
        {
            return false;
        }

        return race == other.race ||
               string.Equals(
                   race.raceId,
                   other.race.raceId,
                   System.StringComparison.OrdinalIgnoreCase
               );
    }

    private bool WouldCreateCycle(
        SubraceDefinition candidate)
    {
        if (candidate == null)
            return false;

        HashSet<SubraceDefinition> visited = new()
    {
        this
    };

        SubraceDefinition current = candidate;

        while (current != null)
        {
            if (!visited.Add(current))
                return true;

            current = current.GetBaseSubrace();
        }

        return false;
    }

    public bool HasInheritanceCycle()
    {
        return WouldCreateCycle(
            GetBaseSubrace()
        );
    }

    public CharacterAttributes ResolveFinalAttributes()
    {
        return ResolveFinalAttributes(
            new HashSet<SubraceDefinition>()
        );
    }

    private CharacterAttributes ResolveFinalAttributes(
        HashSet<SubraceDefinition> visited)
    {
        CharacterAttributes raceAttributes =
            race != null &&
            race.FinalAttributesPreview != null
                ? CharacterAttributes.Copy(
                    race.FinalAttributesPreview
                )
                : CharacterAttributes.CreateDefault(10);

        if (!visited.Add(this))
            return raceAttributes;

        SubraceDefinition resolvedBase =
            GetBaseSubrace();

        CharacterAttributes inheritedAttributes =
            resolvedBase != null
                ? resolvedBase.ResolveFinalAttributes(
                    visited
                )
                : raceAttributes;

        visited.Remove(this);

        CharacterAttributeModifiers differences =
            IsDefaultSubrace()
                ? CharacterAttributeModifiers.CreateZero()
                : attributeDifferences ??
                  CharacterAttributeModifiers.CreateZero();

        return CharacterAttributes.AddModifiers(
            inheritedAttributes,
            differences
        );
    }

    public CharacterBaseStats ResolveBaseStats()
    {
        return ResolveBaseStats(
            new HashSet<SubraceDefinition>()
        );
    }

    private CharacterBaseStats ResolveBaseStats(
        HashSet<SubraceDefinition> visited)
    {
        CharacterBaseStats raceBaseStats =
            race != null
                ? CharacterBaseStats.Copy(
                    race.baseStats
                )
                : CharacterBaseStats.CreateHumanDefault();

        if (!visited.Add(this))
            return raceBaseStats;

        SubraceDefinition resolvedBase =
            GetBaseSubrace();

        CharacterBaseStats inheritedStats =
            resolvedBase != null
                ? resolvedBase.ResolveBaseStats(
                    visited
                )
                : raceBaseStats;

        visited.Remove(this);

        CharacterBaseStats differences =
            IsDefaultSubrace()
                ? CharacterBaseStats.CreateZero()
                : baseStatModifiers ??
                  CharacterBaseStats.CreateZero();

        return CharacterBaseStats.Add(
            inheritedStats,
            differences
        );
    }

    public void RecalculatePreview()
    {
        if (attributeDifferences == null)
        {
            attributeDifferences =
                CharacterAttributeModifiers.CreateZero();
        }

        if (baseStatModifiers == null)
        {
            baseStatModifiers =
                CharacterBaseStats.CreateZero();
        }

        if (IsDefaultSubrace())
        {
            baseSubrace = null;

            attributeDifferences =
                CharacterAttributeModifiers.CreateZero();

            baseStatModifiers =
                CharacterBaseStats.CreateZero();
        }

        finalBaseStatsPreview =
            ResolveBaseStats();

        finalAttributesPreview =
            ResolveFinalAttributes();

        totalAttributePointsPreview =
            finalAttributesPreview.BasePoints();
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(displayName))
            displayName = name;

        subraceId = MakeId(displayName);

        if (baseSubrace != null &&
            (baseSubrace == this ||
             !IsSameRace(baseSubrace) ||
             WouldCreateCycle(baseSubrace)))
        {
            baseSubrace = null;
        }

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
    private SerializedProperty baseSubrace;
    private SerializedProperty baseStatModifiers;
    private SerializedProperty attributeDifferences;
    private SerializedProperty finalBaseStatsPreview;
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

        baseSubrace =
            serializedObject.FindProperty(
                "baseSubrace"
            );

        baseStatModifiers =
            serializedObject.FindProperty(
                "baseStatModifiers"
            );

        attributeDifferences =
            serializedObject.FindProperty(
                "attributeDifferences"
            );

        finalBaseStatsPreview =
            serializedObject.FindProperty(
                "finalBaseStatsPreview"
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
        DrawInheritance();
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

    private void DrawInheritance()
    {
        EditorGUILayout.LabelField(
            "Subrace Inheritance",
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
                "Assign a Race before configuring inheritance.",
                MessageType.Warning
            );

            EditorGUILayout.Space();
            return;
        }

        SubraceDefinition standardSubrace =
            selectedRace.standardSubrace;

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.ObjectField(
                "Standard Subrace",
                standardSubrace,
                typeof(SubraceDefinition),
                false
            );
        }

        bool isStandard =
            definition != null &&
            definition.IsDefaultSubrace();

        if (isStandard)
        {
            EditorGUILayout.HelpBox(
                "This is the Race's standard Subrace. " +
                "It inherits directly from the Race and " +
                "cannot have stat or attribute differences.",
                MessageType.Info
            );
        }
        else
        {
            EditorGUILayout.PropertyField(
                baseSubrace,
                new GUIContent(
                    "Base Subrace Override"
                )
            );

            EditorGUILayout.HelpBox(
                "Leave the override empty to inherit from " +
                "the Race's standard Subrace.",
                MessageType.Info
            );

            SubraceDefinition resolvedBase =
                definition?.GetBaseSubrace();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField(
                    "Resolved Base Subrace",
                    resolvedBase,
                    typeof(SubraceDefinition),
                    false
                );
            }

            if (resolvedBase == null)
            {
                EditorGUILayout.HelpBox(
                    "No valid base Subrace could be resolved.",
                    MessageType.Warning
                );
            }
        }

        if (definition != null &&
            definition.HasInheritanceCycle())
        {
            EditorGUILayout.HelpBox(
                "This inheritance relationship creates a cycle.",
                MessageType.Error
            );
        }

        EditorGUILayout.Space();
    }

    private void DrawCombatStats()
    {
        EditorGUILayout.LabelField(
            "Base Combat Stats",
            EditorStyles.boldLabel
        );

        SubraceDefinition definition =
            target as SubraceDefinition;

        bool isStandard =
            definition != null &&
            definition.IsDefaultSubrace();

        string baseName =
            GetBaseName(definition);

        if (isStandard)
        {
            EditorGUILayout.HelpBox(
                "The standard Subrace uses the Race's " +
                "base combat stats with no differences.",
                MessageType.Info
            );
        }
        else
        {
            EditorGUILayout.HelpBox(
                $"These values are the combat-stat " +
                $"differences from {baseName}.",
                MessageType.Info
            );
        }

        using (new EditorGUI.DisabledScope(isStandard))
        {
            EditorGUILayout.PropertyField(
                baseStatModifiers,
                new GUIContent(
                    "Differences From Base"
                ),
                true
            );
        }

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.PropertyField(
                finalBaseStatsPreview,
                new GUIContent(
                    "Final Base Combat Stats"
                ),
                true
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

    private void DrawAttributeInheritance()
    {
        EditorGUILayout.LabelField(
            "Attributes",
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

        bool isStandard =
            definition != null &&
            definition.IsDefaultSubrace();

        string baseName =
            GetBaseName(definition);

        if (isStandard)
        {
            EditorGUILayout.HelpBox(
                "The standard Subrace uses the Race's " +
                "attributes with no differences.",
                MessageType.Info
            );
        }
        else
        {
            EditorGUILayout.HelpBox(
                $"These values are the attribute " +
                $"differences from {baseName}.",
                MessageType.Info
            );
        }

        using (new EditorGUI.DisabledScope(isStandard))
        {
            EditorGUILayout.PropertyField(
                attributeDifferences,
                new GUIContent(
                    "Differences From Base"
                ),
                true
            );
        }

        int modifierTotal =
            definition
                ?.attributeDifferences
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

        if (!isStandard &&
            modifierTotal != 0)
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

    private string GetBaseName(
        SubraceDefinition definition)
    {
        if (definition == null)
            return "the Race";

        if (definition.IsDefaultSubrace())
        {
            return definition.race != null
                ? $"{definition.race.displayName} Race"
                : "the Race";
        }

        SubraceDefinition resolvedBase =
            definition.GetBaseSubrace();

        if (resolvedBase != null)
        {
            return !string.IsNullOrWhiteSpace(
                resolvedBase.displayName)
                    ? resolvedBase.displayName
                    : resolvedBase.name;
        }

        return definition.race != null
            ? $"{definition.race.displayName} Race"
            : "the Race";
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