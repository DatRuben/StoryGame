using UnityEngine;

public sealed class HeldItemGripPoints :
    MonoBehaviour
{
    [Header("Grip Points")]

    [SerializeField]
    private Transform primaryHandGrip;

    [SerializeField]
    private Transform secondaryHandGrip;

    [SerializeField]
    private Transform mouthGrip;

    public Transform PrimaryHandGrip =>
        primaryHandGrip;

    public Transform SecondaryHandGrip =>
        secondaryHandGrip;

    public Transform MouthGrip =>
        mouthGrip;

    public Transform GetPrimaryGrip(
        GripType gripType)
    {
        switch (gripType)
        {
            case GripType.Hand:
                return primaryHandGrip;

            case GripType.Mouth:
                return mouthGrip;

            default:
                return null;
        }
    }
}