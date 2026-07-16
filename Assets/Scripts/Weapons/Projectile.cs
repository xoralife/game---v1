using UnityEngine;

namespace FPSTrainingRoom.Weapons
{
    public enum ProjectileType { Raycast, Physics, HeatSeeking }

    public class Projectile : MonoBehaviour
    {
        [Header("Projectile Settings")]
        public ProjectileType projectileType = ProjectileType.Raycast;
        public float speed = 500f;
        public float damage = 35f;
        public float maxLifetime = 3f;
        public bool useGravity = false;

        [Header("Physics Bullet")]
        public float bulletMass = 0.02f;
        public float dragCoefficient = 0.001f;

        [Header("Spread")]
        public float baseSpread = 0f;
        public float spreadMultiplier = 1f;

        protected Vector3 direction;
        protected float currentDamage;
        protected float currentSpeed;
        protected float lifeTimer;
        protected Rigidbody rb;
        protected Vector3 startPosition;

        public System.Action<Vector3, Vector3> OnHit;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody>();
            startPosition = transform.position;
        }

        public virtual void Initialize(Vector3 fireDirection, float damageModifier = 1f,
            float spreadModifier = 1f)
        {
            direction = fireDirection.normalized;
            currentDamage = damage * damageModifier;
            currentSpeed = speed;
            lifeTimer = 0f;

            float spreadAngle = baseSpread * spreadMultiplier * spreadModifier;
            if (spreadAngle > 0.001f)
            {
                float angleX = Random.Range(-spreadAngle, spreadAngle);
                float angleY = Random.Range(-spreadAngle, spreadAngle);
                direction = Quaternion.Euler(angleY, angleX, 0f) * direction;
            }

            if (projectileType == ProjectileType.Physics && rb != null)
            {
                rb.mass = bulletMass;
                rb.drag = dragCoefficient;
                rb.useGravity = useGravity;
                rb.linearVelocity = direction * currentSpeed;
            }
        }

        protected virtual void Update()
        {
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= maxLifetime)
            {
                Destroy(gameObject);
                return;
            }

            if (projectileType == ProjectileType.Raycast)
            {
                float distance = currentSpeed * Time.deltaTime;
                if (Physics.Raycast(transform.position, direction, out RaycastHit hit, distance))
                {
                    HandleHit(hit.point, hit.normal, hit.collider);
                    return;
                }
                transform.position += direction * distance;
            }
            else if (projectileType == ProjectileType.Physics && rb != null)
            {
                direction = rb.linearVelocity.normalized;
                currentSpeed = rb.linearVelocity.magnitude;
            }
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (projectileType != ProjectileType.Raycast)
            {
                HandleHit(collision.contacts[0].point,
                    collision.contacts[0].normal,
                    collision.collider);
            }
        }

        protected virtual void HandleHit(Vector3 point, Vector3 normal, Collider collider)
        {
            OnHit?.Invoke(point, normal);

            var dummy = collider.GetComponent<Training.TargetDummy>();
            if (dummy != null)
                dummy.OnHit(currentDamage, point, direction);

            Destroy(gameObject);
        }
    }
}
