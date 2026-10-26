using DungeonSurvivors.Analytics;
using DungeonSurvivors.Commerce;
using DungeonSurvivors.Core;
using DungeonSurvivors.Gameplay.Enemies;
using DungeonSurvivors.Gameplay.Player;
using DungeonSurvivors.Gameplay.Progression;
using DungeonSurvivors.UI;
using UnityEngine;

namespace DungeonSurvivors.Systems
{
    /// <summary>
    /// 局流程控制（W2）：玩家死亡 -> 结算（发灵魂/记最佳）-> 可看广告复活/重开/退出；Esc 暂停；
    /// 击败第 10 波 Boss -> VICTORY 结算。
    /// 面试可讲点：事件驱动流程、广告 Mock 接口、Time.timeScale 暂停、场景重载时 EventBus.Clear。
    /// </summary>
    public class GameFlowController : MonoBehaviour
    {
        [SerializeField] private ResultPanel resultPanel;
        [SerializeField] private PausePanel pausePanel;
        [SerializeField] private MockAdService adService;
        [SerializeField] private EnemySpawner spawner;

        private bool runEnded;
        private bool revivedThisRun;
        private bool victory;
        private string lastStats;
        private string lastTitle;
        private int lastSouls;

        public void Configure(ResultPanel result, PausePanel pause, MockAdService ads)
        {
            resultPanel = result;
            pausePanel = pause;
            adService = ads;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
        }

        private void Update()
        {
            if (runEnded) return;
            if (Time.timeScale <= 0.5f) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            if (runEnded) return;
            runEnded = true;
            victory = false;

            if (GameManager.Instance != null) GameManager.Instance.EndRun();

            AchievementService.Unlock("first_death");

            EndRunAndShowResult("失败");
        }

        /// <summary>击败第 10 波 Boss 即胜利（一局目标）。</summary>
        private void OnEnemyDied(EnemyDiedEvent evt)
        {
            if (runEnded || !evt.IsBoss) return;

            if (spawner == null) spawner = FindFirstObjectByType<EnemySpawner>();
            if (spawner == null || spawner.CurrentWave < 10) return;

            runEnded = true;
            victory = true;

            if (GameManager.Instance != null) GameManager.Instance.EndRun();

            EndRunAndShowResult("胜利");
        }

        private void EndRunAndShowResult(string title)
        {
            int kills = RunStats.Instance != null ? RunStats.Instance.Kills : 0;
            float time = RunStats.Instance != null ? RunStats.Instance.RunTime : 0f;
            int level = GetPlayerLevel();
            float soulsMultiplier = DailyChallengeService.IsActive ? DailyChallengeService.SoulsMultiplier : 1f;
            if (victory) soulsMultiplier *= 2f;

            int soulsEarned = Mathf.RoundToInt((kills * 2 + level * 5) * soulsMultiplier);

            MetaProgression.RegisterRun(soulsEarned, kills, time);
            AnalyticsService.TrackRunEnd(kills, level, Mathf.FloorToInt(time));
            lastSouls = soulsEarned;
            lastStats = BuildStats(kills, time, level);
            lastTitle = title;

            ShowResult();
            Debug.Log("[GameFlow] Run ended: " + title + " | kills=" + kills + " souls=" + soulsEarned);
        }

        private void ShowResult()
        {
            if (resultPanel == null) return;
            resultPanel.Show(lastTitle, lastStats, lastSouls, !victory && !revivedThisRun,
                RetryRun, TryRevive, GoToMenu, QuitGame);
        }

        private void TryRevive()
        {
            if (adService == null)
            {
                DoRevive();
                return;
            }

            if (resultPanel != null) resultPanel.Hide();
            adService.ShowRewardedAd("revive", OnAdCompleted);
        }

        private void OnAdCompleted(bool success)
        {
            if (success)
            {
                DoRevive();
            }
            else
            {
                ShowResult();
            }
        }

        private void DoRevive()
        {
            PlayerController player = PlayerController.Instance;
            if (player == null) return;

            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health == null) return;

            revivedThisRun = true;
            runEnded = false;
            AnalyticsService.TrackRevive();
            AchievementService.Unlock("revive_once");
            health.Revive(health.MaxHealth / 2);

            if (GameManager.Instance != null) GameManager.Instance.ResumeRun();
            Debug.Log("[GameFlow] Player revived by rewarded ad.");
        }

        private int GetPlayerLevel()
        {
            PlayerController player = PlayerController.Instance;
            if (player == null) return 1;

            PlayerExperience experience = player.GetComponent<PlayerExperience>();
            return experience != null ? experience.Level : 1;
        }

        private string BuildStats(int kills, float time, int level)
        {
            int totalSeconds = Mathf.FloorToInt(time);
            string timeText = (totalSeconds / 60).ToString("00") + ":" + (totalSeconds % 60).ToString("00");
            return "时间 " + timeText + "    击杀 " + kills + "    等级 " + level;
        }

        private void TogglePause()
        {
            if (GameManager.Instance == null) return;

            if (GameManager.Instance.State == GameState.Paused)
            {
                if (pausePanel != null) pausePanel.Hide();
                GameManager.Instance.PauseGame(false);
            }
            else
            {
                if (pausePanel != null) pausePanel.Show(ResumeRun, RetryRun, GoToMenu);
                GameManager.Instance.PauseGame(true);
            }
        }

        private void ResumeRun()
        {
            if (pausePanel != null) pausePanel.Hide();
            if (GameManager.Instance != null) GameManager.Instance.PauseGame(false);
        }

        private void RetryRun()
        {
            if (GameManager.Instance != null) GameManager.Instance.RestartRun();
        }

        private void GoToMenu()
        {
            if (GameManager.Instance != null) GameManager.Instance.GoToMenu();
        }

        private void QuitGame()
        {
            Application.Quit();
        }
    }
}
