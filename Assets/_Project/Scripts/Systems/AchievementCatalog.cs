using System.Collections.Generic;

namespace DungeonSurvivors.Systems
{
    public class AchievementDefinition
    {
        public readonly string Id;
        public readonly string Title;
        public readonly string Description;

        public AchievementDefinition(string id, string title, string description)
        {
            Id = id;
            Title = title;
            Description = description;
        }
    }

    /// <summary>
    /// 成就配置（MVP）：6 个轻量成就，覆盖击杀/等级/死亡/复活/天赋，展示"完整游戏壳"。
    /// 面试可讲点：成就条件与事件解耦；存 id 到存档，方便扩展与本地化。
    /// </summary>
    public static class AchievementCatalog
    {
        public static readonly List<AchievementDefinition> Achievements = new List<AchievementDefinition>
        {
            new AchievementDefinition("first_blood", "初次杀戮", "击杀第一个勇者"),
            new AchievementDefinition("slayer_50", "屠戮者", "单局击杀 50 名勇者"),
            new AchievementDefinition("level_5", "进化", "达到 5 级"),
            new AchievementDefinition("first_death", "初次死亡", "死亡一次"),
            new AchievementDefinition("revive_once", "第二次机会", "看广告复活一次"),
            new AchievementDefinition("talent_1", "变异起点", "解锁第一个天赋")
        };
    }
}
