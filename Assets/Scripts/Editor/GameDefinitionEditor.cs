using UnityEditor;
using UnityEngine;

[CustomEditor(
    typeof(GameDefinition),
    true,
    isFallback = true
)]
public sealed class GameDefinitionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawIdentity((GameDefinition)target);
        DrawDefaultInspector();
    }

    public static void DrawIdentity(GameDefinition definition)
    {
        if (definition == null)
            return;

        EditorGUILayout.LabelField(
            "Identity",
            EditorStyles.boldLabel
        );

        EditorGUILayout.LabelField(
            "Name",
            definition.name
        );

        EditorGUILayout.LabelField(
            "ID",
            definition.Id
        );

        EditorGUILayout.Space();
    }
}
