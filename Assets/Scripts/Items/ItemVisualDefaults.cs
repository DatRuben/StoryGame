using UnityEngine;

[CreateAssetMenu(
    menuName = "Game/Items/Item Visual Defaults"
)]
public sealed class ItemVisualDefaults :
    ScriptableObject
{
    [Header("World Visual")]

    [SerializeField]
    private GameObject fallbackWorldPrefab;

    public GameObject FallbackWorldPrefab =>
        fallbackWorldPrefab;
}