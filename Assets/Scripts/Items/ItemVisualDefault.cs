using UnityEngine;

[CreateAssetMenu(
    menuName = "Game/Items/Item Visual Default"
)]
public sealed class ItemVisualDefault :
    ScriptableObject
{
    [Header("World Visual")]

    [SerializeField]
    private GameObject fallbackWorldPrefab;

    public GameObject FallbackWorldPrefab =>
        fallbackWorldPrefab;

    public GameObject ResolveWorldPrefab(
        ItemDefinition definition)
    {
        if (definition != null &&
            definition.worldPrefab != null)
        {
            return definition.worldPrefab;
        }

        return fallbackWorldPrefab;
    }
}