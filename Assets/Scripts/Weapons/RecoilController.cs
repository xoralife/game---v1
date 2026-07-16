using UnityEngine;

namespace FPSTrainingRoom.Weapons
{
    public class RecoilController : MonoBehaviour
    {
        [Header("Recoil Settings")]
        public float recoilRecoverySpeed = 8f;
        public float recoilRotationSpeed = 6f;
        public bool useCompensation = false;

        [Header("Current State")]
        public Vector2 currentRecoil;
        public float currentSpread;
        public Vector2 accumulatedRecoil;
        public bool isFiring;

        protected Camera playerCamera;
        protected Vector3 targetRotation;
        protected Vector3 currentRotation;

        protected virtual void Start()
        {
            playerCamera = GetComponentInParent<Camera>();
            if (playerCamera == null)
                playerCamera = Camera.main;
        }

        protected virtual void Update()
        {
            if (!isFiring)
            {
                currentRecoil = Vector2.Lerp(currentRecoil, Vector2.zero,
                    Time.deltaTime * recoilRecoverySpeed);
                accumulatedRecoil = Vector2.Lerp(accumulatedRecoil, Vector2.zero,
                    Time.deltaTime * recoilRecoverySpeed * 0.5f);
                currentSpread = Mathf.Lerp(currentSpread, 0f,
                    Time.deltaTime * recoilRecoverySpeed);
            }

            if (playerCamera != null)
            {
                targetRotation = new Vector3(
                    -accumulatedRecoil.y * 0.5f,
                    accumulatedRecoil.x * 0.5f,
                    0f
                );

                currentRotation = Vector3.Lerp(currentRotation, targetRotation,
                    Time.deltaTime * recoilRotationSpeed);

                if (useCompensation)
                {
                    Vector3 compensation = new Vector3(
                        accumulatedRecoil.y * 0.4f,
                        -accumulatedRecoil.x * 0.4f,
                        0f
                    );
                    currentRotation = Vector3.Lerp(currentRotation,
                        currentRotation + compensation,
                        Time.deltaTime * recoilRotationSpeed);
                }

                playerCamera.transform.localRotation = Quaternion.Euler(currentRotation);
            }
        }

        public virtual void ApplyRecoil(Vector2 patternOffset, float spread)
        {
            isFiring = true;
            currentRecoil = patternOffset;
            currentSpread = spread;
            accumulatedRecoil += patternOffset * 0.15f;
            accumulatedRecoil.x = Mathf.Clamp(accumulatedRecoil.x, -5f, 5f);
            accumulatedRecoil.y = Mathf.Clamp(accumulatedRecoil.y, -5f, 5f);

            if (useCompensation)
            {
                Vector2 compensation = new Vector2(
                    -patternOffset.x * 0.6f,
                    -patternOffset.y * 0.6f
                );
                currentRecoil += compensation;
                accumulatedRecoil += compensation * 0.3f;
            }
        }

        public virtual void SetNotFiring()
        {
            isFiring = false;
        }

        public virtual void ResetRecoil()
        {
            currentRecoil = Vector2.zero;
            accumulatedRecoil = Vector2.zero;
            currentSpread = 0f;
            currentRotation = Vector3.zero;
            targetRotation = Vector3.zero;
        }
    }
}
