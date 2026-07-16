using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FPSTrainingRoom.Weapons;
using FPSTrainingRoom.Targeting;

namespace FPSTrainingRoom.Training
{
    public class TrainingUI : MonoBehaviour
    {
        [Header("Panel References")]
        public GameObject controlPanel;
        public GameObject recoilPanel;
        public GameObject targetingPanel;
        public GameObject statsPanel;

        [Header("Recoil Controls")]
        public Slider recoilIntensitySlider;
        public Toggle compensationToggle;
        public Toggle patternVisualizationToggle;
        public TextMeshProUGUI recoilIntensityLabel;

        [Header("Targeting Controls")]
        public Toggle aimAssistToggle;
        public Toggle targetLockToggle;
        public Toggle bulletMagnetismToggle;
        public Toggle heatSeekingToggle;
        public Slider magnetismStrengthSlider;
        public TextMeshProUGUI magnetismStrengthLabel;

        [Header("Training Controls")]
        public Toggle infiniteAmmoToggle;
        public Toggle godModeToggle;
        public Slider timeScaleSlider;
        public TextMeshProUGUI timeScaleLabel;
        public Button resetButton;
        public Button nextTargetGroupButton;

        [Header("Stats Display")]
        public TextMeshProUGUI accuracyText;
        public TextMeshProUGUI killsText;
        public TextMeshProUGUI damageDealtText;
        public TextMeshProUGUI shotsFiredText;
        public TextMeshProUGUI shotsHitText;
        public TextMeshProUGUI scoreText;

        [Header("Weapon Info")]
        public TextMeshProUGUI weaponNameText;
        public TextMeshProUGUI ammoText;
        public TextMeshProUGUI fireModeText;

        protected StatsTracker statsTracker;
        protected TrainingRoomManager roomManager;
        protected WeaponBase currentWeapon;
        protected bool isPanelVisible = true;

        protected virtual void Start()
        {
            roomManager = TrainingRoomManager.Instance;

            statsTracker = GetComponent<StatsTracker>();
            if (statsTracker == null)
                statsTracker = FindFirstObjectByType<StatsTracker>();

            SetupUIListeners();
            UpdateUIReferences();
        }

        protected virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape))
            {
                isPanelVisible = !isPanelVisible;
                if (controlPanel != null)
                    controlPanel.SetActive(isPanelVisible);
            }

            if (isPanelVisible)
            {
                UpdateStats();
                UpdateWeaponInfo();
            }
        }

        protected virtual void SetupUIListeners()
        {
            if (recoilIntensitySlider != null)
            {
                recoilIntensitySlider.onValueChanged.AddListener(OnRecoilIntensityChanged);
                recoilIntensitySlider.value = 1f;
            }

            if (compensationToggle != null)
            {
                compensationToggle.onValueChanged.AddListener(OnCompensationToggled);
            }

            if (patternVisualizationToggle != null)
            {
                patternVisualizationToggle.onValueChanged.AddListener(
                    OnPatternVisualizationToggled);
            }

            if (aimAssistToggle != null)
                aimAssistToggle.onValueChanged.AddListener(OnAimAssistToggled);

            if (targetLockToggle != null)
                targetLockToggle.onValueChanged.AddListener(OnTargetLockToggled);

            if (bulletMagnetismToggle != null)
                bulletMagnetismToggle.onValueChanged.AddListener(OnBulletMagnetismToggled);

            if (heatSeekingToggle != null)
                heatSeekingToggle.onValueChanged.AddListener(OnHeatSeekingToggled);

            if (magnetismStrengthSlider != null)
            {
                magnetismStrengthSlider.onValueChanged.AddListener(OnMagnetismStrengthChanged);
                magnetismStrengthSlider.value = 0.5f;
            }

            if (infiniteAmmoToggle != null)
                infiniteAmmoToggle.onValueChanged.AddListener(OnInfiniteAmmoToggled);

            if (godModeToggle != null)
                godModeToggle.onValueChanged.AddListener(OnGodModeToggled);

            if (timeScaleSlider != null)
            {
                timeScaleSlider.onValueChanged.AddListener(OnTimeScaleChanged);
                timeScaleSlider.value = 1f;
            }

            if (resetButton != null)
                resetButton.onClick.AddListener(OnResetClicked);

            if (nextTargetGroupButton != null)
                nextTargetGroupButton.onClick.AddListener(OnNextTargetGroupClicked);
        }

        protected virtual void OnRecoilIntensityChanged(float value)
        {
            var patterns = FindObjectsByType<SprayPattern>(
                FindObjectsSortMode.None);
            foreach (var pattern in patterns)
                pattern.intensityMultiplier = value;

            if (recoilIntensityLabel != null)
                recoilIntensityLabel.text = $"Recoil: {value:F1}x";
        }

        protected virtual void OnCompensationToggled(bool value)
        {
            var controllers = FindObjectsByType<RecoilController>(
                FindObjectsSortMode.None);
            foreach (var ctrl in controllers)
                ctrl.useCompensation = value;
        }

        protected virtual void OnPatternVisualizationToggled(bool value)
        {
            if (roomManager != null)
                roomManager.showRecoilPatterns = value;
        }

        protected virtual void OnAimAssistToggled(bool value)
        {
            var assists = FindObjectsByType<AimAssist>(
                FindObjectsSortMode.None);
            foreach (var assist in assists)
                assist.enabled = value;
        }

        protected virtual void OnTargetLockToggled(bool value)
        {
            var locks = FindObjectsByType<TargetLock>(
                FindObjectsSortMode.None);
            foreach (var tl in locks)
                tl.enableTargetLock = value;
        }

        protected virtual void OnBulletMagnetismToggled(bool value)
        {
            var magnets = FindObjectsByType<BulletMagnetism>(
                FindObjectsSortMode.None);
            foreach (var magnet in magnets)
                magnet.enableMagnetism = value;
        }

        protected virtual void OnHeatSeekingToggled(bool value)
        {
            var launchers = FindObjectsByType<HeatSeekingLauncher>(
                FindObjectsSortMode.None);
            foreach (var launcher in launchers)
                launcher.enabled = value;
        }

        protected virtual void OnMagnetismStrengthChanged(float value)
        {
            var magnets = FindObjectsByType<BulletMagnetism>(
                FindObjectsSortMode.None);
            foreach (var magnet in magnets)
                magnet.magnetismStrength = value;

            if (magnetismStrengthLabel != null)
                magnetismStrengthLabel.text = $"Strength: {value:F2}";
        }

        protected virtual void OnInfiniteAmmoToggled(bool value)
        {
            if (roomManager != null)
            {
                roomManager.infiniteAmmo = value;
                if (value)
                {
                    var weapons = FindObjectsByType<WeaponBase>(
                        FindObjectsSortMode.None);
                    foreach (var w in weapons)
                        w.currentAmmo = w.magazineSize;
                }
            }
        }

        protected virtual void OnGodModeToggled(bool value)
        {
            if (roomManager != null)
                roomManager.godMode = value;
        }

        protected virtual void OnTimeScaleChanged(float value)
        {
            Time.timeScale = value;
            if (timeScaleLabel != null)
                timeScaleLabel.text = $"Speed: {value:F1}x";
        }

        protected virtual void OnResetClicked()
        {
            if (roomManager != null)
                roomManager.ResetTraining();
        }

        protected virtual void OnNextTargetGroupClicked()
        {
            if (roomManager != null)
                roomManager.NextTargetGroup();
        }

        protected virtual void UpdateStats()
        {
            if (statsTracker == null) return;

            if (accuracyText != null)
                accuracyText.text = $"Accuracy: {statsTracker.GetAccuracy():F1}%";

            if (killsText != null)
                killsText.text = $"Kills: {statsTracker.TotalKills}";

            if (damageDealtText != null)
                damageDealtText.text = $"Damage: {statsTracker.TotalDamageDealt}";

            if (shotsFiredText != null)
                shotsFiredText.text = $"Fired: {statsTracker.TotalShotsFired}";

            if (shotsHitText != null)
                shotsHitText.text = $"Hits: {statsTracker.TotalShotsHit}";

            if (scoreText != null)
                scoreText.text = $"Score: {statsTracker.TotalScore}";
        }

        protected virtual void UpdateWeaponInfo()
        {
            var switcher = FindFirstObjectByType<WeaponSwitcher>();
            if (switcher != null)
                currentWeapon = switcher.GetCurrentWeapon();

            if (currentWeapon == null) return;

            if (weaponNameText != null)
                weaponNameText.text = currentWeapon.weaponName;

            if (ammoText != null)
            {
                string infinite = roomManager != null && roomManager.infiniteAmmo
                    ? "INF" : currentWeapon.currentAmmo.ToString();
                ammoText.text = $"{infinite} / {currentWeapon.magazineSize}";
            }

            if (fireModeText != null)
                fireModeText.text = $"Mode: {currentWeapon.fireMode}";
        }

        protected virtual void UpdateUIReferences()
        {
            if (roomManager != null)
            {
                if (infiniteAmmoToggle != null)
                    infiniteAmmoToggle.isOn = roomManager.infiniteAmmo;
                if (godModeToggle != null)
                    godModeToggle.isOn = roomManager.godMode;
            }
        }

        protected virtual void OnDestroy()
        {
            Time.timeScale = 1f;
        }
    }
}
