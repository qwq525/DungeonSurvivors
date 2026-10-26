using System;
using DungeonSurvivors.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonSurvivors.UI
{
    /// <summary>
    /// 主菜单（W2）：开始 / 天赋 / 设置 / 退出；只在 GameState.Menu 时显示。
    /// 面试可讲点：菜单与游戏状态机的关系；重开场景时用状态判断是否显示菜单。
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Button startButton;
        [SerializeField] private Button dailyButton;
        [SerializeField] private Button codexButton;
        [SerializeField] private Button talentsButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button achievementsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private TalentPanel talentPanel;
        [SerializeField] private SettingsPanel settingsPanel;
        [SerializeField] private AchievementPanel achievementPanel;
        [SerializeField] private CodexPanel codexPanel;
        [SerializeField] private TutorialPanel tutorialPanel;

        public void Configure(GameObject panelRoot, Button start, Button daily, Button codex, Button talents, Button settings, Button achievements,
            Button quit, TalentPanel talentsPanel, SettingsPanel settingsPanelReference, AchievementPanel achievementsPanelReference,
            CodexPanel codexPanelReference, TutorialPanel tutorialPanelReference)
        {
            root = panelRoot;
            startButton = start;
            dailyButton = daily;
            codexButton = codex;
            talentsButton = talents;
            settingsButton = settings;
            achievementsButton = achievements;
            quitButton = quit;
            talentPanel = talentsPanel;
            settingsPanel = settingsPanelReference;
            achievementPanel = achievementsPanelReference;
            codexPanel = codexPanelReference;
            tutorialPanel = tutorialPanelReference;
        }

        private void Awake()
        {
            if (startButton != null) startButton.onClick.AddListener(delegate { DailyChallengeService.Reset(); OnStart(); });
            if (dailyButton != null) dailyButton.onClick.AddListener(OnStartDaily);
            if (codexButton != null) codexButton.onClick.AddListener(delegate { if (codexPanel != null) codexPanel.Show(); });
            if (talentsButton != null) talentsButton.onClick.AddListener(delegate { if (talentPanel != null) talentPanel.Show(); });
            if (settingsButton != null) settingsButton.onClick.AddListener(delegate { if (settingsPanel != null) settingsPanel.Show(); });
            if (achievementsButton != null) achievementsButton.onClick.AddListener(delegate { if (achievementPanel != null) achievementPanel.Show(); });
            if (quitButton != null) quitButton.onClick.AddListener(delegate { Application.Quit(); });
        }

        private void OnStartDaily()
        {
            DailyChallengeService.Begin();
            if (root != null) root.SetActive(false);
            if (DungeonSurvivors.Core.GameManager.Instance != null)
            {
                DungeonSurvivors.Core.GameManager.Instance.StartRun();
            }
        }

        private void Start()
        {
            bool shouldShow = DungeonSurvivors.Core.GameManager.Instance == null
                              || DungeonSurvivors.Core.GameManager.Instance.State == DungeonSurvivors.Core.GameState.Menu;
            if (root != null) root.SetActive(shouldShow);
        }

        private void OnStart()
        {
            if (TutorialService.ShouldShow && tutorialPanel != null)
            {
                tutorialPanel.Show(BeginRun);
                return;
            }

            BeginRun();
        }

        private void BeginRun()
        {
            TutorialService.MarkSeen();
            if (root != null) root.SetActive(false);
            if (DungeonSurvivors.Core.GameManager.Instance != null)
            {
                DungeonSurvivors.Core.GameManager.Instance.StartRun();
            }
        }
    }
}
