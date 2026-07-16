using UnityEngine;
using System;
using FPSTrainingRoom.Weapons;

namespace FPSTrainingRoom.Training
{
    public class StatsTracker : MonoBehaviour
    {
        [Header("Current Session Stats")]
        public int TotalKills { get; protected set; }
        public int TotalDeaths { get; protected set; }
        public float TotalDamageDealt { get; protected set; }
        public float TotalDamageTaken { get; protected set; }
        public int TotalShotsFired { get; protected set; }
        public int TotalShotsHit { get; protected set; }
        public int TotalHeadshots { get; protected set; }
        public int TotalScore { get; protected set; }

        [Header("Combo Tracking")]
        public int currentCombo = 0;
        public int maxCombo = 0;
        public float comboTimer = 0f;
        public float comboTimeout = 3f;

        [Header("Accuracy Tracking")]
        public float sessionStartTime;

        public System.Action<int> OnKill;
        public System.Action<int> OnComboUpdate;
        public System.Action<int> OnScoreUpdate;

        protected virtual void Start()
        {
            sessionStartTime = Time.time;
            var dummies = FindObjectsByType<TargetDummy>(
                FindObjectsSortMode.None);
            foreach (var dummy in dummies)
            {
                dummy.OnHitCallback += OnTargetHit;
                dummy.OnDestroyedEvent.AddListener(() => OnTargetKilled(dummy));
            }
        }

        protected virtual void Update()
        {
            if (currentCombo > 0)
            {
                comboTimer -= Time.deltaTime;
                if (comboTimer <= 0f)
                {
                    currentCombo = 0;
                    OnComboUpdate?.Invoke(0);
                }
            }
        }

        public virtual void RegisterShot()
        {
            TotalShotsFired++;
        }

        public virtual void RegisterHit(float damage, bool isHeadshot = false)
        {
            TotalShotsHit++;
            TotalDamageDealt += damage;

            if (isHeadshot)
            {
                TotalHeadshots++;
                currentCombo += 2;
            }
            else
            {
                currentCombo++;
            }

            comboTimer = comboTimeout;

            if (currentCombo > maxCombo)
                maxCombo = currentCombo;

            OnComboUpdate?.Invoke(currentCombo);

            int scoreGained = Mathf.RoundToInt(damage) * (1 + currentCombo / 5);
            TotalScore += scoreGained;
            OnScoreUpdate?.Invoke(TotalScore);
        }

        protected virtual void OnTargetHit(float damage, Vector3 point, Vector3 direction)
        {
            bool isHeadshot = false;
            RegisterHit(damage, isHeadshot);
        }

        protected virtual void OnTargetKilled(TargetDummy target)
        {
            TotalKills++;
            int killScore = target.scoreValue * (1 + currentCombo);
            TotalScore += killScore;
            OnKill?.Invoke(TotalKills);
            OnScoreUpdate?.Invoke(TotalScore);
        }

        public virtual float GetAccuracy()
        {
            if (TotalShotsFired == 0) return 0f;
            return (float)TotalShotsHit / TotalShotsFired * 100f;
        }

        public virtual float GetKDRatio()
        {
            if (TotalDeaths == 0) return TotalKills;
            return (float)TotalKills / TotalDeaths;
        }

        public virtual float GetSessionTime()
        {
            return Time.time - sessionStartTime;
        }

        public virtual float GetDamagePerShot()
        {
            if (TotalShotsHit == 0) return 0f;
            return TotalDamageDealt / TotalShotsHit;
        }

        public virtual void ResetStats()
        {
            TotalKills = 0;
            TotalDeaths = 0;
            TotalDamageDealt = 0f;
            TotalDamageTaken = 0f;
            TotalShotsFired = 0;
            TotalShotsHit = 0;
            TotalHeadshots = 0;
            TotalScore = 0;
            currentCombo = 0;
            maxCombo = 0;
            comboTimer = 0f;
            sessionStartTime = Time.time;
        }
    }
}
