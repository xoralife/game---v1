using UnityEngine;

namespace WaveSurvival
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health")]
        public float maxHealth = 100f;
        public float currentHealth;

        [Header("Damage")]
        public float invulnerabilityTime = 1f;
        public Color damageFlashColor = Color.red;

        [Header("Audio")]
        public AudioClip hurtSound;
        public AudioClip deathSound;

        AudioSource audioSource;
        Camera cam;
        float invulnTimer;

        public System.Action OnDamaged;
        public System.Action OnDeath;

        void Start()
        {
            currentHealth = maxHealth;
            audioSource = GetComponent<AudioSource>();
            cam = GetComponentInChildren<Camera>();
        }

        void Update()
        {
            if (invulnTimer > 0f)
                invulnTimer -= Time.deltaTime;
        }

        public void TakeDamage(float damage)
        {
            if (invulnTimer > 0f || currentHealth <= 0f)
                return;

            currentHealth -= damage;
            invulnTimer = invulnerabilityTime;

            OnDamaged?.Invoke();

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
    }
}
