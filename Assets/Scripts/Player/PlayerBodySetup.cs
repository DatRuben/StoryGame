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

    public void ApplyBody(
        SubraceDefinition subraceDefinition,
        FinalCharacterStats finalStats)
    {
        if (subraceDefinition == null)
        {
            Debug.LogWarning(
                "PlayerBodySetup could not apply body because SubraceDefinition is missing.",
                this
            );

            return;
        }

        ApplyCapsule(subraceDefinition);
        ApplyGroundCheck(subraceDefinition);
        ApplyCameraPivot(subraceDefinition);
        ApplyRigidbody(finalStats);
    }

    private void ApplyCapsule(
        SubraceDefinition subraceDefinition)
    {
        if (capsuleCollider == null)
        {
            Debug.LogWarning(
                "PlayerBodySetup could not apply capsule settings because CapsuleCollider is missing.",
                this
            );

            return;
        }

        capsuleCollider.direction = 1;
        capsuleCollider.radius = GetCapsuleRadius(subraceDefinition);
        capsuleCollider.height = GetCapsuleHeight(subraceDefinition);
        capsuleCollider.center = new Vector3(
            0f,
            capsuleCollider.height * 0.5f,
            0f
        );
    }

    private float GetCapsuleRadius(
        SubraceDefinition subraceDefinition)
    {
        switch (subraceDefinition.size)
        {
            case RaceSize.Size1:
            case RaceSize.Size1Feral:
                return 0.35f;

            case RaceSize.Size3:
            case RaceSize.Size3Feral:
                return 0.75f;

            case RaceSize.Dragon:
                return 0.9f;

            case RaceSize.BigDragon:
                return 1.15f;

            default:
                return 0.5f;
        }
    }

    private float GetCapsuleHeight(
        SubraceDefinition subraceDefinition)
    {
        switch (subraceDefinition.size)
        {
            case RaceSize.Size1:
                return 1.4f;

            case RaceSize.Size1Feral:
                return 1.0f;

            case RaceSize.TallerSize2:
                return 2.3f;

            case RaceSize.Size3:
                return 2.7f;

            case RaceSize.Size2Feral:
                return 1.3f;

            case RaceSize.Size3Feral:
                return 1.7f;

            case RaceSize.Dragon:
                return 1.9f;

            case RaceSize.BigDragon:
                return 2.3f;

            default:
                return 2f;
        }
    }

    private void ApplyGroundCheck(
        SubraceDefinition subraceDefinition)
    {
        if (groundCheck == null)
        {
            Debug.LogWarning(
                "PlayerBodySetup could not apply ground check position because GroundCheck is missing.",
                this
            );

            return;
        }

        groundCheck.localPosition =
            new Vector3(
                0f,
                -0.05f,
                0f
            );
    }

    private void ApplyCameraPivot(
        SubraceDefinition subraceDefinition)
    {
        if (cameraPivot == null)
        {
            Debug.LogWarning(
                "PlayerBodySetup could not apply camera pivot position because CameraPivot is missing.",
                this
            );

            return;
        }

        cameraPivot.localPosition =
            new Vector3(
                0f,
                GetCameraPivotHeight(subraceDefinition),
                0f
            );
    }

    private float GetCameraPivotHeight(
        SubraceDefinition subraceDefinition)
    {
        switch (subraceDefinition.size)
        {
            case RaceSize.Size1:
                return 0.7f;

            case RaceSize.Size1Feral:
                return 0.55f;

            case RaceSize.TallerSize2:
                return 1.1f;

            case RaceSize.Size3:
                return 1.35f;

            case RaceSize.Size2Feral:
                return 0.7f;

            case RaceSize.Size3Feral:
                return 0.95f;

            case RaceSize.Dragon:
                return 1.05f;

            case RaceSize.BigDragon:
                return 1.25f;

            default:
                return 0.9f;
        }
    }

    private void ApplyRigidbody(
        FinalCharacterStats finalStats)
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