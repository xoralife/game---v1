using UnityEngine;
using System.Collections.Generic;

namespace FPSTrainingRoom.Training
{
    public class SpawnManager : MonoBehaviour
    {
        [Header("Spawn Settings")]
        public GameObject[] targetPrefabs;
        public Transform[] spawnPoints;
        public int maxTargets = 10;
        public float spawnInterval = 2f;
        public bool autoSpawn = true;
        public bool randomizePrefabs = true;

        [Header("Spawning Rules")]
        public bool spawnInWaves = false;
        public int targetsPerWave = 5;
        public float waveDelay = 5f;
        public bool increaseDifficulty = false;
        public float difficultyIncreaseRate = 0.1f;

        protected List<TargetDummy> activeTargets = new List<TargetDummy>();
        protected float spawnTimer;
        protected int currentWave;
        protected float currentDifficulty = 1f;
        protected int targetsSpawnedThisWave;

        public System.Action<int> OnWaveStarted;
        public System.Action OnAllTargetsCleared;
        public System.Action<TargetDummy> OnTargetSpawned;

        protected virtual void Update()
        {
            if (!autoSpawn) return;

            activeTargets.RemoveAll(t => t == null || !t.isActiveAndEnabled);

            if (spawnInWaves)
            {
                if (targetsSpawnedThisWave >= targetsPerWave)
                {
                    if (activeTargets.Count == 0)
                    {
                        currentWave++;
                        targetsSpawnedThisWave = 0;
                        spawnTimer = waveDelay;

                        if (increaseDifficulty)
                            currentDifficulty += difficultyIncreaseRate;

                        OnWaveStarted?.Invoke(currentWave);
                    }
                    return;
                }
            }

            if (activeTargets.Count >= maxTargets) return;

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnTarget();
                spawnTimer = spawnInterval / currentDifficulty;
                targetsSpawnedThisWave++;
            }
        }

        protected virtual void SpawnTarget()
        {
            if (targetPrefabs == null || targetPrefabs.Length == 0) return;
            if (spawnPoints == null || spawnPoints.Length == 0) return;

            GameObject prefab = targetPrefabs[Random.Range(0, targetPrefabs.Length)];
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            GameObject instance = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            var target = instance.GetComponent<TargetDummy>();

            if (target != null)
            {
                if (increaseDifficulty)
                {
                    target.maxHealth *= currentDifficulty;
                    target.ThreatLevel = currentDifficulty;
                    target.CurrentHealth = target.maxHealth;
                }

                activeTargets.Add(target);
                OnTargetSpawned?.Invoke(target);
            }

            var movingTarget = instance.GetComponent<MovingTarget>();
            if (movingTarget != null && increaseDifficulty)
            {
                movingTarget.SetSpeed(movingTarget.moveSpeed * currentDifficulty);
            }
        }

        public virtual void SpawnWave(int waveNumber)
        {
            currentWave = waveNumber;
            targetsSpawnedThisWave = 0;
            spawnTimer = 0f;
            OnWaveStarted?.Invoke(waveNumber);
        }

        public virtual void ClearAllTargets()
        {
            foreach (var target in activeTargets)
            {
                if (target != null)
                    Destroy(target.gameObject);
            }
            activeTargets.Clear();
            OnAllTargetsCleared?.Invoke();
        }

        public virtual int GetActiveTargetCount() => activeTargets.Count;
        public virtual int GetCurrentWave() => currentWave;

        public virtual void SetSpawnInterval(float interval)
        {
            spawnInterval = interval;
        }

        public virtual void SetMaxTargets(int max)
        {
            maxTargets = max;
        }

        protected virtual void OnDrawGizmos()
        {
            if (spawnPoints != null)
            {
                Gizmos.color = Color.magenta;
                foreach (var point in spawnPoints)
                {
                    if (point != null)
                    {
                        Gizmos.DrawWireSphere(point.position, 0.5f);
                        Gizmos.DrawRay(point.position, point.forward * 2f);
                    }
                }
            }
        }
    }
}
