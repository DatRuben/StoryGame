using UnityEngine;

[DisallowMultipleComponent]
public sealed class ItemVisualResolver :
    MonoBehaviour
{
    [SerializeField]
    private ItemVisualDefaults visualDefaults;

    public GameObject ResolveWorldPrefab(
        ItemDefinition definition)
    {
        if (definition != null &&
            definition.worldPrefab != null)
        {
            return definition.worldPrefab;
        }

        return visualDefaults != null
            ? visualDefaults.FallbackWorldPrefab
            : null;
    }
}