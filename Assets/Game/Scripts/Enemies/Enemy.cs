using UnityEngine;
using WaveSurvival.Player;

namespace WaveSurvival.Enemies
{
    public class Enemy : MonoBehaviour
    {
        [Header("Stats")]
        public float maxHealth = 100f;
        public float currentHealth;
        public float moveSpeed = 4f;
        public float damage = 20f;
        public float attackRange = 2f;
        public float attackCooldown = 1f;

        [Header("Effects")]
        public GameObject deathEffect;
        public AudioClip deathSound;

        protected Transform player;
        protected float attackTimer;

        public System.Action OnDeath;

        void Start()
        {
            currentHealth = maxHealth;
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.isGameOver)
                return;

            if (attackTimer > 0f)
                attackTimer -= Time.deltaTime;

            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                    player = playerObj.transform;
                return;
            }

            float dist = Vector3.Distance(transform.position, player.position);

            if (dist > attackRange)
            {
                transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
                transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            }
            else if (attackTimer <= 0f)
            {
                Attack();
            }
        }

        protected virtual void Attack()
        {
            attackTimer = attackCooldown;
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null)
                ph.TakeDamage(damage);
        }

        public void TakeDamage(float amount)
        {
            currentHealth -= amount;
            if (currentHealth <= 0f)
                Die();
        }

        protected virtual void Die()
        {
            if (deathEffect != null)
                Instantiate(deathEffect, transform.position, Quaternion.identity);

            if (deathSound != null)
            {
                AudioSource.PlayClipAtPoint(deathSound, transform.position);
            }

            OnDeath?.Invoke();
            Destroy(gameObject);
        }
    }
}
