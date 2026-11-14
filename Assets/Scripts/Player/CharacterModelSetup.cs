using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(HeldItemAnchors))]
[RequireComponent(typeof(CharacterActionPoints))]


[System.Serializable]
public sealed class WieldStyleAnimationBinding
{
    public WeaponWieldStyleDefinition style;
    public AnimatorOverrideController controller;
}



//holding out anim
[System.Serializable]
public sealed class HeldItemPresentationBinding
{
    [Header("Grip Arrangement")]
    public GripType gripType = GripType.Hand;

    [Min(1)]
    public int assignedGripCount = 1;

    [Tooltip(
        "For one-hand grips: 0 = left, 1 = right. " +
        "Use -1 for other arrangements."
    )]
    public int singleHandIndex = -1;

    [Header("Presentation")]
    public Transform socket;
    public AnimatorOverrideController controller;

    public bool Matches(
        GripType type,
        int count,
        int handIndex)
    {
        if (gripType != type ||
            assignedGripCount != count)
        {
            return false;
        }

        if (type == GripType.Hand && count == 1)
            return singleHandIndex == handIndex;

        return true;
    }
}

//wielding model setup

public sealed class CharacterModelSetup :
    MonoBehaviour
{
    public Animator AnimatorComponent =>
        GetComponent<Animator>();

    public HeldItemAnchors HeldItemAnchors =>
        GetComponent<HeldItemAnchors>();

    public CharacterActionPoints ActionPoints =>
        GetComponent<CharacterActionPoints>();

    //wield animation section

    [Header("Wield Style Animations")]
    [SerializeField]
    private WieldStyleAnimationBinding[] wieldAnimations;

    public bool TryGetWieldController(
        WeaponWieldStyleDefinition style,
        out AnimatorOverrideController controller)
    {
        controller = null;

        if (style == null || wieldAnimations == null)
            return false;

        foreach (WieldStyleAnimationBinding binding
                 in wieldAnimations)
        {
            if (binding == null ||
                binding.style != style ||
                binding.controller == null)
            {
                continue;
            }

            controller = binding.controller;
            return true;
        }

        return false;
    }


    [Header("Shared Rig Wield Configuration")]
    [SerializeField]
    private WeaponWieldRigConfiguration wieldConfiguration;

    public WeaponWieldRigConfiguration WieldConfiguration =>
        wieldConfiguration;


    [Header("Holding-Out Presentation")]
    [SerializeField]
    private HeldItemPresentationBinding[] heldItemPresentations;

    public bool TryGetHeldItemPresentation(
        GripType gripType,
        int gripCount,
        int singleHandIndex,
        out HeldItemPresentationBinding result)
    {
        result = null;

        if (heldItemPresentations == null)
            return false;

        foreach (HeldItemPresentationBinding binding
                 in heldItemPresentations)
        {
            if (binding == null ||
                !binding.Matches(
                    gripType,
                    gripCount,
                    singleHandIndex))
            {
                continue;
            }

            // Reject ambiguous configurations.
            if (result != null)
            {
                result = null;
                return false;
            }

            result = binding;
        }

        return result != null && result.socket != null;
    }

}