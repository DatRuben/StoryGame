using UnityEngine;

public class RaceProfileApplier : MonoBehaviour
{
    [SerializeField] private RaceProfile raceProfile;

    [Header("References")]
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private PlayerWeaponSlots playerWeaponSlots;
    [SerializeField] private PlayerEquipment playerEquipment;

    public RaceProfile CurrentRaceProfile => raceProfile;

    private void Awake()
    {
        ApplyRaceProfile();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
            ApplyRaceProfile();
    }

    public void ApplyRaceProfile()
    {
        if (raceProfile == null)
            return;

        if (capsuleCollider != null)
        {
            capsuleCollider.height = raceProfile.capsuleHeight;
            capsuleCollider.radius = raceProfile.capsuleRadius;
            capsuleCollider.center = raceProfile.capsuleCenter;
            capsuleCollider.direction = (int)raceProfile.capsuleDirection;
        }

        if (groundCheck != null)
        {
            groundCheck.localPosition = raceProfile.groundCheckLocalPosition;
        }

        if (cameraPivot != null)
        {
            cameraPivot.localPosition = raceProfile.cameraPivotLocalPosition;
        }

        if (playerWeaponSlots != null)
        {
            playerWeaponSlots.ApplyRaceProfile(raceProfile);
        }

        if (playerEquipment != null)
        {
            playerEquipment.ApplyRaceProfile(raceProfile);
        }
    }

    public void SetRaceProfile(RaceProfile newRaceProfile)
    {
        raceProfile = newRaceProfile;
        ApplyRaceProfile();
    }
}