using UnityEngine;

namespace FPSTrainingRoom.Targeting
{
    public class BulletMagnetism : MonoBehaviour
    {
        [Header("Magnetism Settings")]
        public bool enableMagnetism = true;
        public float magnetismRadius = 3f;
        public float magnetismStrength = 0.6f;
        public float maxDeflectionAngle = 10f;
        public float magnetEffectiveRange = 50f;

        [Header("Priority")]
        public bool preferHeadshots = false;
        public float headshotBias = 0.3f;

        [Header("Visualization")]
        public bool showDebugRays = false;

        protected Camera playerCamera;
        protected AimAssist aimAssist;

        protected virtual void Start()
        {
            playerCamera = GetComponent<Camera>();
            if (playerCamera == null)
                playerCamera = Camera.main;

            aimAssist = GetComponent<AimAssist>();
        }

        public virtual Vector3 CalculateMagneticAimPoint(Vector3 aimOrigin)
        {
            if (!enableMagnetism) return aimOrigin;

            TargetDummy target = null;
            if (aimAssist != null)
                target = aimAssist.CurrentTarget;

            if (target == null)
            {
                // Find nearest target to crosshair
                target = FindNearestTargetToCrosshair();
            }

            if (target == null) return aimOrigin;

            float distance = Vector3.Distance(aimOrigin, target.transform.position);
            if (distance > magnetEffectiveRange) return aimOrigin;

            Vector3 targetPoint = target.transform.position + Vector3.up * 0.9f;

            if (preferHeadshots)
            {
                targetPoint += Vector3.up * headshotBias;
            }

            Vector3 directionToTarget = (targetPoint - aimOrigin).normalized;
            Vector3 aimDirection = (aimOrigin + playerCamera.transform.forward * 100f - aimOrigin).normalized;

            float angle = Vector3.Angle(aimDirection, directionToTarget);
            if (angle > maxDeflectionAngle) return aimOrigin;

            float distanceFactor = 1f - (distance / magnetEffectiveRange);
            float strength = magnetismStrength * distanceFactor;

            Vector3 magnetizedPoint = Vector3.Lerp(
                aimOrigin + aimDirection * distance,
                targetPoint,
                strength
            );

            if (showDebugRays)
            {
                Debug.DrawRay(aimOrigin, aimDirection * distance, Color.gray, 0.1f);
                Debug.DrawLine(aimOrigin, targetPoint, Color.magenta, 0.1f);
                Debug.DrawSphere(magnetizedPoint, 0.1f, Color.green, 0.1f);
            }

            return magnetizedPoint;
        }

        protected virtual TargetDummy FindNearestTargetToCrosshair()
        {
            if (playerCamera == null) return null;

            TargetDummy closest = null;
            float closestAngle = float.MaxValue;

            var targets = FindObjectsByType<TargetDummy>(
                FindObjectsSortMode.None);

            foreach (var target in targets)
            {
                if (!target.isActiveAndEnabled) continue;

                Vector3 direction = target.transform.position - transform.position;
                float distance = direction.magnitude;
                if (distance > magnetEffectiveRange) continue;

                float angle = Vector3.Angle(
                    playerCamera.transform.forward, direction);

                if (angle < closestAngle)
                {
                    closestAngle = angle;
                    closest = target;
                }
            }

            if (closestAngle > maxDeflectionAngle * 2f)
                return null;

            return closest;
        }

        public virtual bool IsTargetInMagnetismRange(TargetDummy target)
        {
            if (target == null) return false;

            float distance = Vector3.Distance(transform.position,
                target.transform.position);
            if (distance > magnetEffectiveRange) return false;

            Vector3 direction = target.transform.position - transform.position;
            float angle = Vector3.Angle(
                playerCamera.transform.forward, direction);

            return angle <= maxDeflectionAngle * 2f;
        }
    }
}
