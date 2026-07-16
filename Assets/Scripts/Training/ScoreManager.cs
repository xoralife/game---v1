using UnityEngine;
using System.Collections.Generic;

namespace FPSTrainingRoom.Training
{
    public class ScoreManager : MonoBehaviour
    {
        [Header("Scoring Rules")]
        public int killScore = 100;
        public int headshotBonus = 50;
        public int comboMultiplier = 10;
        public float distanceBonusMultiplier = 1f;

        [Header("Rankings")]
        public string[] rankTitles = new string[]
        {
            "Bronze", "Silver", "Gold", "Platinum", "Diamond", "Master"
        };
        public int[] rankThresholds = new int[]
        {
            1000, 2500, 5000, 10000, 20000, 50000
        };

        [Header("Session")]
        public int currentScore;
        public int highScore;
        public int currentRank;
        public int totalScoreAllTime;

        public System.Action<int> OnScoreChanged;
        public System.Action<int> OnRankChanged;

        public static ScoreManager Instance { get; protected set; }

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        protected virtual void Start()
        {
            totalScoreAllTime = PlayerPrefs.GetInt("TotalScore", 0);
            highScore = PlayerPrefs.GetInt("HighScore", 0);
        }

        public virtual void AddScore(int amount)
        {
            currentScore += amount;
            totalScoreAllTime += amount;

            if (currentScore > highScore)
            {
                highScore = currentScore;
                PlayerPrefs.SetInt("HighScore", highScore);
            }

            PlayerPrefs.SetInt("TotalScore", totalScoreAllTime);

            int newRank = CalculateRank();
            if (newRank != currentRank)
            {
                currentRank = newRank;
                OnRankChanged?.Invoke(currentRank);
            }

            OnScoreChanged?.Invoke(currentScore);
        }

        public virtual void AddKillScore(float distance, bool isHeadshot, int combo)
        {
            int score = killScore;

            float distanceFactor = 1f + (distance * distanceBonusMultiplier * 0.01f);
            score = Mathf.RoundToInt(score * distanceFactor);

            if (isHeadshot)
                score += headshotBonus;

            score += combo * comboMultiplier;

            AddScore(score);
        }

        public virtual int CalculateRank()
        {
            for (int i = rankThresholds.Length - 1; i >= 0; i--)
            {
                if (currentScore >= rankThresholds[i])
                    return i;
            }
            return 0;
        }

        public virtual string GetRankTitle(int rank)
        {
            if (rank >= 0 && rank < rankTitles.Length)
                return rankTitles[rank];
            return "Unranked";
        }

        public virtual int GetScoreToNextRank()
        {
            int nextRank = currentRank + 1;
            if (nextRank >= rankThresholds.Length)
                return 0;
            return rankThresholds[nextRank] - currentScore;
        }

        public virtual void ResetSession()
        {
            currentScore = 0;
            currentRank = 0;
            OnScoreChanged?.Invoke(0);
            OnRankChanged?.Invoke(0);
        }
    }
}
