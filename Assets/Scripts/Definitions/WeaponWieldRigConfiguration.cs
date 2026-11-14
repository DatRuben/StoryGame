
using System;
using UnityEngine;

public enum WeaponAttachmentSocket
{
    LeftHand,
    RightHand,
    Mouth
}






[Serializable]
public sealed class WeaponWieldItemFit
{
    public ItemDefinition weapon;
    public HeldItemGripContext context;

    [Header("Virtual Grip Marker")]

    [Tooltip(
        "Position of the grip reference relative " +
        "to the weapon prefab root, not the socket."
    )]
    public Vector3 localPosition;

    [Tooltip(
        "Rotation of the grip reference relative " +
        "to the weapon prefab root."
    )]
    public Vector3 localEulerAngles;

    [Tooltip(
        "Additional visual scale for this character. " +
        "One preserves the prefab's authored scale."
    )]
    public Vector3 localScaleMultiplier = Vector3.one;

    public Quaternion LocalRotation =>
        Quaternion.Euler(localEulerAngles);
}



[Serializable]
public sealed class WeaponWieldRigVariant
{
    [Header("Handling")]
    public WeaponWieldStyleDefinition style;

    public GripType gripType = GripType.Hand;

    [Min(1)]
    public int assignedGripCount = 2;

    public WeaponAttachmentSocket primarySocket =
        WeaponAttachmentSocket.RightHand;

    [Header("Animation")]
    public AnimatorOverrideController readyController;


    [Header("Weapon Fitting")]
    [SerializeField]
    private WeaponWieldItemFit[] itemFits;

    public bool TryGetItemFit(
        ItemDefinition weapon,
        HeldItemGripContext context,
        out WeaponWieldItemFit result)
    {
        result = null;

        if (weapon == null || itemFits == null)
            return false;

        foreach (WeaponWieldItemFit fit in itemFits)
        {
            if (fit == null ||
                fit.weapon != weapon ||
                fit.context != context)
            {
                continue;
            }

            // Reject duplicate matching entries.
            if (result != null)
            {
                result = null;
                return false;
            }

            result = fit;
        }

        return result != null;
    }

}

[CreateAssetMenu(
    fileName = "NewWeaponWieldRigConfiguration",
    menuName = "Game/Combat/Weapon Wield Rig Configuration"
)]
public sealed class WeaponWieldRigConfiguration :
    ScriptableObject
{
    [SerializeField]
    private WeaponWieldRigVariant[] variants;

    public bool TryGetVariant(
        WeaponWieldStyleDefinition style,
        GripType gripType,
        int assignedGripCount,
        WeaponAttachmentSocket? requestedSocket,
        out WeaponWieldRigVariant result)
    {
        result = null;

        if (style == null || variants == null)
            return false;

        foreach (WeaponWieldRigVariant variant in variants)
        {
            if (variant == null ||
                variant.style != style ||
                variant.gripType != gripType ||
                variant.assignedGripCount != assignedGripCount)
            {
                continue;
            }

            if (gripType == GripType.Hand &&
                variant.primarySocket ==
                    WeaponAttachmentSocket.Mouth)
            {
                continue;
            }

            if (gripType == GripType.Mouth &&
                variant.primarySocket !=
                    WeaponAttachmentSocket.Mouth)
            {
                continue;
            }

            if (requestedSocket.HasValue &&
                variant.primarySocket !=
                    requestedSocket.Value)
            {
                continue;
            }


            if (result != null)
            {
                result = null;
                return false;
            }

            result = variant;
        }

        return result != null;
    }
}
