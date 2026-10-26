using System;
using System.Collections.Generic;
using DungeonSurvivors.Gameplay.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonSurvivors.UI
{
    /// <summary>
    /// 升级三选一面板（MVP）：由 UpgradeSystem 调用 Show/Hide；按钮点击回传选择。
    /// 面试可讲点：面板只负责显示与转发点击，不持有数值逻辑；Time.timeScale=0 时按钮仍可用。
    /// </summary>
    public class UpgradePanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Text titleText;
        [SerializeField] private Button[] optionButtons;
        [SerializeField] private Text[] optionTitles;
        [SerializeField] private Text[] optionDescriptions;

        private Action<UpgradeOption> onChosen;
        private List<UpgradeOption> currentOptions;

        public void Configure(GameObject panelRoot, Text title, Button[] buttons, Text[] titles, Text[] descriptions)
        {
            root = panelRoot;
            titleText = title;
            optionButtons = buttons;
            optionTitles = titles;
            optionDescriptions = descriptions;
        }

        private void Awake()
        {
            if (root != null) root.SetActive(false);
            if (optionButtons == null) return;

            for (int i = 0; i < optionButtons.Length; i++)
            {
                int index = i;
                if (optionButtons[i] != null)
                {
                    optionButtons[i].onClick.AddListener(delegate { Choose(index); });
                }
            }
        }

        public void Show(List<UpgradeOption> options, Action<UpgradeOption> callback)
        {
            currentOptions = options;
            onChosen = callback;
            if (titleText != null) titleText.text = "升级 · 选择变异";

            for (int i = 0; i < optionButtons.Length; i++)
            {
                bool hasOption = i < options.Count;
                if (optionButtons[i] != null) optionButtons[i].gameObject.SetActive(hasOption);
                if (!hasOption) continue;
                if (optionTitles[i] != null) optionTitles[i].text = options[i].Title;
                if (optionDescriptions[i] != null) optionDescriptions[i].text = options[i].Description;
            }

            if (root != null) root.SetActive(true);
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
            onChosen = null;
            currentOptions = null;
        }

        private void Choose(int index)
        {
            if (currentOptions == null || index < 0 || index >= currentOptions.Count) return;

            UpgradeOption chosen = currentOptions[index];
            Action<UpgradeOption> callback = onChosen;
            if (callback != null) callback(chosen);
        }
    }
}
