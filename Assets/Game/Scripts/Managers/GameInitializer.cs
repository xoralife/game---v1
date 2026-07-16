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
            // Ground with checker pattern
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = Vector3.one * 5f;
            Material groundMat = new Material(Shader.Find("Standard"));
            groundMat.color = new Color(0.25f, 0.25f, 0.3f);
            ground.GetComponent<Renderer>().material = groundMat;

            // Arena floor grid lines using quads
            for (int i = -4; i <= 4; i++)
            {
                CreateLine(new Vector3(i * 2.5f, 0.01f, -11f), new Vector3(0.05f, 0.02f, 22f), new Color(0.3f, 0.3f, 0.35f, 0.5f));
                CreateLine(new Vector3(-11f, 0.01f, i * 2.5f), new Vector3(22f, 0.02f, 0.05f), new Color(0.3f, 0.3f, 0.35f, 0.5f));
            }

            // Walls with material
            Color wallColor = new Color(0.35f, 0.35f, 0.4f);
            CreateWall(new Vector3(0f, 2f, -11f), new Vector3(24f, 4f, 0.5f), wallColor);
            CreateWall(new Vector3(0f, 2f, 11f), new Vector3(24f, 4f, 0.5f), wallColor);
            CreateWall(new Vector3(-11f, 2f, 0f), new Vector3(0.5f, 4f, 24f), wallColor);
            CreateWall(new Vector3(11f, 2f, 0f), new Vector3(0.5f, 4f, 24f), wallColor);

            // Cover objects (barriers)
            CreateCover(new Vector3(-4f, 0.75f, -3f), new Vector3(2f, 1.5f, 0.3f), new Color(0.5f, 0.35f, 0.2f));
            CreateCover(new Vector3(4f, 0.75f, -3f), new Vector3(2f, 1.5f, 0.3f), new Color(0.5f, 0.35f, 0.2f));
            CreateCover(new Vector3(-3f, 1f, 5f), new Vector3(1.5f, 2f, 0.3f), new Color(0.4f, 0.3f, 0.2f));
            CreateCover(new Vector3(3f, 0.5f, 5f), new Vector3(3f, 1f, 0.3f), new Color(0.4f, 0.3f, 0.2f));
            CreateCover(new Vector3(0f, 0.5f, -6f), new Vector3(4f, 1f, 0.3f), new Color(0.4f, 0.3f, 0.2f));

            // Lighting
            GameObject lightObj = new GameObject("DirectionalLight");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            light.intensity = 1.2f;
            light.shadowStrength = 0.8f;

            // Ambient light
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.35f, 0.35f, 0.4f);
        }

        void CreateLine(Vector3 pos, Vector3 scale, Color color)
        {
            GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.name = "GridLine";
            line.transform.position = pos;
            line.transform.localScale = scale;
            line.GetComponent<Renderer>().material.color = color;
            Destroy(line.GetComponent<BoxCollider>());
        }

        void CreateWall(Vector3 pos, Vector3 scale, Color color)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall";
            wall.transform.position = pos;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().material.color = color;
        }

        void CreateCover(Vector3 pos, Vector3 scale, Color color)
        {
            GameObject cover = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cover.name = "Cover";
            cover.transform.position = pos;
            cover.transform.localScale = scale;
            cover.GetComponent<Renderer>().material.color = color;
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

            GameObject gunObj = new GameObject("Gun");
            gunObj.transform.SetParent(camObj.transform);
            gunObj.transform.localPosition = new Vector3(0.3f, -0.25f, 0.4f);

            GameObject gunBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gunBody.name = "GunBody";
            gunBody.transform.SetParent(gunObj.transform);
            gunBody.transform.localPosition = Vector3.zero;
            gunBody.transform.localScale = new Vector3(0.04f, 0.04f, 0.25f);
            gunBody.GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.22f);
            Destroy(gunBody.GetComponent<BoxCollider>());

            GameObject gunBarrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            gunBarrel.name = "GunBarrel";
            gunBarrel.transform.SetParent(gunObj.transform);
            gunBarrel.transform.localPosition = new Vector3(0f, 0f, 0.22f);
            gunBarrel.transform.localScale = new Vector3(0.015f, 0.08f, 0.015f);
            gunBarrel.GetComponent<Renderer>().material.color = new Color(0.15f, 0.15f, 0.17f);
            Destroy(gunBarrel.GetComponent<CapsuleCollider>());

            GameObject gunHandle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gunHandle.name = "GunHandle";
            gunHandle.transform.SetParent(gunObj.transform);
            gunHandle.transform.localPosition = new Vector3(0f, -0.03f, -0.05f);
            gunHandle.transform.localScale = new Vector3(0.03f, 0.04f, 0.04f);
            gunHandle.GetComponent<Renderer>().material.color = new Color(0.3f, 0.2f, 0.1f);
            Destroy(gunHandle.GetComponent<BoxCollider>());

            Gun gun = gunObj.AddComponent<Gun>();
            gun.shootMask = ~0;

            // Muzzle flash object
            GameObject muzzle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            muzzle.name = "MuzzleFlash";
            muzzle.transform.SetParent(gunObj.transform);
            muzzle.transform.localPosition = new Vector3(0f, 0f, 0.3f);
            muzzle.transform.localScale = Vector3.one * 0.05f;
            muzzle.GetComponent<Renderer>().material.color = Color.yellow;
            Destroy(muzzle.GetComponent<SphereCollider>());
            muzzle.SetActive(false);
            gun.muzzleFlashObj = muzzle;
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
            enemy.bodyRenderer = enemyBody.GetComponent<Renderer>();
            enemy.originalColor = Color.red;

            wm.enemyPrefab = enemyPrefab;
        }

        void CreateUI()
        {
            GameObject uiObj = new GameObject("UIManager");
            UIManager ui = uiObj.AddComponent<UIManager>();

            Canvas canvas = uiObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = uiObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            uiObj.AddComponent<GraphicRaycaster>();

            // Health bar (left side)
            GameObject hpBarObj = new GameObject("HealthBarBG");
            hpBarObj.transform.SetParent(uiObj.transform);
            RectTransform hpBarBgRt = hpBarObj.AddComponent<RectTransform>();
            hpBarBgRt.anchoredPosition = new Vector2(30f, -40f);
            hpBarBgRt.sizeDelta = new Vector2(250f, 25f);
            Image hpBarBg = hpBarObj.AddComponent<Image>();
            hpBarBg.color = new Color(0.2f, 0.2f, 0.2f, 0.7f);
            ui.healthBarBg = hpBarBg;

            GameObject hpFillObj = new GameObject("HealthFill");
            hpFillObj.transform.SetParent(hpBarObj.transform);
            RectTransform hpFillRt = hpFillObj.AddComponent<RectTransform>();
            hpFillRt.anchoredPosition = Vector2.zero;
            hpFillRt.sizeDelta = new Vector2(250f, 25f);
            hpFillRt.pivot = new Vector2(0f, 0.5f);
            Image hpFill = hpFillObj.AddComponent<Image>();
            hpFill.color = Color.green;
            ui.healthFill = hpFill;

            // HP text
            CreateText(uiObj.transform, "HealthText", "100",
                new Vector2(290f, -40f), new Vector2(60f, 25f), 18, Color.white, out ui.healthText);

            // Ammo (bottom-center)
            CreateText(uiObj.transform, "AmmoText", "30 / 90",
                new Vector2(Screen.width / 2f - 60f, -Screen.height + 50f), new Vector2(120f, 30f), 22, Color.white, out ui.ammoText);
            ui.ammoText.alignment = TextAlignmentOptions.Center;

            // Wave + Enemy count (top-center)
            CreateText(uiObj.transform, "WaveText", "WAVE 1",
                new Vector2(Screen.width / 2f - 100f, -10f), new Vector2(200f, 25f), 26, Color.yellow, out ui.waveText);
            CreateText(uiObj.transform, "EnemiesText", "Enemies: 0",
                new Vector2(Screen.width / 2f - 80f, -35f), new Vector2(160f, 20f), 16, Color.white, out ui.enemiesText);

            // Score (top-right)
            CreateText(uiObj.transform, "ScoreText", "Score: 0",
                new Vector2(Screen.width - 200f, -15f), new Vector2(180f, 25f), 22, Color.white, out ui.scoreText);

            // Combo (center-right)
            CreateText(uiObj.transform, "ComboText", "",
                new Vector2(Screen.width / 2f + 120f, Screen.height / 2f - 20f), new Vector2(100f, 40f), 32, Color.cyan, out ui.comboText);

            // Kill feed (top-left area)
            CreateText(uiObj.transform, "KillFeed", "",
                new Vector2(20f, -80f), new Vector2(250f, 100f), 14, Color.white, out ui.killFeedText);
            ui.killFeedText.alignment = TextAlignmentOptions.UpperLeft;

            // Wave announce (center)
            CreateText(uiObj.transform, "WaveAnnounce", "WAVE 1",
                new Vector2(Screen.width / 2f - 150f, Screen.height / 2f - 30f),
                new Vector2(300f, 60f), 48, Color.yellow, out ui.waveAnnounceText);

            // Wave progress bar (top-center, under wave text)
            GameObject waveBarObj = new GameObject("WaveProgressBG");
            waveBarObj.transform.SetParent(uiObj.transform);
            RectTransform waveBarBgRt = waveBarObj.AddComponent<RectTransform>();
            waveBarBgRt.anchoredPosition = new Vector2(Screen.width / 2f - 100f, -55f);
            waveBarBgRt.sizeDelta = new Vector2(200f, 6f);
            Image waveBarBg = waveBarObj.AddComponent<Image>();
            waveBarBg.color = new Color(0.2f, 0.2f, 0.2f, 0.6f);
            ui.waveProgressBg = waveBarBg;

            GameObject waveFillObj = new GameObject("WaveProgressFill");
            waveFillObj.transform.SetParent(waveBarObj.transform);
            RectTransform waveFillRt = waveFillObj.AddComponent<RectTransform>();
            waveFillRt.anchoredPosition = Vector2.zero;
            waveFillRt.sizeDelta = new Vector2(200f, 6f);
            waveFillRt.pivot = new Vector2(0f, 0.5f);
            Image waveFill = waveFillObj.AddComponent<Image>();
            waveFill.color = Color.yellow;
            ui.waveProgressFill = waveFill;

            // Game Over Panel
            GameObject panel = new GameObject("GameOverPanel");
            panel.transform.SetParent(uiObj.transform);
            RectTransform panelRt = panel.AddComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.sizeDelta = Vector2.zero;
            Image panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0f, 0f, 0f, 0.85f);
            panel.SetActive(false);

            CreateText(panel.transform, "GameOverText", "GAME OVER",
                new Vector2(0f, 50f), new Vector2(400f, 80f), 64, Color.red, out _);
            CreateText(panel.transform, "FinalScoreText", "Score: 0",
                new Vector2(0f, -30f), new Vector2(400f, 40f), 36, Color.white, out ui.finalScoreText);
            CreateText(panel.transform, "HighScoreText", "High Score: 0",
                new Vector2(0f, -80f), new Vector2(400f, 40f), 28, Color.yellow, out ui.highScoreText);

            // Restart button
            GameObject btnObj = new GameObject("RestartButton");
            btnObj.transform.SetParent(panel.transform);
            RectTransform btnRt = btnObj.AddComponent<RectTransform>();
            btnRt.sizeDelta = new Vector2(220f, 55f);
            btnRt.anchoredPosition = new Vector2(0f, -150f);
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.15f, 0.5f, 0.15f);
            btnImg.sprite = null;
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.transition = Selectable.Transition.ColorTint;

            CreateText(btnObj.transform, "RestartText", "RESTART",
                Vector2.zero, new Vector2(220f, 55f), 28, Color.white, out _);
            ui.restartButton = btn;
        }

        void CreateText(Transform parent, string name, string text,
            Vector2 pos, Vector2 size, int fontSize, Color color, out TextMeshProUGUI tmpText)
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
    }
}
