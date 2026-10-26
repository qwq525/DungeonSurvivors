using DungeonSurvivors.Core;
using DungeonSurvivors.Gameplay.Player;
using UnityEngine;

namespace DungeonSurvivors.Gameplay.Enemies
{
    /// <summary>敌人战斗配置：近战贴身，远程保持距离并发射投射物。</summary>
    public class EnemyCombatProfile : MonoBehaviour
    {
        public enum AttackType
        {
            Melee,
            Ranged
        }

        [SerializeField] private AttackType attackType = AttackType.Melee;
        [SerializeField] private float attackRange = 0.7f;
        [SerializeField] private float retreatDistance = 2.5f;
        [SerializeField] private float attackInterval = 0.8f;
        [SerializeField] private int damage = 3;
        [SerializeField] private EnemyProjectile projectilePrefab;

        public bool IsRanged { get { return attackType == AttackType.Ranged; } }
        public float AttackRange { get { return attackRange; } }
        public float RetreatDistance { get { return retreatDistance; } }
        public float AttackInterval { get { return Mathf.Max(0.2f, attackInterval); } }
        public int Damage { get { return damage; } }
        public EnemyProjectile ProjectilePrefab { get { return projectilePrefab; } }

        public void Configure(AttackType type, float range, float retreat, float interval, int damageAmount,
            EnemyProjectile projectile)
        {
            attackType = type;
            attackRange = Mathf.Max(0.4f, range);
            retreatDistance = Mathf.Max(0.5f, retreat);
            attackInterval = Mathf.Max(0.2f, interval);
            damage = Mathf.Max(1, damageAmount);
            projectilePrefab = projectile;
        }
    }
}
