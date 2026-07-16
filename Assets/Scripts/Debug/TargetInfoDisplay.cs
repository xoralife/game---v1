using UnityEngine;
using FPSTrainingRoom.Targeting;
using FPSTrainingRoom.Training;

namespace FPSTrainingRoom.DebugTools
{
    public class TargetInfoDisplay : MonoBehaviour
    {
        [Header("Display Settings")]
        public bool showTargetInfo = true;
        public bool showHealthBar = true;
        public bool showDistance = true;
        public bool showThreatLevel = true;
        public float maxDisplayDistance = 50f;

        [Header("Health Bar")]
        public Vector2 healthBarSize = new Vector2(40f, 5f);
        public Color healthBarBackground = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        public Color healthBarColor = Color.green;
        public Color lowHealthColor = Color.red;
        public float lowHealthThreshold = 0.3f;

        [Header("Text")]
        public Color textColor = Color.white;
        public int fontSize = 11;

        protected Camera playerCamera;
        protected GUIStyle labelStyle;

        protected virtual void Start()
        {
            playerCamera = GetComponent<Camera>();
            if (playerCamera == null)
                playerCamera = Camera.main;

            labelStyle = new GUIStyle();
            labelStyle.fontSize = fontSize;
            labelStyle.normal.textColor = textColor;
            labelStyle.alignment = TextAnchor.MiddleCenter;
        }

        protected virtual void OnGUI()
        {
            if (!showTargetInfo || playerCamera == null) return;

            var targets = FindObjectsByType<TargetDummy>(
                FindObjectsSortMode.None);

            foreach (var target in targets)
            {
                if (!target.isActiveAndEnabled) continue;

                float distance = Vector3.Distance(transform.position,
                    target.transform.position);
                if (distance > maxDisplayDistance) continue;

                Vector3 worldPos = target.transform.position + Vector3.up * 2.2f;
                Vector3 screenPos = playerCamera.WorldToScreenPoint(worldPos);

                if (screenPos.z < 0f) continue;

                Vector2 guiPos = new Vector2(screenPos.x - healthBarSize.x / 2f,
                    Screen.height - screenPos.y);

                if (showHealthBar)
                    DrawHealthBar(guiPos, target);

                if (showDistance)
                {
                    string distanceText = $"{Mathf.RoundToInt(distance)}m";

                    if (showThreatLevel)
                        distanceText += $" | Threat: {target.ThreatLevel:F1}";

                    GUI.color = textColor;
                    GUI.Label(new Rect(guiPos.x - 20f, guiPos.y - 20f,
                        healthBarSize.x + 40f, 18f), distanceText, labelStyle);
                    GUI.color = Color.white;
                }
            }
        }

        protected virtual void DrawHealthBar(Vector2 position, TargetDummy target)
        {
            // Background
            GUI.color = healthBarBackground;
            GUI.DrawTexture(new Rect(position.x, position.y,
                healthBarSize.x, healthBarSize.y), Texture2D.whiteTexture);

            // Health fill
            float healthPercent = target.CurrentHealth / target.maxHealth;
            Color barColor = healthPercent > lowHealthThreshold
                ? healthBarColor : lowHealthColor;
            GUI.color = barColor;
            GUI.DrawTexture(new Rect(position.x, position.y,
                healthBarSize.x * healthPercent, healthBarSize.y),
                Texture2D.whiteTexture);

            // Health text
            GUI.color = textColor;
            string healthText = $"{Mathf.RoundToInt(target.CurrentHealth)}/{Mathf.RoundToInt(target.maxHealth)}";
            GUI.Label(new Rect(position.x - 10f, position.y - 15f,
                healthBarSize.x + 20f, 14f), healthText, labelStyle);

            GUI.color = Color.white;
        }
    }
}
