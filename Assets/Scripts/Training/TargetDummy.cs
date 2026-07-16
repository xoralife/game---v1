using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace FPSTrainingRoom.Training
{
    public class TargetDummy : MonoBehaviour
    {
        [Header("Health")]
        public float maxHealth = 100f;
        public float CurrentHealth { get; protected set; }
        public bool isInvincible = false;
        public bool autoReset = true;
        public float resetDelay = 3f;

        [Header("Hit Feedback")]
        public GameObject hitEffectPrefab;
        public GameObject destroyEffectPrefab;
        public TextMeshPro damageNumberPrefab;
        public Transform damageNumberSpawnPoint;

        [Header("Scoring")]
        public int scoreValue = 100;
        public int headshotBonus = 50;
        public float ThreatLevel { get; set; } = 1f;

        [Header("Events")]
        public UnityEvent OnHitEvent;
        public UnityEvent OnDestroyedEvent;
        public UnityEvent OnResetEvent;

        protected float resetTimer;
        protected bool isDestroyed;
        protected Collider targetCollider;
        protected Renderer targetRenderer;
        protected Color originalColor;
        protected MaterialPropertyBlock propBlock;

        public float LastHitTime { get; protected set; }
        public System.Action<float, Vector3, Vector3> OnHitCallback;

        protected virtual void Start()
        {
            CurrentHealth = maxHealth;
            targetCollider = GetComponent<Collider>();
            targetRenderer = GetComponent<Renderer>();
            propBlock = new MaterialPropertyBlock();

            if (targetRenderer != null)
                originalColor = targetRenderer.material.color;
        }

        protected virtual void Update()
        {
            if (isDestroyed && autoReset)
            {
                resetTimer -= Time.deltaTime;
                if (resetTimer <= 0f)
                    ResetTarget();
            }
        }

        public virtual void OnHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
        {
            if (isDestroyed) return;

            LastHitTime = Time.time;

            if (!isInvincible)
            {
                CurrentHealth -= damage;
                ThreatLevel += damage * 0.01f;
            }

            ShowHitFeedback(hitPoint, damage);
            OnHitEvent?.Invoke();
            OnHitCallback?.Invoke(damage, hitPoint, hitDirection);

            if (hitEffectPrefab != null)
                Instantiate(hitEffectPrefab, hitPoint, Quaternion.LookRotation(hitDirection));

            float shakeIntensity = Mathf.Clamp01(damage / maxHealth) * 0.2f;
            if (shakeIntensity > 0.01f)
            {
                Vector3 shake = Random.insideUnitSphere * shakeIntensity;
                transform.position += shake;
            }

            if (CurrentHealth <= 0f && !isInvincible)
                DestroyTarget();
        }

        protected virtual void ShowHitFeedback(Vector3 hitPoint, float damage)
        {
            if (damageNumberPrefab != null)
            {
                Vector3 spawnPos = damageNumberSpawnPoint != null
                    ? damageNumberSpawnPoint.position
                    : hitPoint + Vector3.up * 0.5f;

                var damageText = Instantiate(damageNumberPrefab, spawnPos, Quaternion.identity);
                damageText.text = Mathf.RoundToInt(damage).ToString();
                damageText.color = damage >= maxHealth * 0.5f ? Color.red : Color.yellow;
                damageText.fontSize = damage >= maxHealth * 0.5f ? 48f : 32f;

                Destroy(damageText.gameObject, 1f);
            }

            if (targetRenderer != null)
            {
                targetRenderer.GetPropertyBlock(propBlock);
                propBlock.SetColor("_Color", Color.red);
                targetRenderer.SetPropertyBlock(propBlock);
                Invoke(nameof(ResetColor), 0.1f);
            }
        }

        protected virtual void ResetColor()
        {
            if (targetRenderer != null)
            {
                targetRenderer.GetPropertyBlock(propBlock);
                propBlock.SetColor("_Color", originalColor);
                targetRenderer.SetPropertyBlock(propBlock);
            }
        }

        protected virtual void DestroyTarget()
        {
            isDestroyed = true;

            if (destroyEffectPrefab != null)
                Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);

            if (targetRenderer != null)
                targetRenderer.enabled = false;

            if (targetCollider != null)
                targetCollider.enabled = false;

            OnDestroyedEvent?.Invoke();

            if (autoReset)
                resetTimer = resetDelay;
        }

        public virtual void ResetTarget()
        {
            isDestroyed = false;
            CurrentHealth = maxHealth;
            ThreatLevel = 1f;

            if (targetRenderer != null)
                targetRenderer.enabled = true;

            if (targetCollider != null)
                targetCollider.enabled = true;

            ResetColor();
            OnResetEvent?.Invoke();
        }

        public virtual void SetThreatLevel(float threat)
        {
            ThreatLevel = threat;
        }

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.Lerp(Color.green, Color.red,
                1f - (CurrentHealth / Mathf.Max(maxHealth, 1f)));
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 1.5f, 0.2f);
        }
    }
}
