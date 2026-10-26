using UnityEngine;

namespace DungeonSurvivors.Core
{
    public readonly struct EnemyDiedEvent
    {
        public readonly Vector2 Position;
        public readonly int XpValue;
        public readonly bool IsElite;
        public readonly bool IsBoss;

        public EnemyDiedEvent(Vector2 position, int xpValue, bool isElite = false, bool isBoss = false)
        {
            Position = position;
            XpValue = xpValue;
            IsElite = isElite;
            IsBoss = isBoss;
        }
    }

    public readonly struct ExperiencePickupEvent
    {
        public readonly int Amount;

        public ExperiencePickupEvent(int amount)
        {
            Amount = amount;
        }
    }

    public readonly struct WeaponFiredEvent
    {
    }

    public readonly struct PlayerDamagedEvent
    {
        public readonly int Amount;

        public PlayerDamagedEvent(int amount)
        {
            Amount = amount;
        }
    }

    public readonly struct DamageDealtEvent
    {
        public readonly Vector2 Position;
        public readonly int Amount;
        public readonly bool IsCritical;

        public DamageDealtEvent(Vector2 position, int amount, bool isCritical)
        {
            Position = position;
            Amount = amount;
            IsCritical = isCritical;
        }
    }

    public readonly struct LevelUpEvent
    {
        public readonly int Level;

        public LevelUpEvent(int level)
        {
            Level = level;
        }
    }

    public readonly struct PlayerDiedEvent
    {
    }
}
