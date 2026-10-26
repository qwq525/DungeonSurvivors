using System.Collections.Generic;

namespace DungeonSurvivors.Systems
{
    public class TalentDefinition
    {
        public readonly string Id;
        public readonly string DisplayName;
        public readonly int MaxLevel;
        public readonly int BaseCost;
        public readonly string Description;

        public TalentDefinition(string id, string displayName, int maxLevel, int baseCost, string description)
        {
            Id = id;
            DisplayName = displayName;
            MaxLevel = maxLevel;
            BaseCost = baseCost;
            Description = description;
        }
    }

    /// <summary>
    /// 局外天赋配置（MVP）：id / 名称 / 最大等级 / 基础花费 / 描述。
    /// 面试可讲点：天赋用 id+level 存（方便扩展），成本随等级线性增长，效果在开局统一应用。
    /// </summary>
    public static class TalentCatalog
    {
        public static readonly List<TalentDefinition> Talents = new List<TalentDefinition>
        {
            new TalentDefinition("claws", "利爪", 5, 20, "每级 +1 伤害"),
            new TalentDefinition("hide", "厚皮", 5, 20, "每级 +10 最大生命"),
            new TalentDefinition("legs", "快腿", 5, 25, "每级 +3% 移动速度"),
            new TalentDefinition("fangs", "獠牙", 5, 25, "每级 +3% 攻击速度")
        };
    }
}
