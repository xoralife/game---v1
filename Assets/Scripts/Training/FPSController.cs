using UnityEngine;
using FPSTrainingRoom.Weapons;

namespace FPSTrainingRoom.Training
{
    [RequireComponent(typeof(CharacterController))]
    public class FPSController : MonoBehaviour
    {
        [Header("Movement")]
        public float walkSpeed = 5f;
        public float sprintSpeed = 7.5f;
        public float crouchSpeed = 2.5f;
        public float acceleration = 10f;
        public float jumpHeight = 2f;
        public float gravity = -20f;

        [Header("Look")]
        public float mouseSensitivity = 2f;
        public float verticalLookLimit = 80f;
        public bool invertY = false;

        [Header("Crouch")]
        public float crouchHeight = 1f;
        public float standingHeight = 2f;
        public float crouchTransitionSpeed = 10f;

        [Header("Audio")]
        public AudioSource footstepAudio;
        public AudioClip[] footstepClips;
        public float footstepInterval = 0.5f;

        protected CharacterController characterController;
        protected Camera playerCamera;
        protected Vector3 moveDirection;
        protected Vector3 velocity;
        protected float currentSpeed;
        protected float verticalRotation;
        protected bool isCrouching;
        protected float footstepTimer;
        protected bool isGrounded;

        public System.Action OnJump;
        public System.Action OnCrouch;
        public System.Action OnStand;

        public bool IsSprinting { get; protected set; }
        public bool IsMoving { get; protected set; }

        protected virtual void Start()
        {
            characterController = GetComponent<CharacterController>();
            playerCamera = GetComponentInChildren<Camera>();

            if (playerCamera == null)
                playerCamera = Camera.main;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        protected virtual void Update()
        {
            HandleLook();
            HandleMovement();
            HandleJump();
            HandleCrouch();
            HandleSprint();
            HandleFootsteps();
        }

        protected virtual void HandleLook()
        {
            if (playerCamera == null) return;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * (invertY ? -1f : 1f);

            transform.Rotate(Vector3.up, mouseX);

            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -verticalLookLimit, verticalLookLimit);

            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }

        protected virtual void HandleMovement()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            IsMoving = Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f;

            Vector3 forward = transform.forward * vertical;
            Vector3 right = transform.right * horizontal;
            moveDirection = (forward + right).normalized;

            float targetSpeed = walkSpeed;
            if (IsSprinting && IsMoving)
                targetSpeed = sprintSpeed;
            if (isCrouching)
                targetSpeed = crouchSpeed;

            currentSpeed = Mathf.Lerp(currentSpeed,
                targetSpeed * (IsMoving ? 1f : 0f),
                Time.deltaTime * acceleration);

            characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
        }

        protected virtual void HandleJump()
        {
            isGrounded = characterController.isGrounded;

            if (isGrounded && velocity.y < 0f)
                velocity.y = -2f;

            if (isGrounded && Input.GetButtonDown("Jump"))
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                OnJump?.Invoke();
            }

            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }

        protected virtual void HandleCrouch()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                isCrouching = !isCrouching;
                if (isCrouching)
                {
                    characterController.height = crouchHeight;
                    OnCrouch?.Invoke();
                }
                else
                {
                    characterController.height = standingHeight;
                    OnStand?.Invoke();
                }
            }
        }

        protected virtual void HandleSprint()
        {
            IsSprinting = Input.GetKey(KeyCode.LeftShift) && IsMoving && !isCrouching;
        }

        protected virtual void HandleFootsteps()
        {
            if (!IsMoving || !isGrounded)
            {
                footstepTimer = 0f;
                return;
            }

            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f && footstepAudio != null && footstepClips.Length > 0)
            {
                footstepAudio.PlayOneShot(
                    footstepClips[Random.Range(0, footstepClips.Length)]);
                footstepTimer = footstepInterval;
            }
        }

        protected virtual void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
