using UnityEngine;

public class PlayerBodySetup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform cameraPivot;

    private void Awake()
    {
        if (capsuleCollider == null)
            capsuleCollider = GetComponent<CapsuleCollider>();

        if (groundCheck == null)
            groundCheck = FindChildRecursive(transform, "GroundCheck");

        if (cameraPivot == null)
            cameraPivot = FindChildRecursive(transform, "CameraPivot");
    }

    public void ApplyRaceBody(
        RaceProfile raceProfile,
        FinalCharacterStats finalStats)
    {
        if (raceProfile == null)
        {
            Debug.LogWarning(
                "PlayerBodySetup could not apply race body because RaceProfile is missing.",
                this
            );

            return;
        }

        ApplyCapsule(raceProfile);
        ApplyGroundCheck(raceProfile);
        ApplyCameraPivot(raceProfile);
        ApplyRigidbody(finalStats);
    }

    private void ApplyCapsule(RaceProfile raceProfile)
    {
        if (capsuleCollider == null)
        {
            Debug.LogWarning(
                "PlayerBodySetup could not apply capsule settings because CapsuleCollider is missing.",
                this
            );

            return;
        }

        capsuleCollider.radius = raceProfile.capsuleRadius;
        capsuleCollider.height = raceProfile.capsuleHeight;
        capsuleCollider.center = raceProfile.capsuleCenter;
        capsuleCollider.direction = (int)raceProfile.capsuleDirection;
    }

    private void ApplyRigidbody(FinalCharacterStats finalStats)
    {
        if (finalStats == null)
        {
            Debug.LogWarning(
                "PlayerBodySetup could not apply Rigidbody settings because FinalCharacterStats is missing.",
                this
            );

            return;
        }

        Rigidbody rb =
            GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogWarning(
                "PlayerBodySetup could not apply mass because Rigidbody is missing.",
                this
            );

            return;
        }

        rb.mass = Mathf.Max(0.01f, finalStats.mass);
    }

    private void ApplyGroundCheck(RaceProfile raceProfile)
    {
        if (groundCheck == null)
        {
            Debug.LogWarning(
                "PlayerBodySetup could not apply ground check position because GroundCheck is missing.",
                this
            );

            return;
        }

        groundCheck.localPosition = raceProfile.groundCheckLocalPosition;
    }

    private void ApplyCameraPivot(RaceProfile raceProfile)
    {
        if (cameraPivot == null)
        {
            Debug.LogWarning(
                "PlayerBodySetup could not apply camera pivot position because CameraPivot is missing.",
                this
            );

            return;
        }

        cameraPivot.localPosition = raceProfile.cameraPivotLocalPosition;
    }

    private Transform FindChildRecursive(
        Transform parent,
        string childName)
    {
        if (parent == null)
            return null;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            if (child.name == childName)
                return child;

            Transform found =
                FindChildRecursive(child, childName);

            if (found != null)
                return found;
        }

        return null;
    }
}