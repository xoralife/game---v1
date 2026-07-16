using UnityEngine;

namespace WaveSurvival.Enemies
{
    public enum EnemyType { Normal, Fast, Boss }

    public class Enemy : MonoBehaviour
    {
        [Header("Stats")]
        public EnemyType enemyType = EnemyType.Normal;
        public float maxHealth = 50f;
        public float currentHealth;
        public float moveSpeed = 4f;
        public float damage = 20f;
        public float attackRange = 2f;
        public float attackCooldown = 1f;
        public int scoreValue = 10;

        [Header("Visual")]
        public Renderer bodyRenderer;
        public Color originalColor = Color.red;
        public ParticleSystem deathParticles;

        [Header("Audio")]
        public AudioClip deathSound;
        public AudioClip hitSound;

        protected Transform player;
        protected float attackTimer;
        protected float hitFlashTimer;
        protected MaterialPropertyBlock propBlock;
        protected bool isDead;

        public System.Action OnDeath;

        void Start()
        {
            currentHealth = maxHealth;
            propBlock = new MaterialPropertyBlock();
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;

            ApplyTypeVisuals();
        }

        public void ApplyTypeVisuals()
        {
            if (bodyRenderer == null) return;

            switch (enemyType)
            {
                case EnemyType.Normal:
                    bodyRenderer.material.color = Color.red;
                    originalColor = Color.red;
                    break;
                case EnemyType.Fast:
                    bodyRenderer.material.color = Color.yellow;
                    originalColor = Color.yellow;
                    moveSpeed = 6f;
                    maxHealth = 30f;
                    currentHealth = 30f;
                    scoreValue = 15;
                    break;
                case EnemyType.Boss:
                    bodyRenderer.material.color = new Color(0.5f, 0f, 0.5f);
                    originalColor = new Color(0.5f, 0f, 0.5f);
                    moveSpeed = 3f;
                    maxHealth = 200f;
                    currentHealth = 200f;
                    damage = 40f;
                    scoreValue = 50;
                    transform.localScale = Vector3.one * 1.5f;
                    break;
            }
        }

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.isGameOver || isDead)
                return;

            if (attackTimer > 0f)
                attackTimer -= Time.deltaTime;

            if (hitFlashTimer > 0f)
                hitFlashTimer -= Time.deltaTime;
            else if (bodyRenderer != null && propBlock != null)
                ResetColor();

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
            if (isDead) return;

            currentHealth -= amount;
            HitFlash();

            if (hitSound != null)
                AudioSource.PlayClipAtPoint(hitSound, transform.position);

            if (currentHealth <= 0f)
                Die();
        }

        void HitFlash()
        {
            if (bodyRenderer == null || propBlock == null) return;

            bodyRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color", Color.white);
            bodyRenderer.SetPropertyBlock(propBlock);
            hitFlashTimer = 0.08f;
        }

        void ResetColor()
        {
            if (bodyRenderer == null || propBlock == null) return;
            bodyRenderer.GetPropertyBlock(propBlock);
            targetColor = originalColor;
        }

        Color targetColor;

        void OnWillRenderObject()
        {
            if (propBlock == null || bodyRenderer == null) return;
            bodyRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color", targetColor);
            bodyRenderer.SetPropertyBlock(propBlock);
        }

        protected virtual void Die()
        {
            isDead = true;

            // Death particles
            if (deathParticles != null)
            {
                var ps = Instantiate(deathParticles, transform.position, Quaternion.identity);
                ps.Play();
                Destroy(ps.gameObject, 2f);
            }
            else
            {
                // Fallback: small explosion of spheres
                for (int i = 0; i < 5; i++)
                {
                    GameObject frag = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    frag.transform.position = transform.position + Random.insideUnitSphere * 0.5f;
                    frag.transform.localScale = Vector3.one * 0.1f;
                    frag.GetComponent<Renderer>().material.color = originalColor;
                    Destroy(frag.GetComponent<SphereCollider>());
                    Rigidbody rb = frag.AddComponent<Rigidbody>();
                    rb.AddExplosionForce(200f, transform.position, 3f);
                    rb.linearDamping = 0.5f;
                    Destroy(frag, 1.5f);
                }
            }

            if (deathSound != null)
                AudioSource.PlayClipAtPoint(deathSound, transform.position);

            // Drop pickup
            if (PickupManager.Instance != null)
                PickupManager.Instance.SpawnPickup(transform.position);

            OnDeath?.Invoke();

            if (GameManager.Instance != null)
                GameManager.Instance.AddScore(scoreValue);

            Destroy(gameObject);
        }
    }
}
