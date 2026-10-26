using DungeonSurvivors.Core;
using DungeonSurvivors.Gameplay.Player;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonSurvivors.UI
{
    /// <summary>右下角技能图标 + 圆形冷却（初始技能：空格闪避）。</summary>
    public class SkillCooldownUI : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Image cooldownFill;
        [SerializeField] private Text keyLabel;
        [SerializeField] private DashAbility dash;

        public void Configure(GameObject panelRoot, Image fillImage, Text keyText)
        {
            root = panelRoot;
            cooldownFill = fillImage;
            keyLabel = keyText;
        }

        private void Update()
        {
            if (root == null) return;

            bool inMenu = GameManager.Instance != null && GameManager.Instance.State == GameState.Menu;
            if (root.activeSelf == inMenu) root.SetActive(!inMenu);
            if (inMenu) return;

            if (dash == null)
            {
                PlayerController player = PlayerController.Instance;
                if (player != null) dash = player.GetComponent<DashAbility>();
            }

            if (dash == null) return;

            float cooldown = dash.CooldownNormalized; // 1 = 刚使用，0 = 可用
            if (cooldownFill != null)
            {
                cooldownFill.fillAmount = cooldown;
            }

            if (keyLabel != null)
            {
                keyLabel.text = cooldown <= 0.001f ? "SPACE" : Mathf.CeilToInt(cooldown * 1.2f) + "s";
                keyLabel.color = cooldown <= 0.001f ? new Color(0.12f, 0.1f, 0.16f) : new Color(1f, 0.95f, 0.8f);
            }
        }
    }
}
