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

        CharacterController cc;
        Camera cam;
        Vector3 velocity;
        float verticalRotation;
        bool isGrounded;

        void Start()
        {
            cc = GetComponent<CharacterController>();
            cam = GetComponentInChildren<Camera>();
            if (cam == null)
                cam = Camera.main;

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

        void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
