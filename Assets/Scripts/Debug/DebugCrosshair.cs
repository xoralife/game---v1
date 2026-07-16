using UnityEngine;

namespace FPSTrainingRoom.DebugTools
{
    public class DebugCrosshair : MonoBehaviour
    {
        [Header("Crosshair Settings")]
        public float size = 30f;
        public float gap = 10f;
        public float thickness = 2f;
        public bool showCenterDot = true;
        public float centerDotSize = 4f;

        [Header("Colors")]
        public Color defaultColor = Color.white;
        public Color hitConfirmColor = Color.green;
        public Color killConfirmColor = Color.red;

        [Header("Dynamic")]
        public bool expandOnFire = true;
        public float expandAmount = 15f;
        public float expandSpeed = 8f;
        public bool rotateOnFire = false;
        public float rotateAmount = 5f;

        protected float currentExpand;
        protected float currentRotation;
        protected Color currentColor;
        protected Camera playerCamera;

        protected virtual void Start()
        {
            playerCamera = GetComponent<Camera>();
            if (playerCamera == null)
                playerCamera = Camera.main;

            currentColor = defaultColor;
        }

        protected virtual void Update()
        {
            currentExpand = Mathf.Lerp(currentExpand, 0f,
                Time.deltaTime * expandSpeed);
            currentRotation = Mathf.Lerp(currentRotation, 0f,
                Time.deltaTime * expandSpeed);

            if (Input.GetMouseButtonDown(0))
                NotifyFire();
        }

        protected virtual void OnGUI()
        {
            if (playerCamera == null) return;

            Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            float currentSize = size + currentExpand;
            float currentGap = gap + currentExpand * 0.3f;

            GUI.color = currentColor;
            Matrix4x4 originalMatrix = GUI.matrix;

            if (Mathf.Abs(currentRotation) > 0.1f)
            {
                GUIUtility.RotateAroundPivot(currentRotation, center);
            }

            // Top
            GUI.DrawTexture(new Rect(center.x - thickness / 2f,
                center.y - currentGap - currentSize, thickness, currentSize),
                Texture2D.whiteTexture);

            // Bottom
            GUI.DrawTexture(new Rect(center.x - thickness / 2f,
                center.y + currentGap, thickness, currentSize),
                Texture2D.whiteTexture);

            // Left
            GUI.DrawTexture(new Rect(center.x - currentGap - currentSize,
                center.y - thickness / 2f, currentSize, thickness),
                Texture2D.whiteTexture);

            // Right
            GUI.DrawTexture(new Rect(center.x + currentGap,
                center.y - thickness / 2f, currentSize, thickness),
                Texture2D.whiteTexture);

            // Center dot
            if (showCenterDot)
            {
                GUI.DrawTexture(new Rect(center.x - centerDotSize / 2f,
                    center.y - centerDotSize / 2f, centerDotSize, centerDotSize),
                    Texture2D.whiteTexture);
            }

            GUI.matrix = originalMatrix;
            GUI.color = Color.white;
        }

        public virtual void NotifyFire()
        {
            if (expandOnFire)
                currentExpand = expandAmount;

            if (rotateOnFire)
                currentRotation = Random.Range(-rotateAmount, rotateAmount);
        }

        public virtual void NotifyHit()
        {
            currentColor = hitConfirmColor;
            Invoke(nameof(ResetColor), 0.1f);
        }

        public virtual void NotifyKill()
        {
            currentColor = killConfirmColor;
            Invoke(nameof(ResetColor), 0.2f);
        }

        protected virtual void ResetColor()
        {
            currentColor = defaultColor;
        }
    }
}
