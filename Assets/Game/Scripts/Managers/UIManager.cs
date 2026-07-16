using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace WaveSurvival
{
    public class UIManager : MonoBehaviour
    {
        [Header("HUD References")]
        public Image healthFill;
        public Image healthBarBg;
        public TextMeshProUGUI healthText;
        public TextMeshProUGUI ammoText;
        public TextMeshProUGUI waveText;
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI enemiesText;
        public TextMeshProUGUI comboText;
        public TextMeshProUGUI killFeedText;
        public TextMeshProUGUI waveAnnounceText;
        public Image waveProgressFill;
        public Image waveProgressBg;

        [Header("Game Over")]
        public TextMeshProUGUI finalScoreText;
        public TextMeshProUGUI highScoreText;
        public Button restartButton;

        public float announceDuration = 2f;

        PlayerHealth playerHealth;
        WaveManager waveManager;
        Player.Gun gun;
        int killCombo;
        float comboTimer;
        List<string> killFeed = new List<string>();

        void Start()
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
            waveManager = FindFirstObjectByType<WaveManager>();
            gun = FindFirstObjectByType<Player.Gun>();

            // Auto-find UI
            FindUI();

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateHealthBar;
                playerHealth.OnLowHPWarning += OnLowHP;
                playerHealth.OnDamaged += OnPlayerDamaged;
            }

            if (waveManager != null)
            {
                waveManager.OnWaveStarted += OnWaveStarted;
                waveManager.OnWaveCleared += OnWaveCleared;
                waveManager.OnEnemyKilled += OnEnemyKilled;
            }

            if (restartButton != null)
                restartButton.onClick.AddListener(RestartGame);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameOver += ShowGameOver;
                GameManager.Instance.OnScoreChanged += UpdateScore;
            }

            if (waveAnnounceText != null)
                waveAnnounceText.gameObject.SetActive(false);

            if (comboText != null)
                comboText.gameObject.SetActive(false);

            if (killFeedText != null)
                killFeedText.text = "";
        }

        void FindUI()
        {
            Transform t;
            if (healthFill == null && (t = transform.Find("HealthBarBG/HealthFill")) != null)
                healthFill = t.GetComponent<Image>();
            if (healthBarBg == null && (t = transform.Find("HealthBarBG")) != null)
                healthBarBg = t.GetComponent<Image>();
            if (healthText == null && (t = transform.Find("HealthText")) != null)
                healthText = t.GetComponent<TextMeshProUGUI>();
            if (ammoText == null && (t = transform.Find("AmmoText")) != null)
                ammoText = t.GetComponent<TextMeshProUGUI>();
            if (waveText == null && (t = transform.Find("WaveText")) != null)
                waveText = t.GetComponent<TextMeshProUGUI>();
            if (scoreText == null && (t = transform.Find("ScoreText")) != null)
                scoreText = t.GetComponent<TextMeshProUGUI>();
            if (enemiesText == null && (t = transform.Find("EnemiesText")) != null)
                enemiesText = t.GetComponent<TextMeshProUGUI>();
            if (comboText == null && (t = transform.Find("ComboText")) != null)
                comboText = t.GetComponent<TextMeshProUGUI>();
            if (killFeedText == null && (t = transform.Find("KillFeed")) != null)
                killFeedText = t.GetComponent<TextMeshProUGUI>();
            if (waveAnnounceText == null && (t = transform.Find("WaveAnnounce")) != null)
                waveAnnounceText = t.GetComponent<TextMeshProUGUI>();
            if (waveProgressFill == null && (t = transform.Find("WaveProgressBG/WaveProgressFill")) != null)
                waveProgressFill = t.GetComponent<Image>();
            if (waveProgressBg == null && (t = transform.Find("WaveProgressBG")) != null)
                waveProgressBg = t.GetComponent<Image>();

            Transform panel = transform.Find("GameOverPanel");
            if (panel != null)
            {
                if (finalScoreText == null && (t = panel.Find("FinalScoreText")) != null)
                    finalScoreText = t.GetComponent<TextMeshProUGUI>();
                if (highScoreText == null && (t = panel.Find("HighScoreText")) != null)
                    highScoreText = t.GetComponent<TextMeshProUGUI>();
                if (restartButton == null && (t = panel.Find("RestartButton")) != null)
                    restartButton = t.GetComponent<Button>();
            }
        }

        void Update()
        {
            UpdateHUD();
            UpdateCombo();
        }

        void UpdateHUD()
        {
            if (gun != null && ammoText != null)
            {
                string reloading = gun.IsReloading ? " [RELOADING]" : "";
                ammoText.text = $"{gun.CurrentAmmo} / {gun.TotalReserve}{reloading}";
            }

            if (waveManager != null)
            {
                if (waveText != null)
                    waveText.text = $"WAVE {waveManager.CurrentWave}";
                if (enemiesText != null)
                    enemiesText.text = $"Enemies: {waveManager.GetEnemiesRemaining()}";
                if (waveProgressFill != null)
                    waveProgressFill.fillAmount = waveManager.GetWaveProgress();
            }
        }

        void UpdateHealthBar(float percent)
        {
            if (healthFill != null)
            {
                healthFill.fillAmount = percent;
                healthFill.color = percent > 0.5f ? Color.green :
                    percent > 0.25f ? Color.yellow : Color.red;
            }
            if (healthText != null)
            {
                float hp = playerHealth != null ? playerHealth.currentHealth : 0;
                healthText.text = Mathf.RoundToInt(hp).ToString();
            }
        }

        void UpdateScore(int score)
        {
            if (scoreText != null)
                scoreText.text = $"Score: {score}";
        }

        void OnLowHP(bool isLow)
        {
            if (healthBarBg != null)
                healthBarBg.color = isLow ? new Color(0.3f, 0.1f, 0.1f, 0.7f) : new Color(0.2f, 0.2f, 0.2f, 0.7f);
        }

        void OnPlayerDamaged()
        {
            // Screen shake effect handled by camera
        }

        void UpdateCombo()
        {
            if (killCombo > 0)
            {
                comboTimer -= Time.deltaTime;
                if (comboTimer <= 0f)
                {
                    killCombo = 0;
                    if (comboText != null)
                        comboText.gameObject.SetActive(false);
                }
            }
        }

        void OnEnemyKilled()
        {
            killCombo++;
            comboTimer = 3f;

            if (comboText != null)
            {
                comboText.gameObject.SetActive(true);
                comboText.text = $"{killCombo}x COMBO!";
                comboText.color = killCombo >= 10 ? Color.red :
                    killCombo >= 5 ? Color.yellow : Color.cyan;
            }

            // Kill feed
            string enemyName = "Enemy";
            killFeed.Insert(0, $"+{enemyName}");
            if (killFeed.Count > 5)
                killFeed.RemoveAt(killFeed.Count - 1);
            if (killFeedText != null)
                killFeedText.text = string.Join("\n", killFeed);
            CancelInvoke(nameof(ClearKillFeed));
            Invoke(nameof(ClearKillFeed), 4f);
        }

        void ClearKillFeed()
        {
            killFeed.Clear();
            if (killFeedText != null)
                killFeedText.text = "";
        }

        void OnWaveStarted(int wave)
        {
            if (waveAnnounceText != null)
            {
                bool isBoss = wave % 5 == 0;
                waveAnnounceText.gameObject.SetActive(true);
                waveAnnounceText.text = isBoss ? $"!!! BOSS WAVE {wave} !!!" : $"WAVE {wave}";
                waveAnnounceText.fontSize = isBoss ? 36 : 48;
                waveAnnounceText.color = isBoss ? Color.red : Color.yellow;
                Invoke(nameof(HideWaveAnnounce), announceDuration);
            }
        }

        void OnWaveCleared()
        {
            if (waveAnnounceText != null)
            {
                waveAnnounceText.gameObject.SetActive(true);
                waveAnnounceText.text = "WAVE CLEARED!";
                waveAnnounceText.color = Color.green;
                Invoke(nameof(HideWaveAnnounce), announceDuration);
            }
        }

        void HideWaveAnnounce()
        {
            if (waveAnnounceText != null)
                waveAnnounceText.gameObject.SetActive(false);
        }

        void ShowGameOver()
        {
            if (finalScoreText != null && GameManager.Instance != null)
                finalScoreText.text = $"Score: {GameManager.Instance.score}";
            if (highScoreText != null && GameManager.Instance != null)
                highScoreText.text = $"High Score: {GameManager.Instance.highScore}";

            Transform panel = transform.Find("GameOverPanel");
            if (panel != null)
            {
                panel.gameObject.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        void RestartGame()
        {
            Transform panel = transform.Find("GameOverPanel");
            if (panel != null)
                panel.gameObject.SetActive(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (GameManager.Instance != null)
                GameManager.Instance.RestartGame();

            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}
