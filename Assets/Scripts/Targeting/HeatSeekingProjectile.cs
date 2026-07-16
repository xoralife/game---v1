using UnityEngine;

namespace FPSTrainingRoom.Targeting
{
    public class HeatSeekingProjectile : MonoBehaviour
    {
        [Header("Homing Settings")]
        public float turnSpeed = 180f;
        public float acceleration = 50f;
        public float maxSpeed = 300f;
        public float lifetime = 5f;

        [Header("Lock-On")]
        public float lockOnRange = 50f;
        public float lockOnAngle = 30f;
        public LayerMask targetLayers = -1;
        public LayerMask obstacleMask = -1;
        public bool requireLineOfSight = true;

        [Header("Lead Prediction")]
        public bool enableLeadPrediction = true;
        public float predictionFactor = 0.5f;
        public AnimationCurve predictionCurve = AnimationCurve.Linear(0f, 1f, 1f, 0.5f);

        [Header("Proximity Detection")]
        public float proximityDetonationRadius = 2f;
        public bool enableProximityDetonation = true;

        [Header("Visualization")]
        public TrailRenderer trailRenderer;
        public Light engineLight;
        public ParticleSystem engineParticle;

        protected Transform target;
        protected Rigidbody rb;
        protected float currentSpeed;
        protected float lifeTimer;
        protected bool hasDetonated;
        protected Vector3 lastTargetPosition;
        protected Vector3 predictedPosition;

        public System.Action<Vector3, Vector3> OnDetonate;

        public virtual void Initialize(Transform lockOnTarget)
        {
            target = lockOnTarget;
            rb = GetComponent<Rigidbody>();
            if (rb == null)
                rb = gameObject.AddComponent<Rigidbody>();

            rb.useGravity = false;
            rb.linearDamping = 0.5f;
            currentSpeed = maxSpeed * 0.3f;
            rb.linearVelocity = transform.forward * currentSpeed;

            if (target != null)
                lastTargetPosition = target.position;

            if (engineLight != null)
                engineLight.enabled = true;

            if (engineParticle != null)
                engineParticle.Play();
        }

        protected virtual void Update()
        {
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= lifetime || hasDetonated)
            {
                Detonate(transform.position, -transform.forward);
                return;
            }

            if (target == null)
                FindNewTarget();

            if (target != null)
            {
                UpdatePrediction();
                SteerTowardTarget();
            }

            UpdateSpeed();

            if (enableProximityDetonation && target != null)
            {
                float distance = Vector3.Distance(transform.position, target.position);
                if (distance <= proximityDetonationRadius)
                {
                    Detonate(target.position, transform.forward);
                    return;
                }
            }

            // Check for direct collision
            float checkDistance = currentSpeed * Time.deltaTime;
            if (Physics.Raycast(transform.position, transform.forward,
                out RaycastHit hit, checkDistance))
            {
                Detonate(hit.point, hit.normal);
                return;
            }

            if (trailRenderer != null && !trailRenderer.emitting)
                trailRenderer.emitting = true;
        }

        protected virtual void FixedUpdate()
        {
            if (rb != null && !hasDetonated)
            {
                rb.linearVelocity = transform.forward * currentSpeed;
            }
        }

        protected virtual void FindNewTarget()
        {
            float bestScore = float.MaxValue;
            Transform bestTarget = null;

            var allDummies = FindObjectsByType<TargetDummy>(
                FindObjectsSortMode.None);

            foreach (var dummy in allDummies)
            {
                if (!dummy.isActiveAndEnabled) continue;

                Vector3 direction = dummy.transform.position - transform.position;
                float distance = direction.magnitude;
                if (distance > lockOnRange) continue;

                float angle = Vector3.Angle(transform.forward, direction);
                if (angle > lockOnAngle) continue;

                if (requireLineOfSight)
                {
                    if (Physics.Raycast(transform.position, direction.normalized,
                        out RaycastHit hit, distance, obstacleMask))
                    {
                        if (!hit.collider.TryGetComponent<TargetDummy>(out _))
                            continue;
                    }
                }

                float score = angle * 2f + distance * 0.3f;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestTarget = dummy.transform;
                }
            }

            if (bestTarget != null)
                target = bestTarget;
        }

        protected virtual void UpdatePrediction()
        {
            if (target == null) return;

            Vector3 targetVelocity = (target.position - lastTargetPosition) / Time.deltaTime;
            lastTargetPosition = target.position;

            if (!enableLeadPrediction) return;

            float distance = Vector3.Distance(transform.position, target.position);
            float timeToTarget = distance / Mathf.Max(currentSpeed, 1f);

            float predictionWeight = predictionCurve.Evaluate(
                Mathf.Clamp01(timeToTarget / lifetime)) * predictionFactor;

            predictedPosition = target.position + targetVelocity * timeToTarget * predictionWeight;
        }

        protected virtual void SteerTowardTarget()
        {
            Vector3 targetPos = enableLeadPrediction ? predictedPosition : target.position;
            Vector3 directionToTarget = (targetPos - transform.position).normalized;

            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRotation, turnSpeed * Time.deltaTime
            );

            Debug.DrawRay(transform.position, directionToTarget * 10f, Color.red);
        }

        protected virtual void UpdateSpeed()
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed,
                acceleration * Time.deltaTime);
        }

        protected virtual void Detonate(Vector3 point, Vector3 normal)
        {
            if (hasDetonated) return;
            hasDetonated = true;

            OnDetonate?.Invoke(point, normal);

            if (engineLight != null)
                engineLight.enabled = false;

            if (engineParticle != null)
                engineParticle.Stop();

            Destroy(gameObject, 0.1f);
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (!hasDetonated)
            {
                Detonate(collision.contacts[0].point, collision.contacts[0].normal);
            }
        }

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, proximityDetonationRadius);

            if (target != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, target.position);

                if (enableLeadPrediction)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(predictedPosition, 0.3f);
                }
            }
        }
    }
}
