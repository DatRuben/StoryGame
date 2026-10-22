using UnityEngine;

public readonly struct ElementApplication
{
    public CombatElement Element { get; }

    public float Buildup { get; }

    public float Intensity { get; }

    public float Duration { get; }

    public CombatSourceContext Source { get; }

    public ElementApplication(
        CombatElement element,
        float buildup,
        float intensity,
        float duration,
        CombatSourceContext source)
    {
        Element = element;

        Buildup =
            Mathf.Max(
                0f,
                buildup
            );

        Intensity =
            Mathf.Max(
                0f,
                intensity
            );

        Duration =
            Mathf.Max(
                0,
                duration
            );

        Source = source;
    }
}