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

[CreateAssetMenu(menuName = "Game/Race Definition")]
public class RaceDefinition : ScriptableObject
{
    [Header("Identity")]
    [HideInInspector]
    public string raceId;

    public string displayName;
    public BaseRace baseRace;

    [TextArea]
    public string description;

    [Header("Standard Subrace")]
    public SubraceDefinition standardSubrace;

    [Header("Base Combat Stats")]
    public CharacterBaseStats baseStats =
        CharacterBaseStats.CreateHumanDefault();

    [Header("Compared To Human")]
    public CharacterAttributeModifiers modifiersFromHuman =
        CharacterAttributeModifiers.CreateZero();

    [Header("Calculated Preview")]
    [SerializeField]
    private CharacterAttributes finalAttributesPreview =
        CharacterAttributes.CreateDefault(10);

    [SerializeField]
    private int totalAttributePointsPreview;

    public CharacterAttributes FinalAttributesPreview =>
        finalAttributesPreview;

    public int TotalAttributePointsPreview =>
        totalAttributePointsPreview;

    [Header("Lineage Rules")]
    public LineageType allowedLineageType =
        LineageType.HybridAncestry;

    [Tooltip(
        "The lineage initially selected for an Animal Species race."
    )]
    public LineageDefinition defaultLineage;

    [Min(0)]
    public int minLineages = 0;

    [Min(0)]
    public int maxLineages = 0;

    public bool CanUseLineages()
    {
        return maxLineages > 0;
    }

    public bool IsLineageAllowed(
        LineageSelection lineage,
        SubraceDefinition selectedSubrace)
    {
        if (lineage == null ||
            !CanUseLineages())
        {
            return false;
        }

        return lineage.IsAllowedFor(
            this,
            selectedSubrace
        );
    }

    public LineageSelection GetDefaultLineageSelection()
    {
        if (allowedLineageType !=
                LineageType.AnimalSpecies ||
            defaultLineage == null ||
            !defaultLineage.IsAllowedForRace(this))
        {
            return null;
        }

        return LineageSelection.FromCustomLineage(
            defaultLineage
        );
    }

    public bool AreLineageSelectionsValid(
        SubraceDefinition subraceDefinition,
        List<LineageSelection> lineages,
        out string errorMessage)
    {
        int count =
            lineages == null
                ? 0
                : lineages.Count;

        if (!IsLineageCountValid(
            count,
            out errorMessage))
        {
            return false;
        }

        if (lineages == null)
            return true;

        HashSet<string> usedSelectionIds =
            new HashSet<string>(
                System.StringComparer.OrdinalIgnoreCase
            );

        foreach (LineageSelection lineage
                 in lineages)
        {
            if (lineage == null ||
                !lineage.IsValid)
            {
                errorMessage =
                    "Missing or invalid lineage selection.";

                return false;
            }

            if (string.IsNullOrWhiteSpace(
                lineage.SelectionId))
            {
                errorMessage =
                    $"{lineage.DisplayName} has no lineage selection ID.";

                return false;
            }

            if (!usedSelectionIds.Add(
                lineage.SelectionId))
            {
                errorMessage =
                    $"{lineage.DisplayName} was selected more than once.";

                return false;
            }

            if (!IsLineageAllowed(
                lineage,
                subraceDefinition))
            {
                errorMessage =
                    $"{displayName} cannot use lineage " +
                    $"{lineage.DisplayName}.";

                return false;
            }
        }

        errorMessage = "";
        return true;
    }

    private bool IsLineageCountValid(
        int count,
        out string errorMessage)
    {
        errorMessage = "";

        if (!CanUseLineages())
        {
            if (count > 0)
            {
                errorMessage =
                    $"{displayName} cannot use lineages.";

                return false;
            }

            return true;
        }

        if (count < minLineages)
        {
            errorMessage =
                $"{displayName} requires at least " +
                $"{minLineages} lineage.";

            return false;
        }

        if (count > maxLineages)
        {
            errorMessage =
                $"{displayName} can only use up to " +
                $"{maxLineages} lineages.";

            return false;
        }

        return true;
    }

    public void RecalculatePreview()
    {
        if (finalAttributesPreview == null)
        {
            finalAttributesPreview =
                CharacterAttributes.CreateDefault(10);
        }

        modifiersFromHuman =
            CharacterAttributeModifiers.FromDifference(
                finalAttributesPreview,
                CharacterAttributes.CreateDefault(10)
            );

        totalAttributePointsPreview =
            finalAttributesPreview.BasePoints();
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(displayName))
            displayName = name;

        raceId = MakeId(displayName);

        RecalculatePreview();

        int lineageLimit =
            allowedLineageType ==
            LineageType.HybridAncestry
                ? 2
                : 3;

        minLineages = Mathf.Clamp(
            minLineages,
            0,
            lineageLimit
        );

        maxLineages = Mathf.Clamp(
            maxLineages,
            0,
            lineageLimit
        );

        if (maxLineages < minLineages)
            maxLineages = minLineages;

        if (allowedLineageType !=
                LineageType.AnimalSpecies ||
            !CanUseLineages())
        {
            defaultLineage = null;
        }
        else if (defaultLineage != null &&
                 !defaultLineage.IsAllowedForRace(this))
        {
            defaultLineage = null;
        }
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
[CustomEditor(typeof(RaceDefinition))]
public class RaceDefinitionEditor : Editor
{
    private SerializedProperty allowedLineageType;
    private SerializedProperty minLineages;
    private SerializedProperty maxLineages;
    private SerializedProperty defaultLineage;
    private SerializedProperty modifiersFromHuman;
    private SerializedProperty finalAttributesPreview;
    private SerializedProperty totalAttributePointsPreview;

    private void OnEnable()
    {
        allowedLineageType =
            serializedObject.FindProperty(
                "allowedLineageType"
            );

        minLineages =
            serializedObject.FindProperty(
                "minLineages"
            );

        maxLineages =
            serializedObject.FindProperty(
                "maxLineages"
            );

        defaultLineage =
            serializedObject.FindProperty(
                "defaultLineage"
            );
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty property =
            serializedObject.GetIterator();

        bool enterChildren = true;

        while (property.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (property.propertyPath ==
                "modifiersFromHuman")
            {
                continue;
            }

            if (property.propertyPath ==
                "finalAttributesPreview")
            {
                DrawAttributes();
                continue;
            }

            if (property.propertyPath ==
                "totalAttributePointsPreview")
            {
                continue;
            }

            if (property.propertyPath == "defaultLineage")
                continue;

            if (property.propertyPath == "minLineages")
            {
                DrawLineageDropdowns();
                continue;
            }

            if (property.propertyPath == "maxLineages")
                continue;

            using (new EditorGUI.DisabledScope(
                property.propertyPath == "m_Script"))
            {
                EditorGUILayout.PropertyField(
                    property,
                    true
                );
            }
        }

        serializedObject.ApplyModifiedProperties();
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

        EditorGUILayout.Space();

        EditorGUILayout.LabelField(
            "Compared To Human",
            EditorStyles.boldLabel
        );

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.PropertyField(
                modifiersFromHuman,
                new GUIContent("Attribute Differences"),
                true
            );

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

    private void DrawLineageDropdowns()
    {
        LineageType lineageType =
            (LineageType)allowedLineageType.intValue;

        int limit =
            lineageType == LineageType.AnimalSpecies
                ? 3
                : 2;

        int minimum =
            Mathf.Clamp(
                minLineages.intValue,
                0,
                limit
            );

        int[] minimumValues =
            MakeValues(0, limit);

        minimum = EditorGUILayout.IntPopup(
            "Minimum Lineages",
            minimum,
            MakeLabels(minimumValues),
            minimumValues
        );

        int maximum =
            Mathf.Clamp(
                maxLineages.intValue,
                minimum,
                limit
            );

        int[] maximumValues =
            MakeValues(minimum, limit);

        maximum = EditorGUILayout.IntPopup(
            "Maximum Lineages",
            maximum,
            MakeLabels(maximumValues),
            maximumValues
        );

        minLineages.intValue = minimum;
        maxLineages.intValue = maximum;

        DrawDefaultLineage();
    }

    private void DrawDefaultLineage()
    {
        LineageType lineageType =
            (LineageType)allowedLineageType.enumValueIndex;

        if (lineageType !=
                LineageType.AnimalSpecies ||
            maxLineages.intValue <= 0)
        {
            return;
        }

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(
            defaultLineage,
            new GUIContent("Default Lineage")
        );

        RaceDefinition raceDefinition =
            target as RaceDefinition;

        LineageDefinition selectedDefault =
            defaultLineage.objectReferenceValue
            as LineageDefinition;

        if (selectedDefault == null)
        {
            EditorGUILayout.HelpBox(
                "Animal Species races need a default lineage.",
                MessageType.Warning
            );

            return;
        }

        if (raceDefinition != null &&
            !selectedDefault.IsAllowedForRace(
                raceDefinition))
        {
            EditorGUILayout.HelpBox(
                "The default lineage must belong to this race.",
                MessageType.Error
            );
        }
    }

    private static int[] MakeValues(
        int minimum,
        int maximum)
    {
        int[] values =
            new int[maximum - minimum + 1];

        for (int i = 0; i < values.Length; i++)
            values[i] = minimum + i;

        return values;
    }

    private static string[] MakeLabels(
        int[] values)
    {
        string[] labels =
            new string[values.Length];

        for (int i = 0; i < values.Length; i++)
            labels[i] = values[i].ToString();

        return labels;
    }
}
#endif