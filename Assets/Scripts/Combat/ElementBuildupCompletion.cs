using System.Collections.Generic;
using UnityEngine;

public sealed class ElementBuildupCompletion
{
    public CombatElement Element { get; }

    public int CompletionNumber { get; }

    public float Threshold { get; }

    private readonly Dictionary<
        GameObject,
        float>
        contributorBuildup;

    public IReadOnlyDictionary<
        GameObject,
        float>
        ContributorBuildup =>
            contributorBuildup;

    internal ElementBuildupCompletion(
        CombatElement element,
        int completionNumber,
        float threshold,
        IReadOnlyDictionary<
            GameObject,
            float> contributors)
    {
        Element = element;

        CompletionNumber =
            Mathf.Max(
                1,
                completionNumber
            );

        Threshold =
            Mathf.Max(
                0f,
                threshold
            );

        contributorBuildup =
            new Dictionary<
                GameObject,
                float>();

        if (contributors == null)
            return;

        foreach (KeyValuePair<
                     GameObject,
                     float> contributor
                 in contributors)
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