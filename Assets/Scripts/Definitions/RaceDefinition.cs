using System.Collections.Generic;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
            selectedSubrace,
            allowedLineageType
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
        finalAttributesPreview =
            CharacterAttributes.AddModifiers(
                CharacterAttributes.CreateDefault(10),
                modifiersFromHuman
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