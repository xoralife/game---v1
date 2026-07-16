using UnityEngine;

namespace WaveSurvival
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float walkSpeed = 6f;
        public float sprintSpeed = 9f;
        public float jumpHeight = 2f;
        public float gravity = -20f;

        [Header("Look")]
        public float mouseSensitivity = 2f;
        public float verticalLookLimit = 80f;

        [Header("Head Bob")]
        public float bobFrequency = 10f;
        public float bobAmplitude = 0.04f;

        [Header("Footsteps")]
        public AudioClip[] footstepClips;
        public float footstepInterval = 0.45f;

        CharacterController cc;
        Camera cam;
        AudioSource audioSource;
        Vector3 velocity;
        float verticalRotation;
        bool isGrounded;
        float bobTimer;
        float footstepTimer;
        Vector3 cameraInitialPos;

        void Start()
        {
            cc = GetComponent<CharacterController>();
            cam = GetComponentInChildren<Camera>();
            if (cam == null)
                cam = Camera.main;

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            if (cam != null)
                cameraInitialPos = cam.transform.localPosition;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.isGameOver)
                return;

            Look();
            Move();
            Jump();
            HeadBob();
            Footsteps();
        }

        void Look()
        {
            float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
            float my = Input.GetAxis("Mouse Y") * mouseSensitivity;

            transform.Rotate(Vector3.up, mx);
            verticalRotation -= my;
            verticalRotation = Mathf.Clamp(verticalRotation, -verticalLookLimit, verticalLookLimit);

            if (cam != null)
                cam.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }

        void Move()
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            bool sprint = Input.GetKey(KeyCode.LeftShift);

            Vector3 move = (transform.right * h + transform.forward * v).normalized;
            float speed = sprint ? sprintSpeed : walkSpeed;
            cc.Move(move * speed * Time.deltaTime);
        }

        void Jump()
        {
            isGrounded = cc.isGrounded;
            if (isGrounded && velocity.y < 0)
                velocity.y = -2f;

            if (isGrounded && Input.GetButtonDown("Jump"))
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            velocity.y += gravity * Time.deltaTime;
            cc.Move(velocity * Time.deltaTime);
        }

        void HeadBob()
        {
            if (cam == null) return;

            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            bool isMoving = Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;

            if (isMoving && isGrounded)
            {
                bobTimer += Time.deltaTime * bobFrequency;
                float x = Mathf.Sin(bobTimer) * bobAmplitude;
                float y = Mathf.Sin(bobTimer * 2f) * bobAmplitude * 0.5f;
                cam.transform.localPosition = cameraInitialPos + new Vector3(x, y, 0f);
            }
            else
            {
                bobTimer = 0f;
                cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, cameraInitialPos, Time.deltaTime * 6f);
            }
        }

        void Footsteps()
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            bool isMoving = Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;

            if (isMoving && isGrounded)
            {
                footstepTimer -= Time.deltaTime;
                if (footstepTimer <= 0f && footstepClips.Length > 0)
                {
                    audioSource.PlayOneShot(footstepClips[Random.Range(0, footstepClips.Length)]);
                    footstepTimer = footstepInterval;
                }
            }
            else
            {
                footstepTimer = 0f;
            }
        }

        void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
