using UnityEngine;
using System.Collections.Generic;
using FPSTrainingRoom.Weapons;

namespace FPSTrainingRoom.DebugTools
{
    public class RecoilDebugOverlay : MonoBehaviour
    {
        [Header("Overlay Settings")]
        public bool showOverlay = true;
        public int maxPatternPoints = 100;
        public float pointScale = 2f;
        public Color patternColor = Color.cyan;
        public Color compensatedPathColor = Color.green;
        public Color spreadIndicatorColor = Color.yellow;

        [Header("Screen Position")]
        public Vector2 overlayPosition = new Vector2(20f, 20f);
        public Vector2 overlaySize = new Vector2(200f, 200f);

        protected List<Vector2> patternPoints = new List<Vector2>();
        protected List<Vector2> compensatedPoints = new List<Vector2>();
        protected RecoilController recoilController;
        protected Camera playerCamera;
        protected Rect overlayRect;

        protected virtual void Start()
        {
            playerCamera = GetComponent<Camera>();
            if (playerCamera == null)
                playerCamera = Camera.main;

            recoilController = GetComponent<RecoilController>();
            if (recoilController == null)
                recoilController = FindFirstObjectByType<RecoilController>();
        }

        protected virtual void Update()
        {
            if (!showOverlay || recoilController == null) return;

            if (recoilController.isFiring && recoilController.currentRecoil.magnitude > 0.01f)
            {
                patternPoints.Add(recoilController.currentRecoil);

                if (recoilController.useCompensation)
                {
                    Vector2 compensated = recoilController.currentRecoil * 0.6f;
                    compensatedPoints.Add(compensated);
                }

                if (patternPoints.Count > maxPatternPoints)
                    patternPoints.RemoveAt(0);
                if (compensatedPoints.Count > maxPatternPoints)
                    compensatedPoints.RemoveAt(0);
            }
        }

        protected virtual void OnGUI()
        {
            if (!showOverlay) return;

            overlayRect = new Rect(overlayPosition.x, overlayPosition.y,
                overlaySize.x, overlaySize.y);

            GUI.Box(overlayRect, "Recoil Pattern");
            GUI.BeginGroup(overlayRect);

            Vector2 center = overlaySize / 2f;

            // Draw crosshair center
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(center.x - 0.5f, center.y - 10f, 1f, 20f),
                Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(center.x - 10f, center.y - 0.5f, 20f, 1f),
                Texture2D.whiteTexture);

            // Draw spread circle
            if (recoilController != null)
            {
                GUI.color = spreadIndicatorColor;
                float spreadSize = recoilController.currentSpread * pointScale;
                GUI.DrawTexture(new Rect(center.x - spreadSize / 2f,
                    center.y - spreadSize / 2f, spreadSize, spreadSize),
                    Texture2D.whiteTexture);
            }

            // Draw pattern points
            for (int i = 0; i < patternPoints.Count; i++)
            {
                float alpha = (float)i / patternPoints.Count;
                GUI.color = new Color(patternColor.r, patternColor.g, patternColor.b, alpha);

                Vector2 pointPos = center + patternPoints[i] * pointScale;
                GUI.DrawTexture(new Rect(pointPos.x - 1f, pointPos.y - 1f, 3f, 3f),
                    Texture2D.whiteTexture);
            }

            // Draw compensated path
            for (int i = 0; i < compensatedPoints.Count; i++)
            {
                float alpha = (float)i / compensatedPoints.Count;
                GUI.color = new Color(compensatedPathColor.r, compensatedPathColor.g,
                    compensatedPathColor.b, alpha);

                Vector2 pointPos = center + compensatedPoints[i] * pointScale;
                GUI.DrawTexture(new Rect(pointPos.x - 1f, pointPos.y - 1f, 3f, 3f),
                    Texture2D.whiteTexture);
            }

            GUI.color = Color.white;
            GUI.EndGroup();
        }

        public virtual void ClearPattern()
        {
            patternPoints.Clear();
            compensatedPoints.Clear();
        }
    }
}
