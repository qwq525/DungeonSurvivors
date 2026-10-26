using System.Text;
using DungeonSurvivors.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonSurvivors.UI
{
    /// <summary>成就面板：主菜单打开，显示 6 个成就的完成状态。</summary>
    public class AchievementPanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Text content;
        [SerializeField] private Button backButton;

        public void Configure(GameObject panelRoot, Text contentText, Button back)
        {
            root = panelRoot;
            content = contentText;
            backButton = back;
        }

        private void Awake()
        {
            if (root != null) root.SetActive(false);
            if (backButton != null) backButton.onClick.AddListener(Hide);
        }

        public void Show()
        {
            Refresh();
            if (root != null) root.SetActive(true);
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }

        private void Refresh()
        {
            if (content == null) return;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("成就  " + AchievementService.UnlockedCount + " / " + AchievementService.TotalCount);
            builder.AppendLine();

            for (int i = 0; i < AchievementCatalog.Achievements.Count; i++)
            {
                AchievementDefinition achievement = AchievementCatalog.Achievements[i];
                bool unlocked = AchievementService.Has(achievement.Id);
                builder.AppendLine((unlocked ? "[X] " : "[ ] ") + achievement.Title);
                builder.AppendLine("      " + achievement.Description);
            }

            content.text = builder.ToString();
        }
    }
}
