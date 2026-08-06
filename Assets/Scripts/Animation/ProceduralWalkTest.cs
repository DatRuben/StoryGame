using UnityEngine;

public class ProceduralWalkTest : MonoBehaviour
{
    [Header("Arm Rest Pose Fix")]
    public float armDownAngle = 25f;
    public Vector3 leftArmDownAxis = Vector3.forward;
    public Vector3 rightArmDownAxis = Vector3.back;

    [Header("Movement")]
    public bool moveForward = false;
    public Transform moveRoot;
    public float moveSpeed = 1.5f;

    [Header("Movement Detection")]
    public Rigidbody movementBody;

    [Header("Movement Speed Matching")]

    [Min(0.01f)]
    public float normalMovementSpeed = 8f;

    [Min(0.01f)]
    public float minimumCycleSpeed = 0.65f;

    [Min(0.01f)]
    public float maximumCycleSpeed = 1.6f;

    [Tooltip("Allows the animation to run without a Rigidbody, such as in ModelTest.")]
    public bool previewWalking = false;

    [Min(0f)]
    public float minimumMovementSpeed = 0.1f;

    [Min(0.01f)]
    public float movementBlendSpeed = 8f;

    [Header("Body Bob")]
    public Transform visualRoot;
    public float bobHeight = 0.02f;

    [Header("Upper Body Bones")]
    public Transform leftUpperArm;
    public Transform rightUpperArm;
    public Transform leftLowerArm;
    public Transform rightLowerArm;
    public Transform spine;

    [Header("Leg Bones")]
    public Transform leftUpperLeg;
    public Transform rightUpperLeg;
    public Transform leftLowerLeg;
    public Transform rightLowerLeg;

    [Header("Swing")]
    public float walkSpeed = 3f;
    public float animationSpeedMultiplier = 1f;
    public float legSwingAngle = 20f;
    public float kneeBendAngle = 35f;
    public float armSwingAngle = 15f;
    public float spineTwistAngle = 0f;

    [Header("Separate Leg Axes")]
    public Vector3 leftLegSwingAxis = Vector3.right;
    public Vector3 rightLegSwingAxis = Vector3.right;

    [Header("Axes")]
    public Vector3 legSwingAxis = Vector3.right;
    public Vector3 kneeBendAxis = Vector3.right;
    public Vector3 armSwingAxis = Vector3.right;
    public Vector3 spineTwistAxis = Vector3.up;

    [Header("Arm Swing Axes")]
    public Vector3 leftArmSwingAxis = Vector3.right;
    public Vector3 rightArmSwingAxis = Vector3.right;

    [Header("Elbows")]
    public float elbowBendAngle = 15f;
    public Vector3 leftElbowAxis = Vector3.right;
    public Vector3 rightElbowAxis = Vector3.right;

    private Quaternion leftUpperLegStart;
    private Quaternion rightUpperLegStart;
    private Quaternion leftLowerLegStart;
    private Quaternion rightLowerLegStart;

    private Quaternion leftArmStart;
    private Quaternion rightArmStart;
    private Quaternion leftLowerArmStart;
    private Quaternion rightLowerArmStart;

    private Quaternion spineStart;
    private Vector3 visualRootStart;

    private float movementBlend;
    private float walkTime;

    private float currentMovementSpeed;

    private void Start()
    {
        if (movementBody == null)
        {
            movementBody =
                GetComponentInParent<Rigidbody>();
        }

        if (leftUpperLeg != null)
            leftUpperLegStart = leftUpperLeg.localRotation;

        if (rightUpperLeg != null)
            rightUpperLegStart = rightUpperLeg.localRotation;

        if (leftLowerLeg != null)
            leftLowerLegStart = leftLowerLeg.localRotation;

        if (rightLowerLeg != null)
            rightLowerLegStart = rightLowerLeg.localRotation;

        if (leftUpperArm != null)
            leftArmStart = leftUpperArm.localRotation;

        if (rightUpperArm != null)
            rightArmStart = rightUpperArm.localRotation;

        if (leftLowerArm != null)
            leftLowerArmStart = leftLowerArm.localRotation;

        if (rightLowerArm != null)
            rightLowerArmStart = rightLowerArm.localRotation;

        if (spine != null)
            spineStart = spine.localRotation;

        if (visualRoot != null)
            visualRootStart = visualRoot.localPosition;
    }

    private void Update()
    {
        bool isMoving =
            previewWalking ||
            moveForward ||
            IsMoving();

        float targetBlend =
            isMoving ? 1f : 0f;

        movementBlend =
            Mathf.MoveTowards(
                movementBlend,
                targetBlend,
                movementBlendSpeed *
                Time.deltaTime
            );

        if (movementBlend > 0.001f)
        {
            float movementSpeedRatio =
                normalMovementSpeed > 0f
                    ? currentMovementSpeed /
                      normalMovementSpeed
                    : 1f;

            float cycleSpeed =
                Mathf.Clamp(
                    movementSpeedRatio,
                    minimumCycleSpeed,
                    maximumCycleSpeed
                );

            if (previewWalking || moveForward)
                cycleSpeed = 1f;

            walkTime +=
                Time.deltaTime *
                walkSpeed *
                animationSpeedMultiplier *
                cycleSpeed;
        }

        if (moveForward)
        {
            Transform root =
                moveRoot != null
                    ? moveRoot
                    : transform;

            root.position +=
                root.forward *
                moveSpeed *
                Time.deltaTime;
        }

        ApplyWalkPose();
    }

    private bool IsMoving()
    {
        if (movementBody == null)
        {
            currentMovementSpeed = 0f;
            return false;
        }

        Vector3 horizontalVelocity =
            movementBody.linearVelocity;

        horizontalVelocity.y = 0f;

        currentMovementSpeed =
            horizontalVelocity.magnitude;

        return currentMovementSpeed >
               minimumMovementSpeed;
    }

    private void ApplyWalkPose()
    {
        float leftSwing =
            Mathf.Sin(walkTime);

        float rightSwing =
            Mathf.Sin(
                walkTime +
                Mathf.PI
            );

        const float kneeTimingOffset = 0.6f;

        float leftKneeBend =
            Mathf.Clamp01(
                Mathf.Sin(
                    walkTime +
                    kneeTimingOffset
                )
            ) *
            kneeBendAngle *
            movementBlend;

        float rightKneeBend =
            Mathf.Clamp01(
                Mathf.Sin(
                    walkTime +
                    Mathf.PI +
                    kneeTimingOffset
                )
            ) *
            kneeBendAngle *
            movementBlend;

        float leftElbowBend =
            Mathf.Clamp01(
                Mathf.Sin(
                    walkTime +
                    Mathf.PI +
                    0.4f
                )
            ) *
            elbowBendAngle *
            movementBlend;

        float rightElbowBend =
            Mathf.Clamp01(
                Mathf.Sin(
                    walkTime +
                    0.4f
                )
            ) *
            elbowBendAngle *
            movementBlend;

        float bob =
            Mathf.Abs(
                Mathf.Sin(walkTime)
            ) *
            bobHeight *
            movementBlend;

        if (visualRoot != null)
        {
            visualRoot.localPosition =
                visualRootStart +
                Vector3.up * bob;
        }

        if (leftUpperLeg != null)
        {
            leftUpperLeg.localRotation =
                Quaternion.AngleAxis(
                    leftSwing *
                    legSwingAngle *
                    movementBlend,
                    leftLegSwingAxis
                ) *
                leftUpperLegStart;
        }

        if (rightUpperLeg != null)
        {
            rightUpperLeg.localRotation =
                Quaternion.AngleAxis(
                    rightSwing *
                    legSwingAngle *
                    movementBlend,
                    rightLegSwingAxis
                ) *
                rightUpperLegStart;
        }

        if (leftLowerLeg != null)
        {
            leftLowerLeg.localRotation =
                Quaternion.AngleAxis(
                    leftKneeBend,
                    kneeBendAxis
                ) *
                leftLowerLegStart;
        }

        if (rightLowerLeg != null)
        {
            rightLowerLeg.localRotation =
                Quaternion.AngleAxis(
                    rightKneeBend,
                    kneeBendAxis
                ) *
                rightLowerLegStart;
        }

        if (leftUpperArm != null)
        {
            Quaternion armDown =
                Quaternion.AngleAxis(
                    armDownAngle,
                    leftArmDownAxis
                );

            Quaternion armSwing =
                Quaternion.AngleAxis(
                    rightSwing *
                    armSwingAngle *
                    movementBlend,
                    leftArmSwingAxis
                );

            leftUpperArm.localRotation =
                armSwing *
                armDown *
                leftArmStart;
        }

        if (rightUpperArm != null)
        {
            Quaternion armDown =
                Quaternion.AngleAxis(
                    armDownAngle,
                    rightArmDownAxis
                );

            Quaternion armSwing =
                Quaternion.AngleAxis(
                    -leftSwing *
                    armSwingAngle *
                    movementBlend,
                    rightArmSwingAxis
                );

            rightUpperArm.localRotation =
                armSwing *
                armDown *
                rightArmStart;
        }

        if (leftLowerArm != null)
        {
            leftLowerArm.localRotation =
                Quaternion.AngleAxis(
                    leftElbowBend,
                    leftElbowAxis
                ) *
                leftLowerArmStart;
        }

        if (rightLowerArm != null)
        {
            rightLowerArm.localRotation =
                Quaternion.AngleAxis(
                    rightElbowBend,
                    rightElbowAxis
                ) *
                rightLowerArmStart;
        }

        if (spine != null)
        {
            spine.localRotation =
                spineStart *
                Quaternion.AngleAxis(
                    leftSwing *
                    spineTwistAngle *
                    movementBlend,
                    spineTwistAxis
                );
        }
    }
}