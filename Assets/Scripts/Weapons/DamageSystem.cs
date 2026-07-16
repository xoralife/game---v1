using UnityEngine;

namespace FPSTrainingRoom.Weapons
{
    [System.Serializable]
    public struct DamageFalloff
    {
        public float maxDamageRange;
        public float minDamageRange;
        [Range(0f, 1f)]
        public float minDamageMultiplier;
    }

    [System.Serializable]
    public struct LimbMultiplier
    {
        public string limbTag;
        public float multiplier;
    }

    public class DamageSystem : MonoBehaviour
    {
        [Header("Damage Settings")]
        public float baseDamage = 35f;
        public DamageFalloff falloff = new DamageFalloff
        {
            maxDamageRange = 15f,
            minDamageRange = 80f,
            minDamageMultiplier = 0.3f
        };

        [Header("Limb Multipliers")]
        public LimbMultiplier[] limbMultipliers = new LimbMultiplier[]
        {
            new LimbMultiplier { limbTag = "Head", multiplier = 2.5f },
            new LimbMultiplier { limbTag = "Chest", multiplier = 1.2f },
            new LimbMultiplier { limbTag = "Arm", multiplier = 0.8f },
            new LimbMultiplier { limbTag = "Leg", multiplier = 0.6f }
        };

        [Header("Armor")]
        public bool useArmorSystem = true;
        public float armorDamageReduction = 0.3f;
        public float armorPenetration = 0.5f;

        [Header("Visual Debug")]
        public bool showDamageNumbers = true;
        public GameObject damageNumberPrefab;

        public virtual float CalculateDamage(float distance, string limbTag, bool hasArmor)
        {
            float damage = baseDamage;

            float distanceMultiplier = 1f;
            if (distance > falloff.maxDamageRange)
            {
                float t = Mathf.Clamp01(
                    (distance - falloff.maxDamageRange) /
                    (falloff.minDamageRange - falloff.maxDamageRange)
                );
                distanceMultiplier = Mathf.Lerp(1f, falloff.minDamageMultiplier, t);
            }
            damage *= distanceMultiplier;

            float limbMultiplier = 1f;
            foreach (var limb in limbMultipliers)
            {
                if (limb.limbTag == limbTag)
                {
                    limbMultiplier = limb.multiplier;
                    break;
                }
            }
            damage *= limbMultiplier;

            if (hasArmor && useArmorSystem)
            {
                float armorEffectiveness = armorDamageReduction * (1f - armorPenetration);
                damage *= (1f - armorEffectiveness);
            }

            return Mathf.Max(damage, 1f);
        }

        public virtual void ShowDamageNumber(Vector3 position, float damage, bool isCrit)
        {
            if (!showDamageNumbers || damageNumberPrefab == null) return;

            var obj = Instantiate(damageNumberPrefab, position, Quaternion.identity);
            var text = obj.GetComponentInChildren<TMPro.TextMeshPro>();
            if (text != null)
            {
                text.text = Mathf.RoundToInt(damage).ToString();
                text.color = isCrit ? Color.red : Color.yellow;
                text.fontSize = isCrit ? 36f : 24f;
            }
            Destroy(obj, 1.5f);
        }
    }
}
