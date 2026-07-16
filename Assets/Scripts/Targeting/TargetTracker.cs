using UnityEngine;
using System.Collections.Generic;

namespace FPSTrainingRoom.Targeting
{
    public class TargetTracker : MonoBehaviour
    {
        [Header("Tracking Settings")]
        public float trackingRange = 60f;
        public float trackingAngle = 45f;
        public LayerMask targetLayers = -1;
        public LayerMask occlusionLayers = -1;

        [Header("Track History")]
        public int maxTrackHistory = 100;
        public float trackTimeout = 5f;

        protected Dictionary<TargetDummy, TrackEntry> trackedTargets =
            new Dictionary<TargetDummy, TrackEntry>();
        protected Camera playerCamera;

        public class TrackEntry
        {
            public TargetDummy target;
            public float firstSeenTime;
            public float lastSeenTime;
            public Vector3 lastKnownPosition;
            public Vector3 lastKnownVelocity;
            public int hitCount;
            public bool isVisible;

            public TrackEntry(TargetDummy t, float time)
            {
                target = t;
                firstSeenTime = time;
                lastSeenTime = time;
                lastKnownPosition = t.transform.position;
                isVisible = true;
            }
        }

        public System.Action<TargetDummy> OnNewTargetTracked;
        public System.Action<TargetDummy> OnTargetLost;

        protected virtual void Start()
        {
            playerCamera = GetComponent<Camera>();
            if (playerCamera == null)
                playerCamera = Camera.main;
        }

        protected virtual void Update()
        {
            var allTargets = FindObjectsByType<TargetDummy>(
                FindObjectsSortMode.None);

            foreach (var target in allTargets)
            {
                if (!target.isActiveAndEnabled) continue;
                ProcessTarget(target);
            }

            CleanupStaleTracks();
        }

        protected virtual void ProcessTarget(TargetDummy target)
        {
            Vector3 direction = target.transform.position - transform.position;
            float distance = direction.magnitude;
            bool canSee = false;

            if (distance <= trackingRange)
            {
                float angle = Vector3.Angle(playerCamera.transform.forward, direction);
                if (angle <= trackingAngle)
                {
                    if (!Physics.Raycast(transform.position, direction.normalized,
                        out RaycastHit hit, distance, occlusionLayers) ||
                        hit.collider.TryGetComponent<TargetDummy>(out _))
                    {
                        canSee = true;
                    }
                }
            }

            if (canSee)
            {
                if (!trackedTargets.ContainsKey(target))
                {
                    var entry = new TrackEntry(target, Time.time);
                    trackedTargets.Add(target, entry);
                    OnNewTargetTracked?.Invoke(target);
                }

                var track = trackedTargets[target];
                track.lastSeenTime = Time.time;
                track.lastKnownPosition = target.transform.position;
                track.isVisible = true;
            }
            else if (trackedTargets.ContainsKey(target))
            {
                var track = trackedTargets[target];
                track.lastKnownPosition = target.transform.position;
                track.isVisible = false;
            }
        }

        protected virtual void CleanupStaleTracks()
        {
            List<TargetDummy> stale = new List<TargetDummy>();

            foreach (var kvp in trackedTargets)
            {
                if (Time.time - kvp.Value.lastSeenTime > trackTimeout)
                {
                    stale.Add(kvp.Key);
                    OnTargetLost?.Invoke(kvp.Key);
                }
            }

            foreach (var target in stale)
                trackedTargets.Remove(target);
        }

        public virtual TrackEntry GetTrack(TargetDummy target)
        {
            trackedTargets.TryGetValue(target, out TrackEntry entry);
            return entry;
        }

        public virtual bool IsTracked(TargetDummy target)
        {
            return trackedTargets.ContainsKey(target);
        }

        public virtual List<TrackEntry> GetActiveTracks()
        {
            return new List<TrackEntry>(trackedTargets.Values);
        }

        public virtual int GetTrackCount() => trackedTargets.Count;
    }
}
