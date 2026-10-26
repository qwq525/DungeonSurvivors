using DungeonSurvivors.Core;
using UnityEngine;

namespace DungeonSurvivors.Systems
{
    /// <summary>成就服务：解锁/查询，写入存档并即时保存。</summary>
    public static class AchievementService
    {
        public static bool Has(string id)
        {
            return SaveSystem.Current.meta.achievements.Contains(id);
        }

        public static void Unlock(string id)
        {
            if (Has(id)) return;

            SaveSystem.Current.meta.achievements.Add(id);
            SaveSystem.Save();
            Debug.Log("[Achievement] Unlocked: " + id);
        }

        public static int UnlockedCount
        {
            get { return SaveSystem.Current.meta.achievements.Count; }
        }

        public static int TotalCount
        {
            get { return AchievementCatalog.Achievements.Count; }
        }
    }
}
