using UnityEngine;

public sealed class ItemActionPoints :
    MonoBehaviour
{
    [Header("Action Points")]

    [SerializeField]
    private Transform castPoint;

    public Transform CastPoint =>
        castPoint;
}