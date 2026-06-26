using UnityEngine;

public class ProceduralWalkTest : MonoBehaviour
{
    [Header("Movement")]
    public bool moveForward = false;
    public float moveSpeed = 1.5f;

    [Header("Body Bob")]
    public Transform visualRoot;
    public float bobHeight = 0.05f;

    [Header("Upper Body Bones")]
    public Transform leftUpperArm;
    public Transform rightUpperArm;
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

    Quaternion leftUpperLegStart;
    Quaternion rightUpperLegStart;
    Quaternion leftLowerLegStart;
    Quaternion rightLowerLegStart;
    Quaternion leftArmStart;
    Quaternion rightArmStart;
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
            leftUpperArm.localRotation =
               Quaternion.AngleAxis(rightSwing * armSwingAngle, armSwingAxis) * leftArmStart;
        }

        if (rightUpperArm)
        {
            rightUpperArm.localRotation =
               Quaternion.AngleAxis(leftSwing * armSwingAngle, armSwingAxis) * rightArmStart;
        }

        if (spine)
        {
            spine.localRotation =
                spineStart * Quaternion.AngleAxis(leftSwing * spineTwistAngle, spineTwistAxis);
        }
    }
}