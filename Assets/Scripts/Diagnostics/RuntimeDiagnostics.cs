using System.Collections.Generic;
using UnityEngine;

public enum RuntimeDiagnosticStatus
{
    Ready,
    Blocked,
    Optional,
    Missing,
    Misbound,
    Invalid,
    Warning
}

public readonly struct RuntimeDiagnosticCheck
{
    public string Id { get; }

    public string Label { get; }

    public RuntimeDiagnosticStatus Status { get; }

    public string Details { get; }

    public RuntimeDiagnosticCheck(
        string id,
        string label,
        RuntimeDiagnosticStatus status,
        string details = "")
    {
        Id = id;
        Label = label;
        Status = status;
        Details = details;
    }
}

[DisallowMultipleComponent]
public sealed class RuntimeDiagnostics :
    MonoBehaviour
{
    private readonly List<
        RuntimeDiagnosticCheck>
        checks =
            new List<
                RuntimeDiagnosticCheck>();

    public IReadOnlyList<
        RuntimeDiagnosticCheck>
        Checks =>
            checks;

    public void Refresh()
    {
        checks.Clear();

        CheckElementBuildupSystem();
    }

    private void CheckElementBuildupSystem()
    {
        ElementBuildupSystem system =
            FindFirstObjectByType<
                ElementBuildupSystem>();

        if (system == null)
        {
            checks.Add(
                new RuntimeDiagnosticCheck(
                    "combat.element_buildup.system",
                    "Element Buildup System",
                    RuntimeDiagnosticStatus.Missing,
                    "No ElementBuildupSystem exists in the current scene."
                )
            );

            return;
        }

        checks.Add(
            new RuntimeDiagnosticCheck(
                "combat.element_buildup.system",
                "Element Buildup System",
                RuntimeDiagnosticStatus.Ready,
                system.name
            )
        );
    }
}