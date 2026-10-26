using System.Collections.Generic;
using DungeonSurvivors.Gameplay.Enemies;
using UnityEngine;

namespace DungeonSurvivors.Gameplay.Weapons
{
    /// <summary>
    /// 环绕飞刀（第二种武器）：围绕玩家旋转，每 tick 对范围内敌人造成伤害；通过升级解锁/强化。
    /// 面试可讲点：第二把武器的实现取舍（手工距离检测而非碰撞体）、解锁式武器池、与升级系统解耦。
    /// </summary>
    public class OrbitWeapon : MonoBehaviour
    {
        [SerializeField] private float radius = 1.15f;
        [SerializeField] private float rotationSpeed = 200f;
        [SerializeField] private int damage = 2;
        [SerializeField] private float tickInterval = 0.25f;
        [SerializeField] private int bladeCount = 2;

        private readonly List<Transform> blades = new List<Transform>();
        private float angle;
        private float tickTimer;
        private Sprite bladeSprite;

        public bool IsActive { get; private set; }

        public void Activate()
        {
            if (IsActive) return;
            IsActive = true;
            DungeonSurvivors.Systems.CodexService.Discover("weapon:orbit");
            CreateBlades();
        }

        public void AddDamage(int amount)
        {
            damage += Mathf.Max(1, amount);
        }

        private void CreateBlades()
        {
            if (bladeSprite == null)
            {
                Texture2D texture = Texture2D.whiteTexture;
                bladeSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f), 1f);
            }

            for (int i = 0; i < bladeCount; i++)
            {
                GameObject blade = new GameObject("OrbitBlade");
                blade.transform.SetParent(transform, false);
                SpriteRenderer renderer = blade.AddComponent<SpriteRenderer>();
                renderer.sprite = bladeSprite;
                renderer.color = new Color32(255, 179, 71, 255);
                renderer.sortingOrder = 9;
                blade.transform.localScale = Vector3.one * 0.35f;
                blades.Add(blade.transform);
            }
        }

        private void Update()
        {
            if (!IsActive) return;

            angle += rotationSpeed * Time.deltaTime;
            for (int i = 0; i < blades.Count; i++)
            {
                float radians = (angle + i * (Mathf.PI * 2f / blades.Count)) * Mathf.Deg2Rad;
                blades[i].localPosition = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * radius;
            }

            tickTimer -= Time.deltaTime;
            if (tickTimer > 0f) return;

            tickTimer = tickInterval;
            DamageEnemiesInRange();
        }

        private void DamageEnemiesInRange()
        {
            Vector2 center = transform.position;
            for (int i = EnemyBase.ActiveEnemies.Count - 1; i >= 0; i--)
            {
                EnemyBase enemy = EnemyBase.ActiveEnemies[i];
                if (enemy == null || !enemy.IsAlive) continue;

                if (((Vector2)enemy.transform.position - center).sqrMagnitude <= radius * radius)
                {
                    enemy.TakeDamage(damage, enemy.transform.position);
                }
            }
        }

        private void OnDestroy()
        {
            if (bladeSprite != null) Destroy(bladeSprite);
        }
    }
}
