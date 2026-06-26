using UnityEngine;

public class ProceduralWalkTest : MonoBehaviour
{
    [Header("Arm Rest Pose Fix")]
    public float armDownAngle = 25f;
    public Vector3 leftArmDownAxis = Vector3.forward;
    public Vector3 rightArmDownAxis = Vector3.back;

    [Header("Movement")]
    public bool moveForward = false;
    public float moveSpeed = 1.5f;

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

    Quaternion leftUpperLegStart;
    Quaternion rightUpperLegStart;
    Quaternion leftLowerLegStart;
    Quaternion rightLowerLegStart;

    Quaternion leftArmStart;
    Quaternion rightArmStart;
    Quaternion leftLowerArmStart;
    Quaternion rightLowerArmStart;

    Quaternion spineStart;
    Vector3 visualRootStart;

    void Start()
    {
        if (leftUpperLeg) leftUpperLegStart = leftUpperLeg.localRotation;
        if (rightUpperLeg) rightUpperLegStart = rightUpperLeg.localRotation;
        if (leftLowerLeg) leftLowerLegStart = leftLowerLeg.localRotation;
        if (rightLowerLeg) rightLowerLegStart = rightLowerLeg.localRotation;

        if (leftUpperArm) leftArmStart = leftUpperArm.localRotation;
        if (rightUpperArm) rightArmStart = rightUpperArm.localRotation;
        if (leftLowerArm) leftLowerArmStart = leftLowerArm.localRotation;
        if (rightLowerArm) rightLowerArmStart = rightLowerArm.localRotation;

        if (spine) spineStart = spine.localRotation;

        if (visualRoot) visualRootStart = visualRoot.localPosition;
    }

    void Update()
    {
        float t = Time.time * walkSpeed;

        float leftSwing = Mathf.Sin(t);
        float rightSwing = Mathf.Sin(t + Mathf.PI);

        float kneeTimingOffset = 0.6f;
        float leftKneeBend = Mathf.Clamp01(Mathf.Sin(t + kneeTimingOffset)) * kneeBendAngle;
        float rightKneeBend = Mathf.Clamp01(Mathf.Sin(t + Mathf.PI + kneeTimingOffset)) * kneeBendAngle;

        float leftElbowBend = Mathf.Clamp01(Mathf.Sin(t + Mathf.PI + 0.4f)) * elbowBendAngle;
        float rightElbowBend = Mathf.Clamp01(Mathf.Sin(t + 0.4f)) * elbowBendAngle;

        float bob = Mathf.Abs(Mathf.Sin(t)) * bobHeight;

        if (moveForward)
        {
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }

        if (visualRoot)
        {
            visualRoot.localPosition = visualRootStart + Vector3.up * bob;
        }

        if (leftUpperLeg)
        {
            leftUpperLeg.localRotation =
                Quaternion.AngleAxis(leftSwing * legSwingAngle, leftLegSwingAxis) * leftUpperLegStart;
        }

        if (rightUpperLeg)
        {
            rightUpperLeg.localRotation =
                Quaternion.AngleAxis(rightSwing * legSwingAngle, rightLegSwingAxis) * rightUpperLegStart;
        }

        if (leftLowerLeg)
        {
            leftLowerLeg.localRotation =
                Quaternion.AngleAxis(leftKneeBend, legSwingAxis) * leftLowerLegStart;
        }

        if (rightLowerLeg)
        {
            rightLowerLeg.localRotation =
                Quaternion.AngleAxis(rightKneeBend, legSwingAxis) * rightLowerLegStart;
        }

        if (leftUpperArm)
        {
            Quaternion armDown = Quaternion.AngleAxis(armDownAngle, leftArmDownAxis);
            Quaternion armSwing = Quaternion.AngleAxis(rightSwing * armSwingAngle, leftArmSwingAxis);

            leftUpperArm.localRotation =
                armSwing * armDown * leftArmStart;
        }

        if (rightUpperArm)
        {
            Quaternion armDown = Quaternion.AngleAxis(armDownAngle, rightArmDownAxis);
            Quaternion armSwing = Quaternion.AngleAxis(-leftSwing * armSwingAngle, rightArmSwingAxis);

            rightUpperArm.localRotation =
                armSwing * armDown * rightArmStart;
        }

        if (leftLowerArm)
        {
            leftLowerArm.localRotation =
                Quaternion.AngleAxis(leftElbowBend, leftElbowAxis) * leftLowerArmStart;
        }

        if (rightLowerArm)
        {
            rightLowerArm.localRotation =
                Quaternion.AngleAxis(rightElbowBend, rightElbowAxis) * rightLowerArmStart;
        }

        if (spine)
        {
            spine.localRotation =
                spineStart * Quaternion.AngleAxis(leftSwing * spineTwistAngle, spineTwistAxis);
        }
    }
}