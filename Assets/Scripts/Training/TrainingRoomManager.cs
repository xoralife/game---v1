using UnityEngine;
using FPSTrainingRoom.Weapons;
using FPSTrainingRoom.Targeting;

namespace FPSTrainingRoom.Training
{
    public class TrainingRoomManager : MonoBehaviour
    {
        [Header("References")]
        public GameObject playerPrefab;
        public Transform playerSpawnPoint;
        public WeaponBase[] startingWeapons;

        [Header("Training Settings")]
        public bool infiniteAmmo = false;
        public bool godMode = false;
        public bool showRecoilPatterns = true;
        public bool showAimAssistVisuals = true;

        [Header("Scene Objects")]
        public GameObject[] targetGroups;
        public Light[] sceneLights;
        public AudioSource backgroundAudio;

        protected GameObject playerInstance;
        protected WeaponBase currentWeapon;
        protected int currentTargetGroup;

        public System.Action OnTrainingStarted;
        public System.Action OnTrainingReset;
        public System.Action<bool> OnInfiniteAmmoToggled;
        public System.Action<bool> OnGodModeToggled;

        public static TrainingRoomManager Instance { get; protected set; }

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
            InitializeTrainingRoom();
        }

        protected virtual void InitializeTrainingRoom()
        {
            if (playerPrefab != null && playerSpawnPoint != null)
            {
                playerInstance = Instantiate(playerPrefab,
                    playerSpawnPoint.position, playerSpawnPoint.rotation);
            }

            ActivateTargetGroup(0);

            OnTrainingStarted?.Invoke();
        }

        public virtual void ResetTraining()
        {
            var targets = FindObjectsByType<TargetDummy>(
                FindObjectsSortMode.None);
            foreach (var target in targets)
                target.ResetTarget();

            if (playerInstance != null)
            {
                playerInstance.transform.position = playerSpawnPoint.position;
                playerInstance.transform.rotation = playerSpawnPoint.rotation;
            }

            OnTrainingReset?.Invoke();
        }

        public virtual void ToggleInfiniteAmmo()
        {
            infiniteAmmo = !infiniteAmmo;
            OnInfiniteAmmoToggled?.Invoke(infiniteAmmo);

            var weapons = FindObjectsByType<WeaponBase>(
                FindObjectsSortMode.None);
            foreach (var weapon in weapons)
            {
                if (infiniteAmmo)
                    weapon.currentAmmo = weapon.magazineSize;
            }
        }

        public virtual void ToggleGodMode()
        {
            godMode = !godMode;
            OnGodModeToggled?.Invoke(godMode);
        }

        public virtual void ActivateTargetGroup(int groupIndex)
        {
            for (int i = 0; i < targetGroups.Length; i++)
            {
                if (targetGroups[i] != null)
                    targetGroups[i].SetActive(i == groupIndex);
            }
            currentTargetGroup = groupIndex;
        }

        public virtual void NextTargetGroup()
        {
            int next = (currentTargetGroup + 1) % targetGroups.Length;
            ActivateTargetGroup(next);
        }

        public virtual void PreviousTargetGroup()
        {
            int prev = (currentTargetGroup - 1 + targetGroups.Length) % targetGroups.Length;
            ActivateTargetGroup(prev);
        }

        public virtual void SetSceneLighting(float intensity)
        {
            foreach (var light in sceneLights)
            {
                if (light != null)
                    light.intensity = intensity;
            }
        }

        public virtual void ToggleBackgroundAudio()
        {
            if (backgroundAudio != null)
            {
                if (backgroundAudio.isPlaying)
                    backgroundAudio.Stop();
                else
                    backgroundAudio.Play();
            }
        }

        public virtual void SetTimeScale(float scale)
        {
            Time.timeScale = Mathf.Clamp(scale, 0.1f, 2f);
        }
    }
}
