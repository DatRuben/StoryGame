using System.Collections.Generic;
using UnityEngine;

public sealed class EntityElementalState :
    MonoBehaviour
{
    private readonly Dictionary<
        CombatElement,
        ElementBuildupState>
        buildupStates =
            new Dictionary<
                CombatElement,
                ElementBuildupState>();

    public IReadOnlyDictionary<
        CombatElement,
        ElementBuildupState>
        BuildupStates =>
            buildupStates;

    public bool TryGetBuildupState(
        CombatElement element,
        out ElementBuildupState state)
    {
        state = null;

        if (element == CombatElement.None)
            return false;

        return buildupStates.TryGetValue(
            element,
            out state
        );
    }

    internal ElementBuildupState
        GetOrCreateBuildupState(
            CombatElement element)
    {
        if (element == CombatElement.None)
            return null;

        if (buildupStates.TryGetValue(
                element,
                out ElementBuildupState state))
        {
            return state;
        }

        state =
            new ElementBuildupState(
                element
            );

        buildupStates.Add(
            element,
            state
        );

        return state;
    }
}