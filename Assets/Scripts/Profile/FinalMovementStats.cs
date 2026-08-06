using System;

[Serializable]
public class FinalMovementStats
{
    public float walkSpeed;
    public float sprintSpeed;
    public float groundAcceleration;
    public float airAcceleration;
    public float deceleration;
    public float jumpForce;

    public DodgeType dodgeType;

    public float dodgeDistance;
    public float dodgeDuration;
    public float dodgeCooldown;
    public float dodgeStaminaCost;
    public float dodgeControl;

    public static FinalMovementStats CreateDefault()
    {
        return new FinalMovementStats
        {
            walkSpeed = 8f,
            sprintSpeed = 12f,
            groundAcceleration = 8f,
            airAcceleration = 2f,
            deceleration = 16f,
            jumpForce = 7f,

            dodgeType = DodgeType.MediumDash,
            dodgeDistance = 5f,
            dodgeDuration = 0.3f,
            dodgeCooldown = 0.7f,
            dodgeStaminaCost = 25f,
            dodgeControl = 0.15f
        };
    }

    public static FinalMovementStats Copy(
        FinalMovementStats source)
    {
        if (source == null)
            return CreateDefault();

        return new FinalMovementStats
        {
            walkSpeed = source.walkSpeed,
            sprintSpeed = source.sprintSpeed,
            groundAcceleration =
                source.groundAcceleration,
            airAcceleration =
                source.airAcceleration,
            deceleration = source.deceleration,
            jumpForce = source.jumpForce,

            dodgeType = source.dodgeType,
            dodgeDistance = source.dodgeDistance,
            dodgeDuration = source.dodgeDuration,
            dodgeCooldown = source.dodgeCooldown,
            dodgeStaminaCost =
                source.dodgeStaminaCost,
            dodgeControl = source.dodgeControl
        };
    }
}