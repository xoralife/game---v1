using UnityEngine;

namespace FPSTrainingRoom.DebugTools
{
    public class HitMarkerUI : MonoBehaviour
    {
        [Header("Hit Marker Settings")]
        public float hitMarkerSize = 24f;
        public float hitMarkerDuration = 0.2f;
        public Color hitMarkerColor = Color.white;
        public Color killMarkerColor = Color.red;
        public Color headshotMarkerColor = Color.yellow;

        [Header("Animation")]
        public bool animateHitMarker = true;
        public AnimationCurve hitMarkerScale = AnimationCurve.EaseOut(0f, 1.5f, 1f, 0.8f);
        public AnimationCurve hitMarkerAlpha = AnimationCurve.EaseIn(0f, 1f, 1f, 0f);

        protected float hitMarkerTimer;
        protected bool isHeadshot;
        protected bool isKill;
        protected Camera playerCamera;

        protected virtual void Start()
        {
            playerCamera = GetComponent<Camera>();
            if (playerCamera == null)
                playerCamera = Camera.main;
        }

        protected virtual void Update()
        {
            if (hitMarkerTimer > 0f)
                hitMarkerTimer -= Time.deltaTime;
        }

        public virtual void ShowHitMarker(bool headshot = false, bool kill = false)
        {
            hitMarkerTimer = hitMarkerDuration;
            isHeadshot = headshot;
            isKill = kill;
        }

        protected virtual void OnGUI()
        {
            if (hitMarkerTimer <= 0f) return;

            Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            float t = 1f - (hitMarkerTimer / hitMarkerDuration);

            float scale = animateHitMarker ? hitMarkerScale.Evaluate(t) : 1f;
            float alpha = animateHitMarker ? hitMarkerAlpha.Evaluate(t) : 1f;

            Color color = isKill ? killMarkerColor :
                isHeadshot ? headshotMarkerColor : hitMarkerColor;
            color.a = alpha;
            GUI.color = color;

            float size = hitMarkerSize * scale;
            float halfSize = size / 2f;
            float thickness = 2f;

            // Four brackets forming an X shape
            // Top-left
            GUI.DrawTexture(new Rect(center.x - halfSize, center.y - halfSize,
                size * 0.4f, thickness), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(center.x - halfSize, center.y - halfSize,
                thickness, size * 0.4f), Texture2D.whiteTexture);

            // Top-right
            GUI.DrawTexture(new Rect(center.x + halfSize - size * 0.4f,
                center.y - halfSize, size * 0.4f, thickness),
                Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(center.x + halfSize - thickness,
                center.y - halfSize, thickness, size * 0.4f),
                Texture2D.whiteTexture);

            // Bottom-left
            GUI.DrawTexture(new Rect(center.x - halfSize,
                center.y + halfSize - thickness, size * 0.4f, thickness),
                Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(center.x - halfSize,
                center.y + halfSize - size * 0.4f, thickness, size * 0.4f),
                Texture2D.whiteTexture);

            // Bottom-right
            GUI.DrawTexture(new Rect(center.x + halfSize - size * 0.4f,
                center.y + halfSize - thickness, size * 0.4f, thickness),
                Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(center.x + halfSize - thickness,
                center.y + halfSize - size * 0.4f, thickness, size * 0.4f),
                Texture2D.whiteTexture);

            // Center dot for kill
            if (isKill)
            {
                float dotSize = 4f * scale;
                GUI.DrawTexture(new Rect(center.x - dotSize / 2f,
                    center.y - dotSize / 2f, dotSize, dotSize),
                    Texture2D.whiteTexture);
            }

            GUI.color = Color.white;
        }
    }
}
