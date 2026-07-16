using UnityEngine;

namespace FPSTrainingRoom.Weapons
{
    public class WeaponAnimationController : MonoBehaviour
    {
        [Header("Sway Settings")]
        public float swayAmount = 0.02f;
        public float swaySmoothing = 6f;
        public float maxSwayAmount = 0.06f;

        [Header("Bob Settings")]
        public float bobFrequency = 10f;
        public float bobAmplitude = 0.015f;
        public float bobSmoothing = 8f;

        [Header("ADS Settings")]
        public float adsFov = 50f;
        public float adsSpeed = 0.35f;
        public Vector3 adsPosition = new Vector3(0f, -0.02f, 0.05f);
        public Vector3 hipPosition = Vector3.zero;

        protected Vector3 initialPosition;
        protected Quaternion initialRotation;
        protected float adsLerp;
        protected bool isADS;
        protected float bobTimer;
        protected Vector3 swayPosition;
        protected Vector3 swayRotation;

        protected WeaponBase weapon;
        protected Camera playerCamera;

        protected virtual void Start()
        {
            initialPosition = transform.localPosition;
            initialRotation = transform.localRotation;
            weapon = GetComponent<WeaponBase>();
            playerCamera = GetComponentInParent<Camera>();
        }

        protected virtual void Update()
        {
            HandleSway();
            HandleBob();
            HandleADS();
        }

        protected virtual void HandleSway()
        {
            float mouseX = Input.GetAxis("Mouse X") * swayAmount;
            float mouseY = Input.GetAxis("Mouse Y") * swayAmount;

            mouseX = Mathf.Clamp(mouseX, -maxSwayAmount, maxSwayAmount);
            mouseY = Mathf.Clamp(mouseY, -maxSwayAmount, maxSwayAmount);

            Vector3 targetSwayPos = new Vector3(-mouseX, -mouseY, 0f);
            swayPosition = Vector3.Lerp(swayPosition, targetSwayPos,
                Time.deltaTime * swaySmoothing);

            Vector3 targetSwayRot = new Vector3(-mouseY * 2f, mouseX * 2f, mouseX);
            swayRotation = Vector3.Lerp(swayRotation, targetSwayRot,
                Time.deltaTime * swaySmoothing);

            transform.localPosition = initialPosition + swayPosition;
            transform.localRotation = initialRotation * Quaternion.Euler(swayRotation);
        }

        protected virtual void HandleBob()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            bool isMoving = Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f;

            if (isMoving)
            {
                bobTimer += Time.deltaTime * bobFrequency;
                float bobX = Mathf.Sin(bobTimer) * bobAmplitude;
                float bobY = Mathf.Sin(bobTimer * 2f) * bobAmplitude * 0.5f;
                transform.localPosition += new Vector3(bobX, bobY, 0f);
            }
            else
            {
                bobTimer = 0f;
            }
        }

        protected virtual void HandleADS()
        {
            if (Input.GetMouseButton(1))
            {
                isADS = true;
                adsLerp = Mathf.Clamp01(adsLerp + Time.deltaTime / adsSpeed);
            }
            else
            {
                isADS = false;
                adsLerp = Mathf.Clamp01(adsLerp - Time.deltaTime / adsSpeed);
            }

            transform.localPosition = Vector3.Lerp(
                hipPosition + swayPosition,
                adsPosition + swayPosition,
                adsLerp
            );

            if (playerCamera != null)
                playerCamera.fieldOfView = Mathf.Lerp(60f, adsFov, adsLerp);

            float fovMultiplier = Mathf.Lerp(1f, 0.7f, adsLerp);
            if (weapon != null && weapon.sprayPattern != null)
                weapon.sprayPattern.intensityMultiplier = fovMultiplier;
        }

        public virtual void PlayFireAnimation()
        {
            Vector3 kick = new Vector3(
                Random.Range(-0.002f, 0.002f),
                -0.005f,
                Random.Range(-0.01f, -0.005f)
            );
            transform.localPosition += kick;
        }

        public virtual void PlayReloadAnimation()
        {
        }
    }
}
