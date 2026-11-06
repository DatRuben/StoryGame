using UnityEngine;

//adding dynamic grip context
public enum HeldItemGripContext
{
    Default,
    DeployedWeapon
}

//adding the fields so that the items can store the transform info based on context 
[System.Serializable]
public sealed class HeldItemGripSet
{
    [SerializeField]
    private HeldItemGripContext context;

    [SerializeField]
    private Transform primaryHandGrip;

    [SerializeField]
    private Transform secondaryHandGrip;

    [SerializeField]
    private Transform mouthGrip;

    [SerializeField]
    private HeldItemGripSet[] gripSets;

    public HeldItemGripContext Context =>
        context;

    public Transform PrimaryHandGrip =>
        primaryHandGrip;

    public Transform SecondaryHandGrip =>
        secondaryHandGrip;

    public Transform MouthGrip =>
        mouthGrip;
}
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