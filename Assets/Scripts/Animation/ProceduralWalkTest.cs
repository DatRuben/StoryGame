using UnityEngine;

public class ProceduralWalkTest : MonoBehaviour
{
    [Header("Movement")]
    public bool moveForward = false;
    public float moveSpeed = 1.5f;

    [Header("Body Bob")]
    public Transform visualRoot;
    public float bobHeight = 0.05f;

    [Header("Bones")]
    public Transform leftUpperLeg;
    public Transform rightUpperLeg;
    public Transform leftUpperArm;
    public Transform rightUpperArm;
    public Transform spine;

    [Header("Swing")]
    public float walkSpeed = 3f;
    public float legSwingAngle = 25f;
    public float armSwingAngle = 20f;
    public float spineTwistAngle = 5f;

    [Header("Axes")]
    public Vector3 legSwingAxis = Vector3.right;
    public Vector3 armSwingAxis = Vector3.right;
    public Vector3 spineTwistAxis = Vector3.up;

    Quaternion leftLegStart;
    Quaternion rightLegStart;
    Quaternion leftArmStart;
    Quaternion rightArmStart;
    Quaternion spineStart;
    Vector3 visualRootStart;

    void Start()
    {
        if (leftUpperLeg) leftLegStart = leftUpperLeg.localRotation;
        if (rightUpperLeg) rightLegStart = rightUpperLeg.localRotation;
        if (leftUpperArm) leftArmStart = leftUpperArm.localRotation;
        if (rightUpperArm) rightArmStart = rightUpperArm.localRotation;
        if (spine) spineStart = spine.localRotation;
        if (visualRoot) visualRootStart = visualRoot.localPosition;
    }

    void Update()
    {
        float t = Time.time * walkSpeed;
        float swing = Mathf.Sin(t);
        float oppositeSwing = Mathf.Sin(t + Mathf.PI);
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
            leftUpperLeg.localRotation = leftLegStart * Quaternion.AngleAxis(swing * legSwingAngle, legSwingAxis);
        }

        if (rightUpperLeg)
        {
            rightUpperLeg.localRotation = rightLegStart * Quaternion.AngleAxis(oppositeSwing * legSwingAngle, legSwingAxis);
        }

        if (leftUpperArm)
        {
            leftUpperArm.localRotation = leftArmStart * Quaternion.AngleAxis(oppositeSwing * armSwingAngle, armSwingAxis);
        }

        if (rightUpperArm)
        {
            rightUpperArm.localRotation = rightArmStart * Quaternion.AngleAxis(swing * armSwingAngle, armSwingAxis);
        }

        if (spine)
        {
            spine.localRotation = spineStart * Quaternion.AngleAxis(swing * spineTwistAngle, spineTwistAxis);
        }
    }
}