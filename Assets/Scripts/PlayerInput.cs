using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private Vector3 forceDirection = Vector3.zero;

    [SerializeField] private Camera playerCamera;
    private Animator animator;

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
        move = playerInput.Player.Move;
        playerInput.Player.Enable();
    }

    private void OnDisable()
    {
        playerInput.Player.Jump.started -= DoJump;
        playerInput.Player.Attack.started -= DoAttack;
        playerInput.Player.Disable();
    }

    private void FixedUpdate()
    {
        forceDirection += move.ReadValue<Vector2>().x * GetCameraRight(playerCamera) * movementForce;
        forceDirection += move.ReadValue<Vector2>().y * GetCameraForward(playerCamera) * movementForce;

        rb.AddForce(forceDirection, ForceMode.Impulse);
        forceDirection = Vector3.zero;

        if (rb.linearVelocity.y < 0f)
            rb.linearVelocity -= Vector3.down * Physics.gravity.y * Time.fixedDeltaTime;

        Vector3 horizontalVelocity = rb.linearVelocity;
        horizontalVelocity.y = 0;
        if (horizontalVelocity.sqrMagnitude > maxSpeed * maxSpeed)
            rb.linearVelocity = horizontalVelocity.normalized * maxSpeed + Vector3.up * rb.linearVelocity.y;

        LookAt();
    }

    private void LookAt()
    {
        Vector3 direction = rb.linearVelocity;
        direction.y = 0f;

        if (move.ReadValue<Vector2>().sqrMagnitude > 0.1f && direction.sqrMagnitude > 0.1f)
            this.rb.rotation = Quaternion.LookRotation(direction, Vector3.up);
        else
            rb.angularVelocity = Vector3.zero;
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
        Debug.Log("Jump attempt");
        forceDirection += Vector3.up * jumpForce;

        if (IsGrounded())
        {
            Debug.Log("Jumped");
        }
        else 
        {             
            Debug.Log("Not grounded");
        }
    }

    private bool IsGrounded()
    {
        Ray ray = new Ray(this.transform.position + Vector3.up * 0.25f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 0.2f))
            return true;
        else
            return false;
    }

    private void DoAttack(InputAction.CallbackContext obj)
    {
        animator.SetTrigger("attack");
    }
}