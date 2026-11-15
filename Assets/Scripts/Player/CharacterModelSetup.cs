using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(HeldItemAnchors))]
[RequireComponent(typeof(CharacterActionPoints))]


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

    [Header("Held Item Position")]
    [Tooltip("Position offset relative to the selected socket.")]
    public Vector3 localPositionOffset = Vector3.zero;

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