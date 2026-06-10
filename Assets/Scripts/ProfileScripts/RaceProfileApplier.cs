using UnityEngine;

public class RaceProfileApplier : MonoBehaviour
{
    [SerializeField] private RaceProfile raceProfile;

    [Header("References")]
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerHolding playerHolding;
    [SerializeField] private PlayerWeaponSlots playerWeaponSlots;

    private void Awake()
    {
        ApplyRaceProfile();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
            ApplyRaceProfile();
    }

    private void ApplyRaceProfile()
    {
        if (raceProfile == null)
            return;

        if (capsuleCollider == null)
            capsuleCollider = GetComponent<CapsuleCollider>();

        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        if (capsuleCollider != null)
        {
            capsuleCollider.height = raceProfile.capsuleHeight;
            capsuleCollider.radius = raceProfile.capsuleRadius;
            capsuleCollider.center = raceProfile.capsuleCenter;
            capsuleCollider.direction = (int)raceProfile.capsuleDirection;
        }

        if (groundCheck != null)
        {
            groundCheck.localPosition =
                raceProfile.groundCheckLocalPosition;
        }

        if (cameraPivot != null)
        {
            cameraPivot.localPosition =
                raceProfile.cameraPivotLocalPosition;
        }

        if (playerInput != null)
        {
            playerInput.ApplyRaceMovement(raceProfile);
        }

        if (playerHolding == null)
            playerHolding = GetComponent<PlayerHolding>();

        if (playerHolding != null)
            playerHolding.ApplyRaceProfile(raceProfile);

        if (playerWeaponSlots == null)
            playerWeaponSlots = GetComponent<PlayerWeaponSlots>();

        if (playerWeaponSlots != null)
            playerWeaponSlots.ApplyRaceProfile(raceProfile);
    }
}