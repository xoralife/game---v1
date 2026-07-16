using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace WaveSurvival
{
    public class MainMenu : MonoBehaviour
    {
        [Header("Menu UI")]
        public GameObject menuPanel;
        public Button playButton;
        public Button quitButton;
        public TextMeshProUGUI highScoreText;
        public TextMeshProUGUI controlsText;

        void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (playButton != null)
                playButton.onClick.AddListener(PlayGame);

            if (quitButton != null)
                quitButton.onClick.AddListener(QuitGame);

            if (highScoreText != null)
                highScoreText.text = $"High Score: {PlayerPrefs.GetInt("HighScore", 0)}";

            if (controlsText != null)
                controlsText.text = "CONTROLS\n" +
                    "WASD - Move\n" +
                    "Shift - Sprint\n" +
                    "Space - Jump\n" +
                    "Mouse - Look\n" +
                    "Left Click - Shoot\n" +
                    "R - Reload\n\n" +
                    "Survive waves of enemies!\n" +
                    "Boss every 5 waves!";
        }

        void PlayGame()
        {
            SceneManager.LoadScene("WaveSurvival");
        }

        void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public static void CreateMainMenuScene()
        {
            GameObject menuObj = new GameObject("MainMenu");
            menuObj.AddComponent<MainMenu>();

            Canvas canvas = menuObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            menuObj.AddComponent<CanvasScaler>();
            menuObj.AddComponent<GraphicRaycaster>();

            // Background panel
            GameObject bgPanel = new GameObject("Background");
            bgPanel.transform.SetParent(menuObj.transform);
            RectTransform bgRt = bgPanel.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            Image bgImg = bgPanel.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.1f, 0.12f);

            // Title
            CreateMenuText(menuObj.transform, "TitleText", "WAVE SURVIVAL",
                new Vector2(0f, 150f), new Vector2(600f, 80f), 56, Color.red);

            // High score
            CreateMenuText(menuObj.transform, "HighScoreText",
                $"High Score: {PlayerPrefs.GetInt("HighScore", 0)}",
                new Vector2(0f, 80f), new Vector2(400f, 40f), 24, Color.yellow);

            // Controls
            CreateMenuText(menuObj.transform, "ControlsText",
                "WASD - Move | Shift - Sprint | Space - Jump\n" +
                "Left Click - Shoot | R - Reload | Mouse - Look\n\n" +
                "Survive waves! Boss every 5 waves!",
                new Vector2(0f, -40f), new Vector2(500f, 120f), 18, Color.white);

            // Play button
            CreateMenuButton(menuObj.transform, "PlayButton", "PLAY",
                new Vector2(0f, -180f), new Vector2(220f, 55f),
                new Color(0.15f, 0.5f, 0.15f));

            // Quit button
            CreateMenuButton(menuObj.transform, "QuitButton", "QUIT",
                new Vector2(0f, -250f), new Vector2(220f, 55f),
                new Color(0.5f, 0.15f, 0.15f));
        }

        static void CreateMenuText(Transform parent, string name, string text,
            Vector2 pos, Vector2 size, int fontSize, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
        }

        static void CreateMenuButton(Transform parent, string name, string label,
            Vector2 pos, Vector2 size, Color color)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent);
            RectTransform btnRt = btnObj.AddComponent<RectTransform>();
            btnRt.anchoredPosition = pos;
            btnRt.sizeDelta = size;
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = color;
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.transition = Selectable.Transition.ColorTint;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform);
            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 28;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
        }
    }
}
