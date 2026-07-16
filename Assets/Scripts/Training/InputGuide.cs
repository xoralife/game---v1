using UnityEngine;

namespace FPSTrainingRoom.Training
{
    public class InputGuide : MonoBehaviour
    {
        [Header("Display Settings")]
        public bool showOnStart = true;
        public KeyCode toggleKey = KeyCode.F1;
        public Vector2 guidePosition = new Vector2(Screen.width / 2f - 150f, 10f);
        public Vector2 guideSize = new Vector2(300f, 250f);
        public Color backgroundColor = new Color(0f, 0f, 0f, 0.7f);
        public Color textColor = Color.white;

        protected bool isVisible;
        protected string guideText;

        protected virtual void Start()
        {
            guideText = BuildGuideText();
            isVisible = showOnStart;
        }

        protected virtual void Update()
        {
            if (Input.GetKeyDown(toggleKey))
                isVisible = !isVisible;
        }

        protected virtual string BuildGuideText()
        {
            return
@"=== FPS TRAINING ROOM ===

MOVEMENT:
  WASD          - Move
  Shift         - Sprint
  Ctrl          - Crouch
  Space         - Jump
  Mouse         - Look

WEAPONS:
  Left Click    - Fire
  Right Click   - Aim Down Sights
  R             - Reload
  1-3           - Switch Weapon
  Scroll Wheel  - Switch Weapon

TRAINING:
  Tab/Esc       - Toggle Control Panel
  F1            - Toggle This Guide
  F2            - Reset Training
  F3            - Toggle Infinite Ammo
  F4            - Toggle Recoil Visualization

CONTROLS:
  Mouse Sens    - Control Panel > Settings
  Time Scale    - Control Panel > Speed
  Recoil        - Control Panel > Recoil
  Aim Assist    - Control Panel > Targeting";
        }

        protected virtual void OnGUI()
        {
            if (!isVisible) return;

            GUI.color = backgroundColor;
            GUI.DrawTexture(new Rect(guidePosition.x, guidePosition.y,
                guideSize.x, guideSize.y), Texture2D.whiteTexture);

            GUI.color = textColor;
            GUIStyle style = new GUIStyle();
            style.fontSize = 12;
            style.normal.textColor = textColor;
            style.alignment = TextAnchor.UpperLeft;
            style.padding = new RectOffset(10, 10, 10, 10);

            GUI.Label(new Rect(guidePosition.x, guidePosition.y,
                guideSize.x, guideSize.y), guideText, style);

            GUI.color = Color.white;
        }
    }
}
