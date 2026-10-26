using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonSurvivors.Core
{
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver
    }

    /// <summary>
    /// 全局游戏入口：维护游戏流程状态机（Menu/Playing/Paused/GameOver），负责暂停与重开。
    /// 面试可讲点：
    /// 1) 单例的生命周期与重复实例处理；
    /// 2) 暂停为什么用 Time.timeScale，以及哪些系统要用 unscaledTime；
    /// 3) 状态切换用事件通知 UI，避免各处直接引用。
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; } = GameState.Menu;
        public event Action<GameState> StateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // QA/录制辅助：带 -autoStart 启动时跳过主菜单直接开局
            if (HasCommandLineFlag("-autoStart"))
            {
                StartRun();
            }
        }

        private static bool HasCommandLineFlag(string flag)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], flag, StringComparison.OrdinalIgnoreCase)) return true;
            }

            return false;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            Time.timeScale = 1f;
        }

        public void StartRun()
        {
            DungeonSurvivors.Analytics.AnalyticsService.TrackRunStart();
            DungeonSurvivors.Systems.CodexService.Discover("weapon:missile");
            SetState(GameState.Playing);
            Time.timeScale = 1f;
        }

        public void PauseGame(bool paused)
        {
            if (State != GameState.Playing && State != GameState.Paused) return;

            SetState(paused ? GameState.Paused : GameState.Playing);
            Time.timeScale = paused ? 0f : 1f;
        }

        public void EndRun()
        {
            SetState(GameState.GameOver);
            Time.timeScale = 0f;
        }

        /// <summary>复活/继续：从 GameOver 或暂停状态恢复到 Playing。</summary>
        public void ResumeRun()
        {
            SetState(GameState.Playing);
            Time.timeScale = 1f;
        }

        public void BackToMenu()
        {
            SetState(GameState.Menu);
            Time.timeScale = 1f;
        }

        /// <summary>重开一局：清空事件总线（避免残留订阅）-> 恢复时间 -> 重载当前场景。</summary>
        public void RestartRun()
        {
            EventBus.Clear();
            DungeonSurvivors.Systems.DailyChallengeService.Reset();
            Time.timeScale = 1f;
            SetState(GameState.Playing);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>回到主菜单：清空事件 + 状态设为 Menu + 重载场景（菜单控制器会显示主菜单）。</summary>
        public void GoToMenu()
        {
            EventBus.Clear();
            DungeonSurvivors.Systems.DailyChallengeService.Reset();
            Time.timeScale = 1f;
            SetState(GameState.Menu);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void SetState(GameState next)
        {
            if (State == next) return;
            State = next;
            StateChanged?.Invoke(next);
        }
    }
}
