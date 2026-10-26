using DungeonSurvivors.Core;
using DungeonSurvivors.Gameplay.Enemies;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonSurvivors.UI
{
    /// <summary>Boss 血条：仅在有存活 Boss 时显示，显示名字/当前血量并随受伤缩短。</summary>
    public class BossHealthBar : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Image fill;
        [SerializeField] private Text label;

        public void Configure(GameObject panelRoot, Image fillImage, Text labelText)
        {
            root = panelRoot;
            fill = fillImage;
            label = labelText;
            if (root != null) root.SetActive(false);
        }

        private void Update()
        {
            if (root == null) return;

            bool inMenu = GameManager.Instance != null && GameManager.Instance.State == GameState.Menu;
            EnemyBase boss = null;
            if (!inMenu)
            {
                for (int i = 0; i < EnemyBase.ActiveEnemies.Count; i++)
                {
                    EnemyBase enemy = EnemyBase.ActiveEnemies[i];
                    if (enemy != null && enemy.IsAlive && enemy.IsBoss)
                    {
                        boss = enemy;
                        break;
                    }
                }
            }

            bool show = boss != null;
            if (root.activeSelf != show) root.SetActive(show);
            if (!show) return;

            if (fill != null)
            {
                Vector2 anchorMax = fill.rectTransform.anchorMax;
                anchorMax.x = Mathf.Clamp01((float)boss.CurrentHealth / Mathf.Max(1, boss.MaxHealth));
                fill.rectTransform.anchorMax = anchorMax;
            }

            if (label != null) label.text = "勇者队长  " + boss.CurrentHealth + " / " + boss.MaxHealth;
        }
    }
}
