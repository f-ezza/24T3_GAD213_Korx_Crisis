using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Korx.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class CharacterLocomotionSystem : MonoBehaviour
    {
        [Header("Functional Options")]
        [SerializeField] private bool allowMove = true;
        [SerializeField] private bool allowSprint = true;
        [SerializeField] private bool allowJump = true;
        [SerializeField] private bool allowCrouch = true;
        [SerializeField] private bool allowDive = true;
        [SerializeField] private bool allowSlide = true;
        [SerializeField] private bool allowMantle = true;

        [Header("Speed Variables")]
        [SerializeField] private float walkSpeed = 7.5f;
        [SerializeField] private float crouchSpeed = 5f;
        [SerializeField] private float sprintSpeed = 10f;

        [Header("Force Variables")]
        [SerializeField] private float diveForce = 8f;
        [SerializeField] private float slideForce = 8f;
        [SerializeField] private float jumpForce = 8f;
        [SerializeField] private float groundDrag = 1f;
        [SerializeField] private float airDrag = 0f;
        [SerializeField] private float airMultiplier = 0.4f;

        [Header("Jump Cooldown")]
        [SerializeField] private float jumpCooldown = 0.5f;

        [Header("Look Variables")]
        [SerializeField] private float sensX = 1f;
        [SerializeField] private float sensY = 1f;
        [SerializeField] private float lookLimit = 90f;
        private float xRotation;
        private float yRotation;

        [Header("External Components")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private CapsuleCollider capsuleCollider;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform orientation;
        [SerializeField] private LayerMask groundMask;

        // Movement State
        public enum MovementState { walking, crouching, sprinting, airborne, diving, sliding, mantling }
        [Header("Movement State")]
        [SerializeField]private MovementState movementState = MovementState.walking;

        //Internal Variables
        private float moveSpeed;
        private Vector3 moveDirection;

        //Internal Movement Variables
        float verticalInput;
        float horizontalInput;

        //Internal Checks
        private bool canJump;
        private bool canCrouch;
        private bool canSprint;
        private bool canDive;
        private bool canSlide;
        private bool canMantle;
        private bool canMouseLook;
        private bool isGrounded;

        // Methods
        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            canJump = true;
        }
        private void HandleJump()
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            movementState = MovementState.airborne;
        }
        private void ResetJump()
        {
            canJump = true;
        }

        private void HandleMouseInput()
        {
            float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
            float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

            yRotation += mouseX;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -lookLimit, lookLimit);

            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
            orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }

        private void HandleMovementInput()
        {
            verticalInput = Input.GetAxisRaw("Vertical");
            horizontalInput = Input.GetAxisRaw("Horizontal");
        }

        private void ApplyFinalMovements()
        {
            moveDirection = (orientation.forward * verticalInput + orientation.right * horizontalInput);

            // Get the current horizontal velocity (only x and z components)
            Vector3 flatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // Preserve current speed magnitude but apply it in the new direction
            float currentSpeed = flatVelocity.magnitude;

            // Ensure the player builds up to their target speed
            if (isGrounded)
            {
                if (currentSpeed < moveSpeed)
                {
                    rb.AddForce(moveDirection * moveSpeed, ForceMode.Acceleration); // Gradually build up speed
                }
            }
            else
            {
                if (currentSpeed < moveSpeed * airMultiplier)
                {
                    rb.AddForce(moveDirection * moveSpeed * airMultiplier, ForceMode.Acceleration); // Build up speed in air
                }
            }

            // Apply direction change instantly without affecting current speed
            if (flatVelocity.magnitude > 0.1f) // If there is significant velocity, preserve it
            {
                rb.velocity = moveDirection * flatVelocity.magnitude + new Vector3(0f, rb.velocity.y, 0f);
            }
        }


        private void Update()
        {
            isGrounded = IsGrounded();
            if (!isGrounded && movementState != MovementState.airborne)
            {
                movementState = MovementState.airborne;
                ChangeState();
            }
            else
            {
                movementState = MovementState.walking;
            }

            if (Input.GetKey(KeyCode.Space) && canJump && isGrounded)
            {
                canJump = false;

                HandleJump();
                Invoke(nameof(ResetJump), jumpCooldown);
            }

            HandleMouseInput();
            HandleMovementInput();
            SpeedControl();
        }

        private void FixedUpdate()
        {
            ApplyFinalMovements();
        }

        //Helpers
        private void SpeedControl()
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if(flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }

        private void ChangeState()
        {
            switch (movementState)
            {
                case MovementState.walking:
                    moveSpeed = walkSpeed;
                    rb.drag = groundDrag;
                    break;
                case MovementState.crouching:
                    moveSpeed = crouchSpeed;
                    break;
                case MovementState.sprinting:
                    moveSpeed = sprintSpeed;
                    break;
                case MovementState.airborne:
                    moveSpeed = sprintSpeed;
                    rb.drag = airDrag;
                    break;
                case MovementState.diving:
                    moveSpeed = sprintSpeed;
                    break;
                case MovementState.sliding:
                    moveSpeed = sprintSpeed;
                    break;
                case MovementState.mantling:
                    moveSpeed = 0f;
                    break;
                default:
                    moveSpeed = walkSpeed;
                    break;
            }
        }
        private bool IsGrounded()
        {
            return Physics.Raycast(transform.position, Vector3.down, capsuleCollider.height / 2f + 0.2f, groundMask);
        }
        public MovementState CurrentMovementState { get { return movementState; } }
    }
}
