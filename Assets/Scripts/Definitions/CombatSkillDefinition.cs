using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum CombatSkillOriginSource
{
    None,
    Character,
    HeldItem
}

public enum CombatSkillHeldItemSource
{
    SkillSourceItem
}

[CreateAssetMenu(
    menuName = "Game/Combat/Combat Skill Definition"
)]
public sealed class CombatSkillDefinition :
    ScriptableObject
{
    [Header("Identity")]
    public string skillName;

    [Header("Gameplay Action")]

    public CombatSkillActionDefinition action;

    [Header("Timing")]

    [Min(0f)]
    public float castTime;

    [Min(0f)]
    public float cooldown;

    [Header("Resource Cost")]

    [Min(0f)]
    public float aetherCost;

    [Header("Action Origin")]

    public CombatSkillOriginSource
        originSource =
            CombatSkillOriginSource.None;

    public CharacterActionPointType
        characterActionPoint =
            CharacterActionPointType.Center;

    public string customActionPointId;

    public CombatSkillHeldItemSource
        heldItemSource =
            CombatSkillHeldItemSource.SkillSourceItem;
}

#if UNITY_EDITOR
[CustomEditor(typeof(CombatSkillDefinition))]
public sealed class CombatSkillDefinitionEditor :
    Editor
{
    private SerializedProperty skillName;
    private SerializedProperty castTime;
    private SerializedProperty cooldown;
    private SerializedProperty aetherCost;
    private SerializedProperty action;

    private SerializedProperty originSource;
    private SerializedProperty characterActionPoint;
    private SerializedProperty customActionPointId;
    private SerializedProperty heldItemSource;

    private void OnEnable()
    {
        skillName =
            serializedObject.FindProperty(
                "skillName"
            );

        action =
            serializedObject.FindProperty(
                "action"
            );

        castTime =
            serializedObject.FindProperty(
                "castTime"
            );

        cooldown =
            serializedObject.FindProperty(
                "cooldown"
            );

        aetherCost =
            serializedObject.FindProperty(
                "aetherCost"
            );

        originSource =
            serializedObject.FindProperty(
                "originSource"
            );

        characterActionPoint =
            serializedObject.FindProperty(
                "characterActionPoint"
            );

        customActionPointId =
            serializedObject.FindProperty(
                "customActionPointId"
            );

        heldItemSource =
            serializedObject.FindProperty(
                "heldItemSource"
            );
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawIdentity();
        DrawTiming();
        DrawResourceCost();
        DrawActionOrigin();
        DrawGameplayAction();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawGameplayAction()
    {
        EditorGUILayout.LabelField(
            "Gameplay Action",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(
            action
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
            skillName
        );

        EditorGUILayout.Space();
    }

    private void DrawTiming()
    {
        EditorGUILayout.LabelField(
            "Timing",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(
            castTime
        );

        EditorGUILayout.PropertyField(
            cooldown
        );

        EditorGUILayout.Space();
    }

    private void DrawResourceCost()
    {
        EditorGUILayout.LabelField(
            "Resource Cost",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(
            aetherCost
        );

        EditorGUILayout.Space();
    }

    private void DrawActionOrigin()
    {
        EditorGUILayout.LabelField(
            "Action Origin",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(
            originSource,
            new GUIContent(
                "Origin Source"
            )
        );

        CombatSkillOriginSource selectedSource =
            (CombatSkillOriginSource)
                originSource.enumValueIndex;

        switch (selectedSource)
        {
            case CombatSkillOriginSource.None:
                break;

            case CombatSkillOriginSource.Character:
                DrawCharacterOrigin();
                break;

            case CombatSkillOriginSource.HeldItem:
                DrawHeldItemOrigin();
                break;
        }

        EditorGUILayout.Space();
    }

    private void DrawHeldItemOrigin()
    {
        EditorGUILayout.PropertyField(
            heldItemSource,
            new GUIContent(
                "Held Item Source"
            )
        );
    }

    private void DrawCharacterOrigin()
    {
        EditorGUILayout.PropertyField(
            characterActionPoint,
            new GUIContent(
                "Character Action Point"
            )
        );

        CharacterActionPointType selectedPoint =
            (CharacterActionPointType)
                characterActionPoint.enumValueIndex;

        if (selectedPoint !=
            CharacterActionPointType.Custom)
        {
            return;
        }

        EditorGUILayout.PropertyField(
            customActionPointId,
            new GUIContent(
                "Custom Action Point ID"
            )
        );
    }
}
#endif