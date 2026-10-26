using System.Collections.Generic;
using DungeonSurvivors.Core;
using DungeonSurvivors.Gameplay.Player;
using DungeonSurvivors.Systems;
using UnityEngine;

namespace DungeonSurvivors.Gameplay.Enemies
{
    /// <summary>
    /// 敌人刷新器（W1-3）：按"波次"刷怪——每 waveInterval 秒开一波，波内每隔 dripInterval 秒刷一只；
    /// 每波数量递增、敌人血量/移速随波次成长；同屏有上限，全部走对象池。
    /// 面试可讲点：波次节奏与难度曲线、对象池回收、为什么统一由 Spawner 管理敌人生命周期。
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemyBase[] enemyPrefabs;
        [SerializeField] private Transform poolRoot;

        [Header("波次")]
        [SerializeField] private float waveInterval = 15f;
        [SerializeField] private int baseEnemiesPerWave = 5;
        [SerializeField] private int enemiesGrowthPerWave = 2;
        [SerializeField] private float dripInterval = 0.35f;

        [Header("限制")]
        [SerializeField] private int maxAlive = 50;
        [SerializeField] private float spawnRadius = 9.5f;
        [SerializeField] private float spawnRadiusY = 9.5f;

        private readonly List<ObjectPool<EnemyBase>> pools = new List<ObjectPool<EnemyBase>>();
        private readonly Dictionary<EnemyBase, ObjectPool<EnemyBase>> poolByInstance = new Dictionary<EnemyBase, ObjectPool<EnemyBase>>();
        private readonly List<EnemyBase> alive = new List<EnemyBase>();
        private float waveTimer;
        private float dripTimer;
        private int pendingEnemies;
        private int waveIndex;

        public int CurrentWave
        {
            get { return waveIndex; }
        }

        public void Configure(EnemyBase prefab, Transform poolRootTransform)
        {
            Configure(new[] { prefab }, poolRootTransform);
        }

        public void Configure(EnemyBase[] prefabs, Transform poolRootTransform)
        {
            enemyPrefabs = prefabs;
            poolRoot = poolRootTransform;
        }

        public void TuneForArena(int baseCount, int growth, int maxAliveCount, float radius, float drip, float radiusY = 9.5f)
        {
            baseEnemiesPerWave = Mathf.Max(1, baseCount);
            enemiesGrowthPerWave = Mathf.Max(0, growth);
            maxAlive = Mathf.Max(10, maxAliveCount);
            spawnRadius = Mathf.Max(3f, radius);
            spawnRadiusY = Mathf.Max(3f, radiusY);
            dripInterval = Mathf.Max(0.05f, drip);
        }

        private void Awake()
        {
            waveTimer = 1.5f;
            if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;
            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                EnemyBase prefab = enemyPrefabs[i];
                if (prefab == null) continue;
                pools.Add(new ObjectPool<EnemyBase>(() => CreateEnemy(prefab), poolRoot, 10, Mathf.Max(20, (maxAlive + 20) / enemyPrefabs.Length)));
            }
        }

        private EnemyBase CreateEnemy(EnemyBase prefab)
        {
            EnemyBase enemy = Instantiate(prefab, poolRoot);
            enemy.Initialize(ReleaseEnemy);
            return enemy;
        }

        private void ReleaseEnemy(EnemyBase enemy)
        {
            alive.Remove(enemy);
            ObjectPool<EnemyBase> owner;
            if (poolByInstance.TryGetValue(enemy, out owner))
            {
                poolByInstance.Remove(enemy);
                owner.Release(enemy);
            }
        }

        private EnemyBase GetEnemy()
        {
            if (pools.Count == 0) return null;
            ObjectPool<EnemyBase> selected = pools[Random.Range(0, pools.Count)];
            EnemyBase enemy = selected.Get();
            poolByInstance[enemy] = selected;
            return enemy;
        }

        private Vector2 GetSpawnPosition()
        {
            if (TerrainGrid.Instance == null) return Vector2.zero;
            PlayerController player = PlayerController.Instance;
            Vector2 center = player != null ? (Vector2)player.transform.position : Vector2.zero;
            return TerrainGrid.Instance.FindSpawnPosition(center, 7f);
        }

        private void Update()
        {
            if (pools.Count == 0) return;
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

            if (pendingEnemies > 0)
            {
                dripTimer -= Time.deltaTime;
                if (dripTimer > 0f) return;

                dripTimer = dripInterval;
                pendingEnemies--;
                if (alive.Count < maxAlive) SpawnOne();
                return;
            }

            waveTimer -= Time.deltaTime;
            if (waveTimer > 0f) return;
            StartNextWave();
        }

        private void StartNextWave()
        {
            waveIndex++;
            pendingEnemies = baseEnemiesPerWave + enemiesGrowthPerWave * (waveIndex - 1);

            if (waveIndex % 5 == 0)
            {
                pendingEnemies = Mathf.Max(1, pendingEnemies / 2);
                SpawnSpecial(true);
            }
            else if (waveIndex % 3 == 0)
            {
                SpawnSpecial(false);
            }

            dripTimer = 0f;
            waveTimer = waveInterval;
            Debug.Log("[Wave] " + waveIndex + " start (" + pendingEnemies + " enemies)");
        }

        /// <summary>Boss 每 5 波、精英每 3 波。</summary>
        private void SpawnSpecial(bool boss)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector2 position = new Vector2(Mathf.Cos(angle) * spawnRadius, Mathf.Sin(angle) * spawnRadiusY);

            EnemyBase enemy = GetEnemy();
            if (enemy == null) return;
            enemy.transform.position = TerrainGrid.Instance != null ? GetSpawnPosition() : position;
            enemy.ApplyRank(waveIndex - 1, !boss, boss);
            alive.Add(enemy);

            Debug.Log("[Wave] " + (boss ? "BOSS" : "ELITE") + " spawned at wave " + waveIndex);
        }

        private void SpawnOne()
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector2 position = new Vector2(Mathf.Cos(angle) * spawnRadius, Mathf.Sin(angle) * spawnRadiusY);

            EnemyBase enemy = GetEnemy();
            if (enemy == null) return;
            enemy.transform.position = TerrainGrid.Instance != null ? GetSpawnPosition() : position;
            if (waveIndex <= 2) Debug.Log("[Spawn] " + enemy.name + " at " + enemy.transform.position);
            enemy.ApplyDifficulty(waveIndex - 1);
            alive.Add(enemy);
        }

        /// <summary>压测用：直接刷 count 个敌人（不消耗波次逻辑），返回实际数量。</summary>
        public int SpawnForStressTest(int count)
        {
            if (pools.Count == 0) return 0;

            for (int i = 0; i < count; i++)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float radius = Random.Range(3f, spawnRadius);
                Vector2 position = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * spawnRadiusY * (radius / Mathf.Max(0.01f, spawnRadius)));

                EnemyBase enemy = GetEnemy();
                if (enemy == null) return i;
                enemy.transform.position = TerrainGrid.Instance != null ? GetSpawnPosition() : position;
                enemy.ApplyDifficulty(0);
                alive.Add(enemy);
            }

            return count;
        }

        /// <summary>压测/重开用：回收所有存活敌人。</summary>
        public void ClearAll()
        {
            for (int i = alive.Count - 1; i >= 0; i--)
            {
                if (alive[i] != null)
                {
                    ObjectPool<EnemyBase> owner;
                    if (poolByInstance.TryGetValue(alive[i], out owner))
                    {
                        poolByInstance.Remove(alive[i]);
                        owner.Release(alive[i]);
                    }
                }
            }

            alive.Clear();
        }
    }
}
