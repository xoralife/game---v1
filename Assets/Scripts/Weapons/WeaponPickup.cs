using UnityEngine;

namespace FPSTrainingRoom.Weapons
{
    public class WeaponPickup : MonoBehaviour
    {
        [Header("Pickup Settings")]
        public WeaponBase weaponPrefab;
        public int ammoCount = 30;
        public float pickupRange = 2f;
        public float rotationSpeed = 60f;
        public bool isAutoPickup = true;

        [Header("Visual")]
        public GameObject pickupModel;
        public Light pickupLight;
        public Color pickupColor = Color.yellow;

        protected Transform player;
        protected bool isAvailable = true;
        protected float respawnTimer;

        public System.Action OnPickup;
        public System.Action OnRespawn;

        protected virtual void Start()
        {
            if (pickupModel != null)
                pickupModel.transform.Rotate(Vector3.up, Random.Range(0f, 360f));

            if (pickupLight != null)
                pickupLight.color = pickupColor;
        }

        protected virtual void Update()
        {
            if (!isAvailable)
            {
                respawnTimer -= Time.deltaTime;
                if (respawnTimer <= 0f)
                {
                    Respawn();
                }
                return;
            }

            if (pickupModel != null)
                pickupModel.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            if (isAutoPickup && player != null)
            {
                float distance = Vector3.Distance(transform.position, player.position);
                if (distance <= pickupRange)
                {
                    TryPickup(player);
                }
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!isAutoPickup && isAvailable && other.CompareTag("Player"))
            {
                TryPickup(other.transform);
            }
        }

        protected virtual void OnTriggerStay(Collider other)
        {
            if (!isAutoPickup && isAvailable && Input.GetKeyDown(KeyCode.F)
                && other.CompareTag("Player"))
            {
                TryPickup(other.transform);
            }
        }

        protected virtual bool TryPickup(Transform collector)
        {
            if (!isAvailable || weaponPrefab == null) return false;

            var switcher = collector.GetComponent<WeaponSwitcher>();
            if (switcher != null)
            {
                var currentWeapon = switcher.GetCurrentWeapon();
                if (currentWeapon != null)
                {
                    currentWeapon.totalReserveAmmo += ammoCount;
                }
            }

            isAvailable = false;
            pickupModel?.SetActive(false);

            if (pickupLight != null)
                pickupLight.enabled = false;

            OnPickup?.Invoke();

            return true;
        }

        protected virtual void Respawn()
        {
            isAvailable = true;
            pickupModel?.SetActive(true);

            if (pickupLight != null)
                pickupLight.enabled = true;

            OnRespawn?.Invoke();
        }

        public virtual void SetRespawnTime(float time)
        {
            respawnTimer = time;
        }
    }
}
