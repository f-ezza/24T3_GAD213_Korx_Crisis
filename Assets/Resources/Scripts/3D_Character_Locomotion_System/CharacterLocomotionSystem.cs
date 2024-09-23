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
        [SerializeField] private float xSens = 1f;
        [SerializeField] private float ySens = 1f;
        [SerializeField] private float mouseX = 0f;
        [SerializeField] private float mouseY = 0f;
        [SerializeField] private float lookLimit = 75f;

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
        private Vector2 moveDirection;

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
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;


            mouseX += Input.GetAxis("Mouse X") * xSens;
            mouseY -= Input.GetAxis("Mouse Y") * ySens;
            mouseY = Mathf.Clamp(mouseY, -lookLimit, lookLimit);

            playerCamera.transform.localRotation = Quaternion.Euler(mouseY, mouseX, 0f);
            orientation.rotation = Quaternion.Euler(0f, mouseX, 0f);
        }

        private void HandleMovementInput()
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            moveDirection = new Vector2(horizontalInput, verticalInput).normalized;
        }

        private void ApplyFinalMovements()
        {

        }


        private void Update()
        {

        }

        //Helpers
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
