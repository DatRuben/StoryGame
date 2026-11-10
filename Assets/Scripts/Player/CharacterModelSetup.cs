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


}