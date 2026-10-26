using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DungeonSurvivors.Analytics
{
    [Serializable]
    public class AnalyticsCounter
    {
        public string key;
        public int value;
    }

    [Serializable]
    public class AnalyticsData
    {
        public int totalRuns;
        public int totalKills;
        public int adViews;
        public int upgradeChoices;
        public int revives;
        public int lastRunKills;
        public int lastRunLevel;
        public int lastRunSeconds;
        public List<AnalyticsCounter> counters = new List<AnalyticsCounter>();
    }

    /// <summary>
    /// 本地埋点服务（Mock 数据层）：记录运行/击杀/广告/升级等事件，存到 persistentDataPath。
    /// 面试可讲点：客户端埋点的作用（留存/难度/商业化的数据依据）、事件设计、本地缓存与批量上报思路。
    /// </summary>
    public static class AnalyticsService
    {
        private const string FileName = "analytics.json";
        private static AnalyticsData data;

        public static AnalyticsData Data
        {
            get
            {
                if (data == null) data = Load();
                return data;
            }
        }

        public static string Path
        {
            get { return System.IO.Path.Combine(Application.persistentDataPath, FileName); }
        }

        public static void TrackRunStart()
        {
            Data.totalRuns++;
            Increment("run_start");
            Save();
        }

        public static void TrackRunEnd(int kills, int level, int seconds)
        {
            Data.totalKills += Mathf.Max(0, kills);
            Data.lastRunKills = kills;
            Data.lastRunLevel = level;
            Data.lastRunSeconds = seconds;
            Increment("run_end");
            Save();
        }

        public static void TrackAdView(string placement)
        {
            Data.adViews++;
            Increment("ad_" + placement);
            Save();
        }

        public static void TrackRevive()
        {
            Data.revives++;
            Increment("revive");
            Save();
        }

        public static void TrackUpgrade(string upgradeTitle)
        {
            Data.upgradeChoices++;
            Increment("upgrade_" + upgradeTitle);
            Save();
        }

        public static void Track(string key)
        {
            Increment(key);
        }

        public static void ResetAll()
        {
            data = new AnalyticsData();
            Save();
        }

        private static void Increment(string key)
        {
            AnalyticsCounter counter = Data.counters.Find(c => c.key == key);
            if (counter == null)
            {
                counter = new AnalyticsCounter { key = key, value = 0 };
                Data.counters.Add(counter);
            }

            counter.value++;
        }

        private static AnalyticsData Load()
        {
            try
            {
                if (File.Exists(Path))
                {
                    AnalyticsData loaded = JsonUtility.FromJson<AnalyticsData>(File.ReadAllText(Path));
                    if (loaded != null)
                    {
                        if (loaded.counters == null) loaded.counters = new List<AnalyticsCounter>();
                        return loaded;
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Analytics] Load failed: " + exception.Message);
            }

            return new AnalyticsData();
        }

        private static void Save()
        {
            try
            {
                File.WriteAllText(Path, JsonUtility.ToJson(Data, true));
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Analytics] Save failed: " + exception.Message);
            }
        }
    }
}
