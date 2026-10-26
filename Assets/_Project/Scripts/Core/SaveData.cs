using System;
using System.Collections.Generic;

namespace DungeonSurvivors.Core
{
    /// <summary>
    /// 存档数据结构（JSON）：设置 + 局外成长 + 最佳记录。
    /// 面试可讲点：数据结构版本号（迁移）、JsonUtility 不支持 Dictionary 所以用 List、默认值兜底。
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int version = SaveSystem.CurrentVersion;
        public SettingsData settings = new SettingsData();
        public MetaData meta = new MetaData();
    }

    [Serializable]
    public class SettingsData
    {
        public float masterVolume = 1f;
        public bool fullscreen = true;
        public int qualityLevel = 1;
    }

    [Serializable]
    public class MetaData
    {
        public int souls;
        public int runsPlayed;
        public int bestKills;
        public int bestSurvivalSeconds;
        public bool tutorialSeen;
        public List<TalentEntry> talents = new List<TalentEntry>();
        public List<string> unlockedWeapons = new List<string>();
        public List<string> achievements = new List<string>();
        public List<string> codex = new List<string>();
    }

    [Serializable]
    public class TalentEntry
    {
        public string id;
        public int level;

        public TalentEntry()
        {
        }

        public TalentEntry(string id, int level)
        {
            this.id = id;
            this.level = level;
        }
    }
}
