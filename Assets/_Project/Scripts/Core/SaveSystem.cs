using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DungeonSurvivors.Core
{
    /// <summary>
    /// 存档系统：JSON 文件放在 Application.persistentDataPath，带版本号与迁移入口。
    /// 面试可讲点：
    /// 1) 为什么不用 PlayerPrefs 存复杂数据；
    /// 2) 版本迁移（加字段/改结构时旧存档怎么办）；
    /// 3) 损坏存档兜底（读失败就用默认值，不让游戏崩）。
    /// </summary>
    public static class SaveSystem
    {
        public const int CurrentVersion = 1;
        private const string FileName = "save.json";

        private static SaveData current;

        public static SaveData Current
        {
            get
            {
                if (current == null) current = Load();
                return current;
            }
        }

        public static string SavePath
        {
            get { return Path.Combine(Application.persistentDataPath, FileName); }
        }

        public static SaveData Load()
        {
            SaveData data = null;
            try
            {
                if (File.Exists(SavePath))
                {
                    string json = File.ReadAllText(SavePath);
                    data = JsonUtility.FromJson<SaveData>(json);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[SaveSystem] Load failed, use defaults: " + exception.Message);
            }

            if (data == null)
            {
                data = new SaveData();
            }

            if (data.version != CurrentVersion)
            {
                data = Migrate(data);
            }

            EnsureCollections(data);
            current = data;
            return data;
        }

        public static void Save()
        {
            try
            {
                string json = JsonUtility.ToJson(Current, true);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[SaveSystem] Save failed: " + exception.Message);
            }
        }

        public static void DeleteSave()
        {
            try
            {
                if (File.Exists(SavePath)) File.Delete(SavePath);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[SaveSystem] Delete failed: " + exception.Message);
            }

            current = new SaveData();
        }

        private static SaveData Migrate(SaveData data)
        {
            Debug.Log("[SaveSystem] Migrate save version " + data.version + " -> " + CurrentVersion);
            // 以后版本升级时在这里逐版本补字段/转换数据
            data.version = CurrentVersion;
            return data;
        }

        private static void EnsureCollections(SaveData data)
        {
            if (data.settings == null) data.settings = new SettingsData();
            if (data.meta == null) data.meta = new MetaData();
            if (data.meta.talents == null) data.meta.talents = new List<TalentEntry>();
            if (data.meta.unlockedWeapons == null) data.meta.unlockedWeapons = new List<string>();
            if (data.meta.achievements == null) data.meta.achievements = new List<string>();
            if (data.meta.codex == null) data.meta.codex = new List<string>();
        }
    }
}
