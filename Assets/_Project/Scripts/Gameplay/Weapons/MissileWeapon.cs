using DungeonSurvivors.Core;
using DungeonSurvivors.Gameplay.Enemies;
using UnityEngine;

namespace DungeonSurvivors.Gameplay.Weapons
{
    /// <summary>
    /// 自动索敌武器（MVP）：冷却好了就找范围内最近的敌人，发射追踪弹。
    /// 面试可讲点：武器用"数据 + 行为"拆开（后续接 ScriptableObject 配置）；索敌的复杂度与优化方向。
    /// </summary>
    public class MissileWeapon : MonoBehaviour
    {
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform poolRoot;
        [SerializeField] private float cooldown = 0.8f;
        [SerializeField] private float range = 7f;
        [SerializeField] private int damage = 3;

        private ObjectPool<Projectile> pool;
        private float cooldownTimer;

        public void Configure(Projectile prefab, Transform poolRootTransform)
        {
            projectilePrefab = prefab;
            poolRoot = poolRootTransform;
        }

        public void AddDamage(int amount)
        {
            damage += Mathf.Max(1, amount);
        }

        public void AddAttackSpeed(float percent)
        {
            cooldown = Mathf.Max(0.15f, cooldown * (1f - percent));
        }

        private void Awake()
        {
            if (projectilePrefab == null) return;
            pool = new ObjectPool<Projectile>(CreateProjectile, poolRoot, 8, 64);
        }

        private Projectile CreateProjectile()
        {
            Projectile projectile = Instantiate(projectilePrefab, poolRoot);
            projectile.Initialize(null, 0, ReleaseProjectile);
            return projectile;
        }

        private void ReleaseProjectile(Projectile projectile)
        {
            pool.Release(projectile);
        }

        private void Update()
        {
            if (pool == null) return;
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer > 0f) return;

            EnemyBase target = FindNearestEnemy();
            if (target == null) return;

            cooldownTimer = cooldown;
            Projectile projectile = pool.Get();
            projectile.transform.position = transform.position;
            projectile.Initialize(target, damage, ReleaseProjectile);
            EventBus.Publish(new WeaponFiredEvent());
        }

        private EnemyBase FindNearestEnemy()
        {
            EnemyBase nearest = null;
            float bestSqrDistance = range * range;
            Vector2 origin = transform.position;

            for (int i = 0; i < EnemyBase.ActiveEnemies.Count; i++)
            {
                EnemyBase enemy = EnemyBase.ActiveEnemies[i];
                if (enemy == null || !enemy.IsAlive) continue;

                float sqrDistance = ((Vector2)enemy.transform.position - origin).sqrMagnitude;
                if (sqrDistance < bestSqrDistance)
                {
                    bestSqrDistance = sqrDistance;
                    nearest = enemy;
                }
            }

            return nearest;
        }
    }
}
