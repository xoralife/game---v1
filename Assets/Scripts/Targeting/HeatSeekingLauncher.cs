using UnityEngine;
using FPSTrainingRoom.Weapons;

namespace FPSTrainingRoom.Targeting
{
    public class HeatSeekingLauncher : WeaponBase
    {
        [Header("Heat-Seeking Settings")]
        public GameObject heatSeekingProjectilePrefab;
        public Transform firePoint;
        public float projectileSpeed = 200f;

        [Header("Lock-On Requirements")]
        public bool requireLockOn = true;
        public float maxLockAngle = 20f;
        public TargetLock targetLockSystem;

        [Header("Fire Modes")]
        public bool singleFire = true;
        public float reloadTimePerShot = 1.5f;

        protected override void Awake()
        {
            base.Awake();
            if (targetLockSystem == null)
                targetLockSystem = GetComponent<TargetLock>();

            if (firePoint == null)
                firePoint = transform;
        }

        protected override bool Fire()
        {
            if (heatSeekingProjectilePrefab == null || currentAmmo <= 0)
                return false;

            Transform fireTarget = null;

            if (requireLockOn && targetLockSystem != null)
            {
                fireTarget = targetLockSystem.GetLockedTarget()?.transform;
                if (fireTarget == null)
                {
                    float angle = Vector3.Angle(transform.forward,
                        (GetAimPoint() - transform.position).normalized);
                    if (angle > maxLockAngle)
                        return false;
                }
            }

            currentAmmo--;
            nextFireTime = Time.time + fireRate;
            shotsFired++;

            var projectileObj = Instantiate(heatSeekingProjectilePrefab,
                firePoint.position, firePoint.rotation);
            var seeker = projectileObj.GetComponent<HeatSeekingProjectile>();

            if (seeker != null)
            {
                if (fireTarget != null)
                    seeker.Initialize(fireTarget);
                else
                    seeker.Initialize(null);
            }

            var proj = projectileObj.GetComponent<Projectile>();
            if (proj != null)
            {
                Vector3 aimDir = GetAimDirection();
                proj.Initialize(aimDir, 1f, 0f);
            }

            OnFire?.Invoke();
            return true;
        }

        protected virtual Vector3 GetAimPoint()
        {
            if (Physics.Raycast(firePoint.position, firePoint.forward,
                out RaycastHit hit, maxRange))
                return hit.point;
            return firePoint.position + firePoint.forward * maxRange;
        }

        protected virtual Vector3 GetAimDirection()
        {
            return (GetAimPoint() - firePoint.position).normalized;
        }

        public override void StartReload()
        {
            if (isReloading || currentAmmo == magazineSize || totalReserveAmmo <= 0)
                return;
            isReloading = true;
            reloadTimer = reloadTimePerShot;
            OnReloadStart?.Invoke();
        }
    }
}
