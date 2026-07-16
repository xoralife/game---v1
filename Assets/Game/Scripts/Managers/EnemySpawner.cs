using UnityEngine;

namespace WaveSurvival
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Points")]
        public Transform[] spawnPoints;
        public float spawnRadius = 2f;

        [Header("Arena Bounds")]
        public Vector3 arenaCenter = Vector3.zero;
        public Vector3 arenaSize = new Vector3(20f, 0f, 20f);
        public bool autoGeneratePoints = true;
        public int pointCount = 8;

        void Start()
        {
            if (autoGeneratePoints && (spawnPoints == null || spawnPoints.Length == 0))
                GenerateSpawnPoints();
        }

        void GenerateSpawnPoints()
        {
            spawnPoints = new Transform[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                float angle = (360f / pointCount) * i;
                float x = Mathf.Sin(angle * Mathf.Deg2Rad) * arenaSize.x * 0.45f;
                float z = Mathf.Cos(angle * Mathf.Deg2Rad) * arenaSize.z * 0.45f;

                GameObject go = new GameObject($"SpawnPoint_{i}");
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
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
            Gizmos.DrawCube(arenaCenter, arenaSize);

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
