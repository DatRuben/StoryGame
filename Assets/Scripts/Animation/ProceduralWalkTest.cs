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

        float leftKneeBend = Mathf.Clamp01(leftSwing) * kneeBendAngle;
        float rightKneeBend = Mathf.Clamp01(rightSwing) * kneeBendAngle;

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
                leftUpperLegStart * Quaternion.AngleAxis(leftSwing * legSwingAngle, legSwingAxis);
        }

        if (rightUpperLeg)
        {
            rightUpperLeg.localRotation =
                rightUpperLegStart * Quaternion.AngleAxis(rightSwing * legSwingAngle, legSwingAxis);
        }

        if (leftLowerLeg)
        {
            leftLowerLeg.localRotation =
                leftLowerLegStart * Quaternion.AngleAxis(leftKneeBend, kneeBendAxis);
        }

        if (rightLowerLeg)
        {
            rightLowerLeg.localRotation =
                rightLowerLegStart * Quaternion.AngleAxis(rightKneeBend, kneeBendAxis);
        }

        if (leftUpperArm)
        {
            leftUpperArm.localRotation =
                leftArmStart * Quaternion.AngleAxis(rightSwing * armSwingAngle, armSwingAxis);
        }

        if (rightUpperArm)
        {
            rightUpperArm.localRotation =
                rightArmStart * Quaternion.AngleAxis(leftSwing * armSwingAngle, armSwingAxis);
        }

        if (spine)
        {
            spine.localRotation =
                spineStart * Quaternion.AngleAxis(leftSwing * spineTwistAngle, spineTwistAxis);
        }
    }
}