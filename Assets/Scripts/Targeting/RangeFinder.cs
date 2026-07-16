using UnityEngine;
using TMPro;

namespace FPSTrainingRoom.Targeting
{
    public class RangeFinder : MonoBehaviour
    {
        [Header("Range Settings")]
        public float maxRange = 200f;
        public LayerMask rangeCheckLayers = -1;
        public bool showRangeInUI = true;

        [Header("UI Reference")]
        public TextMeshProUGUI rangeDisplayText;
        public string displayPrefix = "Range: ";
        public string displaySuffix = "m";
        public Color normalColor = Color.white;
        public Color optimalColor = Color.green;
        public Color farColor = Color.yellow;

        [Header("Optimal Range")]
        public float optimalMin = 5f;
        public float optimalMax = 30f;

        [Header("Audio")]
        public AudioClip pingClip;
        public float pingInterval = 1f;

        protected Camera playerCamera;
        protected float currentRange;
        protected float pingTimer;
        protected AudioSource audioSource;

        public float CurrentRange => currentRange;

        protected virtual void Start()
        {
            playerCamera = GetComponent<Camera>();
            if (playerCamera == null)
                playerCamera = Camera.main;

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && pingClip != null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        protected virtual void Update()
        {
            if (playerCamera == null) return;

            currentRange = CalculateRange();

            if (showRangeInUI)
                UpdateRangeDisplay(currentRange);

            if (pingClip != null && audioSource != null)
            {
                pingTimer -= Time.deltaTime;
                if (pingTimer <= 0f && IsTargetInOptimalRange())
                {
                    audioSource.PlayOneShot(pingClip);
                    pingTimer = pingInterval;
                }
            }
        }

        protected virtual float CalculateRange()
        {
            if (Physics.Raycast(playerCamera.transform.position,
                playerCamera.transform.forward, out RaycastHit hit, maxRange, rangeCheckLayers))
            {
                return hit.distance;
            }

            return maxRange;
        }

        protected virtual void UpdateRangeDisplay(float range)
        {
            if (rangeDisplayText == null) return;

            rangeDisplayText.text = $"{displayPrefix}{Mathf.RoundToInt(range)}{displaySuffix}";

            if (range >= optimalMin && range <= optimalMax)
                rangeDisplayText.color = optimalColor;
            else if (range > optimalMax)
                rangeDisplayText.color = farColor;
            else
                rangeDisplayText.color = normalColor;
        }

        public virtual bool IsTargetInOptimalRange()
        {
            return currentRange >= optimalMin && currentRange <= optimalMax;
        }

        public virtual bool IsTargetInRange(float range)
        {
            return currentRange <= range;
        }

        public virtual float GetDistanceToPoint(Vector3 point)
        {
            return Vector3.Distance(transform.position, point);
        }

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, optimalMin);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, optimalMax);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, maxRange);
        }
    }
}
