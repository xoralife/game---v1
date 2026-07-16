using UnityEngine;
using FPSTrainingRoom.Weapons;

namespace FPSTrainingRoom.DebugTools
{
    public class PatternVisualizer : MonoBehaviour
    {
        [Header("Visualization Settings")]
        public bool showPattern = true;
        public SprayPattern sprayPattern;
        public Transform visualizationOrigin;
        public float pointSpacing = 1f;
        public float pointSize = 0.1f;

        [Header("Colors")]
        public Color pointColor = Color.cyan;
        public Color lineColor = Color.white;
        public Color headshotZoneColor = new Color(1f, 0.5f, 0f, 0.3f);

        [Header("Preview Mode")]
        public bool showPreview = false;
        [Range(1, 100)]
        public int previewShots = 30;

        protected Vector3[] patternPoints;
        protected LineRenderer lineRenderer;

        protected virtual void Start()
        {
            if (visualizationOrigin == null)
                visualizationOrigin = transform;

            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null && showPattern)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
                lineRenderer.startWidth = 0.02f;
                lineRenderer.endWidth = 0.02f;
                lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
                lineRenderer.startColor = lineColor;
                lineRenderer.endColor = lineColor;
            }

            GeneratePatternVisualization();
        }

        protected virtual void OnDrawGizmos()
        {
            if (!showPattern || sprayPattern == null || !Application.isPlaying)
                return;

            Vector3 origin = visualizationOrigin != null
                ? visualizationOrigin.position
                : transform.position;

            int shotCount = showPreview
                ? Mathf.Min(previewShots, sprayPattern.patternLength)
                : sprayPattern.patternLength;

            Gizmos.color = pointColor;
            Vector3 previousPoint = origin;

            for (int i = 0; i < shotCount; i++)
            {
                float t = (float)i / Mathf.Max(sprayPattern.patternLength - 1, 1);
                Vector2 offset = sprayPattern.GetRecoilOffset(i, t);

                Vector3 point = origin + new Vector3(offset.x, offset.y, 0f) * pointSpacing;
                Gizmos.DrawSphere(point, pointSize);

                if (i > 0)
                {
                    Gizmos.color = lineColor;
                    Gizmos.DrawLine(previousPoint, point);
                }

                previousPoint = point;
                Gizmos.color = pointColor;
            }

            // Draw spread visualization at each point
            Gizmos.color = headshotZoneColor;
            for (int i = 0; i < shotCount; i += 5)
            {
                float spread = sprayPattern.GetSpread(i);
                float t = (float)i / Mathf.Max(sprayPattern.patternLength - 1, 1);
                Vector2 offset = sprayPattern.GetRecoilOffset(i, t);
                Vector3 point = origin + new Vector3(offset.x, offset.y, 0f) * pointSpacing;

                Gizmos.DrawWireSphere(point, spread * pointSpacing);
            }
        }

        protected virtual void GeneratePatternVisualization()
        {
            if (sprayPattern == null) return;

            patternPoints = new Vector3[sprayPattern.patternLength];

            for (int i = 0; i < sprayPattern.patternLength; i++)
            {
                float t = (float)i / Mathf.Max(sprayPattern.patternLength - 1, 1);
                Vector2 offset = sprayPattern.GetRecoilOffset(i, t);
                patternPoints[i] = new Vector3(offset.x, offset.y, 0f) * pointSpacing;

                // Connect with line renderer
                if (lineRenderer != null && i > 0)
                {
                    lineRenderer.positionCount = i + 1;
                    lineRenderer.SetPosition(i, transform.TransformPoint(patternPoints[i]));
                }
            }
        }

        public virtual void UpdatePattern(SprayPattern newPattern)
        {
            sprayPattern = newPattern;
            GeneratePatternVisualization();
        }

        protected virtual void OnValidate()
        {
            if (Application.isPlaying && sprayPattern != null)
                GeneratePatternVisualization();
        }
    }
}
