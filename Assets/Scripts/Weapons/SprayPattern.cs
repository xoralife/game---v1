using UnityEngine;

namespace FPSTrainingRoom.Weapons
{
    [CreateAssetMenu(fileName = "NewSprayPattern", menuName = "FPS Training/Spray Pattern", order = 1)]
    public class SprayPattern : ScriptableObject
    {
        [Tooltip("Full-auto rate of fire in rounds per minute")]
        public float fireRateRPM = 600f;

        [Tooltip("Total shots in this pattern before it loops")]
        public int patternLength = 30;

        [Tooltip("Recoil kick per shot (horizontal, vertical)")]
        public AnimationCurve recoilCurveX;
        public AnimationCurve recoilCurveY;

        [Tooltip("Random spread added on top of pattern (degrees)")]
        [Range(0f, 5f)]
        public float spreadMin = 0.1f;
        [Range(0f, 10f)]
        public float spreadMax = 2.5f;

        [Tooltip("How fast spread grows per shot fired")]
        public float spreadIncreasePerShot = 0.08f;

        [Tooltip("How fast spread recovers when not firing (per second)")]
        public float spreadRecoveryRate = 2f;

        [Tooltip("Pattern intensity multiplier")]
        public float intensityMultiplier = 1f;

        public Vector2 GetRecoilOffset(int shotIndex, float t)
        {
            float x = recoilCurveX.Evaluate(t) * intensityMultiplier;
            float y = recoilCurveY.Evaluate(t) * intensityMultiplier;
            return new Vector2(x, y);
        }

        public float GetSpread(int consecutiveShots)
        {
            return Mathf.Lerp(spreadMin, spreadMax,
                Mathf.Clamp01(consecutiveShots * spreadIncreasePerShot));
        }
    }
}
