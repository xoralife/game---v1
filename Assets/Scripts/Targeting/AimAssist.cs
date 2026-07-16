using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace FPSTrainingRoom.Targeting
{
    public class AimAssist : MonoBehaviour
    {
        [Header("Detection")]
        public float detectionRadius = 15f;
        public float detectionAngle = 30f;
        public LayerMask targetLayers = -1;
        public LayerMask obstacleMask = -1;

        [Header("Sticky Crosshair")]
        public bool enableStickyCrosshair = true;
        public float stickyRadius = 5f;
        public float stickyStrength = 0.5f;
        public AnimationCurve stickyFalloff = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        [Header("Bullet Magnetism")]
        public bool enableBulletMagnetism = true;
        public float magnetismAngle = 5f;
        public float magnetismStrength = 0.7f;
        public float magnetismMaxDistance = 50f;

        [Header("Slowdown")]
        public bool enableAimSlowdown = true;
        public float slowdownRadius = 8f;
        [Range(0f, 1f)]
        public float slowdownMultiplier = 0.6f;

        [Header("Visualization")]
        public bool showDebugGizmos = false;

        protected List<TargetDummy> visibleTargets = new List<TargetDummy>();
        protected TargetDummy closestTarget;
        protected Camera playerCamera;

        public System.Action<TargetDummy> OnTargetAcquired;
        public System.Action OnTargetLost;

        public TargetDummy CurrentTarget => closestTarget;

        protected virtual void Start()
        {
            playerCamera = GetComponent<Camera>();
            if (playerCamera == null)
                playerCamera = Camera.main;
        }

        protected virtual void Update()
        {
            UpdateVisibleTargets();

            if (enableStickyCrosshair)
                ApplyStickyCrosshair();

            if (enableAimSlowdown)
                ApplyAimSlowdown();

            if (enableBulletMagnetism && Input.GetMouseButtonDown(0))
                ApplyBulletMagnetism();
        }

        protected virtual void UpdateVisibleTargets()
        {
            visibleTargets.Clear();

            var allTargets = FindObjectsByType<TargetDummy>(
                FindObjectsSortMode.None);

            foreach (var target in allTargets)
            {
                if (!target.isActiveAndEnabled) continue;

                Vector3 directionToTarget = target.transform.position - transform.position;
                float distance = directionToTarget.magnitude;
                if (distance > detectionRadius) continue;

                float angle = Vector3.Angle(playerCamera.transform.forward, directionToTarget);
                if (angle > detectionAngle) continue;

                if (Physics.Raycast(transform.position, directionToTarget.normalized,
                    out RaycastHit hit, distance, obstacleMask))
                {
                    if (!hit.collider.TryGetComponent<TargetDummy>(out _))
                        continue;
                }

                visibleTargets.Add(target);
            }

            TargetDummy previousClosest = closestTarget;

            closestTarget = visibleTargets
                .OrderBy(t => Vector3.Angle(
                    playerCamera.transform.forward,
                    t.transform.position - transform.position))
                .FirstOrDefault();

            if (closestTarget != previousClosest)
            {
                if (closestTarget != null)
                    OnTargetAcquired?.Invoke(closestTarget);
                else
                    OnTargetLost?.Invoke();
            }
        }

        protected virtual void ApplyStickyCrosshair()
        {
            if (closestTarget == null) return;

            Vector3 screenTarget = playerCamera.WorldToScreenPoint(
                closestTarget.transform.position);
            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

            float distance = Vector3.Distance(screenTarget, screenCenter);
            if (distance > stickyRadius) return;

            float t = distance / stickyRadius;
            float strength = stickyFalloff.Evaluate(1f - t) * stickyStrength;

            Vector3 pullDirection = (screenTarget - screenCenter).normalized;
            float pullAmount = strength * (1f - t);

            // Apply subtle mouse movement toward target (educational demo only)
            if (pullAmount > 0.01f)
            {
                float sensitivityX = 2f;
                float sensitivityY = 2f;
                float moveX = pullDirection.x * pullAmount * sensitivityX * Time.deltaTime;
                float moveY = pullDirection.y * pullAmount * sensitivityY * Time.deltaTime;

                // In a real training tool, this shows how aim assist works
                // without actually moving the mouse
                Debug.DrawRay(transform.position,
                    (closestTarget.transform.position - transform.position).normalized * pullAmount,
                    Color.cyan);
            }
        }

        protected virtual void ApplyAimSlowdown()
        {
            if (closestTarget == null) return;

            float distance = Vector3.Distance(transform.position,
                closestTarget.transform.position);
            if (distance <= slowdownRadius)
            {
                float t = distance / slowdownRadius;
                float slowdown = Mathf.Lerp(slowdownMultiplier, 1f, t);
            }
        }

        protected virtual void ApplyBulletMagnetism()
        {
            if (!enableBulletMagnetism || closestTarget == null) return;

            float distance = Vector3.Distance(transform.position,
                closestTarget.transform.position);
            if (distance > magnetismMaxDistance) return;

            float angle = Vector3.Angle(
                playerCamera.transform.forward,
                (closestTarget.transform.position - transform.position).normalized);

            if (angle <= magnetismAngle)
            {
                Debug.DrawLine(transform.position,
                    closestTarget.transform.position, Color.green, 1f);
            }
        }

        public virtual TargetDummy GetTarget()
        {
            return closestTarget;
        }

        public virtual Vector3? GetMagneticAimPoint(Vector3 originalDirection)
        {
            if (!enableBulletMagnetism || closestTarget == null) return null;

            float angle = Vector3.Angle(originalDirection,
                (closestTarget.transform.position - transform.position).normalized);

            if (angle <= magnetismAngle)
            {
                return Vector3.Lerp(
                    originalDirection * 100f,
                    closestTarget.transform.position,
                    magnetismStrength
                );
            }

            return null;
        }

        protected virtual void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            if (playerCamera != null)
            {
                Vector3 forward = playerCamera.transform.forward;
                Vector3 leftBoundary = Quaternion.Euler(0f, -detectionAngle, 0f) * forward;
                Vector3 rightBoundary = Quaternion.Euler(0f, detectionAngle, 0f) * forward;

                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(transform.position, leftBoundary * detectionRadius);
                Gizmos.DrawRay(transform.position, rightBoundary * detectionRadius);

                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, forward * detectionRadius);
            }

            if (closestTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, closestTarget.transform.position);
            }
        }
    }
}
