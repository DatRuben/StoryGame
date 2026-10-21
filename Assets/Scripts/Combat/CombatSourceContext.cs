using UnityEngine;

public readonly struct CombatSourceContext
{
    public GameObject Owner { get; }

    public GameObject Emitter { get; }

    public InventoryItemInstance SourceItem { get; }

    public CombatSourceContext(
        GameObject owner,
        GameObject emitter,
        InventoryItemInstance sourceItem = null)
    {
        Owner = owner;
        Emitter =
            emitter != null
                ? emitter
                : owner;

        SourceItem = sourceItem;
    }
}