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
}