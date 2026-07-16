using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WaveSurvival
{
    public class UIManager : MonoBehaviour
    {
        [Header("HUD")]
        public TextMeshProUGUI healthText;
        public TextMeshProUGUI ammoText;
        public TextMeshProUGUI waveText;
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI enemiesRemainingText;
        public Slider healthSlider;

        [Header("Game Over")]
        public GameObject gameOverPanel;
        public TextMeshProUGUI finalScoreText;
        public TextMeshProUGUI highScoreText;
        public Button restartButton;

        [Header("Wave Announce")]
        public TextMeshProUGUI waveAnnounceText;
        public float announceDuration = 2f;

        PlayerHealth playerHealth;
        WaveManager waveManager;
        Player.Gun gun;

        void Start()
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
            waveManager = FindFirstObjectByType<WaveManager>();
            gun = FindFirstObjectByType<Player.Gun>();

            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            if (restartButton != null)
                restartButton.onClick.AddListener(RestartGame);

            if (waveManager != null)
            {
                waveManager.OnWaveStarted += OnWaveStarted;
                waveManager.OnWaveCleared += OnWaveCleared;
                waveManager.OnEnemyKilled += OnEnemyKilled;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameOver += ShowGameOver;
                GameManager.Instance.OnScoreChanged += UpdateScore;
            }

            if (waveAnnounceText != null)
                waveAnnounceText.gameObject.SetActive(false);
        }

        void Update()
        {
            UpdateHUD();
        }

        void UpdateHUD()
        {
            if (playerHealth != null)
            {
                float hp = playerHealth.currentHealth;
                float maxHp = playerHealth.maxHealth;
                if (healthText != null)
                    healthText.text = $"HP: {Mathf.RoundToInt(hp)}/{Mathf.RoundToInt(maxHp)}";
                if (healthSlider != null)
                    healthSlider.value = hp / maxHp;
            }

            if (gun != null)
            {
                if (ammoText != null)
                    ammoText.text = $"{gun.currentAmmo} / {gun.totalReserve}";
            }

            if (waveManager != null)
            {
                if (waveText != null)
                    waveText.text = $"Wave: {waveManager.currentWave}";
                if (enemiesRemainingText != null)
                    enemiesRemainingText.text = $"Enemies: {waveManager.GetEnemiesRemaining()}";
            }
        }

        void UpdateScore(int score)
        {
            if (scoreText != null)
                scoreText.text = $"Score: {score}";
        }

        void OnWaveStarted(int wave)
        {
            if (waveAnnounceText != null)
            {
                waveAnnounceText.gameObject.SetActive(true);
                waveAnnounceText.text = $"WAVE {wave}";
                Invoke(nameof(HideWaveAnnounce), announceDuration);
            }
        }

        void OnWaveCleared()
        {
            if (waveAnnounceText != null)
            {
                waveAnnounceText.gameObject.SetActive(true);
                waveAnnounceText.text = "WAVE CLEARED!";
                Invoke(nameof(HideWaveAnnounce), announceDuration);
            }
        }

        void OnEnemyKilled()
        {
            // Sounds or visual feedback
        }

        void HideWaveAnnounce()
        {
            if (waveAnnounceText != null)
                waveAnnounceText.gameObject.SetActive(false);
        }

        void ShowGameOver()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (finalScoreText != null && GameManager.Instance != null)
                finalScoreText.text = $"Score: {GameManager.Instance.score}";

            if (highScoreText != null && GameManager.Instance != null)
                highScoreText.text = $"High Score: {GameManager.Instance.highScore}";
        }

        void RestartGame()
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (GameManager.Instance != null)
                GameManager.Instance.RestartGame();

            // Reload scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}
