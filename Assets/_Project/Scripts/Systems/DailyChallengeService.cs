using System;
using UnityEngine;

namespace DungeonSurvivors.Systems
{
    /// <summary>
    /// 每日挑战（MVP）：基于日期固定词缀——敌人移速 +25%、灵魂收益 +50%；
    /// 每日只由主菜单按钮开启，重开/回菜单自动结束。
    /// 面试可讲点：每日挑战的"固定种子/词缀"设计、为什么用日期作为唯一变量、收益加成与难度风险。
    /// </summary>
    public static class DailyChallengeService
    {
        public const float EnemySpeedMultiplier = 1.25f;
        public const float SoulsMultiplier = 1.5f;

        public static bool IsActive { get; private set; }

        public static string DateLabel
        {
            get { return DateTime.Now.ToString("yyyy-MM-dd"); }
        }

        public static void Begin()
        {
            IsActive = true;
            Debug.Log("[Daily] Daily challenge started: " + DateLabel);
        }

        public static void Reset()
        {
            if (IsActive) Debug.Log("[Daily] Daily challenge ended.");
            IsActive = false;
        }
    }
}
