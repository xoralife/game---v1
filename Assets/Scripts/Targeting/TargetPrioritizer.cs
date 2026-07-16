using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace FPSTrainingRoom.Targeting
{
    public enum PriorityMode
    {
        ClosestDistance,
        ClosestToCrosshair,
        LowestHealth,
        HighestThreat,
        MostRecent
    }

    public class TargetPrioritizer : MonoBehaviour
    {
        [Header("Priority Settings")]
        public PriorityMode priorityMode = PriorityMode.ClosestToCrosshair;
        public bool ignoreInactiveTargets = true;
        public bool requireLineOfSight = true;
        public LayerMask occlusionLayers = -1;

        [Header("Scoring")]
        public float distanceWeight = 1f;
        public float angleWeight = 2f;
        public float healthWeight = 1.5f;
        public float threatWeight = 2f;

        protected Camera playerCamera;
        protected TargetingSystem targetingSystem;

        public System.Action<TargetDummy> OnPriorityTargetChanged;

        public TargetDummy CurrentPriority { get; private set; }

        protected virtual void Start()
        {
            playerCamera = GetComponent<Camera>();
            if (playerCamera == null)
                playerCamera = Camera.main;

            targetingSystem = GetComponent<TargetingSystem>();
        }

        protected virtual void Update()
        {
            TargetDummy previous = CurrentPriority;
            CurrentPriority = GetHighestPriorityTarget();

            if (CurrentPriority != previous)
                OnPriorityTargetChanged?.Invoke(CurrentPriority);
        }

        public virtual TargetDummy GetHighestPriorityTarget()
        {
            var targets = FindObjectsByType<TargetDummy>(
                FindObjectsSortMode.None);

            if (targets.Length == 0) return null;

            List<TargetDummy> validTargets = new List<TargetDummy>();

            foreach (var target in targets)
            {
                if (ignoreInactiveTargets && !target.isActiveAndEnabled)
                    continue;

                if (requireLineOfSight && playerCamera != null)
                {
                    Vector3 direction = target.transform.position - playerCamera.transform.position;
                    float distance = direction.magnitude;

                    if (Physics.Raycast(playerCamera.transform.position,
                        direction.normalized, out RaycastHit hit, distance, occlusionLayers))
                    {
                        if (!hit.collider.TryGetComponent<TargetDummy>(out _))
                            continue;
                    }
                }

                validTargets.Add(target);
            }

            if (validTargets.Count == 0) return null;

            return validTargets
                .OrderByDescending(t => CalculatePriorityScore(t))
                .FirstOrDefault();
        }

        protected virtual float CalculatePriorityScore(TargetDummy target)
        {
            float score = 0f;

            switch (priorityMode)
            {
                case PriorityMode.ClosestDistance:
                    float dist = Vector3.Distance(transform.position,
                        target.transform.position);
                    score = 1000f / Mathf.Max(dist, 0.1f);
                    break;

                case PriorityMode.ClosestToCrosshair:
                    if (playerCamera != null)
                    {
                        Vector3 direction = target.transform.position -
                            playerCamera.transform.position;
                        float angle = Vector3.Angle(
                            playerCamera.transform.forward, direction);
                        score = 1000f / Mathf.Max(angle, 0.1f);
                    }
                    break;

                case PriorityMode.LowestHealth:
                    score = 1000f / Mathf.Max(target.CurrentHealth, 1f);
                    break;

                case PriorityMode.HighestThreat:
                    score = target.ThreatLevel * 100f;
                    break;

                case PriorityMode.MostRecent:
                    score = Time.time - target.LastHitTime;
                    break;
            }

            return score;
        }

        public virtual List<TargetDummy> GetPrioritizedList(int maxCount = 5)
        {
            var targets = FindObjectsByType<TargetDummy>(
                FindObjectsSortMode.None)
                .Where(t => !ignoreInactiveTargets || t.isActiveAndEnabled)
                .OrderByDescending(t => CalculatePriorityScore(t))
                .Take(maxCount)
                .ToList();

            return targets;
        }
    }
}
