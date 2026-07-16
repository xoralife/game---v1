using UnityEngine;
using System.Collections.Generic;

namespace FPSTrainingRoom.Targeting
{
    public class TargetingSystem : MonoBehaviour
    {
        [Header("Global Detection")]
        public float maxDetectionRange = 100f;
        public float detectionRefreshRate = 0.2f;
        public LayerMask detectableLayers = -1;
        public LayerMask occlusionLayers = -1;

        [Header("Target Classification")]
        public bool classifyByDistance = true;
        public bool classifyByThreat = false;
        public bool classifyByType = false;

        protected List<TargetDummy> allTargets = new List<TargetDummy>();
        protected List<TargetDummy> inRangeTargets = new List<TargetDummy>();
        protected List<TargetDummy> visibleTargets = new List<TargetDummy>();
        protected float lastDetectionTime;

        public System.Action<List<TargetDummy>> OnTargetsUpdated;

        public List<TargetDummy> AllTargets => allTargets;
        public List<TargetDummy> InRangeTargets => inRangeTargets;
        public List<TargetDummy> VisibleTargets => visibleTargets;

        public static TargetingSystem Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        protected virtual void Update()
        {
            if (Time.time - lastDetectionTime >= detectionRefreshRate)
            {
                RefreshTargetList();
                lastDetectionTime = Time.time;
            }
        }

        protected virtual void RefreshTargetList()
        {
            allTargets.Clear();
            inRangeTargets.Clear();
            visibleTargets.Clear();

            var found = FindObjectsByType<TargetDummy>(
                FindObjectsSortMode.None);
            allTargets.AddRange(found);

            foreach (var target in allTargets)
            {
                if (!target.isActiveAndEnabled) continue;

                float distance = Vector3.Distance(transform.position,
                    target.transform.position);

                if (distance <= maxDetectionRange)
                {
                    inRangeTargets.Add(target);

                    Vector3 direction = target.transform.position - transform.position;
                    if (!Physics.Raycast(transform.position, direction.normalized,
                        out RaycastHit hit, distance, occlusionLayers) ||
                        hit.collider.TryGetComponent<TargetDummy>(out _))
                    {
                        visibleTargets.Add(target);
                    }
                }
            }

            if (classifyByDistance)
                visibleTargets.Sort((a, b) =>
                    Vector3.Distance(transform.position, a.transform.position)
                        .CompareTo(
                    Vector3.Distance(transform.position, b.transform.position)));

            OnTargetsUpdated?.Invoke(visibleTargets);
        }

        public virtual TargetDummy GetClosestVisible()
        {
            if (visibleTargets.Count == 0) return null;
            return visibleTargets[0];
        }

        public virtual TargetDummy GetClosestInRange()
        {
            if (inRangeTargets.Count == 0) return null;

            TargetDummy closest = null;
            float minDist = float.MaxValue;

            foreach (var target in inRangeTargets)
            {
                float dist = Vector3.Distance(transform.position,
                    target.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = target;
                }
            }

            return closest;
        }

        public virtual int GetVisibleCount() => visibleTargets.Count;
        public virtual int GetTotalCount() => allTargets.Count;

        public virtual float GetDistanceToTarget(TargetDummy target)
        {
            if (target == null) return float.MaxValue;
            return Vector3.Distance(transform.position, target.transform.position);
        }

        public virtual bool IsTargetVisible(TargetDummy target)
        {
            return visibleTargets.Contains(target);
        }
    }
}
