using System.Text;
using DungeonSurvivors.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonSurvivors.UI
{
    /// <summary>图鉴面板：按 WEAPONS / ENEMIES / MUTATIONS 分类展示发现进度。</summary>
    public class CodexPanel : MonoBehaviour
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
            builder.AppendLine("图鉴  " + CodexService.DiscoveredCount + " / " + CodexService.TotalCount);
            builder.AppendLine();

            string lastCategory = string.Empty;
            for (int i = 0; i < CodexCatalog.Entries.Count; i++)
            {
                CodexEntry entry = CodexCatalog.Entries[i];
                if (entry.Category != lastCategory)
                {
                    lastCategory = entry.Category;
                    builder.AppendLine(lastCategory);
                }

                bool discovered = CodexService.Has(entry.Id);
                builder.AppendLine("  " + (discovered ? "[X] " + entry.Title : "[ ] ???"));
            }

            content.text = builder.ToString();
        }
    }
}
