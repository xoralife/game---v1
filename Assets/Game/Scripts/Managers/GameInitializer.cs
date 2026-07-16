using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WaveSurvival.Player;

namespace WaveSurvival
{
    public class GameInitializer : MonoBehaviour
    {
        public bool autoInit = true;

        void Start()
        {
            if (autoInit)
                InitializeGame();
        }

        void InitializeGame()
        {
            CreateArena();
            CreatePlayer();
            CreateManagers();
            CreateUI();
        }

        void CreateArena()
        {
            // Ground
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = Vector3.one * 5f;

            // Walls
            CreateWall(new Vector3(0f, 2f, -11f), new Vector3(24f, 4f, 0.5f));
            CreateWall(new Vector3(0f, 2f, 11f), new Vector3(24f, 4f, 0.5f));
            CreateWall(new Vector3(-11f, 2f, 0f), new Vector3(0.5f, 4f, 24f));
            CreateWall(new Vector3(11f, 2f, 0f), new Vector3(0.5f, 4f, 24f));

            // Lighting
            GameObject lightObj = new GameObject("DirectionalLight");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            light.intensity = 1f;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.3f, 0.3f, 0.35f);
        }

        void CreateWall(Vector3 pos, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall";
            wall.transform.position = pos;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().material.color = new Color(0.4f, 0.4f, 0.45f);
        }

        void CreatePlayer()
        {
            GameObject player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = Vector3.zero;

            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            player.AddComponent<PlayerController>();
            PlayerHealth health = player.AddComponent<PlayerHealth>();

            GameObject camObj = new GameObject("Camera");
            camObj.tag = "MainCamera";
            camObj.transform.SetParent(player.transform);
            camObj.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            Camera cam = camObj.AddComponent<Camera>();
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.01f;
            camObj.AddComponent<AudioListener>();

            // Gun
            GameObject gunObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gunObj.name = "Gun";
            gunObj.transform.SetParent(camObj.transform);
            gunObj.transform.localPosition = new Vector3(0.3f, -0.2f, 0.5f);
            gunObj.transform.localScale = new Vector3(0.05f, 0.05f, 0.3f);
            gunObj.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            Destroy(gunObj.GetComponent<BoxCollider>());
            gunObj.GetComponent<Renderer>().material.color = Color.gray;

            Gun gun = gunObj.AddComponent<Gun>();
            gun.shootMask = ~0; // everything

            // Gun muzzle
            GameObject muzzle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            muzzle.name = "MuzzleFlash";
            muzzle.transform.SetParent(gunObj.transform);
            muzzle.transform.localPosition = new Vector3(0f, 0f, 0.35f);
            muzzle.transform.localScale = Vector3.one * 0.03f;
            muzzle.GetComponent<Renderer>().material.color = Color.yellow;
            Destroy(muzzle.GetComponent<SphereCollider>());
            muzzle.SetActive(false);
        }

        void CreateManagers()
        {
            if (FindFirstObjectByType<GameManager>() == null)
            {
                GameObject gmObj = new GameObject("GameManager");
                gmObj.AddComponent<GameManager>();
            }

            GameObject waveObj = new GameObject("WaveManager");
            waveObj.AddComponent<EnemySpawner>();
            waveObj.AddComponent<WaveManager>();

            // Create enemy prefab programmatically
            CreateEnemyPrefab(waveObj.GetComponent<WaveManager>());
        }

        void CreateEnemyPrefab(WaveManager wm)
        {
            GameObject enemyPrefab = new GameObject("EnemyPrefab");
            enemyPrefab.SetActive(false);

            GameObject enemyBody = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemyBody.name = "Body";
            enemyBody.transform.SetParent(enemyPrefab.transform);
            enemyBody.transform.localPosition = Vector3.up * 1f;
            enemyBody.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            enemyBody.GetComponent<Renderer>().material.color = Color.red;
            enemyBody.GetComponent<CapsuleCollider>().isTrigger = false;

            Enemy enemy = enemyPrefab.AddComponent<Enemy>();
            enemy.maxHealth = 50f;

            wm.enemyPrefab = enemyPrefab;
        }

        void CreateUI()
        {
            GameObject uiObj = new GameObject("UIManager");
            uiObj.AddComponent<UIManager>();

            Canvas canvas = uiObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = uiObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            uiObj.AddComponent<GraphicRaycaster>();

            FontData defaultFont = new FontData();
            Color white = Color.white;

            // Create all UI elements
            CreateText(uiObj.transform, "HealthText", "HP: 100/100",
                new Vector2(20f, -20f), new Vector2(200f, 30f), 20, white, out _);
            CreateText(uiObj.transform, "AmmoText", "30 / 90",
                new Vector2(20f, -50f), new Vector2(200f, 30f), 18, white, out _);
            CreateText(uiObj.transform, "ScoreText", "Score: 0",
                new Vector2(Screen.width - 220f, -20f), new Vector2(200f, 30f), 22, white, out _);
            CreateText(uiObj.transform, "WaveText", "Wave: 1",
                new Vector2(Screen.width / 2f - 100f, -20f), new Vector2(200f, 30f), 24, white, out _);
            CreateText(uiObj.transform, "EnemiesText", "Enemies: 0",
                new Vector2(Screen.width / 2f - 100f, -50f), new Vector2(200f, 30f), 18, white, out _);

            // Wave announce (center of screen)
            CreateText(uiObj.transform, "WaveAnnounce", "WAVE 1",
                new Vector2(Screen.width / 2f - 150f, Screen.height / 2f - 30f),
                new Vector2(300f, 60f), 48, Color.yellow, out TextMeshProUGUI announceText);

            // Game Over Panel
            GameObject panel = new GameObject("GameOverPanel");
            panel.transform.SetParent(uiObj.transform);
            RectTransform panelRt = panel.AddComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.sizeDelta = Vector2.zero;
            Image panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0f, 0f, 0f, 0.8f);
            panel.SetActive(false);

            CreateText(panel.transform, "GameOverText", "GAME OVER",
                new Vector2(0f, 50f), new Vector2(400f, 80f), 64, Color.red, out _);
            CreateText(panel.transform, "FinalScoreText", "Score: 0",
                new Vector2(0f, -30f), new Vector2(400f, 40f), 36, white, out TextMeshProUGUI finalScore);
            CreateText(panel.transform, "HighScoreText", "High Score: 0",
                new Vector2(0f, -80f), new Vector2(400f, 40f), 28, Color.yellow, out TextMeshProUGUI highScore);

            // Restart button
            GameObject btnObj = new GameObject("RestartButton");
            btnObj.transform.SetParent(panel.transform);
            RectTransform btnRt = btnObj.AddComponent<RectTransform>();
            btnRt.sizeDelta = new Vector2(200f, 50f);
            btnRt.anchoredPosition = new Vector2(0f, -140f);
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.5f, 0.2f);
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.transition = Selectable.Transition.ColorTint;

            CreateText(btnObj.transform, "RestartText", "RESTART",
                Vector2.zero, new Vector2(200f, 50f), 24, white, out _);
        }

        void CreateText(Transform parent, string name, string text,
            Vector2 pos, Vector2 size, int fontSize, Color color,
            out TextMeshProUGUI tmpText)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            tmpText = go.AddComponent<TextMeshProUGUI>();
            tmpText.text = text;
            tmpText.fontSize = fontSize;
            tmpText.color = color;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.fontStyle = FontStyles.Bold;
        }

        struct FontData
        {
        }
    }
}
