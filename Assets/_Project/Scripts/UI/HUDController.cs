using DungeonSurvivors.Core;
using DungeonSurvivors.Gameplay.Enemies;
using DungeonSurvivors.Gameplay.Player;
using DungeonSurvivors.Gameplay.Progression;
using DungeonSurvivors.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonSurvivors.UI
{
    /// <summary>
    /// HUD（MVP）：血条、经验条、等级、计时、击杀数、波次。
    /// 面试可讲点：HUD 只做显示（不持有游戏逻辑）；通过玩家组件/统计组件读取数据；UI 更新用 fillAmount 而不是重建 UI。
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private Image healthFill;
        [SerializeField] private Image xpFill;
        [SerializeField] private Text levelText;
        [SerializeField] private Text timerText;
        [SerializeField] private Text killsText;
        [SerializeField] private Text waveText;
        [SerializeField] private GameObject hudGroup;
        [SerializeField] private Text healthText;
        [SerializeField] private Text xpText;

        private PlayerHealth playerHealth;
        private PlayerExperience playerExperience;
        private EnemySpawner enemySpawner;
        private float runTime;
        private bool bound;

        public void Configure(Image healthFillImage, Image xpFillImage, Text levelLabel, Text timerLabel,
            Text killsLabel, Text waveLabel, GameObject group, Text healthLabel, Text xpLabel)
        {
            healthFill = healthFillImage;
            xpFill = xpFillImage;
            levelText = levelLabel;
            timerText = timerLabel;
            killsText = killsLabel;
            waveText = waveLabel;
            hudGroup = group;
            healthText = healthLabel;
            xpText = xpLabel;
        }

        private void Update()
        {
            if (hudGroup != null)
            {
                bool inMenu = GameManager.Instance != null && GameManager.Instance.State == GameState.Menu;
                if (hudGroup.activeSelf == inMenu) hudGroup.SetActive(!inMenu);
            }

            if (!bound)
            {
                TryBindPlayer();
            }

            if (!bound) return;

            if (GameManager.Instance != null && GameManager.Instance.State == GameState.Playing)
            {
                runTime += Time.deltaTime;
            }

            if (healthFill != null) SetBarFill(healthFill.rectTransform, playerHealth.Normalized);
            if (healthText != null) healthText.text = Mathf.CeilToInt(playerHealth.CurrentHealth) + " / " + playerHealth.MaxHealth;
            if (xpFill != null) SetBarFill(xpFill.rectTransform, playerExperience.NormalizedProgress);
            if (xpText != null) xpText.text = "EXP " + playerExperience.CurrentXp + " / " + playerExperience.XpToNextLevel;
            if (levelText != null) levelText.text = "等级 " + playerExperience.Level;
            if (timerText != null) timerText.text = FormatTime(runTime);
            if (killsText != null) killsText.text = "击杀 " + (RunStats.Instance != null ? RunStats.Instance.Kills : 0);
            if (waveText != null) waveText.text = (DailyChallengeService.IsActive ? "每日 · 波次 " : "波次 ") + (enemySpawner != null ? enemySpawner.CurrentWave : 1);
        }

        private void TryBindPlayer()
        {
            PlayerController player = PlayerController.Instance;
            if (player == null) return;

            playerHealth = player.GetComponent<PlayerHealth>();
            playerExperience = player.GetComponent<PlayerExperience>();
            enemySpawner = Object.FindFirstObjectByType<EnemySpawner>();
            bound = playerHealth != null && playerExperience != null;
            if (bound) Debug.Log("[HUD] Bound player, maxHealth=" + playerHealth.MaxHealth);
        }

        private static void SetBarFill(RectTransform fill, float normalized)
        {
            if (fill == null) return;
            normalized = Mathf.Clamp01(normalized);
            Vector2 anchorMax = fill.anchorMax;
            anchorMax.x = normalized;
            fill.anchorMax = anchorMax;
        }

        private static string FormatTime(float seconds)
        {
            int totalSeconds = Mathf.FloorToInt(seconds);
            int minutes = totalSeconds / 60;
            int secs = totalSeconds % 60;
            return minutes.ToString("00") + ":" + secs.ToString("00");
        }
    }
}
