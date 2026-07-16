using UnityEngine;
using System.Collections.Generic;

namespace FPSTrainingRoom.Targeting
{
    public class TargetLock : MonoBehaviour
    {
        [Header("Lock-On Settings")]
        public bool enableTargetLock = true;
        public float lockOnRange = 50f;
        public float lockOnAngle = 15f;
        public float lockOnTime = 0.3f;
        public LayerMask targetLayers = -1;
        public LayerMask obstacleMask = -1;

        [Header("Snap Settings")]
        public bool enableSnap = true;
        public float snapSpeed = 8f;
        public float snapAngle = 20f;
        public AnimationCurve snapCurve = AnimationCurve.EaseOut(0f, 0f, 1f, 1f);

        [Header("Lock Indicators")]
        public Color lockedColor = Color.red;
        public Color lockingColor = Color.yellow;
        public float lockPulseSpeed = 2f;

        [Header("Multiple Locks")]
        public bool allowMultipleLocks = false;
        public int maxLocks = 4;

        protected TargetDummy lockedTarget;
        protected List<TargetDummy> lockedTargets = new List<TargetDummy>();
        protected float lockProgress;
        protected bool isLocking;
        protected Camera playerCamera;
        protected Quaternion snapStartRotation;
        protected Quaternion snapTargetRotation;
        protected float snapTimer;

        public System.Action<TargetDummy> OnLockAcquired;
        public System.Action<TargetDummy> OnLockLost;
        public System.Action OnAllLocksLost;

        public TargetDummy GetLockedTarget() => lockedTarget;
        public List<TargetDummy> GetLockedTargets() => lockedTargets;
        public bool IsLocked => lockedTarget != null;
        public float LockProgress => lockProgress;

        protected virtual void Start()
        {
            playerCamera = GetComponent<Camera>();
            if (playerCamera == null)
                playerCamera = Camera.main;
        }

        protected virtual void Update()
        {
            if (!enableTargetLock) return;

            if (enableSnap && snapTimer > 0f)
            {
                snapTimer -= Time.deltaTime;
                float t = 1f - (snapTimer / 1f);
                float curveValue = snapCurve.Evaluate(t);

                if (playerCamera != null)
                {
                    playerCamera.transform.rotation = Quaternion.Slerp(
                        snapStartRotation, snapTargetRotation, curveValue);
                }

                return;
            }

            if (Input.GetMouseButton(1))
            {
                AttemptLock();
            }
            else
            {
                if (isLocking)
                {
                    isLocking = false;
                    lockProgress = 0f;
                }

                if (lockedTarget != null)
                {
                    if (allowMultipleLocks)
                        ReleaseAllLocks();
                    else
                        ReleaseLock();
                }
            }

            if (lockedTarget != null && !IsTargetValid(lockedTarget))
            {
                ReleaseLock();
            }
        }

        protected virtual void AttemptLock()
        {
            TargetDummy bestTarget = FindBestTarget();

            if (bestTarget == null)
            {
                if (isLocking)
                {
                    isLocking = false;
                    lockProgress = 0f;
                }
                return;
            }

            if (lockedTarget == bestTarget)
            {
                // Maintain existing lock
                return;
            }

            if (!isLocking)
            {
                isLocking = true;
                lockProgress = 0f;
            }

            lockProgress += Time.deltaTime / lockOnTime;

            if (lockProgress >= 1f)
            {
                AcquireLock(bestTarget);
            }
        }

        protected virtual TargetDummy FindBestTarget()
        {
            TargetDummy best = null;
            float bestScore = float.MaxValue;

            var allTargets = FindObjectsByType<TargetDummy>(
                FindObjectsSortMode.None);

            foreach (var target in allTargets)
            {
                if (!target.isActiveAndEnabled) continue;
                if (allowMultipleLocks && lockedTargets.Contains(target)) continue;

                Vector3 direction = target.transform.position - transform.position;
                float distance = direction.magnitude;
                if (distance > lockOnRange) continue;

                float angle = Vector3.Angle(playerCamera.transform.forward, direction);
                if (angle > lockOnAngle) continue;

                if (Physics.Raycast(transform.position, direction.normalized,
                    out RaycastHit hit, distance, obstacleMask))
                {
                    if (!hit.collider.TryGetComponent<TargetDummy>(out _))
                        continue;
                }

                float score = angle * 2f + distance * 0.5f;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = target;
                }
            }

            return best;
        }

        protected virtual void AcquireLock(TargetDummy target)
        {
            if (allowMultipleLocks)
            {
                if (lockedTargets.Count >= maxLocks) return;
                lockedTargets.Add(target);
            }
            else
            {
                if (lockedTarget != null)
                    ReleaseLock();
                lockedTarget = target;
            }

            isLocking = false;
            lockProgress = 0f;

            if (enableSnap)
            {
                SnapToTarget(target);
            }

            OnLockAcquired?.Invoke(target);
        }

        protected virtual void SnapToTarget(TargetDummy target)
        {
            if (playerCamera == null) return;

            Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
            snapStartRotation = playerCamera.transform.rotation;
            snapTargetRotation = Quaternion.LookRotation(directionToTarget);

            float angle = Quaternion.Angle(snapStartRotation, snapTargetRotation);
            if (angle <= snapAngle)
            {
                snapTimer = 1f;
            }
        }

        protected virtual void ReleaseLock()
        {
            if (lockedTarget != null)
            {
                var oldTarget = lockedTarget;
                lockedTarget = null;
                OnLockLost?.Invoke(oldTarget);
                OnAllLocksLost?.Invoke();
            }
        }

        protected virtual void ReleaseAllLocks()
        {
            foreach (var target in lockedTargets)
            {
                OnLockLost?.Invoke(target);
            }
            lockedTargets.Clear();
            OnAllLocksLost?.Invoke();
        }

        protected virtual bool IsTargetValid(TargetDummy target)
        {
            if (target == null || !target.isActiveAndEnabled) return false;

            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance > lockOnRange) return false;

            return true;
        }

        protected virtual void OnGUI()
        {
            if (!enableTargetLock) return;

            if (lockedTarget != null)
            {
                DrawLockIndicator(lockedTarget, lockedColor);
            }

            if (isLocking)
            {
                var target = FindBestTarget();
                if (target != null)
                    DrawLockIndicator(target, lockingColor);
            }

            foreach (var target in lockedTargets)
            {
                DrawLockIndicator(target, lockedColor);
            }
        }

        protected virtual void DrawLockIndicator(TargetDummy target, Color color)
        {
            if (target == null || playerCamera == null) return;

            Vector3 screenPos = playerCamera.WorldToScreenPoint(
                target.transform.position + Vector3.up * 1.5f);
            if (screenPos.z < 0f) return;

            float pulse = Mathf.Sin(Time.time * lockPulseSpeed) * 0.3f + 0.7f;
            GUI.color = new Color(color.r, color.g, color.b, pulse);

            float size = 20f;
            Rect rect = new Rect(screenPos.x - size / 2f,
                Screen.height - screenPos.y - size / 2f, size, size);

            GUI.Box(rect, "X");
            GUI.color = Color.white;
        }

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, lockOnRange);

            if (playerCamera != null)
            {
                Vector3 forward = playerCamera.transform.forward;
                Vector3 leftBoundary = Quaternion.Euler(0f, -lockOnAngle, 0f) * forward;
                Vector3 rightBoundary = Quaternion.Euler(0f, lockOnAngle, 0f) * forward;

                Gizmos.color = Color.magenta;
                Gizmos.DrawRay(transform.position, leftBoundary * lockOnRange);
                Gizmos.DrawRay(transform.position, rightBoundary * lockOnRange);
            }
        }
    }
}
