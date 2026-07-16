using UnityEngine;

namespace WaveSurvival
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health")]
        public float maxHealth = 100f;
        public float currentHealth;

        [Header("Regen")]
        public bool enableRegen = true;
        public float regenDelay = 3f;
        public float regenRate = 10f;

        [Header("Damage")]
        public float invulnerabilityTime = 0.5f;

        [Header("Audio")]
        public AudioClip hurtSound;
        public AudioClip deathSound;

        AudioSource audioSource;
        float invulnTimer;
        float regenTimer;
        bool lowHpWarning;
        float damageFlashTimer;

        public System.Action OnDamaged;
        public System.Action OnDeath;
        public System.Action<float> OnHealthChanged;
        public System.Action<bool> OnLowHPWarning;

        public float HealthPercent => currentHealth / maxHealth;
        public bool IsLowHP => currentHealth <= maxHealth * 0.3f;

        void Start()
        {
            currentHealth = maxHealth;
            audioSource = GetComponent<AudioSource>();
        }

        void Update()
        {
            if (invulnTimer > 0f)
                invulnTimer -= Time.deltaTime;

            if (damageFlashTimer > 0f)
                damageFlashTimer -= Time.deltaTime;

            // Health regen
            if (enableRegen && currentHealth < maxHealth && currentHealth > 0f)
            {
                regenTimer -= Time.deltaTime;
                if (regenTimer <= 0f)
                {
                    currentHealth = Mathf.Min(currentHealth + regenRate * Time.deltaTime, maxHealth);
                    OnHealthChanged?.Invoke(HealthPercent);
                }
            }

            // Low HP warning
            if (IsLowHP && !lowHpWarning)
            {
                lowHpWarning = true;
                OnLowHPWarning?.Invoke(true);
            }
            else if (!IsLowHP && lowHpWarning)
            {
                lowHpWarning = false;
                OnLowHPWarning?.Invoke(false);
            }
        }

        public void TakeDamage(float damage)
        {
            if (invulnTimer > 0f || currentHealth <= 0f)
                return;

            currentHealth -= damage;
            invulnTimer = invulnerabilityTime;
            regenTimer = regenDelay;
            damageFlashTimer = 0.15f;

            OnDamaged?.Invoke();
            OnHealthChanged?.Invoke(HealthPercent);

            if (hurtSound != null && audioSource != null)
                audioSource.PlayOneShot(hurtSound);

            if (currentHealth <= 0f)
                Die();
        }

        void Die()
        {
            OnDeath?.Invoke();
            if (deathSound != null && audioSource != null)
                audioSource.PlayOneShot(deathSound);

            if (GameManager.Instance != null)
                GameManager.Instance.GameOver();
        }

        public void Heal(float amount)
        {
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            OnHealthChanged?.Invoke(HealthPercent);
        }

        public void RefillHealth()
        {
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(HealthPercent);
        }
    }
}
