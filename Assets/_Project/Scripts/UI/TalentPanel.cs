using DungeonSurvivors.Core;
using DungeonSurvivors.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonSurvivors.UI
{
    /// <summary>
    /// 天赋面板（W2）：花灵魂升级 4 个局外天赋；每级成本 = 基础成本 × 下一等级。
    /// 面试可讲点：数据驱动列表、按钮状态（钱不够/满级置灰）、存档即时保存。
    /// </summary>
    public class TalentPanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Text soulsText;
        [SerializeField] private Button[] talentButtons;
        [SerializeField] private Text[] talentLabels;
        [SerializeField] private Button backButton;

        public void Configure(GameObject panelRoot, Text souls, Button[] buttons, Text[] labels, Button back)
        {
            root = panelRoot;
            soulsText = souls;
            talentButtons = buttons;
            talentLabels = labels;
            backButton = back;
        }

        private void Awake()
        {
            if (root != null) root.SetActive(false);
            if (backButton != null) backButton.onClick.AddListener(Hide);

            for (int i = 0; i < talentButtons.Length; i++)
            {
                int index = i;
                if (talentButtons[i] != null)
                {
                    talentButtons[i].onClick.AddListener(delegate { Buy(index); });
                }
            }
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
            if (soulsText != null) soulsText.text = "灵魂 " + MetaProgression.Souls;

            for (int i = 0; i < talentButtons.Length; i++)
            {
                TalentDefinition talent = TalentCatalog.Talents[i];
                int level = MetaProgression.GetTalentLevel(talent.Id);
                bool maxed = level >= talent.MaxLevel;
                int cost = talent.BaseCost * (level + 1);

                if (talentLabels[i] != null)
                {
                    talentLabels[i].text = maxed
                        ? talent.DisplayName + "  等级 " + level + "/" + talent.MaxLevel + "  已满级"
                        : talent.DisplayName + "  等级 " + level + "/" + talent.MaxLevel + "  - " + cost + " 灵魂";
                }

                if (talentButtons[i] != null)
                {
                    talentButtons[i].interactable = !maxed && MetaProgression.Souls >= cost;
                }
            }
        }

        private void Buy(int index)
        {
            TalentDefinition talent = TalentCatalog.Talents[index];
            int level = MetaProgression.GetTalentLevel(talent.Id);
            if (level >= talent.MaxLevel) return;

            int cost = talent.BaseCost * (level + 1);
            if (!MetaProgression.TrySpendSouls(cost)) return;

            MetaProgression.SetTalentLevel(talent.Id, level + 1);
            AchievementService.Unlock("talent_1");
            Refresh();
        }
    }
}
