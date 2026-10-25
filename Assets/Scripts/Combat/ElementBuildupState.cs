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

    internal void AddBuildup(
        float amount,
        GameObject contributor)
    {
        if (amount <= 0f)
            return;

        CurrentBuildup += amount;

        if (contributor == null)
            return;

        if (contributorBuildup.TryGetValue(
                contributor,
                out float existingAmount))
        {
            contributorBuildup[contributor] =
                existingAmount + amount;

            return;
        }

        contributorBuildup.Add(
            contributor,
            amount
        );
    }

    internal void BeginNextCycle(
        float overflowBuildup,
        IReadOnlyDictionary<
            GameObject,
            float> overflowContributors)
    {
        CompletedCycles++;

        CurrentBuildup =
            Mathf.Max(
                0f,
                overflowBuildup
            );

        contributorBuildup.Clear();

        if (overflowContributors == null)
            return;

        foreach (KeyValuePair<
                     GameObject,
                     float> contributor
                 in overflowContributors)
        {
            if (contributor.Key == null ||
                contributor.Value <= 0f)
            {
                continue;
            }

            contributorBuildup.Add(
                contributor.Key,
                contributor.Value
            );
        }
    }
}