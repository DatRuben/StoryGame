using UnityEngine;

public sealed class HeldItemAnchors :
    MonoBehaviour
{
    [SerializeField]
    private Transform leftHand;

    [SerializeField]
    private Transform rightHand;

    [SerializeField]
    private Transform mouth;

    public Transform LeftHand =>
        leftHand;

    public Transform RightHand =>
        rightHand;

    public Transform Mouth =>
        mouth;
}