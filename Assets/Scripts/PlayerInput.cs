using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Unity.Cinemachine;

public class PlayerInput : MonoBehaviour
{
    // input fields
    private PlayerInputActions playerInput;
    private InputAction move;

    // movement fields
    private Rigidbody rb;

    [SerializeField] private float groundAcceleration = 8f;
    [SerializeField] private float airAcceleration = 2f;
    [SerializeField] private float deceleration = 16f;

    [SerializeField] private float jumpForce = 7f;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private float groundCheckDistance = 0.75f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float maxGroundAngle = 55f;

    [SerializeField] private float fallMultiplier = 4f;
    [SerializeField] private float lowJumpMultiplier = 3f;
    [SerializeField] private float jumpUpMultiplier = 1.5f;

    [SerializeField] private float sprintSpeed = 15f;
    [SerializeField] private float walkSpeed = 10f;

    [SerializeField] private float wallContactMemory = 0.1f;
    [SerializeField] private float jumpGroundIgnoreTime = 0.15f;

    [SerializeField] private bool preventSlopeSliding = true;
    [SerializeField] private float slopeSlideCancelStrength = 1f;

    private Vector3 groundNormal = Vector3.up;
    private Vector3 wallNormal = Vector3.zero;
    private float lastWallContactTime = -999f;
    private float lastJumpTime = -999f;

    // camera fields
    [SerializeField] private CinemachineOrbitalFollow orbitalFollow;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Camera playerCamera;

    // UI fields
    [SerializeField] private TextMeshProUGUI speedText;

    private Animator animator;

    private bool cameraLocked = false;
    public bool CameraLocked => cameraLocked;

    private bool isSprinting = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = new PlayerInputActions();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        playerInput.Player.Jump.started += DoJump;
        playerInput.Player.Attack.started += DoAttack;
        playerInput.Player.CameraLock.started += ToggleCameraLock;

        move = playerInput.Player.Move;

        playerInput.Player.Sprint.started += StartSprint;
        playerInput.Player.Sprint.canceled += StopSprint;

        playerInput.Player.Enable();
    }

    private void OnDisable()
    {
        playerInput.Player.Jump.started -= DoJump;
        playerInput.Player.Attack.started -= DoAttack;
        playerInput.Player.CameraLock.started -= ToggleCameraLock;

        playerInput.Player.Sprint.started -= StartSprint;
        playerInput.Player.Sprint.canceled -= StopSprint;

        playerInput.Player.Disable();
    }

    private void FixedUpdate()
    {
        bool grounded =
            IsGrounded() &&
            Time.time - lastJumpTime > jumpGroundIgnoreTime;

        Vector2 input = move.ReadValue<Vector2>();

        Vector3 moveForward = GetCameraForward(playerCamera);
        Vector3 moveRight = GetCameraRight(playerCamera);

        Vector3 movement =
            input.x * moveRight +
            input.y * moveForward;

        if (movement.sqrMagnitude > 1f)
            movement.Normalize();

        Vector3 facingDirection;

        if (cameraLocked)
        {
            // Locked mode:
            // Player faces the camera/crosshair forward direction.
            facingDirection = cameraTransform.forward;
            facingDirection.y = 0f;

            if (facingDirection.sqrMagnitude > 0.01f)
                facingDirection.Normalize();
        }
        else
        {
            // Unlocked mode:
            // Player faces movement direction.
            facingDirection = movement;
            facingDirection.y = 0f;

            if (facingDirection.sqrMagnitude > 0.01f)
                facingDirection.Normalize();
        }

        // Move along the slope surface when grounded
        if (grounded && movement.sqrMagnitude > 0.01f)
        {
            movement = Vector3.ProjectOnPlane(
                movement,
                groundNormal
            ).normalized;
        }

        // If touching a wall/steep surface, do not turn forward input into wall-surfing
        if (movement.sqrMagnitude > 0.01f &&
            RecentlyTouchedWall(out Vector3 recentWallNormal))
        {
            float intoWallAmount =
                Vector3.Dot(movement.normalized, recentWallNormal);

            // Strongly pressing into the wall/steep slope = stop movement
            if (intoWallAmount < -0.35f)
            {
                movement = Vector3.zero;
            }
            // Lightly angled into the wall = allow some sliding
            else if (intoWallAmount < 0f)
            {
                movement = Vector3.ProjectOnPlane(
                    movement,
                    recentWallNormal
                );

                movement.y = 0f;

                if (movement.sqrMagnitude > 0.01f)
                    movement.Normalize();
                else
                    movement = Vector3.zero;
            }
        }

        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        Vector3 targetVelocity = movement * currentSpeed;

        Vector3 currentVelocityAlongSurface;

        if (grounded)
        {
            currentVelocityAlongSurface =
                Vector3.ProjectOnPlane(rb.linearVelocity, groundNormal);
        }
        else
        {
            currentVelocityAlongSurface = rb.linearVelocity;
            currentVelocityAlongSurface.y = 0f;
        }

        Vector3 velocityChange =
            targetVelocity - currentVelocityAlongSurface;

        bool hasMovementDirection =
            movement.sqrMagnitude > 0.01f;

        float acceleration;

        if (grounded)
        {
            acceleration =
                hasMovementDirection
                ? groundAcceleration
                : deceleration;
        }
        else
        {
            acceleration =
                hasMovementDirection
                ? airAcceleration
                : deceleration;
        }

        rb.AddForce(
            velocityChange * acceleration,
            ForceMode.Acceleration
        );

        // Prevent sliding down walkable slopes
        if (preventSlopeSliding && grounded)
        {
            float slopeAngle =
                Vector3.Angle(groundNormal, Vector3.up);

            if (slopeAngle > 0.1f && slopeAngle <= maxGroundAngle)
            {
                Vector3 gravityAlongSlope =
                    Vector3.ProjectOnPlane(
                        Physics.gravity,
                        groundNormal
                    );

                rb.AddForce(
                    -gravityAlongSlope * slopeSlideCancelStrength,
                    ForceMode.Acceleration
                );
            }
        }

        // Remove velocity pushing into walls/steep surfaces
        if (RecentlyTouchedWall(out Vector3 recentWallNormalAfterForce))
        {
            float intoWall =
                Vector3.Dot(rb.linearVelocity, recentWallNormalAfterForce);

            if (intoWall < 0f)
            {
                rb.linearVelocity -=
                    recentWallNormalAfterForce * intoWall;
            }
        }

        // Extra gravity only while airborne
        if (!grounded)
        {
            if (rb.linearVelocity.y < 0f)
            {
                rb.linearVelocity += Vector3.up *
                    Physics.gravity.y *
                    (fallMultiplier - 1f) *
                    Time.fixedDeltaTime;
            }
            else if (rb.linearVelocity.y > 0f)
            {
                float upwardMultiplier =
                    Keyboard.current.spaceKey.isPressed
                    ? jumpUpMultiplier
                    : lowJumpMultiplier;

                rb.linearVelocity += Vector3.up *
                    Physics.gravity.y *
                    (upwardMultiplier - 1f) *
                    Time.fixedDeltaTime;
            }
        }

        Vector3 velocityToClamp;

        if (grounded)
        {
            velocityToClamp =
                Vector3.ProjectOnPlane(rb.linearVelocity, groundNormal);
        }
        else
        {
            velocityToClamp = rb.linearVelocity;
            velocityToClamp.y = 0f;
        }

        if (velocityToClamp.sqrMagnitude > currentSpeed * currentSpeed)
        {
            Vector3 clampedVelocity =
                velocityToClamp.normalized * currentSpeed;

            if (grounded)
            {
                rb.linearVelocity = clampedVelocity;
            }
            else
            {
                rb.linearVelocity =
                    clampedVelocity +
                    Vector3.up * rb.linearVelocity.y;
            }
        }

        Vector3 horizontalVelocity = rb.linearVelocity;
        horizontalVelocity.y = 0f;

        Vector3 surfaceVelocity = grounded
            ? Vector3.ProjectOnPlane(rb.linearVelocity, groundNormal)
            : horizontalVelocity;

        if (speedText != null)
        {
            speedText.text =
                "Grounded: " + grounded +
                "\nHorizontal Speed: " + horizontalVelocity.magnitude.ToString("F2") +
                "\nSurface Speed: " + surfaceVelocity.magnitude.ToString("F2");
        }

        LookAt(facingDirection);
    }

    private void LookAt(Vector3 desiredDirection)
    {
        Vector3 direction = desiredDirection;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction, Vector3.up);

            rb.rotation = Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
        }
    }

    private Vector3 GetCameraForward(Camera playerCamera)
    {
        Vector3 forward = playerCamera.transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.01f)
            return transform.forward;

        return forward.normalized;
    }

    private Vector3 GetCameraRight(Camera playerCamera)
    {
        Vector3 right = playerCamera.transform.right;
        right.y = 0f;

        if (right.sqrMagnitude < 0.01f)
            return transform.right;

        return right.normalized;
    }

    private void DoJump(InputAction.CallbackContext obj)
    {
        if (!IsGrounded())
            return;

        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        lastJumpTime = Time.time;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private bool IsGrounded()
    {
        bool nearGround = Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        if (!nearGround)
        {
            groundNormal = Vector3.up;
            return false;
        }

        Vector3 rayStart =
            groundCheck.position + Vector3.up * 0.25f;

        if (Physics.Raycast(
            rayStart,
            Vector3.down,
            out RaycastHit hit,
            groundCheckDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore))
        {
            float groundAngle =
                Vector3.Angle(hit.normal, Vector3.up);

            if (groundAngle <= maxGroundAngle)
            {
                groundNormal = hit.normal;
                return true;
            }
        }

        groundNormal = Vector3.up;
        return false;
    }

    private void DoAttack(InputAction.CallbackContext obj)
    {
        animator.SetTrigger("attack");
    }

    private void ToggleCameraLock(InputAction.CallbackContext obj)
    {
        cameraLocked = !cameraLocked;
    }

    private void StartSprint(InputAction.CallbackContext obj)
    {
        isSprinting = true;
    }

    private void StopSprint(InputAction.CallbackContext obj)
    {
        isSprinting = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            float angle =
                Vector3.Angle(contact.normal, Vector3.up);

            // Anything steeper than maxGroundAngle counts as a wall/steep surface
            if (angle > maxGroundAngle)
            {
                wallNormal = contact.normal;
                wallNormal.y = 0f;

                if (wallNormal.sqrMagnitude > 0.001f)
                {
                    wallNormal.Normalize();
                    lastWallContactTime = Time.time;
                }

                break;
            }
        }
    }

    private bool RecentlyTouchedWall(out Vector3 recentWallNormal)
    {
        recentWallNormal = wallNormal;

        return Time.time - lastWallContactTime <= wallContactMemory &&
               wallNormal.sqrMagnitude > 0.001f;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}