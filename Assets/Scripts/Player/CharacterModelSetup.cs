using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(HeldItemAnchors))]
public sealed class CharacterModelSetup :
    MonoBehaviour
{
    public Animator AnimatorComponent =>
        GetComponent<Animator>();

    public HeldItemAnchors HeldItemAnchors =>
        GetComponent<HeldItemAnchors>();
}