using System;
using UnityEngine;

[Serializable]
public sealed class CombatElementBuildupRule
{
    [SerializeField]
    private CombatElement element =
        CombatElement.Fire;

    [SerializeField]
    [Min(0.01f)]
    private float baseThreshold = 100f;

    [SerializeField]
    [Min(0f)]
    private float thresholdIncreasePerCompletion =
        25f;

    [SerializeField]
    [Min(0.01f)]
    private float maximumThreshold = 200f;

    [SerializeField]
    [Min(0f)]
    private float minimumProcInterval = 10f;

    public float MinimumProcInterval =>
        Mathf.Max(
            0f,
            minimumProcInterval
        );

    public CombatElement Element =>
        element;

    public float GetThreshold(
        int completedCycles)
    {
        float safeBase =
            Mathf.Max(
                0.01f,
                baseThreshold
            );

        float safeIncrease =
            Mathf.Max(
                0f,
                thresholdIncreasePerCompletion
            );

        float safeMaximum =
            Mathf.Max(
                safeBase,
                maximumThreshold
            );

        int safeCycles =
            Mathf.Max(
                0,
                completedCycles
            );

        return Mathf.Min(
            safeBase +
            safeIncrease * safeCycles,
            safeMaximum
        );
    }
}

[CreateAssetMenu(
    menuName =
        "Game/Combat/Element Buildup Rules"
)]
public sealed class CombatElementBuildupRules :
    ScriptableObject
{
    [SerializeField]
    private CombatElementBuildupRule[] rules =
        Array.Empty<
            CombatElementBuildupRule>();

    public bool TryGetThreshold(
        CombatElement element,
        int completedCycles,
        out float threshold)
    {
        threshold = 0f;

        if (element == CombatElement.None)
            return false;

        for (int i = 0;
             i < rules.Length;
             i++)
        {
            CombatElementBuildupRule rule =
                rules[i];

            if (rule == null ||
                rule.Element != element)
            {
                continue;
            }

            threshold =
                rule.GetThreshold(
                    completedCycles
                );

            return true;
        }

        return false;
    }
}