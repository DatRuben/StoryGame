using System;

[Serializable]
public class ActiveStatusEffect
{
    public StatusEffectDefinition definition;

    public int stacks = 1;

    public float remainingDuration;

    public bool IsTimed =>
        definition != null &&
        definition.duration > 0f;

    public bool IsExpired =>
        IsTimed &&
        remainingDuration <= 0f;

    public ActiveStatusEffect(
        StatusEffectDefinition newDefinition)
    {
        definition = newDefinition;
        stacks = 1;

        remainingDuration =
            newDefinition != null
                ? newDefinition.duration
                : 0f;
    }

    public void RefreshDuration()
    {
        if (definition == null)
            return;

        remainingDuration =
            definition.duration;
    }

    public void AddStack()
    {
        if (definition == null)
            return;

        stacks =
            Math.Min(
                stacks + 1,
                Math.Max(
                    1,
                    definition.maxStacks
                )
            );
    }

    public void Tick(
        float deltaTime)
    {
        if (!IsTimed)
            return;

        remainingDuration -=
            Math.Max(
                0f,
                deltaTime
            );
    }
}