using System.Collections.Generic;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum CharacterAppearanceOptionCategory
{
    Head,
    Ears,
    Horns,
    Tail,
    Hair,
    Eyes,
    Markings
}

public enum CharacterAppearanceOptionAvailability
{
    Hidden,
    Locked,
    Available
}

[CreateAssetMenu(
    menuName = "Game/Character Appearance Option"
)]
public class CharacterAppearanceOptionDefinition :
    ScriptableObject
{
    [HideInInspector]
    public string optionId;

    public string displayName;

    public CharacterAppearanceOptionCategory category;

    [Tooltip(
        "The 2D image shown on this option's character creator button."
    )]
    public Sprite optionImage;

    [Tooltip(
        "The actual 3D cosmetic prefab used on the character."
    )]
    public GameObject modelPrefab;

    [Tooltip(
        "The race this appearance option belongs to."
    )]
    public RaceDefinition race;

    [Tooltip(
        "Leave both requirement lists empty to make " +
        "the option available to every subrace and " +
        "lineage of the selected race."
    )]
    public List<SubraceDefinition> allowedSubraces = new();

    public List<LineageDefinition> allowedLineages = new();

    [Tooltip(
        "Used as a fallback when the previously selected " +
        "option is no longer available."
    )]
    public bool isDefaultOption;

    public CharacterAppearanceOptionAvailability GetAvailability(
        RaceDefinition selectedRace,
        SubraceDefinition selectedSubrace,
        List<LineageDefinition> selectedLineages)
    {
        if (!IsShownForRace(selectedRace))
        {
            return CharacterAppearanceOptionAvailability.Hidden;
        }

        if (!HasAncestryRequirements())
        {
            return CharacterAppearanceOptionAvailability.Available;
        }

        if (IsAllowedForSubrace(selectedSubrace))
        {
            return CharacterAppearanceOptionAvailability.Available;
        }

        if (IsAllowedForAnyLineage(selectedLineages))
        {
            return CharacterAppearanceOptionAvailability.Available;
        }

        return CharacterAppearanceOptionAvailability.Locked;
    }

    private bool IsShownForRace(
        RaceDefinition selectedRace)
    {
        if (selectedRace == null ||
            race == null)
        {
            return false;
        }

        return race == selectedRace ||
               race.raceId == selectedRace.raceId;
    }

    private bool HasAncestryRequirements()
    {
        bool hasSubraceRequirements =
            allowedSubraces != null &&
            allowedSubraces.Count > 0;

        bool hasLineageRequirements =
            allowedLineages != null &&
            allowedLineages.Count > 0;

        return hasSubraceRequirements ||
               hasLineageRequirements;
    }

    private bool IsAllowedForSubrace(
        SubraceDefinition selectedSubrace)
    {
        if (selectedSubrace == null ||
            allowedSubraces == null)
        {
            return false;
        }

        foreach (SubraceDefinition subraceDefinition
                 in allowedSubraces)
        {
            if (subraceDefinition == null)
                continue;

            if (subraceDefinition == selectedSubrace ||
                subraceDefinition.subraceId ==
                selectedSubrace.subraceId)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsAllowedForAnyLineage(
        List<LineageDefinition> selectedLineages)
    {
        if (selectedLineages == null ||
            allowedLineages == null)
        {
            return false;
        }

        foreach (LineageDefinition selectedLineage
                 in selectedLineages)
        {
            if (selectedLineage == null)
                continue;

            foreach (LineageDefinition allowedLineage
                     in allowedLineages)
            {
                if (allowedLineage == null)
                    continue;

                if (allowedLineage == selectedLineage ||
                    allowedLineage.lineageId ==
                    selectedLineage.lineageId)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(displayName))
            displayName = name;

        optionId = MakeId(name);
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
[CustomEditor(typeof(CharacterAppearanceOptionDefinition))]
public class CharacterAppearanceOptionDefinitionEditor : Editor
{
    private SerializedProperty displayName;
    private SerializedProperty category;
    private SerializedProperty optionImage;
    private SerializedProperty modelPrefab;
    private SerializedProperty race;
    private SerializedProperty allowedSubraces;
    private SerializedProperty allowedLineages;
    private SerializedProperty isDefaultOption;

    private void OnEnable()
    {
        displayName =
            serializedObject.FindProperty("displayName");

        category =
            serializedObject.FindProperty("category");

        optionImage =
            serializedObject.FindProperty("optionImage");

        modelPrefab =
            serializedObject.FindProperty("modelPrefab");

        race =
            serializedObject.FindProperty("race");

        allowedSubraces =
            serializedObject.FindProperty("allowedSubraces");

        allowedLineages =
            serializedObject.FindProperty("allowedLineages");

        isDefaultOption =
            serializedObject.FindProperty("isDefaultOption");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space(6f);

        DrawRace();
        DrawIdentity();
        DrawVisuals();

        RemoveInvalidRequirements();
        DrawRequirements();

        DrawDefaults();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawIdentity()
    {
        EditorGUILayout.LabelField(
            "Identity",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(displayName);
        EditorGUILayout.PropertyField(category);

        EditorGUILayout.Space();
    }

    private void DrawVisuals()
    {
        EditorGUILayout.LabelField(
            "Button Display",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(
            optionImage,
            new GUIContent("Option Image")
        );

        EditorGUILayout.Space();

        EditorGUILayout.LabelField(
            "Character Model",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(
            modelPrefab,
            new GUIContent("Model Prefab")
        );

        EditorGUILayout.Space();
    }

    private void DrawRace()
    {
        EditorGUILayout.PropertyField(
            race,
            new GUIContent("Race")
        );

        EditorGUILayout.Space();
    }

    private void DrawRequirements()
    {
        EditorGUILayout.LabelField(
            "Ancestry Requirements",
            EditorStyles.boldLabel
        );

        List<RaceDefinition> races =
            GetSelectedRaces();

        if (races.Count == 0)
        {
            EditorGUILayout.LabelField(
                "Select a race first."
            );

            EditorGUILayout.Space();
            return;
        }

        DrawSubraceOptions(races);
        EditorGUILayout.Space();
        DrawLineageOptions(races);

        EditorGUILayout.Space();
    }

    private void DrawSubraceOptions(
        List<RaceDefinition> races)
    {
        EditorGUILayout.LabelField(
            "Allowed Subraces",
            EditorStyles.boldLabel
        );

        List<SubraceDefinition> definitions =
            GetValidSubraces(races);

        if (definitions.Count == 0)
        {
            EditorGUILayout.LabelField(
                "No connected subraces found."
            );

            return;
        }

        foreach (SubraceDefinition definition
                 in definitions)
        {
            bool selected =
                ContainsReference(
                    allowedSubraces,
                    definition
                );

            bool nextSelected =
                EditorGUILayout.ToggleLeft(
                    definition.displayName,
                    selected
                );

            if (nextSelected != selected)
            {
                SetReferenceSelected(
                    allowedSubraces,
                    definition,
                    nextSelected
                );
            }
        }
    }

    private void DrawLineageOptions(
        List<RaceDefinition> races)
    {
        EditorGUILayout.LabelField(
            "Allowed Lineages",
            EditorStyles.boldLabel
        );

        List<LineageDefinition> definitions =
            GetValidLineages(races);

        if (definitions.Count == 0)
        {
            EditorGUILayout.LabelField(
                "No connected lineages found."
            );

            return;
        }

        foreach (LineageDefinition definition
                 in definitions)
        {
            bool selected =
                ContainsReference(
                    allowedLineages,
                    definition
                );

            bool nextSelected =
                EditorGUILayout.ToggleLeft(
                    definition.displayName,
                    selected
                );

            if (nextSelected != selected)
            {
                SetReferenceSelected(
                    allowedLineages,
                    definition,
                    nextSelected
                );
            }
        }
    }

    private void DrawDefaults()
    {
        EditorGUILayout.LabelField(
            "Defaults",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(
            isDefaultOption,
            new GUIContent("Is Default Option")
        );
    }

    private List<RaceDefinition> GetSelectedRaces()
    {
        List<RaceDefinition> races = new();

        RaceDefinition selectedRace =
            race.objectReferenceValue
            as RaceDefinition;

        if (selectedRace != null)
            races.Add(selectedRace);

        return races;
    }

    private List<SubraceDefinition> GetValidSubraces(
        List<RaceDefinition> races)
    {
        List<SubraceDefinition> definitions = new();

        string[] guids =
            AssetDatabase.FindAssets(
                "t:SubraceDefinition"
            );

        foreach (string guid in guids)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(guid);

            SubraceDefinition definition =
                AssetDatabase.LoadAssetAtPath<
                    SubraceDefinition
                >(path);

            if (definition == null ||
                !IsSelectedRace(
                    definition.race,
                    races
                ))
            {
                continue;
            }

            definitions.Add(definition);
        }

        definitions.Sort(
            (a, b) =>
                string.Compare(
                    a.displayName,
                    b.displayName,
                    System.StringComparison.OrdinalIgnoreCase
                )
        );

        return definitions;
    }

    private List<LineageDefinition> GetValidLineages(
        List<RaceDefinition> races)
    {
        List<LineageDefinition> definitions = new();

        string[] guids =
            AssetDatabase.FindAssets(
                "t:LineageDefinition"
            );

        foreach (string guid in guids)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(guid);

            LineageDefinition definition =
                AssetDatabase.LoadAssetAtPath<
                    LineageDefinition
                >(path);

            if (definition == null ||
                !IsSelectedRace(
                    definition.allowedRace,
                    races
                ))
            {
                continue;
            }

            definitions.Add(definition);
        }

        definitions.Sort(
            (a, b) =>
                string.Compare(
                    a.displayName,
                    b.displayName,
                    System.StringComparison.OrdinalIgnoreCase
                )
        );

        return definitions;
    }

    private bool IsSelectedRace(
        RaceDefinition race,
        List<RaceDefinition> selectedRaces)
    {
        if (race == null ||
            selectedRaces == null)
        {
            return false;
        }

        foreach (RaceDefinition selectedRace
                 in selectedRaces)
        {
            if (selectedRace == null)
                continue;

            if (selectedRace == race ||
                selectedRace.raceId == race.raceId)
            {
                return true;
            }
        }

        return false;
    }

    private bool ContainsReference(
        SerializedProperty list,
        UnityEngine.Object value)
    {
        for (int i = 0;
             i < list.arraySize;
             i++)
        {
            if (list
                .GetArrayElementAtIndex(i)
                .objectReferenceValue == value)
            {
                return true;
            }
        }

        return false;
    }

    private void SetReferenceSelected(
        SerializedProperty list,
        UnityEngine.Object value,
        bool selected)
    {
        if (selected)
        {
            if (ContainsReference(list, value))
                return;

            int index = list.arraySize;

            list.InsertArrayElementAtIndex(index);

            list
                .GetArrayElementAtIndex(index)
                .objectReferenceValue = value;

            return;
        }

        for (int i = list.arraySize - 1;
             i >= 0;
             i--)
        {
            if (list
                .GetArrayElementAtIndex(i)
                .objectReferenceValue != value)
            {
                continue;
            }

            RemoveElement(list, i);
        }
    }

    private void RemoveInvalidRequirements()
    {
        List<RaceDefinition> races =
            GetSelectedRaces();

        for (int i = allowedSubraces.arraySize - 1;
             i >= 0;
             i--)
        {
            SubraceDefinition definition =
                allowedSubraces
                    .GetArrayElementAtIndex(i)
                    .objectReferenceValue
                as SubraceDefinition;

            if (definition == null ||
                !IsSelectedRace(
                    definition.race,
                    races
                ))
            {
                RemoveElement(
                    allowedSubraces,
                    i
                );
            }
        }

        for (int i = allowedLineages.arraySize - 1;
             i >= 0;
             i--)
        {
            LineageDefinition definition =
                allowedLineages
                    .GetArrayElementAtIndex(i)
                    .objectReferenceValue
                as LineageDefinition;

            if (definition == null ||
                !IsSelectedRace(
                    definition.allowedRace,
                    races
                ))
            {
                RemoveElement(
                    allowedLineages,
                    i
                );
            }
        }
    }

    private void RemoveElement(
        SerializedProperty list,
        int index)
    {
        int oldSize = list.arraySize;

        list.DeleteArrayElementAtIndex(index);

        if (list.arraySize == oldSize)
            list.DeleteArrayElementAtIndex(index);
    }
}
#endif