using UnityEngine;
using System.Collections.Generic;
using FPSTrainingRoom.Training;

namespace FPSTrainingRoom.DebugTools
{
    public class ShotImpactDisplay : MonoBehaviour
    {
        [Header("Display Settings")]
        public bool showShotMarkers = true;
        public Color hitColor = Color.green;
        public Color missColor = Color.red;
        public Color headshotColor = Color.yellow;

        [Header("Decay")]
        public float markerLifetime = 5f;
        public bool fadeOverTime = true;

        [Header("Grouping Analysis")]
        public bool showGroupingAnalysis = true;
        public int minShotsForAnalysis = 3;
        public Color groupingColor = Color.magenta;

        protected struct ShotMarker
        {
            public Vector3 position;
            public float time;
            public bool isHit;
            public bool isHeadshot;
        }

        protected List<ShotMarker> shotMarkers = new List<ShotMarker>();
        protected Camera playerCamera;

        protected virtual void Start()
        {
            playerCamera = GetComponent<Camera>();
            if (playerCamera == null)
                playerCamera = Camera.main;
        }

        protected virtual void Update()
        {
            float currentTime = Time.time;
            shotMarkers.RemoveAll(m => currentTime - m.time > markerLifetime);
        }

        public virtual void RegisterShot(Vector3 point, bool hit, bool headshot = false)
        {
            shotMarkers.Add(new ShotMarker
            {
                position = point,
                time = Time.time,
                isHit = hit,
                isHeadshot = headshot
            });
        }

        protected virtual void OnGUI()
        {
            if (!showShotMarkers || playerCamera == null) return;

            foreach (var marker in shotMarkers)
            {
                Vector3 screenPos = playerCamera.WorldToScreenPoint(marker.position);
                if (screenPos.z < 0f) continue;

                float age = (Time.time - marker.time) / markerLifetime;
                float alpha = fadeOverTime ? 1f - age : 1f;

                Color color = marker.isHeadshot ? headshotColor :
                    marker.isHit ? hitColor : missColor;
                color.a = alpha;
                GUI.color = color;

                float size = Mathf.Lerp(8f, 4f, age);
                Vector2 pos = new Vector2(screenPos.x - size / 2f,
                    Screen.height - screenPos.y - size / 2f);

                if (marker.isHeadshot)
                    DrawHeadshotIndicator(pos, size);
                else if (marker.isHit)
                    DrawHitIndicator(pos, size);
                else
                    DrawMissIndicator(pos, size);

                GUI.color = Color.white;
            }

            if (showGroupingAnalysis && shotMarkers.Count >= minShotsForAnalysis)
                DrawGroupingAnalysis();
        }

        protected virtual void DrawHitIndicator(Vector2 pos, float size)
        {
            GUI.DrawTexture(new Rect(pos.x, pos.y, size, size), Texture2D.whiteTexture);
        }

        protected virtual void DrawMissIndicator(Vector2 pos, float size)
        {
            GUI.DrawTexture(new Rect(pos.x - 2f, pos.y, size + 4f, 1f),
                Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(pos.x, pos.y - 2f, 1f, size + 4f),
                Texture2D.whiteTexture);
        }

        protected virtual void DrawHeadshotIndicator(Vector2 pos, float size)
        {
            float starSize = size * 1.5f;
            Vector2 center = pos + Vector2.one * size / 2f;
            // Draw diamond
            GUI.DrawTexture(new Rect(center.x - starSize / 2f, center.y - 1f,
                starSize, 2f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(center.x - 1f, center.y - starSize / 2f,
                2f, starSize), Texture2D.whiteTexture);
        }

        protected virtual void DrawGroupingAnalysis()
        {
            if (shotMarkers.Count < minShotsForAnalysis) return;

            // Calculate center of mass
            Vector2 screenCenter = Vector2.zero;
            int validCount = 0;

            foreach (var marker in shotMarkers)
            {
                Vector3 screenPos = playerCamera.WorldToScreenPoint(marker.position);
                if (screenPos.z < 0f) continue;

                screenCenter += new Vector2(screenPos.x, Screen.height - screenPos.y);
                validCount++;
            }

            if (validCount < minShotsForAnalysis) return;

            screenCenter /= validCount;

            GUI.color = groupingColor;
            float indicatorSize = 10f;
            GUI.DrawTexture(new Rect(screenCenter.x - 1f, screenCenter.y - indicatorSize,
                2f, indicatorSize * 2f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(screenCenter.x - indicatorSize, screenCenter.y - 1f,
                indicatorSize * 2f, 2f), Texture2D.whiteTexture);

            GUI.color = Color.white;
        }

        public virtual float GetGroupingRadius()
        {
            if (shotMarkers.Count < 2) return 0f;

            Vector3 avgPos = Vector3.zero;
            foreach (var marker in shotMarkers)
                avgPos += marker.position;
            avgPos /= shotMarkers.Count;

            float maxDist = 0f;
            foreach (var marker in shotMarkers)
            {
                float dist = Vector3.Distance(avgPos, marker.position);
                if (dist > maxDist) maxDist = dist;
            }

            return maxDist;
        }

        public virtual void ClearMarkers()
        {
            shotMarkers.Clear();
        }
    }
}
