using DungeonSurvivors.Core;
using UnityEngine;

namespace DungeonSurvivors.Gameplay.Player
{
    /// <summary>
    /// 玩家生命：订阅受伤事件扣血，死亡时发 PlayerDiedEvent。
    /// 面试可讲点：为什么用事件而不是让敌人直接引用玩家（解耦、方便加护盾/无敌帧）；伤害数字/红闪由表现层订阅。
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private float invulnerableAfterHit = 0.5f;

        private int health;
        private float invulnerableTimer;
        private bool invincible;

        public void SetInvincible(bool value)
        {
            invincible = value;
        }

        public int CurrentHealth
        {
            get { return health; }
        }

        public int MaxHealth
        {
            get { return maxHealth; }
        }

        public float Normalized
        {
            get { return maxHealth <= 0 ? 0f : Mathf.Clamp01((float)health / maxHealth); }
        }

        public void AddMaxHealth(int amount, bool heal)
        {
            int add = Mathf.Max(1, amount);
            maxHealth += add;
            if (heal) health += add;
            health = Mathf.Min(health, maxHealth);
        }

        /// <summary>看广告复活：恢复到指定血量，并给 2 秒无敌保护。</summary>
        public void Revive(int amount)
        {
            if (health > 0) return;
            health = Mathf.Clamp(amount, 1, maxHealth);
            invulnerableTimer = 2f;
        }

        public bool IsAlive
        {
            get { return health > 0; }
        }

        private void Awake()
        {
            health = maxHealth;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<PlayerDamagedEvent>(OnDamaged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerDamagedEvent>(OnDamaged);
        }

        private void Update()
        {
            if (invulnerableTimer > 0f)
            {
                invulnerableTimer -= Time.deltaTime;
            }
        }

        private void OnDamaged(PlayerDamagedEvent evt)
        {
            if (health <= 0 || invulnerableTimer > 0f || invincible) return;

            health -= Mathf.Max(0, evt.Amount);
            invulnerableTimer = invulnerableAfterHit;
            Debug.Log("[PlayerHealth] Damaged " + Mathf.Max(0, evt.Amount) + " -> " + health + "/" + maxHealth);

            if (health <= 0)
            {
                health = 0;
                EventBus.Publish(new PlayerDiedEvent());
            }
        }
    }
}
