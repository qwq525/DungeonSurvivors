using UnityEngine;

namespace DungeonSurvivors.Gameplay.Combat
{
    /// <summary>统一受伤入口：武器/陷阱/技能都只依赖这个接口，方便后面加暴击/减伤/护盾。</summary>
    public interface IDamageable
    {
        bool IsAlive { get; }
        void TakeDamage(int amount, Vector2 hitPoint);
    }
}
