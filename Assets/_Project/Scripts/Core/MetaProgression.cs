using UnityEngine;

namespace DungeonSurvivors.Core
{
    /// <summary>
    /// 局外成长服务：灵魂货币、最佳记录、天赋等级（静态服务，读写 SaveSystem）。
    /// 面试可讲点：局外数值与局内分离；每局结束结算货币；天赋树用 id+level 存，方便扩展。
    /// </summary>
    public static class MetaProgression
    {
        public static int Souls
        {
            get { return SaveSystem.Current.meta.souls; }
        }

        public static int RunsPlayed
        {
            get { return SaveSystem.Current.meta.runsPlayed; }
        }

        public static int BestKills
        {
            get { return SaveSystem.Current.meta.bestKills; }
        }

        public static int BestSurvivalSeconds
        {
            get { return SaveSystem.Current.meta.bestSurvivalSeconds; }
        }

        public static void AddSouls(int amount)
        {
            if (amount <= 0) return;
            SaveSystem.Current.meta.souls += amount;
        }

        public static bool TrySpendSouls(int amount)
        {
            if (amount <= 0) return true;
            if (SaveSystem.Current.meta.souls < amount) return false;
            SaveSystem.Current.meta.souls -= amount;
            return true;
        }

        /// <summary>一局结束：累计货币 + 刷新最佳记录 + 保存。</summary>
        public static void RegisterRun(int soulsEarned, int kills, float survivalSeconds)
        {
            MetaData meta = SaveSystem.Current.meta;
            meta.souls += Mathf.Max(0, soulsEarned);
            meta.runsPlayed++;
            if (kills > meta.bestKills) meta.bestKills = kills;

            int seconds = Mathf.Max(0, Mathf.FloorToInt(survivalSeconds));
            if (seconds > meta.bestSurvivalSeconds) meta.bestSurvivalSeconds = seconds;

            SaveSystem.Save();
        }

        public static int GetTalentLevel(string id)
        {
            TalentEntry entry = SaveSystem.Current.meta.talents.Find(t => t.id == id);
            return entry != null ? entry.level : 0;
        }

        public static void SetTalentLevel(string id, int level)
        {
            TalentEntry entry = SaveSystem.Current.meta.talents.Find(t => t.id == id);
            if (entry == null)
            {
                SaveSystem.Current.meta.talents.Add(new TalentEntry(id, level));
            }
            else
            {
                entry.level = level;
            }

            SaveSystem.Save();
        }
    }
}
