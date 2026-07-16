using UnityEngine;

namespace FPSTrainingRoom.Weapons
{
    public class WeaponConfigurator : MonoBehaviour
    {
        [Header("Quick Config")]
        public bool applyConfigOnStart = true;

        [Header("Override Settings")]
        public bool overrideRecoil = false;
        public Vector2 recoilMultiplier = Vector2.one;

        public bool overrideSpread = false;
        [Range(0f, 5f)]
        public float spreadMultiplier = 1f;

        public bool overrideFireRate = false;
        public float fireRateMultiplier = 1f;

        public bool overrideDamage = false;
        public float damageMultiplier = 1f;

        public bool infiniteAmmo = false;
        public bool noSpread = false;
        public bool zeroRecoil = false;

        protected WeaponBase[] weapons;

        protected virtual void Start()
        {
            if (applyConfigOnStart)
                ApplyConfiguration();
        }

        public virtual void ApplyConfiguration()
        {
            weapons = FindObjectsByType<WeaponBase>(
                FindObjectsSortMode.None);

            foreach (var weapon in weapons)
            {
                if (infiniteAmmo)
                {
                    weapon.currentAmmo = weapon.magazineSize;
                    weapon.totalReserveAmmo = 9999;
                }

                if (zeroRecoil && weapon.sprayPattern != null)
                {
                    weapon.sprayPattern.intensityMultiplier = 0f;
                    weapon.sprayPattern.spreadMin = 0f;
                    weapon.sprayPattern.spreadMax = 0f;
                }

                if (noSpread && weapon.sprayPattern != null)
                {
                    weapon.sprayPattern.spreadMin = 0f;
                    weapon.sprayPattern.spreadMax = 0f;
                }

                if (overrideRecoil && weapon.sprayPattern != null)
                {
                    weapon.sprayPattern.intensityMultiplier = recoilMultiplier.magnitude / 2f;
                }

                if (overrideSpread && weapon.sprayPattern != null)
                {
                    weapon.sprayPattern.spreadMin *= spreadMultiplier;
                    weapon.sprayPattern.spreadMax *= spreadMultiplier;
                }

                if (overrideFireRate)
                {
                    weapon.fireRateRPM *= fireRateMultiplier;
                }

                if (overrideDamage)
                {
                    weapon.damage *= damageMultiplier;
                }
            }
        }

        public virtual void SetZeroRecoil(bool enabled)
        {
            zeroRecoil = enabled;
            ApplyConfiguration();
        }

        public virtual void SetNoSpread(bool enabled)
        {
            noSpread = enabled;
            ApplyConfiguration();
        }

        public virtual void SetInfiniteAmmo(bool enabled)
        {
            infiniteAmmo = enabled;
            ApplyConfiguration();
        }

        public virtual void ResetToDefaults()
        {
            zeroRecoil = false;
            noSpread = false;
            infiniteAmmo = false;
            overrideRecoil = false;
            overrideSpread = false;
            overrideFireRate = false;
            overrideDamage = false;
            recoilMultiplier = Vector2.one;
            spreadMultiplier = 1f;
            fireRateMultiplier = 1f;
            damageMultiplier = 1f;

            weapons = FindObjectsByType<WeaponBase>(
                FindObjectsSortMode.None);
            foreach (var weapon in weapons)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }
        }
    }
}
