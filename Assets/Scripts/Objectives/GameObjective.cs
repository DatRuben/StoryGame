using System;
using UnityEngine;

public abstract class GameObjective :
    MonoBehaviour
{
    [SerializeField]
    private string displayText;

    public string DisplayText =>
        string.IsNullOrWhiteSpace(displayText)
            ? name
            : displayText;

    public bool IsComplete
    {
        get;
        private set;
    }

    public event Action<GameObjective>
        Completed;

    protected void Complete()
    {
        if (IsComplete)
            return;

        IsComplete = true;

        Completed?.Invoke(this);
    }
}