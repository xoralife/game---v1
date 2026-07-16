using UnityEngine;

namespace WaveSurvival
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        public int spawnPointCount = 12;
        public float spawnRadius = 2f;

        [Header("Arena Bounds")]
        public Vector3 arenaCenter = Vector3.zero;
        public Vector3 arenaSize = new Vector3(22f, 0f, 22f);

        protected Transform[] spawnPoints;

        void Start()
        {
            GenerateSpawnPoints();
        }

        void GenerateSpawnPoints()
        {
            spawnPoints = new Transform[spawnPointCount];

            // Circle perimeter
            int circlePoints = spawnPointCount / 2;
            for (int i = 0; i < circlePoints; i++)
            {
                float angle = (360f / circlePoints) * i;
                float radius = Mathf.Min(arenaSize.x, arenaSize.z) * 0.45f;
                float x = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
                float z = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;

                GameObject go = new GameObject($"SpawnPoint_Circle_{i}");
                go.transform.SetParent(transform);
                go.transform.position = arenaCenter + new Vector3(x, 0f, z);
                spawnPoints[i] = go.transform;
            }

            // Random interior points (behind cover)
            for (int i = circlePoints; i < spawnPointCount; i++)
            {
                float x = Random.Range(-arenaSize.x * 0.3f, arenaSize.x * 0.3f);
                float z = Random.Range(-arenaSize.z * 0.3f, arenaSize.z * 0.3f);

                GameObject go = new GameObject($"SpawnPoint_Interior_{i}");
                go.transform.SetParent(transform);
                go.transform.position = arenaCenter + new Vector3(x, 0f, z);
                spawnPoints[i] = go.transform;
            }
        }

        public Vector3 GetRandomSpawnPosition()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                return arenaCenter + new Vector3(
                    Random.Range(-arenaSize.x * 0.4f, arenaSize.x * 0.4f),
                    0f,
                    Random.Range(-arenaSize.z * 0.4f, arenaSize.z * 0.4f)
                );
            }

            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Vector3 randomOffset = new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                0f,
                Random.Range(-spawnRadius, spawnRadius)
            );
            return point.position + randomOffset;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
            Gizmos.DrawCube(arenaCenter, new Vector3(arenaSize.x, 0.5f, arenaSize.z));

            if (spawnPoints != null)
            {
                Gizmos.color = Color.red;
                foreach (var point in spawnPoints)
                {
                    if (point != null)
                    {
                        Gizmos.DrawWireSphere(point.position, 0.3f);
                        Gizmos.DrawWireSphere(point.position, spawnRadius);
                    }
                }
            }
        }
    }
}
