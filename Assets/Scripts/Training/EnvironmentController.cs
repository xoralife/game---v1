using UnityEngine;

namespace FPSTrainingRoom.Training
{
    public class EnvironmentController : MonoBehaviour
    {
        [Header("Lighting")]
        public Light mainLight;
        public float defaultIntensity = 1f;
        public Color defaultColor = Color.white;
        public Gradient timeOfDayGradient;

        [Header("Time of Day")]
        public bool simulateTimeOfDay = false;
        public float timeSpeed = 0.1f;
        [Range(0f, 24f)]
        public float currentTime = 12f;
        public AnimationCurve lightIntensityCurve = AnimationCurve.EaseInOut(6f, 0f, 12f, 1f);

        [Header("Fog")]
        public bool enableFog = false;
        public Color fogColor = Color.gray;
        public float fogDensity = 0.01f;

        [Header("Weather")]
        public bool enableRain = false;
        public ParticleSystem rainParticle;
        public float rainIntensity = 100f;

        [Header("Soundscape")]
        public AudioSource ambientAudio;
        public AudioClip[] ambientClips;
        public float ambientVolume = 0.5f;

        protected float originalTimeScale;

        protected virtual void Start()
        {
            originalTimeScale = Time.timeScale;

            if (mainLight == null)
                mainLight = FindFirstObjectByType<Light>();

            if (enableFog)
            {
                RenderSettings.fog = true;
                RenderSettings.fogColor = fogColor;
                RenderSettings.fogDensity = fogDensity;
            }
            else
            {
                RenderSettings.fog = false;
            }

            if (rainParticle != null)
                rainParticle.gameObject.SetActive(enableRain);
        }

        protected virtual void Update()
        {
            if (simulateTimeOfDay)
            {
                currentTime += Time.deltaTime * timeSpeed;
                if (currentTime >= 24f) currentTime = 0f;

                UpdateLighting();
            }

            if (ambientAudio != null && !ambientAudio.isPlaying && ambientClips.Length > 0)
            {
                ambientAudio.clip = ambientClips[Random.Range(0, ambientClips.Length)];
                ambientAudio.volume = ambientVolume;
                ambientAudio.Play();
            }
        }

        protected virtual void UpdateLighting()
        {
            if (mainLight == null) return;

            float intensity = lightIntensityCurve.Evaluate(currentTime);
            mainLight.intensity = Mathf.Lerp(0.1f, defaultIntensity, intensity);

            if (timeOfDayGradient != null)
                mainLight.color = timeOfDayGradient.Evaluate(currentTime / 24f);
            else
                mainLight.color = Color.Lerp(
                    new Color(0.2f, 0.2f, 0.5f),
                    defaultColor,
                    intensity);

            RenderSettings.fogDensity = Mathf.Lerp(0.05f, 0f, intensity);
        }

        public virtual void SetTimeOfDay(float hour)
        {
            currentTime = Mathf.Clamp(hour, 0f, 24f);
        }

        public virtual void SetTimeSpeed(float speed)
        {
            timeSpeed = speed;
        }

        public virtual void ToggleRain()
        {
            enableRain = !enableRain;
            if (rainParticle != null)
            {
                rainParticle.gameObject.SetActive(enableRain);
                if (enableRain)
                {
                    var emission = rainParticle.emission;
                    emission.rateOverTime = rainIntensity;
                }
            }
        }

        public virtual void SetRainIntensity(float intensity)
        {
            rainIntensity = intensity;
            if (rainParticle != null && enableRain)
            {
                var emission = rainParticle.emission;
                emission.rateOverTime = intensity;
            }
        }

        public virtual void ToggleFog()
        {
            enableFog = !enableFog;
            RenderSettings.fog = enableFog;
        }

        public virtual void SetFogDensity(float density)
        {
            fogDensity = density;
            RenderSettings.fogDensity = density;
        }
    }
}
