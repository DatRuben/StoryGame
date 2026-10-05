using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(HeldItemAnchors))]
[RequireComponent(typeof(CharacterActionPoints))]
public sealed class CharacterModelSetup :
    MonoBehaviour
{
    public Animator AnimatorComponent =>
        GetComponent<Animator>();

    public HeldItemAnchors HeldItemAnchors =>
        GetComponent<HeldItemAnchors>();

    public CharacterActionPoints ActionPoints =>
        GetComponent<CharacterActionPoints>();
}