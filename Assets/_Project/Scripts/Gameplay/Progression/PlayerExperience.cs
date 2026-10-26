using DungeonSurvivors.Core;
using DungeonSurvivors.Systems;
using UnityEngine;

namespace DungeonSurvivors.Gameplay.Progression
{
    /// <summary>
    /// 玩家经验/等级：接收拾取事件，累计经验；升级时发 LevelUpEvent（后续 HUD/三选一面板订阅）。
    /// 面试可讲点：事件驱动、升级曲线、UI 与逻辑解耦。
    /// </summary>
    public class PlayerExperience : MonoBehaviour
    {
        [SerializeField] private int level = 1;
        [SerializeField] private int currentXp;
        [SerializeField] private int xpToNextLevel = 5;
        [SerializeField] private float xpGrowth = 1.25f;

        public int Level
        {
            get { return level; }
        }

        public int CurrentXp
        {
            get { return currentXp; }
        }

        public int XpToNextLevel
        {
            get { return xpToNextLevel; }
        }

        public float NormalizedProgress
        {
            get { return xpToNextLevel <= 0 ? 0f : Mathf.Clamp01((float)currentXp / xpToNextLevel); }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<ExperiencePickupEvent>(OnExperiencePickup);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ExperiencePickupEvent>(OnExperiencePickup);
        }

        private void OnExperiencePickup(ExperiencePickupEvent evt)
        {
            currentXp += Mathf.Max(0, evt.Amount);

            while (currentXp >= xpToNextLevel)
            {
                currentXp -= xpToNextLevel;
                level++;
                xpToNextLevel = Mathf.CeilToInt(xpToNextLevel * xpGrowth);
                EventBus.Publish(new LevelUpEvent(level));

                if (level >= 5) AchievementService.Unlock("level_5");
            }
        }
    }
}
