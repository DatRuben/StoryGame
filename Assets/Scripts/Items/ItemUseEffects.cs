using UnityEngine;

public abstract class ItemUseEffect :
    ScriptableObject
{
    public abstract bool TryApply(
        GameObject user
    );
}