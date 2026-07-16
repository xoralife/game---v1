using UnityEngine;

namespace WaveSurvival
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public int score;
        public int highScore;
        public bool isGameOver;

        public System.Action OnGameOver;
        public System.Action OnGameRestart;
        public System.Action<int> OnScoreChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            highScore = PlayerPrefs.GetInt("HighScore", 0);
        }

        public void AddScore(int amount)
        {
            score += amount;
            OnScoreChanged?.Invoke(score);
        }

        public void GameOver()
        {
            isGameOver = true;
            if (score > highScore)
            {
                highScore = score;
                PlayerPrefs.SetInt("HighScore", highScore);
            }
            OnGameOver?.Invoke();
        }

        public void RestartGame()
        {
            score = 0;
            isGameOver = false;
            OnGameRestart?.Invoke();
        }
    }
}
