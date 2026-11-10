
using System;
using UnityEngine;

public enum WeaponAttachmentSocket
{
    LeftHand,
    RightHand,
    Mouth
}

[Serializable]
public sealed class WeaponWieldSocketPose
{
    public HeldItemGripContext context;

    // Target pose for the item's grip marker,
    // relative to the selected socket.
    public Vector3 localPosition;
    public Vector3 localEulerAngles;
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

    [Header("Socket-Relative Grip Targets")]
    public WeaponWieldSocketPose[] gripTargets;
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
        WeaponAttachmentSocket socket,
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
                variant.assignedGripCount != assignedGripCount ||
                variant.primarySocket != socket)
            {
                continue;
            }

            // Ambiguous configurations are invalid.
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
