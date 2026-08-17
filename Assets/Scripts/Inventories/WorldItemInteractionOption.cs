public enum WorldItemInteractionAction
{
    Store,
    Hold,
    Backpack
}

public readonly struct WorldItemInteractionOption
{
    public WorldItemInteractionAction Action { get; }
    public string Label { get; }
    public bool IsAvailable { get; }
    public string DisabledReason { get; }

    public WorldItemInteractionOption(
        WorldItemInteractionAction action,
        string label,
        bool isAvailable,
        string disabledReason = "")
    {
        Action = action;
        Label = label;
        IsAvailable = isAvailable;
        DisabledReason = disabledReason;
    }
}