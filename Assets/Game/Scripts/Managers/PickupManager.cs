using UnityEngine;

namespace WaveSurvival
{
    public class PickupManager : MonoBehaviour
    {
        public static PickupManager Instance;

        [Header("Pickup Settings")]
        public float healthRestore = 30f;
        public int ammoRestore = 15;
        public float pickupLifetime = 8f;
        public float pickupRange = 2f;
        public float spawnChance = 0.4f;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        public void SpawnPickup(Vector3 position)
        {
            if (Random.value > spawnChance) return;

            bool isHealth = Random.value < 0.5f;
            GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pickup.name = isHealth ? "HealthPickup" : "AmmoPickup";
            pickup.transform.position = position + Vector3.up * 0.5f;
            pickup.transform.localScale = Vector3.one * 0.3f;
            Destroy(pickup.GetComponent<SphereCollider>());

            SphereCollider trigger = pickup.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = pickupRange;

            Renderer rend = pickup.GetComponent<Renderer>();
            rend.material.color = isHealth ? Color.green : Color.cyan;
            rend.material.shader = Shader.Find("Standard");

            // Glow effect - small particles
            GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glow.transform.SetParent(pickup.transform);
            glow.transform.localPosition = Vector3.zero;
            glow.transform.localScale = Vector3.one * 0.8f;
            glow.GetComponent<Renderer>().material.color = new Color(
                rend.material.color.r, rend.material.color.g, rend.material.color.b, 0.3f);
            Destroy(glow.GetComponent<SphereCollider>());

            Pickup pickupScript = pickup.AddComponent<Pickup>();
            pickupScript.isHealth = isHealth;
            pickupScript.healthAmount = healthRestore;
            pickupScript.ammoAmount = ammoRestore;
            pickupScript.lifetime = pickupLifetime;

            // Float animation
            pickup.AddComponent<PickupFloat>();

            Destroy(pickup, pickupLifetime);
        }
    }

    public class Pickup : MonoBehaviour
    {
        public bool isHealth;
        public float healthAmount;
        public int ammoAmount;
        public float lifetime;

        void Start()
        {
            Destroy(gameObject, lifetime);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            PlayerHealth health = other.GetComponent<PlayerHealth>();
            Player.Gun gun = other.GetComponentInChildren<Player.Gun>();

            if (isHealth && health != null)
            {
                health.Heal(healthAmount);
            }
            else if (!isHealth && gun != null)
            {
                gun.totalReserve += ammoAmount;
            }

            // Pickup effect
            GameObject fx = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fx.transform.position = transform.position;
            fx.transform.localScale = Vector3.one * 0.5f;
            fx.GetComponent<Renderer>().material.color = isHealth ? Color.green : Color.cyan;
            Destroy(fx.GetComponent<SphereCollider>());
            Destroy(fx, 0.3f);

            Destroy(gameObject);
        }

        void Update()
        {
            // Bobbing animation
            transform.Rotate(Vector3.up, 90f * Time.deltaTime);
        }
    }

    public class PickupFloat : MonoBehaviour
    {
        Vector3 startPos;
        float offset;

        void Start()
        {
            startPos = transform.position;
            offset = Random.Range(0f, Mathf.PI * 2f);
        }

        void Update()
        {
            transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * 2f + offset) * 0.15f;
        }
    }
}
