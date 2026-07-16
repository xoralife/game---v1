using UnityEngine;
using FPSTrainingRoom.Weapons;
using FPSTrainingRoom.Targeting;
using FPSTrainingRoom.DebugTools;

namespace FPSTrainingRoom.Training
{
    public class TrainingRoomInitializer : MonoBehaviour
    {
        [Header("Auto Setup")]
        public bool autoSetupOnStart = true;
        public GameObject playerPrefab;
        public GameObject weaponPrefab;
        public GameObject targetPrefab;

        [Header("Spawn Positions")]
        public Vector3 playerSpawn = new Vector3(0f, 1f, -10f);
        public Vector3[] targetPositions = new Vector3[]
        {
            new Vector3(-5f, 1f, 10f),
            new Vector3(0f, 1f, 15f),
            new Vector3(5f, 1f, 10f),
            new Vector3(-3f, 2f, 20f),
            new Vector3(3f, 2f, 20f)
        };

        protected virtual void Start()
        {
            if (autoSetupOnStart)
                InitializeTrainingRoom();
        }

        public virtual void InitializeTrainingRoom()
        {
            SetupPlayer();
            SetupWeapons();
            SetupTargets();
            SetupUI();
            SetupEnvironment();
        }

        protected virtual void SetupPlayer()
        {
            GameObject player = new GameObject("Player");
            player.transform.position = playerSpawn;

            var controller = player.AddComponent<FPSController>();

            GameObject cameraObj = new GameObject("PlayerCamera");
            cameraObj.transform.SetParent(player.transform);
            cameraObj.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            var cam = cameraObj.AddComponent<Camera>();
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.01f;

            var audioListener = cameraObj.AddComponent<AudioListener>();

            var aimAssist = cameraObj.AddComponent<AimAssist>();
            var targetLock = cameraObj.AddComponent<TargetLock>();
            var bulletMagnetism = cameraObj.AddComponent<BulletMagnetism>();
            var crosshair = cameraObj.AddComponent<CrosshairIndicator>();
            var debugCrosshair = cameraObj.AddComponent<DebugCrosshair>();
            var hitMarker = cameraObj.AddComponent<HitMarkerUI>();
            var recoilDebug = cameraObj.AddComponent<RecoilDebugOverlay>();
            var shotDisplay = cameraObj.AddComponent<ShotImpactDisplay>();
            var targetInfo = cameraObj.AddComponent<TargetInfoDisplay>();
            var rangeFinder = cameraObj.AddComponent<RangeFinder>();
            var targetingSystem = cameraObj.AddComponent<TargetingSystem>();
            var targetTracker = cameraObj.AddComponent<TargetTracker>();
            var prioritizer = cameraObj.AddComponent<TargetPrioritizer>();
        }

        protected virtual void SetupWeapons()
        {
            GameObject weaponHolder = new GameObject("WeaponHolder");

            var switcher = weaponHolder.AddComponent<WeaponSwitcher>();
            switcher.weaponSlots = new System.Collections.Generic.List<GameObject>();

            GameObject rifleObj = new GameObject("AssaultRifle");
            rifleObj.transform.SetParent(weaponHolder.transform);
            var weapon = rifleObj.AddComponent<WeaponBase>();
            weapon.weaponName = "Assault Rifle";
            weapon.fireRateRPM = 600f;
            weapon.magazineSize = 30;
            weapon.currentAmmo = 30;
            weapon.totalReserveAmmo = 120;

            var recoil = rifleObj.AddComponent<RecoilController>();
            var animController = rifleObj.AddComponent<WeaponAnimationController>();
            var muzzleFlash = rifleObj.AddComponent<MuzzleFlash>();

            var pattern = ScriptableObject.CreateInstance<SprayPattern>();
            pattern.name = "Default Pattern";
            pattern.fireRateRPM = 600f;
            pattern.patternLength = 30;
            pattern.recoilCurveX = new AnimationCurve(
                new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0.5f));
            pattern.recoilCurveY = new AnimationCurve(
                new Keyframe(0f, 0.5f), new Keyframe(0.3f, 1.5f),
                new Keyframe(0.6f, 2f), new Keyframe(1f, 2.5f));
            weapon.sprayPattern = pattern;

            switcher.weaponSlots.Add(rifleObj);
            rifleObj.SetActive(true);
        }

        protected virtual void SetupTargets()
        {
            var targetManager = new GameObject("TargetManager");
            var spawnManager = targetManager.AddComponent<SpawnManager>();

            foreach (var pos in targetPositions)
            {
                GameObject targetObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                targetObj.transform.position = pos;
                targetObj.transform.localScale = new Vector3(0.8f, 1.5f, 0.8f);
                targetObj.name = $"Target_{pos.x:F0}_{pos.z:F0}";

                var target = targetObj.AddComponent<TargetDummy>();
                target.maxHealth = 100f;
                target.autoReset = true;
                target.resetDelay = 3f;

                var collider = targetObj.GetComponent<Collider>();
                collider.isTrigger = false;
            }
        }

        protected virtual void SetupUI()
        {
            var uiObj = new GameObject("TrainingUI");
            var ui = uiObj.AddComponent<TrainingUI>();
            var stats = uiObj.AddComponent<StatsTracker>();
        }

        protected virtual void SetupEnvironment()
        {
            // Ground
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            ground.transform.localScale = new Vector3(5f, 1f, 5f);

            // Lighting
            GameObject lightObj = new GameObject("DirectionalLight");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            light.intensity = 1f;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.3f, 0.3f, 0.35f);

            // Walls
            CreateWall(new Vector3(0f, 2f, -15f), new Vector3(30f, 4f, 0.5f));
            CreateWall(new Vector3(0f, 2f, 25f), new Vector3(30f, 4f, 0.5f));
            CreateWall(new Vector3(-15f, 2f, 5f), new Vector3(0.5f, 4f, 40f));
            CreateWall(new Vector3(15f, 2f, 5f), new Vector3(0.5f, 4f, 40f));

            // Barriers at various ranges
            CreateBarrier(new Vector3(0f, 0.5f, 5f), new Vector3(3f, 1f, 0.3f));
            CreateBarrier(new Vector3(-3f, 0.5f, 8f), new Vector3(2f, 1.5f, 0.3f));
            CreateBarrier(new Vector3(4f, 0.5f, 12f), new Vector3(2f, 1f, 0.3f));
        }

        protected virtual void CreateWall(Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall";
            wall.transform.position = position;
            wall.transform.localScale = scale;
            var renderer = wall.GetComponent<Renderer>();
            renderer.material.color = new Color(0.4f, 0.4f, 0.45f);
        }

        protected virtual void CreateBarrier(Vector3 position, Vector3 scale)
        {
            GameObject barrier = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barrier.name = "Barrier";
            barrier.transform.position = position;
            barrier.transform.localScale = scale;
            var renderer = barrier.GetComponent<Renderer>();
            renderer.material.color = new Color(0.5f, 0.3f, 0.2f);

            barrier.AddComponent<BoxCollider>();
        }
    }
}
