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
        public float waveDelay = 3f;

        [Header("Boss Wave")]
        public int bossWaveInterval = 5;
        public float bossHP = 200f;

        [Header("Difficulty Scaling")]
        public float hpMultiplierPerWave = 1.1f;
        public float speedMultiplierPerWave = 1.05f;

        [Header("Enemy Prefabs")]
        public GameObject enemyPrefab;
        public GameObject fastEnemyPrefab;

        protected int enemiesAlive;
        protected int enemiesToSpawn;
        protected float spawnTimer;
        protected bool waveInProgress;
        protected int totalEnemiesThisWave;
        protected int enemiesSpawnedSoFar;

        public System.Action<int> OnWaveStarted;
        public System.Action OnWaveCleared;
        public System.Action<int> OnEnemySpawned;
        public System.Action OnEnemyKilled;

        public int CurrentWave => currentWave;
        public int EnemiesAlive => enemiesAlive;
        public int TotalEnemiesThisWave => totalEnemiesThisWave;
        public int EnemiesSpawnedSoFar => enemiesSpawnedSoFar;

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
                    spawnTimer = Mathf.Max(spawnDelay - currentWave * 0.03f, 0.3f);
                }
            }
        }

        public void StartNextWave()
        {
            currentWave++;
            bool isBossWave = (currentWave % bossWaveInterval == 0);

            totalEnemiesThisWave = baseEnemyCount + (currentWave - 1) * enemiesPerWaveIncrease;
            enemiesToSpawn = totalEnemiesThisWave;
            enemiesAlive = totalEnemiesThisWave;
            enemiesSpawnedSoFar = 0;
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

            bool isBossWave = (currentWave % bossWaveInterval == 0);
            Vector3 spawnPos = spawner.GetRandomSpawnPosition();
            GameObject prefab = enemyPrefab;

            // Boss wave: first spawn is boss, rest are fast enemies
            if (isBossWave && enemiesSpawnedSoFar == 0)
            {
                prefab = enemyPrefab;
            }
            else if (currentWave >= 3 && Random.value < 0.3f + currentWave * 0.02f)
            {
                prefab = fastEnemyPrefab ?? enemyPrefab;
            }

            GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);
            Enemy enemy = obj.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.OnDeath += OnEnemyDied;

                // Difficulty scaling
                float hpMult = Mathf.Pow(hpMultiplierPerWave, currentWave - 1);
                float speedMult = Mathf.Pow(speedMultiplierPerWave, currentWave - 1);

                if (isBossWave && enemiesSpawnedSoFar == 0)
                {
                    enemy.enemyType = EnemyType.Boss;
                    enemy.maxHealth = bossHP * hpMult;
                    enemy.currentHealth = bossHP * hpMult;
                    enemy.damage = 40f;
                    enemy.scoreValue = 50;
                    enemy.ApplyTypeVisuals();
                }
                else
                {
                    enemy.maxHealth *= hpMult;
                    enemy.currentHealth = enemy.maxHealth;
                    enemy.moveSpeed *= speedMult;
                }
            }

            enemiesToSpawn--;
            enemiesSpawnedSoFar++;
            OnEnemySpawned?.Invoke(enemiesSpawnedSoFar);
        }

        void OnEnemyDied()
        {
            enemiesAlive--;
            OnEnemyKilled?.Invoke();

            if (enemiesAlive <= 0 && enemiesToSpawn <= 0)
            {
                waveInProgress = false;
                OnWaveCleared?.Invoke();
                Invoke(nameof(StartNextWave), waveDelay);
            }
        }

        public int GetTotalEnemiesInWave()
        {
            return baseEnemyCount + (currentWave - 1) * enemiesPerWaveIncrease;
        }

        public int GetEnemiesRemaining() => enemiesAlive;

        public float GetWaveProgress()
        {
            if (totalEnemiesThisWave == 0) return 0f;
            return 1f - (float)enemiesAlive / totalEnemiesThisWave;
        }
    }
}
