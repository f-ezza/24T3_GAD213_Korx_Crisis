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

        [Header("Crouching")]
        [SerializeField] private float crouchYScale;
        [SerializeField] private float startYScale;

        [Header("Sliding")]
        [SerializeField] private float maxDiveTime;
        [SerializeField] private float diveYScale;
        private float diveTimer;

        [Header("Sliding")]
        [SerializeField] private float maxSlideTime;
        [SerializeField] private float slideYScale;
        private float slideTimer;

        [Header("Jump Cooldown")]
        [SerializeField] private float jumpCooldown = 0.5f;

        [Header("Mantling System")]
        [SerializeField] private float mantleMaxHeight = 1.5f;
        [SerializeField] private float mantleReachDistance = 1f;
        [SerializeField] private float mantleSpeed = 5f;
        [SerializeField] private LayerMask mantleableLayers;

        private bool isMantling = false;
        private Vector3 mantleStartPos;
        private Vector3 mantleEndPos;

        [Header("Look Variables")]
        [SerializeField] private float sensX = 1f;
        [SerializeField] private float sensY = 1f;
        [SerializeField] private float lookLimit = 90f;
        private float xRotation;
        private float yRotation;

        [Header("Slope Movement")]
        [SerializeField] private float maxSlopeAngle = 45f;
        RaycastHit slopeHit;
        private bool exitingSlope;

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
            rb = GetComponent<Rigidbody>();
            capsuleCollider = GetComponent<CapsuleCollider>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            canJump = true;
            startYScale = transform.localScale.y;
        }

        private void HandleJump()
        {
            exitingSlope = true;

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
        private void ResetJump()
        {
            canJump = true;
            exitingSlope = false;
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

            if (Input.GetKey(KeyCode.Space) && canJump && allowJump && isGrounded)
            {
                if (!TryMantle())
                {
                    canJump = false;
                    HandleJump();
                    Invoke(nameof(ResetJump), jumpCooldown);
                }
            }

            if (Input.GetKey(KeyCode.LeftShift) && allowSprint && isGrounded && movementState != MovementState.crouching)
            {
                movementState = MovementState.sprinting;
            }

            if (Input.GetKeyDown(KeyCode.LeftControl) && allowCrouch && isGrounded)
            {
                Crouch();
            }
            else if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                StandUp();
            }

            if ((Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftAlt)) && allowSlide && isGrounded && movementState == MovementState.sprinting)
            {
                StartSlide();
            }
        }

        private void ApplyFinalMovements()
        {
            moveDirection = (orientation.forward * verticalInput + orientation.right * horizontalInput);

            if (OnSlope() && !exitingSlope)
            {
                rb.AddForce(GetSlopeMoveDirection() * moveSpeed, ForceMode.Force);

                if (rb.velocity.y > 0f)
                {
                    rb.AddForce(Vector3.down * 5f, ForceMode.Force);
                }
            }
            else if (isGrounded)
            {
                rb.AddForce(moveDirection * moveSpeed, ForceMode.Force);
            }
            else if (!isGrounded)
            {
                rb.AddForce(moveDirection * moveSpeed * airMultiplier, ForceMode.Force);
            }

            rb.useGravity = !OnSlope();
        }


        private void Update()
        {
            switch (movementState)
            {
                case MovementState.walking:
                    moveSpeed = walkSpeed;
                    rb.drag = groundDrag;
                    break;
                case MovementState.crouching:
                    moveSpeed = crouchSpeed;
                    rb.drag = groundDrag;
                    break;
                case MovementState.sprinting:
                    moveSpeed = sprintSpeed;
                    rb.drag = groundDrag;
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

            isGrounded = IsGrounded();
            if (!isGrounded && movementState != MovementState.airborne)
            {
                movementState = MovementState.airborne;
            }
            else if (rb.velocity.magnitude > 0f && movementState != MovementState.crouching && movementState != MovementState.sprinting)
            {
                movementState = MovementState.walking;
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
        private bool TryMantle()
        {
            Vector3 rayStart = orientation.transform.position;
            Vector3 rayDirection = playerCamera.transform.forward;

            Debug.DrawRay(rayStart, rayDirection * mantleReachDistance, Color.red, 1f);

            if (Physics.Raycast(rayStart, rayDirection, out RaycastHit hit, mantleReachDistance, mantleableLayers))
            {
                float obstacleHeight = hit.point.y - transform.position.y;

                if (obstacleHeight > 0 && obstacleHeight <= mantleMaxHeight)
                {
                    StartCoroutine(Mantle(hit));
                    return true;
                }
            }

            return false;
        }
        private void SpeedControl()
        {
            if (OnSlope() && !exitingSlope)
            {
                if (rb.velocity.magnitude > moveSpeed)
                    rb.velocity = rb.velocity.normalized * moveSpeed;
            }
            else
            {
                Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                if (flatVel.magnitude > moveSpeed)
                {
                    Vector3 limitedVel = flatVel.normalized * moveSpeed;
                    rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
                }
            }
        }

        private bool IsGrounded()
        {
            return Physics.Raycast(transform.position, Vector3.down, capsuleCollider.height / 2f + 0.2f, groundMask);
        }

        private bool OnSlope()
        {
            if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, capsuleCollider.height / 2f + 0.3f, groundMask))
            {
                float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
                return angle < maxSlopeAngle && angle != 0;
            }

            return false;
        }

        private IEnumerator Slide()
        {
            movementState = MovementState.sliding;
            slideTimer = maxSlideTime;

            transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);
            rb.AddForce(orientation.forward * slideForce, ForceMode.Impulse);

            while (slideTimer > 0)
            {
                slideTimer -= Time.deltaTime;
                yield return null;
            }

            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            movementState = MovementState.walking;
        }

        private IEnumerator Dive()
        {
            movementState = MovementState.diving;
            diveTimer = maxDiveTime;

            transform.localScale = new Vector3(transform.localScale.x, diveYScale, transform.localScale.z);
            rb.AddForce(orientation.forward * diveForce, ForceMode.Impulse);

            while (diveTimer > 0)
            {
                diveTimer -= Time.deltaTime;
                yield return null;
            }

            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            movementState = MovementState.walking;
        }

        void Crouch()
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            movementState = MovementState.crouching;
        }

        void StandUp()
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            movementState = MovementState.walking;
        }

        void StartSlide()
        {
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            StartCoroutine(Slide());
        }

        private IEnumerator Mantle(RaycastHit hit)
        {
            isMantling = true;
            movementState = MovementState.mantling;
            rb.isKinematic = true;

            mantleStartPos = transform.position;
            mantleEndPos = new Vector3(hit.point.x, hit.point.y + 1f, hit.point.z);

            float elapsedTime = 0f;
            float mantleDuration = Vector3.Distance(mantleStartPos, mantleEndPos) / mantleSpeed;

            while (elapsedTime < mantleDuration)
            {
                transform.position = Vector3.Lerp(mantleStartPos, mantleEndPos, elapsedTime / mantleDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = mantleEndPos;

            rb.isKinematic = false;
            isMantling = false;
            movementState = MovementState.walking;
        }

        private Vector3 GetSlopeMoveDirection()
        {
            return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
        }

        public MovementState CurrentMovementState { get { return movementState; } }
    }
}
