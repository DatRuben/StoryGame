using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Unity.Cinemachine;

public class PlayerInput : MonoBehaviour
{
    //input fields
    private PlayerInputActions playerInput;
    private InputAction move;

    //movement fields
    private Rigidbody rb;
    [SerializeField] private float movementForce = 1f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float fallMultiplier = 3f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    [SerializeField] private CinemachineOrbitalFollow orbitalFollow;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float rotationSpeed = 10f;

    [SerializeField] private TextMeshProUGUI speedText;

    [SerializeField] private Camera playerCamera;
    private Animator animator;

    private bool cameraLocked = false;

    private void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
        playerInput = new PlayerInputActions();
        animator = this.GetComponent<Animator>();
    }

    private void OnEnable()
    {
        playerInput.Player.Jump.started += DoJump;
        playerInput.Player.Attack.started += DoAttack;
        playerInput.Player.CameraLock.started += ToggleCameraLock;

        move = playerInput.Player.Move;

        playerInput.Player.Enable();
    }

    private void OnDisable()
    {
        playerInput.Player.Jump.started -= DoJump;
        playerInput.Player.Attack.started -= DoAttack;
        playerInput.Player.CameraLock.started -= ToggleCameraLock;

        playerInput.Player.Disable();
    }

    private void FixedUpdate()
    {
        Vector2 input = move.ReadValue<Vector2>();

        Vector3 movement =
            input.x * GetCameraRight(playerCamera) +
            input.y * GetCameraForward(playerCamera);

        movement.Normalize();

        Vector3 targetVelocity = movement * maxSpeed;

        Vector3 currentVelocity = rb.linearVelocity;

        Vector3 velocityChange =
            targetVelocity - new Vector3(
                currentVelocity.x,
                0f,
                currentVelocity.z
            );

        float acceleration =
            input.magnitude > 0
            ? movementForce
            : movementForce * 2f;

        rb.AddForce(
            velocityChange * acceleration,
            ForceMode.Acceleration
            );

        //gravity code
        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0f &&
                 !Keyboard.current.spaceKey.isPressed)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
        //gravity end

        Vector3 horizontalVelocity = rb.linearVelocity;
        horizontalVelocity.y = 0;

        speedText.text = "Speed: " + horizontalVelocity.magnitude.ToString("F2");

        if (horizontalVelocity.sqrMagnitude > maxSpeed * maxSpeed)
            rb.linearVelocity = horizontalVelocity.normalized * maxSpeed + Vector3.up * rb.linearVelocity.y;

        LookAt();
    }

    private void LookAt()
    {
        Vector3 direction;

        if (cameraLocked)
        {
            direction = cameraTransform.forward;
            direction.y = 0f;
        }
        else
        {
            direction = rb.linearVelocity;
            direction.y = 0f;
        }

        if (direction.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

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
        forward.y = 0;
        return forward.normalized;
    }

    private Vector3 GetCameraRight(Camera playerCamera)
    {
        Vector3 right = playerCamera.transform.right;
        right.y = 0;
        return right.normalized;
    }

    private void DoJump(InputAction.CallbackContext obj)
    {
        if (!IsGrounded())
            return;

        // Reset downward velocity before jumping
        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private bool IsGrounded()
    {
        return Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    private void DoAttack(InputAction.CallbackContext obj)
    {
        animator.SetTrigger("attack");
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

    private void ToggleCameraLock(InputAction.CallbackContext obj)
    {
        cameraLocked = !cameraLocked;
        orbitalFollow.HorizontalAxis.Recentering.Enabled = cameraLocked;
    }
}
