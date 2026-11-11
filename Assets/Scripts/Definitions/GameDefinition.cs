using UnityEngine;

public abstract class GameDefinition : ScriptableObject
{
    public string Id =>
        DefinitionIdUtility.FromName(name);
}
