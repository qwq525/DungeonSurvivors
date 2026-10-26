using System;
using System.Collections.Generic;
using DungeonSurvivors.Core;
using DungeonSurvivors.Gameplay.Combat;
using DungeonSurvivors.Gameplay.Player;
using DungeonSurvivors.Systems;
using UnityEngine;

namespace DungeonSurvivors.Gameplay.Enemies
{
    /// <summary>
    /// 敌人基类（MVP 版）：
    /// 1) 有限状态机 Idle / Chase / Attack / Hurt / Die；
    /// 2) 用 Transform 移动 + 手工距离检测（同屏几百单位时比 PhysX2D 便宜，后续升级空间哈希）；
    /// 3) 对象池：OnSpawn 重置血量/状态，OnDespawn 清空目标。
    /// 面试可讲点：为什么不用 Animator 状态机、为什么手工检测、对象池回收要重置什么。
    /// </summary>
    public class EnemyBase : MonoBehaviour, IDamageable, IPoolable
    {
        public enum State
        {
            Idle,
            Chase,
            Attack,
            Hurt,
            Die
        }

        [Header("属性")]
        [SerializeField] private float moveSpeed = 2.2f;
        [SerializeField] private int maxHealth = 10;
        [SerializeField] private int contactDamage = 3;
        [SerializeField] private float contactRadius = 0.55f;
        [SerializeField] private float attackInterval = 0.8f;
        [SerializeField] private int xpValue = 1;

        [Header("波次成长")]
        [SerializeField] private int healthPerWave = 3;
        [SerializeField] private float speedPerWavePercent = 0.04f;
        [SerializeField] private float knockbackDistance = 0.12f;

        private SpriteRenderer body;
        private Color baseColor;
        private State state = State.Idle;
        private int health;
        private int baseMaxHealth;
        private float baseMoveSpeed;
        private Vector3 baseScale;
        private int baseXpValue;
        private int baseContactDamage;
        private float attackTimer;
        private float hurtTimer;
        private Action<EnemyBase> despawnCallback;
        private Transform target;

        /// <summary>存活敌人列表（MVP 索敌用；后续换成空间哈希索引）。</summary>
        public static readonly List<EnemyBase> ActiveEnemies = new List<EnemyBase>();

        public bool IsAlive
        {
            get { return health > 0; }
        }

        public bool IsElite { get; private set; }
        public bool IsBoss { get; private set; }

        public void Initialize(Action<EnemyBase> onDespawnCallback)
        {
            despawnCallback = onDespawnCallback;
        }

        private void Awake()
        {
            body = GetComponentInChildren<SpriteRenderer>();
            if (body != null) baseColor = body.color;
            baseMaxHealth = maxHealth;
            baseMoveSpeed = moveSpeed;
            baseScale = transform.localScale;
            baseXpValue = xpValue;
            baseContactDamage = contactDamage;
            health = maxHealth;
        }

        private void OnEnable()
        {
            if (!ActiveEnemies.Contains(this)) ActiveEnemies.Add(this);
        }

        private void OnDisable()
        {
            ActiveEnemies.Remove(this);
        }

        public void OnSpawn()
        {
            health = maxHealth;
            state = State.Idle;
            attackTimer = 0f;
            hurtTimer = 0f;
            target = null;
            xpValue = baseXpValue;
            contactDamage = baseContactDamage;
            IsElite = false;
            IsBoss = false;
            transform.localScale = baseScale;
            if (body != null) body.color = baseColor;
        }

        public void OnDespawn()
        {
            target = null;
        }

        private void Update()
        {
            if (health <= 0) return;

            if (target == null)
            {
                PlayerController player = PlayerController.Instance;
                if (player != null) target = player.transform;
            }

            if (target == null)
            {
                state = State.Idle;
                return;
            }

            Vector2 toPlayer = (Vector2)target.position - (Vector2)transform.position;
            float distance = toPlayer.magnitude;

            switch (state)
            {
                case State.Hurt:
                    hurtTimer -= Time.deltaTime;
                    if (hurtTimer <= 0f)
                    {
                        if (body != null) body.color = baseColor;
                        state = State.Chase;
                    }
                    break;

                case State.Chase:
                    if (distance <= contactRadius)
                    {
                        state = State.Attack;
                        Debug.Log("[Enemy] Attack start, distance=" + distance.ToString("F2") + " radius=" + contactRadius.ToString("F2") + " dmg=" + contactDamage);
                        break;
                    }
                    MoveTowards(toPlayer.normalized);
                    break;

                case State.Attack:
                    if (distance > contactRadius * 1.15f)
                    {
                        state = State.Chase;
                        break;
                    }
                    attackTimer -= Time.deltaTime;
                    if (attackTimer <= 0f)
                    {
                        attackTimer = attackInterval;
                        EventBus.Publish(new PlayerDamagedEvent(contactDamage));
                    }
                    break;

                default:
                    state = State.Chase;
                    break;
            }
        }

        private void MoveTowards(Vector2 direction)
        {
            transform.position += (Vector3)(direction * (moveSpeed * Time.deltaTime));
        }

        public void TakeDamage(int amount, Vector2 hitPoint)
        {
            if (health <= 0) return;

            int applied = Mathf.Max(1, amount);
            health -= applied;
            EventBus.Publish(new DamageDealtEvent(transform.position, applied, false));

            ApplyKnockback();

            if (health <= 0)
            {
                Die();
                return;
            }

            state = State.Hurt;
            hurtTimer = 0.12f;
            if (body != null) body.color = Color.white;
        }

        private void ApplyKnockback()
        {
            PlayerController player = PlayerController.Instance;
            if (player == null || knockbackDistance <= 0f) return;

            Vector2 away = (Vector2)transform.position - (Vector2)player.transform.position;
            if (away.sqrMagnitude < 0.0001f) away = Vector2.up;
            transform.position += (Vector3)(away.normalized * knockbackDistance);
        }

        /// <summary>按波次成长：血量/移速随波次提升（Spawner 在出生时调用）。</summary>
        public void ApplyDifficulty(int waveIndex)
        {
            int wave = Mathf.Max(0, waveIndex);
            maxHealth = baseMaxHealth + healthPerWave * wave;
            health = maxHealth;
            moveSpeed = baseMoveSpeed * (1f + speedPerWavePercent * wave);

            if (DailyChallengeService.IsActive)
            {
                moveSpeed *= DailyChallengeService.EnemySpeedMultiplier;
            }
        }

        /// <summary>精英/Boss 标记：更高血量、体型、伤害与经验（Spawner 在特殊波次调用）。</summary>
        public void ApplyRank(int waveIndex, bool elite, bool boss)
        {
            ApplyDifficulty(waveIndex);
            IsElite = elite;
            IsBoss = boss;

            if (boss)
            {
                maxHealth = Mathf.RoundToInt(maxHealth * 20f);
                health = maxHealth;
                moveSpeed *= 0.8f;
                contactDamage = baseContactDamage * 3;
                xpValue = baseXpValue * 20;
                transform.localScale = baseScale * 2f;
                if (body != null) body.color = new Color(0.85f, 0.25f, 0.3f);
            }
            else if (elite)
            {
                maxHealth = Mathf.RoundToInt(maxHealth * 5f);
                health = maxHealth;
                contactDamage = baseContactDamage * 2;
                xpValue = baseXpValue * 5;
                transform.localScale = baseScale * 1.4f;
                if (body != null) body.color = new Color(1f, 0.75f, 0.3f);
            }
        }

        private void Die()
        {
            EventBus.Publish(new EnemyDiedEvent(transform.position, xpValue, IsElite, IsBoss));
            health = 0;
            if (despawnCallback != null) despawnCallback(this);
        }
    }
}
