using UnityEngine;

namespace FPSTrainingRoom.Weapons
{
    public class MuzzleFlash : MonoBehaviour
    {
        [Header("Flash Settings")]
        public Light flashLight;
        public GameObject flashMesh;
        public float flashDuration = 0.05f;
        public float lightIntensity = 3f;
        public AnimationCurve flashIntensityCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        [Header("Randomization")]
        public Vector2 flashScaleRange = new Vector2(0.5f, 1.5f);
        public Vector2 flashRotationRange = new Vector2(-180f, 180f);
        public bool randomizeColor = false;
        public Color[] flashColors = new Color[]
        {
            Color.white, new Color(1f, 0.8f, 0.4f), new Color(1f, 0.9f, 0.5f)
        };

        [Header("Smoke")]
        public ParticleSystem smokeParticle;
        public ParticleSystem shellEjectParticle;

        protected float flashTimer;
        protected bool isFlashing;
        protected Color currentColor;
        protected Quaternion originalRotation;

        protected virtual void Start()
        {
            originalRotation = transform.localRotation;

            if (flashMesh != null)
                flashMesh.SetActive(false);

            if (flashLight != null)
                flashLight.enabled = false;
        }

        protected virtual void Update()
        {
            if (isFlashing)
            {
                flashTimer -= Time.deltaTime;
                float t = 1f - (flashTimer / flashDuration);
                float intensity = flashIntensityCurve.Evaluate(t) * lightIntensity;

                if (flashLight != null)
                    flashLight.intensity = intensity;

                if (flashTimer <= 0f)
                    HideFlash();
            }
        }

        public virtual void ShowFlash()
        {
            isFlashing = true;
            flashTimer = flashDuration;

            Vector3 randomScale = Vector3.one * Random.Range(flashScaleRange.x, flashScaleRange.y);
            if (flashMesh != null)
            {
                flashMesh.SetActive(true);
                flashMesh.transform.localScale = randomScale;
                flashMesh.transform.localRotation = originalRotation *
                    Quaternion.Euler(0f, 0f, Random.Range(flashRotationRange.x, flashRotationRange.y));
            }

            if (flashLight != null)
            {
                flashLight.enabled = true;
                flashLight.intensity = lightIntensity;

                if (randomizeColor && flashColors.Length > 0)
                {
                    currentColor = flashColors[Random.Range(0, flashColors.Length)];
                    flashLight.color = currentColor;
                }
            }

            if (smokeParticle != null)
                smokeParticle.Play();

            if (shellEjectParticle != null)
                shellEjectParticle.Play();
        }

        protected virtual void HideFlash()
        {
            isFlashing = false;
            if (flashMesh != null)
                flashMesh.SetActive(false);
            if (flashLight != null)
                flashLight.enabled = false;
        }
    }
}
