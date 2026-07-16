using UnityEngine;
using WaveSurvival.Enemies;

namespace WaveSurvival
{
    public class WaveManager : MonoBehaviour
    {
        [Header("Wave Settings")]
        public int currentWave = 0;
        public int baseEnemyCount = 3;
        public int enemiesPerWaveIncrease = 2;
        public float spawnDelay = 1f;

        [Header("Enemy Prefab")]
        public GameObject enemyPrefab;

        protected int enemiesAlive;
        protected int enemiesToSpawn;
        protected float spawnTimer;
        protected bool waveInProgress;

        public System.Action<int> OnWaveStarted;
        public System.Action OnWaveCleared;
        public System.Action<int> OnEnemySpawned;
        public System.Action OnEnemyKilled;

        void Start()
        {
            EnemySpawner spawner = GetComponent<EnemySpawner>();
            if (spawner == null)
                gameObject.AddComponent<EnemySpawner>();

            StartNextWave();
        }

        void Update()
        {
            if (!waveInProgress) return;

            if (enemiesToSpawn > 0)
            {
                spawnTimer -= Time.deltaTime;
                if (spawnTimer <= 0f)
                {
                    SpawnEnemy();
                    spawnTimer = spawnDelay;
                }
            }
        }

        public void StartNextWave()
        {
            currentWave++;
            int totalEnemies = baseEnemyCount + (currentWave - 1) * enemiesPerWaveIncrease;
            enemiesToSpawn = totalEnemies;
            enemiesAlive = totalEnemies;
            waveInProgress = true;
            spawnTimer = 0f;

            OnWaveStarted?.Invoke(currentWave);
        }

        void SpawnEnemy()
        {
            EnemySpawner spawner = GetComponent<EnemySpawner>();
            if (spawner == null || enemyPrefab == null)
            {
                enemiesToSpawn = 0;
                return;
            }

            Vector3 spawnPos = spawner.GetRandomSpawnPosition();
            GameObject obj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            Enemy enemy = obj.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.OnDeath += OnEnemyDied;
            }

            enemiesToSpawn--;
            OnEnemySpawned?.Invoke(enemiesAlive - enemiesToSpawn);
        }

        void OnEnemyDied()
        {
            enemiesAlive--;
            OnEnemyKilled?.Invoke();

            if (enemiesAlive <= 0 && enemiesToSpawn <= 0)
            {
                waveInProgress = false;
                OnWaveCleared?.Invoke();
                Invoke(nameof(StartNextWave), 3f);
            }
        }

        public int GetTotalEnemiesInWave()
        {
            return baseEnemyCount + (currentWave - 1) * enemiesPerWaveIncrease;
        }

        public int GetEnemiesRemaining()
        {
            return enemiesAlive;
        }
    }
}
