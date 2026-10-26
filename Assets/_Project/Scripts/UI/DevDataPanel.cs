using System.Text;
using DungeonSurvivors.Analytics;
using DungeonSurvivors.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonSurvivors.UI
{
    /// <summary>
    /// 开发者数据面板（F1 打开）：显示本地埋点汇总，面试可现场演示"我用数据调难度"。
    /// 面试可讲点：埋点字段选择（局数/击杀/广告/升级选择）、为什么开发期用 F1 面板而不是只看日志。
    /// </summary>
    public class DevDataPanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Text text;

        public void Configure(GameObject panelRoot, Text content)
        {
            root = panelRoot;
            text = content;
        }

        private void Awake()
        {
            if (root != null) root.SetActive(false);
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.F1)) return;

            bool show = root != null && !root.activeSelf;
            if (root != null) root.SetActive(show);
            if (show) Refresh();
        }

        private void Refresh()
        {
            if (text == null) return;

            AnalyticsData data = AnalyticsService.Data;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("DEV DATA (F1 to close)");
            builder.AppendLine();
            builder.AppendLine("Runs:            " + data.totalRuns);
            builder.AppendLine("Total kills:     " + data.totalKills);
            builder.AppendLine("Last run:        " + data.lastRunKills + " kills / Lv." + data.lastRunLevel + " / " + data.lastRunSeconds + "s");
            builder.AppendLine("Ad views:        " + data.adViews);
            builder.AppendLine("Revives:         " + data.revives);
            builder.AppendLine("Upgrades chosen: " + data.upgradeChoices);
            builder.AppendLine();
            builder.AppendLine("Best kills:      " + MetaProgression.BestKills);
            builder.AppendLine("Best survival:   " + MetaProgression.BestSurvivalSeconds + "s");
            builder.AppendLine("Souls:           " + MetaProgression.Souls);
            text.text = builder.ToString();
        }
    }
}
