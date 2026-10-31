
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class StoryGameUIAudit
{
    [MenuItem("Tools/StoryGame/Export UI Hierarchy")]
    private static void Export()
    {
        var report = new StringBuilder();
        var visited = new HashSet<int>();

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            if (!scene.isLoaded)
                continue;

            report.AppendLine("SCENE: " + scene.path);

            GameObject[] roots = scene.GetRootGameObjects();

            foreach (GameObject root in roots)
            {
                foreach (Canvas canvas in
                    root.GetComponentsInChildren<Canvas>(true))
                {
                    if (HasCanvasAncestor(canvas.transform))
                        continue;

                    WriteObject(
                        canvas.transform, 0, true,
                        report, visited);
                }
            }

            report.AppendLine("SCENE CONTROLLERS:");

            foreach (GameObject root in roots)
            {
                foreach (MonoBehaviour component in
                    root.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (component == null)
                        continue;

                    if (component is Menus ||
                        component is CharacterRuntimeBinder ||
                        component is CursorController ||
                        component is PlayerSpawner ||
                        component is PauseMenuController ||
                        component is InventoryMenuController ||
                        component is RuntimeDiagnosticsUI)
                    {
                        WriteObject(
                            component.transform, 0, false,
                            report, visited);
                    }
                }
            }
        }

        string path = EditorUtility.SaveFilePanel(
            "Save UI hierarchy audit",
            Path.GetDirectoryName(Application.dataPath),
            "StoryGameUIHierarchy",
            "txt");

        if (string.IsNullOrEmpty(path))
            return;

        File.WriteAllText(path, report.ToString());

        Debug.Log("UI hierarchy exported to: " + path);
    }

    private static bool HasCanvasAncestor(Transform node)
    {
        Transform parent = node.parent;

        while (parent != null)
        {
            if (parent.GetComponent<Canvas>() != null)
                return true;

            parent = parent.parent;
        }

        return false;
    }

    private static void WriteObject(
        Transform node,
        int depth,
        bool recursive,
        StringBuilder report,
        HashSet<int> visited)
    {
        GameObject gameObject = node.gameObject;

        if (!visited.Add(gameObject.GetInstanceID()))
            return;

        string indent = new string(' ', depth * 2);

        report.AppendLine(
            indent + GetPath(node) +
            " | Active=" + gameObject.activeSelf);

        if (node is RectTransform rect)
        {
            report.AppendLine(
                indent +
                "  Anchors: " + rect.anchorMin +
                " -> " + rect.anchorMax +
                " | Pivot: " + rect.pivot +
                " | Position: " + rect.anchoredPosition +
                " | SizeDelta: " + rect.sizeDelta +
                " | Offsets: " + rect.offsetMin +
                " / " + rect.offsetMax);
        }

        foreach (Component component in
            gameObject.GetComponents<Component>())
        {
            if (component == null)
            {
                report.AppendLine(indent + "  MISSING SCRIPT");
                continue;
            }

            report.AppendLine(
                indent + "  Component: " +
                component.GetType().Name);

            if (component is CanvasGroup group)
            {
                report.AppendLine(
                    indent + "    Alpha=" + group.alpha +
                    " Interactable=" + group.interactable +
                    " BlocksRaycasts=" + group.blocksRaycasts);
            }

            if (component is CanvasScaler scaler)
            {
                report.AppendLine(
                    indent + "    ScaleMode=" + scaler.uiScaleMode +
                    " ReferenceResolution=" +
                    scaler.referenceResolution +
                    " Match=" + scaler.matchWidthOrHeight);
            }

            if (component is MonoBehaviour behaviour)
                WriteReferences(behaviour, indent, report);
        }

        if (!recursive)
            return;

        foreach (Transform child in node)
        {
            WriteObject(
                child, depth + 1, true,
                report, visited);
        }
    }

    private static void WriteReferences(
        MonoBehaviour behaviour,
        string indent,
        StringBuilder report)
    {
        var serialized = new SerializedObject(behaviour);
        SerializedProperty property = serialized.GetIterator();
        int reported = 0;

        while (property.NextVisible(true) && reported < 60)
        {
            if (property.propertyType !=
                SerializedPropertyType.ObjectReference ||
                property.name == "m_Script")
            {
                continue;
            }

            Object target = property.objectReferenceValue;

            if (target == null)
            {
                if (property.objectReferenceInstanceIDValue != 0)
                {
                    report.AppendLine(
                        indent + "    MISSING REFERENCE: " +
                        property.propertyPath);
                    reported++;
                }

                continue;
            }

            string destination = target.name;

            if (target is Component targetComponent &&
                targetComponent.gameObject.scene.IsValid())
            {
                destination =
                    GetPath(targetComponent.transform);
            }
            else if (target is GameObject targetObject &&
                     targetObject.scene.IsValid())
            {
                destination =
                    GetPath(targetObject.transform);
            }

            report.AppendLine(
                indent + "    " + property.propertyPath +
                " -> " + destination +
                " (" + target.GetType().Name + ")");

            reported++;
        }
    }

    private static string GetPath(Transform node)
    {
        string path = node.name;

        while (node.parent != null)
        {
            node = node.parent;
            path = node.name + "/" + path;
        }

        return path;
    }
}
