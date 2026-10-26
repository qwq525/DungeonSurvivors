using System;
using DungeonSurvivors.Core;
using DungeonSurvivors.Gameplay.Enemies;
using UnityEngine;

namespace DungeonSurvivors.Gameplay.Weapons
{
    /// <summary>
    /// 追踪弹（MVP）：每帧朝目标移动，距离判定命中；目标死亡/超时自动回收。
    /// 面试可讲点：为什么用"目标引用 + 距离判定"而不是物理碰撞；池化对象生命周期管理。
    /// </summary>
    public class Projectile : MonoBehaviour, IPoolable
    {
        [SerializeField] private float speed = 12f;
        [SerializeField] private float hitRadius = 0.25f;
        [SerializeField] private float maxLifetime = 2f;

        private EnemyBase target;
        private int damage;
        private float life;
        private Action<Projectile> releaseCallback;

        public void Initialize(EnemyBase targetEnemy, int damageAmount, Action<Projectile> release)
        {
            target = targetEnemy;
            damage = damageAmount;
            releaseCallback = release;
        }

        public void OnSpawn()
        {
            life = maxLifetime;
        }

        public void OnDespawn()
        {
            target = null;
        }

        private void Update()
        {
            if (target == null || !target.IsAlive)
            {
                Release();
                return;
            }

            life -= Time.deltaTime;
            if (life <= 0f)
            {
                Release();
                return;
            }

            Vector2 toTarget = (Vector2)target.transform.position - (Vector2)transform.position;
            float distance = toTarget.magnitude;
            if (distance <= hitRadius)
            {
                target.TakeDamage(damage, transform.position);
                Release();
                return;
            }

            transform.position += (Vector3)(toTarget.normalized * (speed * Time.deltaTime));
        }

        private void Release()
        {
            if (releaseCallback != null) releaseCallback(this);
        }
    }
}
