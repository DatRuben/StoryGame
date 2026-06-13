using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Unity.Cinemachine;

public class PlayerInput : MonoBehaviour
{
    // Input fields
    private PlayerInputActions playerInput;
    private InputAction move;

    // Movement fields
    private Rigidbody rb;

    [Header("Movement")]
    [SerializeField] private float groundAcceleration = 8f;
    [SerializeField] private float airAcceleration = 2f;
    [SerializeField] private float deceleration = 16f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float sprintSpeed = 15f;
    [SerializeField] private float walkSpeed = 10f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private float groundCheckDistance = 0.75f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float maxGroundAngle = 55f;
    [SerializeField] private float jumpGroundIgnoreTime = 0.15f;

    [Header("Jump Feel")]
    [SerializeField] private float fallMultiplier = 4f;
    [SerializeField] private float lowJumpMultiplier = 3f;
    [SerializeField] private float jumpUpMultiplier = 1.5f;

    [Header("Slope Handling")]
    [SerializeField] private bool preventSlopeSliding = true;
    [SerializeField] private float slopeSlideCancelStrength = 1f;

    [Header("Default Movement Anti-Launch")]
    [SerializeField] private bool preventDefaultMovementLaunch = true;
    [SerializeField] private float allowedDefaultUpwardSpeed = 0.5f;
    [SerializeField] private float recentlyGroundedTime = 0.15f;
    [SerializeField] private float flatGroundAngle = 5f;

    [Header("External Launch")]
    [SerializeField] private float defaultExternalLaunchTime = 0.35f;

    [Header("Wall Handling")]
    [SerializeField] private float wallContactMemory = 0.1f;

    [Header("Camera / Rotation")]
    [SerializeField] private CinemachineOrbitalFollow orbitalFollow;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private int cameraLockIgnoreFixedFrames = 2;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI speedText;

    [Header("Inventory / Equipment")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerWeaponSlots playerWeaponSlots;

    private Animator animator;

    private Vector3 groundNormal = Vector3.up;
    private Vector3 wallNormal = Vector3.zero;

    private float lastWallContactTime = -999f;
    private float lastJumpTime = -999f;
    private float lastGroundedTime = -999f;
    private float forcedAirUntil = -999f;

    private bool cameraLocked = false;
    public bool CameraLocked => cameraLocked;
    private int cameraLockFramesToIgnore = 0;

    private bool isSprinting = false;
    private bool isJumpHeld = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = new PlayerInputActions();
        animator = GetComponent<Animator>();
        if (playerInventory == null)
            playerInventory = GetComponent<PlayerInventory>();
        if (playerWeaponSlots == null)
            playerWeaponSlots = GetComponent<PlayerWeaponSlots>();
    }

    private void OnEnable()
    {
        move = playerInput.Player.Move;

        playerInput.Player.Jump.started += DoJump;
        playerInput.Player.Jump.started += StartJumpHold;
        playerInput.Player.Jump.canceled += StopJumpHold;

        playerInput.Player.Attack.started += DoAttack;
        playerInput.Player.CameraLock.started += ToggleCameraLock;

        playerInput.Player.Sprint.started += StartSprint;
        playerInput.Player.Sprint.canceled += StopSprint;

        playerInput.Player.SheatheUnsheathe.started += ToggleWeaponSheathe;

        playerInput.Player.SwitchWeapon.started += SwitchWeaponSet;

        playerInput.Player.Enable();
    }

    private void OnDisable()
    {
        playerInput.Player.Jump.started -= DoJump;
        playerInput.Player.Jump.started -= StartJumpHold;
        playerInput.Player.Jump.canceled -= StopJumpHold;

        playerInput.Player.Attack.started -= DoAttack;
        playerInput.Player.CameraLock.started -= ToggleCameraLock;

        playerInput.Player.Sprint.started -= StartSprint;
        playerInput.Player.Sprint.canceled -= StopSprint;

        playerInput.Player.SheatheUnsheathe.started -= ToggleWeaponSheathe;

        playerInput.Player.SwitchWeapon.started -= SwitchWeaponSet;

        playerInput.Player.Disable();
    }

    private void FixedUpdate()
    {
        bool canUseGround =
            Time.time - lastJumpTime > jumpGroundIgnoreTime;

        bool grounded =
            IsGrounded() &&
            canUseGround;

        if (grounded)
        {
            lastGroundedTime = Time.time;
        }

        Vector2 input =
            move.ReadValue<Vector2>();

        Vector3 movement =
            input.x * GetCameraRight(playerCamera) +
            input.y * GetCameraForward(playerCamera);

        // Important:
        // Default movement should not intentionally add upward velocity.
        movement.y = 0f;

        if (movement.sqrMagnitude > 1f)
            movement.Normalize();

        HandleWallMovement(ref movement);

        float currentSpeed =
            isSprinting ? sprintSpeed : walkSpeed;

        Vector3 targetVelocity =
            movement * currentSpeed;

        Vector3 currentHorizontalVelocity =
            rb.linearVelocity;

        currentHorizontalVelocity.y = 0f;

        Vector3 velocityChange =
            targetVelocity - currentHorizontalVelocity;

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

        PreventSlopeSliding(grounded);
        RemoveWallVelocity();
        PreventDefaultMovementLaunch(grounded);
        ApplyExtraGravity(grounded);
        ClampHorizontalSpeed(currentSpeed);
        UpdateSpeedText(grounded);
        LookAt();
    }

    private void HandleWallMovement(ref Vector3 movement)
    {
        if (movement.sqrMagnitude <= 0.01f)
            return;

        if (!RecentlyTouchedWall(out Vector3 recentWallNormal))
            return;

        float intoWallAmount =
            Vector3.Dot(movement.normalized, recentWallNormal);

        // Strongly pressing into the wall = stop movement.
        if (intoWallAmount < -0.5f)
        {
            movement = Vector3.zero;
            return;
        }

        // Slightly angled into the wall = remove only the into-wall part.
        if (intoWallAmount < 0f)
        {
            movement =
                Vector3.ProjectOnPlane(
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

    private void PreventSlopeSliding(bool grounded)
    {
        if (!preventSlopeSliding)
            return;

        if (!grounded)
            return;

        float groundAngle =
            Vector3.Angle(groundNormal, Vector3.up);

        if (groundAngle <= 0.1f)
            return;

        if (groundAngle > maxGroundAngle)
            return;

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

    private void PreventDefaultMovementLaunch(bool grounded)
    {
        if (!preventDefaultMovementLaunch)
            return;

        bool recentlyJumped =
            Time.time - lastJumpTime <= jumpGroundIgnoreTime;

        bool beingForcedIntoAir =
            Time.time <= forcedAirUntil;

        if (recentlyJumped || beingForcedIntoAir)
            return;

        bool recentlyGrounded =
            Time.time - lastGroundedTime <= recentlyGroundedTime;

        if (!grounded && !recentlyGrounded)
            return;

        float groundAngle =
            Vector3.Angle(groundNormal, Vector3.up);

        bool groundedOnRamp =
            grounded &&
            groundAngle > flatGroundAngle &&
            groundAngle <= maxGroundAngle;

        // Let the player climb ramps.
        // Only clamp launch when on flat ground or right after losing ground.
        if (groundedOnRamp)
            return;

        Vector3 velocity =
            rb.linearVelocity;

        if (velocity.y <= allowedDefaultUpwardSpeed)
            return;

        velocity.y = allowedDefaultUpwardSpeed;

        rb.linearVelocity = velocity;
    }

    private void RemoveWallVelocity()
    {
        if (!RecentlyTouchedWall(out Vector3 recentWallNormal))
            return;

        float intoWall =
            Vector3.Dot(rb.linearVelocity, recentWallNormal);

        if (intoWall < 0f)
        {
            rb.linearVelocity -=
                recentWallNormal * intoWall;
        }
    }

    private void ApplyExtraGravity(bool grounded)
    {
        if (grounded)
            return;

        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity +=
                Vector3.up *
                Physics.gravity.y *
                (fallMultiplier - 1f) *
                Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0f)
        {
            float upwardMultiplier =
                isJumpHeld
                ? jumpUpMultiplier
                : lowJumpMultiplier;

            rb.linearVelocity +=
                Vector3.up *
                Physics.gravity.y *
                (upwardMultiplier - 1f) *
                Time.fixedDeltaTime;
        }
    }

    private void ClampHorizontalSpeed(float currentSpeed)
    {
        Vector3 horizontalVelocity =
            rb.linearVelocity;

        horizontalVelocity.y = 0f;

        if (horizontalVelocity.sqrMagnitude <= currentSpeed * currentSpeed)
            return;

        Vector3 clampedHorizontalVelocity =
            horizontalVelocity.normalized * currentSpeed;

        rb.linearVelocity =
            clampedHorizontalVelocity +
            Vector3.up * rb.linearVelocity.y;
    }

    private void UpdateSpeedText(bool grounded)
    {
        if (speedText == null)
            return;

        Vector3 horizontalVelocity =
            rb.linearVelocity;

        horizontalVelocity.y = 0f;

        float groundAngle =
            Vector3.Angle(groundNormal, Vector3.up);

        speedText.text =
            "Grounded: " + grounded +
            "\nHorizontal Speed: " + horizontalVelocity.magnitude.ToString("F2") +
            "\nY Speed: " + rb.linearVelocity.y.ToString("F2") +
            "\nGround Angle: " + groundAngle.ToString("F1");
    }

    private void LookAt()
    {
        if (cameraLockFramesToIgnore > 0)
        {
            cameraLockFramesToIgnore--;
            rb.angularVelocity = Vector3.zero;
            return;
        }

        Vector3 direction;

        if (cameraLocked)
        {
            direction = GetCameraForward(playerCamera);
        }
        else
        {
            direction = rb.linearVelocity;
            direction.y = 0f;
        }

        if (direction.sqrMagnitude <= 0.1f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction, Vector3.up);

        Quaternion smoothedRotation =
            Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );

        rb.MoveRotation(smoothedRotation);
        rb.angularVelocity = Vector3.zero;
    }

    private Vector3 GetCameraForward(Camera camera)
    {
        Vector3 forward =
            camera.transform.forward;

        forward.y = 0f;

        if (forward.sqrMagnitude < 0.01f)
            return transform.forward;

        return forward.normalized;
    }

    private Vector3 GetCameraRight(Camera camera)
    {
        Vector3 right =
            camera.transform.right;

        right.y = 0f;

        if (right.sqrMagnitude < 0.01f)
            return transform.right;

        return right.normalized;
    }

    private void DoJump(InputAction.CallbackContext obj)
    {
        if (!IsGrounded())
            return;

        rb.linearVelocity =
            new Vector3(
                rb.linearVelocity.x,
                0f,
                rb.linearVelocity.z
            );

        lastJumpTime = Time.time;

        rb.AddForce(
            Vector3.up * jumpForce,
            ForceMode.Impulse
        );
    }

    public void ApplyExternalLaunch(Vector3 impulse)
    {
        ApplyExternalLaunch(
            impulse,
            defaultExternalLaunchTime
        );
    }

    public void ApplyExternalLaunch(Vector3 impulse, float forcedAirTime)
    {
        forcedAirUntil =
            Time.time + forcedAirTime;

        rb.AddForce(
            impulse,
            ForceMode.Impulse
        );
    }

    private bool IsGrounded()
    {
        bool nearGround =
            Physics.CheckSphere(
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
        if (animator != null)
            animator.SetTrigger("attack");
    }

    private void ToggleCameraLock(InputAction.CallbackContext obj)
    {
        cameraLocked = !cameraLocked;
        cameraLockFramesToIgnore = cameraLockIgnoreFixedFrames;

        if (rb != null)
            rb.angularVelocity = Vector3.zero;
    }

    private void StartSprint(InputAction.CallbackContext obj)
    {
        isSprinting = true;
    }

    private void StopSprint(InputAction.CallbackContext obj)
    {
        isSprinting = false;
    }

    private void StartJumpHold(InputAction.CallbackContext obj)
    {
        isJumpHeld = true;
    }

    private void StopJumpHold(InputAction.CallbackContext obj)
    {
        isJumpHeld = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            float angle =
                Vector3.Angle(contact.normal, Vector3.up);

            // Anything steeper than maxGroundAngle counts as a wall/steep surface.
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

    public void ApplyRaceMovement(RaceProfile raceProfile)
    {
        if (raceProfile == null)
            return;

        walkSpeed = raceProfile.walkSpeed;
        sprintSpeed = raceProfile.sprintSpeed;
        groundAcceleration = raceProfile.groundAcceleration;
        airAcceleration = raceProfile.airAcceleration;
        deceleration = raceProfile.deceleration;
        jumpForce = raceProfile.jumpForce;
    }

    private void ToggleWeaponSheathe(InputAction.CallbackContext context)
    {
        if (playerWeaponSlots == null)
            return;

        playerWeaponSlots.ToggleWeaponsDrawn();
    }

    private void SwitchWeaponSet(InputAction.CallbackContext context)
    {
        if (playerWeaponSlots == null)
            return;

        int nextWeaponSetIndex =
            playerWeaponSlots.ActiveWeaponSetIndex == 0
                ? 1
                : 0;

        playerWeaponSlots.SetActiveWeaponSet(nextWeaponSetIndex);
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