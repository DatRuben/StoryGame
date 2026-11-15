using UnityEngine;

[DisallowMultipleComponent]
public sealed class HeldItemOrientationReference :
    MonoBehaviour
{
    [SerializeField]
    private Transform orientationReference;

    public Transform OrientationReference =>
        orientationReference;
}