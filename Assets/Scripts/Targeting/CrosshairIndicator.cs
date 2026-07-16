using UnityEngine;

namespace FPSTrainingRoom.Targeting
{
    public class CrosshairIndicator : MonoBehaviour
    {
        [Header("Crosshair Settings")]
        public float crosshairSize = 20f;
        public float crosshairGap = 5f;
        public float crosshairThickness = 2f;

        [Header("Colors")]
        public Color defaultColor = Color.white;
        public Color lockedColor = Color.red;
        public Color trackingColor = Color.yellow;
        public Color hitColor = Color.green;

        [Header("Dynamic Behavior")]
        public bool expandOnFire = true;
        public float expandAmount = 10f;
        public float expandRecovery = 4f;
        public bool showLockIndicator = true;

        protected float currentExpand;
        protected Color currentColor;
        protected AimAssist aimAssist;
        protected TargetLock targetLock;
        protected Camera playerCamera;

        protected virtual void Start()
        {
            playerCamera = GetComponent<Camera>();
            if (playerCamera == null)
                playerCamera = Camera.main;

            aimAssist = GetComponent<AimAssist>();
            targetLock = GetComponent<TargetLock>();
            currentColor = defaultColor;
        }

        protected virtual void Update()
        {
            currentExpand = Mathf.Lerp(currentExpand, 0f,
                Time.deltaTime * expandRecovery);
        }

        protected virtual void OnGUI()
        {
            if (playerCamera == null) return;

            Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            float size = crosshairSize + currentExpand;
            float gap = crosshairGap + currentExpand * 0.5f;

            currentColor = defaultColor;

            if (targetLock != null && targetLock.IsLocked)
                currentColor = lockedColor;
            else if (aimAssist != null && aimAssist.CurrentTarget != null)
                currentColor = trackingColor;

            GUI.color = currentColor;

            DrawCrosshairLine(center, Vector2.up, size, gap);
            DrawCrosshairLine(center, Vector2.down, size, gap);
            DrawCrosshairLine(center, Vector2.left, size, gap);
            DrawCrosshairLine(center, Vector2.right, size, gap);

            if (showLockIndicator && targetLock != null && targetLock.IsLocked)
            {
                GUI.color = lockedColor;
                float lockSize = size * 1.5f;
                DrawLockIndicator(center, lockSize);
            }

            GUI.color = Color.white;
        }

        protected virtual void DrawCrosshairLine(Vector2 center, Vector2 direction,
            float size, float gap)
        {
            Vector2 start = center + direction * gap;
            Vector2 end = center + direction * (gap + size);

            float width = crosshairThickness;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            GUIUtility.RotateAroundPivot(angle, center);
            GUI.DrawTexture(new Rect(start.x, start.y - width / 2f,
                Vector2.Distance(end, start), width), Texture2D.whiteTexture);
            GUIUtility.RotateAroundPivot(-angle, center);
        }

        protected virtual void DrawLockIndicator(Vector2 center, float size)
        {
            float halfSize = size / 2f;
            Rect lockRect = new Rect(center.x - halfSize, center.y - halfSize,
                size, size);
            GUI.Box(lockRect, "");

            // Draw corner brackets
            float bracketSize = size * 0.3f;
            float thickness = crosshairThickness;

            // Top-left
            GUI.DrawTexture(new Rect(center.x - halfSize, center.y - halfSize,
                bracketSize, thickness), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(center.x - halfSize, center.y - halfSize,
                thickness, bracketSize), Texture2D.whiteTexture);

            // Top-right
            GUI.DrawTexture(new Rect(center.x + halfSize - bracketSize,
                center.y - halfSize, bracketSize, thickness),
                Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(center.x + halfSize - thickness,
                center.y - halfSize, thickness, bracketSize),
                Texture2D.whiteTexture);

            // Bottom-left
            GUI.DrawTexture(new Rect(center.x - halfSize,
                center.y + halfSize - thickness, bracketSize, thickness),
                Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(center.x - halfSize,
                center.y + halfSize - bracketSize, thickness, bracketSize),
                Texture2D.whiteTexture);

            // Bottom-right
            GUI.DrawTexture(new Rect(center.x + halfSize - bracketSize,
                center.y + halfSize - thickness, bracketSize, thickness),
                Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(center.x + halfSize - thickness,
                center.y + halfSize - bracketSize, thickness, bracketSize),
                Texture2D.whiteTexture);
        }

        public virtual void NotifyFire()
        {
            if (expandOnFire)
                currentExpand = expandAmount;
        }

        public virtual void NotifyHit()
        {
            currentColor = hitColor;
        }
    }
}
