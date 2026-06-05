using UnityEngine;

[CreateAssetMenu(menuName = "Game/Shapeshift Profile")]
public class ShapeshiftProfile : ScriptableObject
{
    [Header("Identity")]
    public string formName;

    [TextArea]
    public string description;

    [Header("Body / Collider")]
    public float capsuleHeight = 1f;
    public float capsuleRadius = 0.3f;
    public Vector3 capsuleCenter = Vector3.zero;

    [Header("Player Setup")]
    public Vector3 groundCheckLocalPosition = new Vector3(0f, -0.4f, 0f);
    public Vector3 cameraTargetLocalPosition = new Vector3(0f, 0.8f, 0f);

    [Header("Movement")]
    public float walkSpeed = 8f;
    public float sprintSpeed = 12f;
    public float groundAcceleration = 10f;
    public float airAcceleration = 4f;
    public float deceleration = 14f;
    public float jumpForce = 5f;

    [Header("Abilities")]
    public bool canFly = false;

    //[Header("Inventory")]
    //public bool keepsNaturalInventory = true;
    //public bool canAccessNormalInventory = false;
}