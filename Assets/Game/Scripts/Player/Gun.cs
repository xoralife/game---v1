using UnityEngine;
using WaveSurvival.Enemies;

namespace WaveSurvival.Player
{
    public class Gun : MonoBehaviour
    {
        [Header("Shooting")]
        public float fireRate = 0.12f;
        public float damage = 30f;
        public float range = 100f;
        public LayerMask shootMask = -1;

        [Header("Ammo")]
        public int magazineSize = 30;
        public int currentAmmo;
        public int totalReserve = 90;
        public float reloadTime = 1.5f;

        [Header("Recoil")]
        public float recoilAmount = 0.05f;
        public float recoilRecovery = 5f;

        [Header("Effects")]
        public GameObject muzzleFlashObj;
        public AudioClip shootSound;
        public AudioClip reloadSound;

        Camera cam;
        AudioSource audioSource;
        float nextFireTime;
        bool isReloading;
        float reloadTimer;
        Vector3 originalPos;
        Vector3 recoilOffset;

        public System.Action OnShoot;
        public System.Action OnReloadStart;
        public System.Action OnReloadEnd;

        public int CurrentAmmo => currentAmmo;
        public int TotalReserve => totalReserve;
        public int MagazineSize => magazineSize;
        public bool IsReloading => isReloading;

        void Start()
        {
            currentAmmo = magazineSize;
            cam = GetComponentInParent<Camera>();
            if (cam == null)
                cam = Camera.main;
            audioSource = GetComponent<AudioSource>();
            originalPos = transform.localPosition;
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

            // Recoil recovery
            recoilOffset = Vector3.Lerp(recoilOffset, Vector3.zero, Time.deltaTime * recoilRecovery);
            transform.localPosition = originalPos + recoilOffset;
        }

        void Shoot()
        {
            currentAmmo--;
            nextFireTime = Time.time + fireRate;

            // Recoil animation
            recoilOffset += new Vector3(
                Random.Range(-0.005f, 0.005f),
                -0.02f,
                Random.Range(-0.01f, -0.005f)
            );
            recoilOffset = Vector3.ClampMagnitude(recoilOffset, 0.08f);

            // Muzzle flash
            if (muzzleFlashObj != null)
            {
                muzzleFlashObj.SetActive(true);
                Invoke(nameof(HideMuzzleFlash), 0.04f);
            }

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

                // Simple hit effect (small sphere)
                GameObject hitFX = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                hitFX.transform.position = hit.point;
                hitFX.transform.localScale = Vector3.one * 0.04f;
                hitFX.GetComponent<Renderer>().material.color = enemy != null ? Color.red : Color.gray;
                Destroy(hitFX.GetComponent<SphereCollider>());
                Destroy(hitFX, 0.3f);
            }

            OnShoot?.Invoke();
        }

        void HideMuzzleFlash()
        {
            if (muzzleFlashObj != null)
                muzzleFlashObj.SetActive(false);
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
