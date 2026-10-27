using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ElementBuildupSystem :
    MonoBehaviour
{
    [SerializeField]
    private CombatElementBuildupRules
        buildupRules;

    public bool HasBuildupRules =>
        buildupRules != null;

    public event Action<
        EntityElementalState,
        ElementBuildupCompletion>
        OnBuildupCompleted;

    public bool TryGetRule(
        CombatElement element,
        out CombatElementBuildupRule rule)
    {
        rule = null;

        if (buildupRules == null)
        {
            Debug.LogWarning(
                "ElementBuildupSystem is missing its buildup rules.",
                this
            );

            return false;
        }

        return buildupRules.TryGetRule(
            element,
            out rule
        );
    }
}