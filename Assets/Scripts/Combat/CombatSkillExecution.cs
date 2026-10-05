public abstract class CombatSkillExecution
{
    public bool IsComplete { get; protected set; }

    public abstract bool TryBegin(
        out string error);

    public abstract void Tick(
        float deltaTime);

    public virtual void Cancel()
    {
        IsComplete = true;
    }
}