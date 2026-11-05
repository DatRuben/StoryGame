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

//adding dynamic grip context
    public enum HeldItemGripContext
    {
        Default,
        DeployedWeapon
    }

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