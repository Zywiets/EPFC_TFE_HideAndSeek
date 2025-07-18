using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonMovement : MonoBehaviour
{
    [Header("Player Movement")]
    public CharacterController controller;
    public Transform cam;
    public float turnSmoothTime = 0.1f;
    private float _turnSmoothVelocity;
    private bool _isGrounded;
    private bool _isJumping;
    private bool _isCrouching;
    public float jumpHeight = 8f;
    private const float JumpHorizontal = 8f;

    [Header("Player Controls")]
    private PlayerControls _controls;
    private Vector2 _move;
    public float speed = 6f;
    
    [Header("Gravity Logic")]
    public float gravity = -9.81f;
    private Vector3 _velocity;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Movement animations")]
    private Animator _animator;
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        InitializeControls();
    }
    private void InitializeControls()
    {
        _controls = new PlayerControls();
        _controls.Gameplay.Jump.performed += _ => HandleJump();
        _controls.Gameplay.Move.performed += ctx => _move = ctx.ReadValue<Vector2>();
        _controls.Gameplay.Move.canceled += _ => _move = Vector2.zero;
        _controls.Gameplay.Crouch.started += _ => HandleStartCrouch();
        _controls.Gameplay.Crouch.canceled += _ => HandleEndCrouch();
    }
    private void OnEnable()
    {
        _controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        _controls.Gameplay.Disable();
    }
    private void Update()
    {
        CheckGrounded();
        
        ApplyGravity();
        
        HandleMovement();
    }

    private void CheckGrounded()
    {
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (_isGrounded)
        {
            if (!(_velocity.y < 0)) return;
            SetGroundState();
        }
        else
        {
            _animator.SetBool("IsGrounded", false);
            if ((_isJumping && _velocity.y > 0) || _velocity.y < -2)
            {
                SetFallingState();
            }
        }
    }

    private void SetGroundState()
    {
        _velocity.y = -2f;
        _animator.SetBool("IsGrounded", _isGrounded);
        _animator.SetBool("IsJumping", false);
        _animator.SetBool("IsFalling", false);
        _isJumping = false;
    }

    private void SetFallingState()
    {
        _animator.SetBool("IsFalling", true);
        _animator.SetBool("IsJumping", false);
        _isJumping = false;
    }

    private void ApplyGravity()
    {
        _velocity.y += gravity * Time.deltaTime;
        controller.Move(_velocity * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (!_isGrounded) return;
        
        _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        _animator.SetBool("IsJumping", true);
        _isJumping = true;
        _isGrounded = false;
    }

    private void HandleStartCrouch()
    {
        if (!_isGrounded) return;

        _isCrouching = true;
        _animator.SetBool("IsCrouching", _isCrouching);

    }
    private void HandleEndCrouch()
    {
        if (!_isGrounded) return;
        _isCrouching = false;
        _animator.SetBool("IsCrouching", _isCrouching);
        
    }

    private void HandleMovement()
    {
        Vector3 direction = new Vector3(_move.x, 0f, _move.y);
        float inputMagnitude = Mathf.Clamp01(direction.magnitude);
        _animator.SetFloat("Input Magnitude", inputMagnitude, 0.05f, Time.deltaTime);
        float speedInputed = inputMagnitude * speed;
        direction.Normalize();
        if (_isGrounded)
        {
            if (inputMagnitude >= 0.1f)
            {
                _animator.SetBool("IsMoving", true);
                Vector3 moveDir = GetMoveDirection(direction);
                controller.Move(moveDir.normalized * (speedInputed * Time.deltaTime));
            }
            else
            {
                _animator.SetBool("IsMoving", false);
            }
        }
        else
        {
            Vector3 velocity =  GetMoveDirection(direction) * (inputMagnitude * JumpHorizontal);
            controller.Move(velocity * Time.deltaTime);
        }
    }

    // Calculates the direction the controller is pointing instead of the opposite (responding like an aeroplane if not)
    private Vector3 GetMoveDirection(Vector3 direction)
    {
        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, turnSmoothTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
        return Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
    }
}
