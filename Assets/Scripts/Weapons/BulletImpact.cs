using UnityEngine;

namespace FPSTrainingRoom.Weapons
{
    public class BulletImpact : MonoBehaviour
    {
        [Header("Impact Effects")]
        public GameObject defaultImpactPrefab;
        public GameObject concreteImpactPrefab;
        public GameObject metalImpactPrefab;
        public GameObject fleshImpactPrefab;
        public GameObject headshotImpactPrefab;

        [Header("Decals")]
        public GameObject bulletHoleDecal;
        public float decalLifetime = 30f;
        public int maxDecals = 50;

        [Header("Audio")]
        public AudioClip[] defaultSounds;
        public AudioClip[] concreteSounds;
        public AudioClip[] metalSounds;
        public AudioClip[] fleshSounds;
        public AudioClip headshotSound;

        [Header("Settings")]
        public float impactForce = 10f;
        public float impactRadius = 0.1f;

        protected AudioSource audioSource;
        protected System.Collections.Generic.Queue<GameObject> decalPool;

        protected virtual void Start()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;

            decalPool = new System.Collections.Generic.Queue<GameObject>();
        }

        public virtual void SpawnImpact(Vector3 point, Vector3 normal, Collider collider,
            float damage = 35f)
        {
            string tag = collider != null ? collider.tag : "Default";
            GameObject impactPrefab = GetImpactPrefab(tag);
            AudioClip[] soundClips = GetSoundClips(tag);
            GameObject decal = GetDecal(tag);

            if (impactPrefab != null)
            {
                var impact = Instantiate(impactPrefab, point, Quaternion.LookRotation(normal));
                Destroy(impact, 2f);
            }

            if (decal != null)
            {
                var decalInstance = Instantiate(decal, point + normal * 0.01f,
                    Quaternion.LookRotation(normal));
                decalPool.Enqueue(decalInstance);
                if (decalPool.Count > maxDecals)
                {
                    var oldest = decalPool.Dequeue();
                    if (oldest != null) Destroy(oldest);
                }
                Destroy(decalInstance, decalLifetime);
            }

            if (soundClips != null && soundClips.Length > 0 && audioSource != null)
            {
                audioSource.PlayOneShot(soundClips[Random.Range(0, soundClips.Length)]);
            }

            if (collider != null && collider.attachedRigidbody != null)
            {
                collider.attachedRigidbody.AddForceAtPosition(
                    normal * impactForce, point, ForceMode.Impulse);
            }
        }

        protected virtual GameObject GetImpactPrefab(string tag)
        {
            switch (tag)
            {
                case "Concrete": return concreteImpactPrefab;
                case "Metal": return metalImpactPrefab;
                case "Flesh": return fleshImpactPrefab;
                case "Head": return headshotImpactPrefab;
                default: return defaultImpactPrefab;
            }
        }

        protected virtual AudioClip[] GetSoundClips(string tag)
        {
            switch (tag)
            {
                case "Concrete": return concreteSounds;
                case "Metal": return metalSounds;
                case "Flesh": return fleshSounds;
                case "Head": return headshotSound != null
                        ? new AudioClip[] { headshotSound } : fleshSounds;
                default: return defaultSounds;
            }
        }

        protected virtual GameObject GetDecal(string tag)
        {
            if (tag == "Flesh" || tag == "Head") return null;
            return bulletHoleDecal;
        }
    }
}
