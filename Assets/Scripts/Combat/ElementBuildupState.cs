using System.Collections.Generic;
using UnityEngine;

public sealed class ElementBuildupState
{
    public CombatElement Element { get; }

    public float CurrentBuildup { get; private set; }

    public int CompletedCycles { get; private set; }

    private readonly Dictionary<
        GameObject,
        float>
        contributorBuildup =
            new Dictionary<
                GameObject,
                float>();

    public IReadOnlyDictionary<
        GameObject,
        float>
        ContributorBuildup =>
            contributorBuildup;

    public ElementBuildupState(
        CombatElement element)
    {
        Element = element;
    }
}