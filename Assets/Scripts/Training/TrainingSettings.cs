using UnityEngine;

namespace FPSTrainingRoom.Training
{
    [CreateAssetMenu(fileName = "NewTrainingSettings", menuName = "FPS Training/Training Settings", order = 2)]
    public class TrainingSettings : ScriptableObject
    {
        [Header("Training Mode")]
        public bool enableFreeMode = true;
        public bool enableWaveMode = false;
        public bool enableChallengeMode = false;

        [Header("Difficulty")]
        [Range(0.1f, 3f)]
        public float difficultyMultiplier = 1f;
        public bool adaptiveDifficulty = false;
        public float adaptiveRate = 0.1f;

        [Header("Target Settings")]
        public float targetHealthMultiplier = 1f;
        public float targetSpeedMultiplier = 1f;
        public float targetSpawnRate = 2f;
        public int maxConcurrentTargets = 10;

        [Header("Player Settings")]
        public float playerMoveSpeed = 5f;
        public float playerSprintMultiplier = 1.5f;
        public float playerHealth = 100f;
        public bool infiniteAmmoDefault = false;
        public bool godModeDefault = false;

        [Header("Weapon Settings")]
        public float recoilMultiplier = 1f;
        public float spreadMultiplier = 1f;
        public float damageMultiplier = 1f;

        [Header("Aim Assist Settings")]
        public bool aimAssistEnabled = false;
        public bool targetLockEnabled = false;
        public bool bulletMagnetismEnabled = false;
        public bool heatSeekingEnabled = false;
        [Range(0f, 1f)]
        public float aimAssistStrength = 0.5f;

        [Header("Visual Settings")]
        public bool showRecoilPattern = true;
        public bool showShotGroupings = true;
        public bool showAimAssistDebug = false;
        public bool showTargetInfo = true;

        [Header("Time Settings")]
        [Range(0.1f, 2f)]
        public float timeScale = 1f;

        public virtual void ApplySettings()
        {
            Time.timeScale = timeScale;

            var manager = TrainingRoomManager.Instance;
            if (manager == null) return;

            manager.infiniteAmmo = infiniteAmmoDefault;
            manager.godMode = godModeDefault;
            manager.showRecoilPatterns = showRecoilPattern;
            manager.showAimAssistVisuals = showAimAssistDebug;

            var patterns = FindObjectsByType<Weapons.SprayPattern>(
                FindObjectsSortMode.None);
            foreach (var p in patterns)
                p.intensityMultiplier = recoilMultiplier;

            var assists = FindObjectsByType<Targeting.AimAssist>(
                FindObjectsSortMode.None);
            foreach (var a in assists)
            {
                a.enabled = aimAssistEnabled;
                a.stickyStrength = aimAssistStrength;
            }

            var locks = FindObjectsByType<Targeting.TargetLock>(
                FindObjectsSortMode.None);
            foreach (var l in locks)
                l.enableTargetLock = targetLockEnabled;

            var magnets = FindObjectsByType<Targeting.BulletMagnetism>(
                FindObjectsSortMode.None);
            foreach (var m in magnets)
                m.enableMagnetism = bulletMagnetismEnabled;
        }
    }
}
