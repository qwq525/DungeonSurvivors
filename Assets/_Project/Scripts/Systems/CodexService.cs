using DungeonSurvivors.Core;
using UnityEngine;

namespace DungeonSurvivors.Systems
{
    /// <summary>图鉴收集服务：发现即写入存档（即时保存）。</summary>
    public static class CodexService
    {
        public static bool Has(string id)
        {
            return SaveSystem.Current.meta.codex.Contains(id);
        }

        public static void Discover(string id)
        {
            if (string.IsNullOrEmpty(id) || Has(id)) return;

            SaveSystem.Current.meta.codex.Add(id);
            SaveSystem.Save();
            Debug.Log("[Codex] Discovered: " + id);
        }

        public static int DiscoveredCount
        {
            get { return SaveSystem.Current.meta.codex.Count; }
        }

        public static int TotalCount
        {
            get { return CodexCatalog.Entries.Count; }
        }
    }
}
