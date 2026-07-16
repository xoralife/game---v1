using UnityEngine;
using System.Collections.Generic;

namespace FPSTrainingRoom.Weapons
{
    public class WeaponSwitcher : MonoBehaviour
    {
        [Header("Weapon Slots")]
        public List<GameObject> weaponSlots;
        public int currentWeaponIndex = 0;
        public KeyCode[] weaponKeys = new KeyCode[]
        {
            KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3
        };

        protected WeaponBase[] weapons;
        protected bool isSwitching;

        public System.Action<int> OnWeaponSwitched;

        protected virtual void Start()
        {
            weapons = new WeaponBase[weaponSlots.Count];
            for (int i = 0; i < weaponSlots.Count; i++)
            {
                if (weaponSlots[i] != null)
                {
                    weapons[i] = weaponSlots[i].GetComponent<WeaponBase>();
                    weaponSlots[i].SetActive(i == currentWeaponIndex);
                }
            }
        }

        protected virtual void Update()
        {
            if (isSwitching) return;

            for (int i = 0; i < weaponKeys.Length; i++)
            {
                if (i < weaponSlots.Count && Input.GetKeyDown(weaponKeys[i]) && i != currentWeaponIndex)
                {
                    SwitchToWeapon(i);
                    break;
                }
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.1f)
            {
                int direction = scroll > 0f ? 1 : -1;
                int nextIndex = (currentWeaponIndex + direction + weaponSlots.Count) % weaponSlots.Count;
                SwitchToWeapon(nextIndex);
            }
        }

        public virtual void SwitchToWeapon(int index)
        {
            if (index < 0 || index >= weaponSlots.Count || index == currentWeaponIndex)
                return;

            if (weaponSlots[currentWeaponIndex] != null)
                weaponSlots[currentWeaponIndex].SetActive(false);

            currentWeaponIndex = index;

            if (weaponSlots[currentWeaponIndex] != null)
                weaponSlots[currentWeaponIndex].SetActive(true);

            OnWeaponSwitched?.Invoke(currentWeaponIndex);
        }

        public virtual WeaponBase GetCurrentWeapon()
        {
            if (currentWeaponIndex >= 0 && currentWeaponIndex < weapons.Length)
                return weapons[currentWeaponIndex];
            return null;
        }
    }
}
