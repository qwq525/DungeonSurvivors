using System.Collections.Generic;

namespace DungeonSurvivors.Systems
{
    public class CodexEntry
    {
        public readonly string Id;
        public readonly string Category;
        public readonly string Title;

        public CodexEntry(string id, string category, string title)
        {
            Id = id;
            Category = category;
            Title = title;
        }
    }

    /// <summary>
    /// 图鉴配置：武器 / 敌人 / 变异（升级项）三类，解锁后显示。
    /// 面试可讲点：收集系统的数据组织方式（id + 分类 + 标题），方便后续加图标/描述/奖励。
    /// </summary>
    public static class CodexCatalog
    {
        public static readonly List<CodexEntry> Entries = new List<CodexEntry>
        {
            new CodexEntry("weapon:missile", "武器", "飞弹发射器"),
            new CodexEntry("weapon:orbit", "武器", "环绕飞刀"),
            new CodexEntry("enemy:slime", "敌人", "勇者史莱姆"),
            new CodexEntry("enemy:elite", "敌人", "精英勇者"),
            new CodexEntry("enemy:boss", "敌人", "勇者队长"),
            new CodexEntry("upgrade:CLAWS", "变异", "利爪"),
            new CodexEntry("upgrade:FANGS", "变异", "獠牙"),
            new CodexEntry("upgrade:LEGS", "变异", "快腿"),
            new CodexEntry("upgrade:HIDE", "变异", "厚皮"),
            new CodexEntry("upgrade:ORBIT BLADES", "变异", "环绕飞刀"),
            new CodexEntry("upgrade:ORBIT +1", "变异", "飞刀精通")
        };
    }
}
