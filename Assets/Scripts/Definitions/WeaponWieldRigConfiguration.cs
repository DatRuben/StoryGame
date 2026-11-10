
using System;
using UnityEngine;

public enum WeaponAttachmentSocket
{
    LeftHand,
    RightHand,
    Mouth
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
