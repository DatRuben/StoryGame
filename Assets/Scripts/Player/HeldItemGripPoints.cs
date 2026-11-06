using UnityEngine;

//adding dynamic grip context
public enum HeldItemGripContext
{
    Held,
    Ready
}

//adding the fields so that the items can store the transform info based on context 
[System.Serializable]
public sealed class HeldItemGripSet
{
    [SerializeField]
    private HeldItemGripContext context;

    [SerializeField]
    private Transform gripPoint;

    public HeldItemGripContext Context =>
        context;

    public Transform GripPoint =>
        gripPoint;
}


public sealed class HeldItemGripPoints :
    MonoBehaviour
{
    [Header("Grip Points")]

    [Header("Context Grip Points")]

    [SerializeField]
    private HeldItemGripSet[] gripSets;

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

    public bool GetGripPoint(
    HeldItemGripContext context,
    out Transform gripPoint)
    {
        gripPoint = null;

        if (gripSets == null)
            return false;

        foreach (HeldItemGripSet set in gripSets)
        {
            if (set == null ||
                set.Context != context)
            {
                continue;
            }

            gripPoint = set.GripPoint;

            return gripPoint != null;
        }

        return false;
    }
}