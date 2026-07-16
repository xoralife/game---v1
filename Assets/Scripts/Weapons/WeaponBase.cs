using UnityEngine;
using FPSTrainingRoom.Targeting;

namespace FPSTrainingRoom.Weapons
{
    public enum FireMode { SemiAuto, Burst, FullAuto }

    public class WeaponBase : MonoBehaviour
    {
        [Header("Weapon Settings")]
        public string weaponName = "Default Rifle";
        public FireMode fireMode = FireMode.FullAuto;
        public int burstCount = 3;

        [Header("Fire Rate")]
        public float fireRateRPM = 600f;
        public float fireRate => 60f / fireRateRPM;

        [Header("Ammo")]
        public int magazineSize = 30;
        public int currentAmmo;
        public int totalReserveAmmo = 120;
        public float reloadTime = 2.5f;

        [Header("Damage")]
        public float damage = 35f;
        public float maxRange = 100f;

        [Header("Recoil")]
        public SprayPattern sprayPattern;

        [Header("Targeting")]
        public AimAssist aimAssist;
        public TargetLock targetLock;

        protected float nextFireTime;
        protected int shotsFired;
        protected bool isReloading;
        protected float reloadTimer;
        protected int burstShotsFired;

        protected RecoilController recoilController;

        public System.Action OnFire;
        public System.Action OnReloadStart;
        public System.Action OnReloadEnd;

        protected virtual void Awake()
        {
            currentAmmo = magazineSize;
            recoilController = GetComponent<RecoilController>();
            if (recoilController == null)
                recoilController = gameObject.AddComponent<RecoilController>();
        }

        protected virtual void Update()
        {
            if (isReloading)
            {
                reloadTimer -= Time.deltaTime;
                if (reloadTimer <= 0f)
                    FinishReload();
                return;
            }

            if (sprayPattern != null)
                sprayPattern.spreadRecoveryRate = Mathf.Lerp(0f, sprayPattern.spreadRecoveryRate,
                    Time.deltaTime);
        }

        public virtual bool TryFire()
        {
            if (isReloading || Time.time < nextFireTime || currentAmmo <= 0)
                return false;

            if (fireMode == FireMode.SemiAuto)
                return Fire();

            if (fireMode == FireMode.Burst)
            {
                if (burstShotsFired < burstCount)
                {
                    bool result = Fire();
                    burstShotsFired++;
                    if (burstShotsFired >= burstCount)
                        burstShotsFired = 0;
                    return result;
                }
                return false;
            }

            return Fire();
        }

        protected virtual bool Fire()
        {
            currentAmmo--;
            nextFireTime = Time.time + fireRate;
            shotsFired++;

            Vector3 recoilOffset = Vector3.zero;
            if (sprayPattern != null && recoilController != null)
            {
                float t = (shotsFired - 1f) / Mathf.Max(sprayPattern.patternLength - 1, 1);
                Vector2 patternOffset = sprayPattern.GetRecoilOffset(shotsFired - 1, t);
                float spread = sprayPattern.GetSpread(shotsFired);
                recoilController.ApplyRecoil(patternOffset, spread);
                recoilOffset = new Vector3(patternOffset.x, patternOffset.y, 0f);
            }

            FireBullet(recoilOffset);

            OnFire?.Invoke();
            return true;
        }

        protected virtual void FireBullet(Vector3 recoilOffset)
        {
            Transform target = null;
            if (aimAssist != null)
                target = aimAssist.GetTarget();
            else if (targetLock != null)
                target = targetLock.GetLockedTarget();

            Vector3 aimPoint = target != null
                ? target.position + Vector3.up * 0.9f
                : transform.position + transform.forward * maxRange;

            Vector3 aimDirection = (aimPoint - transform.position).normalized;
            Vector3 spreadOffset = new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(-0.5f, 0.5f),
                0f
            );

            Vector3 finalDirection = Quaternion.Euler(
                recoilOffset.y + spreadOffset.y * recoilOffset.magnitude * 0.1f,
                recoilOffset.x + spreadOffset.x * recoilOffset.magnitude * 0.1f,
                0f
            ) * aimDirection;

            if (Physics.Raycast(transform.position, finalDirection, out RaycastHit hit, maxRange))
            {
                if (hit.collider.TryGetComponent<TargetDummy>(out var dummy))
                    dummy.OnHit(damage, hit.point, finalDirection);

                Debug.DrawLine(transform.position, hit.point, Color.red, 1f);
            }
        }

        public virtual void StartReload()
        {
            if (isReloading || currentAmmo == magazineSize || totalReserveAmmo <= 0)
                return;

            isReloading = true;
            reloadTimer = reloadTime;
            OnReloadStart?.Invoke();
        }

        protected virtual void FinishReload()
        {
            int needed = magazineSize - currentAmmo;
            int available = Mathf.Min(needed, totalReserveAmmo);
            currentAmmo += available;
            totalReserveAmmo -= available;
            isReloading = false;
            shotsFired = 0;
            OnReloadEnd?.Invoke();
        }
    }
}
