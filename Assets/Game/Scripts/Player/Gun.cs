using UnityEngine;
using WaveSurvival.Enemies;

namespace WaveSurvival.Player
{
    public class Gun : MonoBehaviour
    {
        [Header="Shooting"]
        public float fireRate = 0.1f;
        public float damage = 30f;
        public float range = 100f;
        public LayerMask shootMask = -1;

        [Header="Ammo"]
        public int magazineSize = 30;
        public int currentAmmo;
        public int totalReserve = 90;
        public float reloadTime = 1.5f;

        [Header="Effects"]
        public GameObject muzzleFlashPrefab;
        public GameObject hitEffectPrefab;
        public AudioClip shootSound;
        public AudioClip reloadSound;

        Camera cam;
        AudioSource audioSource;
        float nextFireTime;
        bool isReloading;
        float reloadTimer;

        public System.Action OnShoot;
        public System.Action OnReloadStart;
        public System.Action OnReloadEnd;

        void Start()
        {
            currentAmmo = magazineSize;
            cam = GetComponentInParent<Camera>();
            if (cam == null)
                cam = Camera.main;
            audioSource = GetComponent<AudioSource>();
        }

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.isGameOver)
                return;

            if (isReloading)
            {
                reloadTimer -= Time.deltaTime;
                if (reloadTimer <= 0f)
                    FinishReload();
                return;
            }

            if (Input.GetMouseButton(0) && Time.time >= nextFireTime && currentAmmo > 0)
                Shoot();

            if (Input.GetKeyDown(KeyCode.R) && currentAmmo < magazineSize && totalReserve > 0)
                StartReload();
        }

        void Shoot()
        {
            currentAmmo--;
            nextFireTime = Time.time + fireRate;

            if (muzzleFlashPrefab != null)
                Instantiate(muzzleFlashPrefab, transform.position, transform.rotation);

            if (shootSound != null && audioSource != null)
                audioSource.PlayOneShot(shootSound);

            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, range, shootMask))
            {
                Enemy enemy = hit.collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    if (GameManager.Instance != null)
                        GameManager.Instance.AddScore(10);
                }

                if (hitEffectPrefab != null)
                    Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            }

            OnShoot?.Invoke();
        }

        void StartReload()
        {
            isReloading = true;
            reloadTimer = reloadTime;
            OnReloadStart?.Invoke();
            if (reloadSound != null && audioSource != null)
                audioSource.PlayOneShot(reloadSound);
        }

        void FinishReload()
        {
            int needed = magazineSize - currentAmmo;
            int give = Mathf.Min(needed, totalReserve);
            currentAmmo += give;
            totalReserve -= give;
            isReloading = false;
            OnReloadEnd?.Invoke();
        }
    }
}
