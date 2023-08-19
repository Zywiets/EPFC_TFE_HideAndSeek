using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonMovement : MonoBehaviour
{
    // Player Movement
    public CharacterController controller;
    public Transform cam;
    public float turnSmoothTime = 0.1f;
    private float _turnSmoothVelocity;

    // PLayer controls
    private PlayerControls _controls;
    private Vector2 _move;
    public float speed = 6f;
    // Gravity Logic
    
    public float gravity = -9.81f;
    private Vector3 _velocity;
    private bool _isGrounded;
    private bool _isJumping;
    public float jumpHeight = 3f;
    
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    // Movement animation
    private Animator _animator;

    // Second Try movement
    public float jumpSpeed = 5f;

    private float _ySpeed;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _controls = new PlayerControls();
        _controls.Gameplay.Jump.performed += lambda => HandleJump();
        _controls.Gameplay.Move.performed += lambda => _move =lambda.ReadValue<Vector2>();
        _controls.Gameplay.Move.canceled += lambda => _move = Vector2.zero;
    }
    private void Update()
    {
        CheckGrounded();
        
        ApplyGravity();
        
        HandleMovement();
        
        UpdateAnimator();
    }

    private void OnEnable()
    {
        _controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        _controls.Gameplay.Disable();
    }

    private void CheckGrounded()
    {
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
            _animator.SetBool("IsGrounded", true);
            _isJumping = false;
        }
        else
        {
            _animator.SetBool("IsGrounded", false);
            if ((_isJumping && _velocity.y < 0) || _velocity.y < -2)
            {
                _animator.SetBool("IsJumping", false);
            }
        }
    }

    private void ApplyGravity()
    {
        _velocity.y += gravity * Time.deltaTime;
        controller.Move(_velocity * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (_isGrounded)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            _animator.SetBool("IsJumping", true);
            _isJumping = true;
            _isGrounded = false;
            
        }
    }

    private void HandleMovement()
    {
        Vector3 direction = new Vector3(_move.x, 0f, _move.y);
        float inputMagnitude = Mathf.Clamp01(direction.magnitude);
        _animator.SetFloat("Input Magnitude", inputMagnitude, 0.05f, Time.deltaTime);
        float speed = inputMagnitude * this.speed;
        direction.Normalize();
        
        // transform.Translate(direction * speed * Time.deltaTime, Space.World);
        //
        // if (direction != Vector3.zero)
        // {
        //     Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
        //
        //     transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        // }
        //
        // ySpeed += Physics.gravity.y * Time.deltaTime;
        // if (Input.GetButtonDown("Jump"))
        // {
        //     ySpeed = jumpSpeed;
        // }
        //
        // Vector3 velocity = direction * magnitude;
        // velocity.y = ySpeed;
        
        if (direction.magnitude >= 0.1f)
        {
            _animator.SetBool("IsMoving", true);
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }
        else
        {
            _animator.SetBool("IsMoving", false);
        }
    }

    private void UpdateAnimator()
    {
        bool isMoving = _move.magnitude > 0.1f;
        _animator.SetBool("IsMoving", isMoving);
        _animator.SetBool("IsFalling", !_isGrounded && _velocity.y < 0);
    }
}
