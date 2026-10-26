using System.Collections;
using System.Collections.Generic;
using DungeonSurvivors.Core;
using DungeonSurvivors.Gameplay.Player;
using DungeonSurvivors.Gameplay.Weapons;
using DungeonSurvivors.UI;
using UnityEngine;

namespace DungeonSurvivors.Gameplay.Progression
{
    /// <summary>
    /// 升级/变异系统（W1-3 MVP）：
    /// 1) 订阅 LevelUpEvent -> 暂停游戏 -> 弹三选一面板 -> 应用效果 -> 恢复；
    /// 2) 选项从候选池随机抽 3 个且不重复（后续加权重/保底/武器升级）；
    /// 3) 效果直接调用玩家/武器组件的公开方法，方便后面换成数据驱动。
    /// 面试可讲点：为什么升级时暂停、随机池去重、效果应用与数值成长曲线。
    /// </summary>
    public class UpgradeSystem : MonoBehaviour
    {
        [SerializeField] private UpgradePanel panel;
        [SerializeField] private MissileWeapon weapon;
        [SerializeField] private OrbitWeapon orbitWeapon;
        [SerializeField] private PlayerHealth playerHealth;

        private PlayerController controller;
        private bool choosing;
        private bool autoUpgrade;
        private List<UpgradeOption> pendingOptions;

        public void Configure(UpgradePanel upgradePanel, MissileWeapon missileWeapon, PlayerHealth health, OrbitWeapon orbit)
        {
            panel = upgradePanel;
            weapon = missileWeapon;
            playerHealth = health;
            orbitWeapon = orbit;
        }

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            if (weapon == null) weapon = GetComponent<MissileWeapon>();
            if (orbitWeapon == null) orbitWeapon = GetComponent<OrbitWeapon>();
            if (playerHealth == null) playerHealth = GetComponent<PlayerHealth>();
            autoUpgrade = HasCommandLineFlag("-autoUpgrade");
        }

        private static bool HasCommandLineFlag(string flag)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], flag, System.StringComparison.OrdinalIgnoreCase)) return true;
            }

            return false;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<LevelUpEvent>(OnLevelUp);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<LevelUpEvent>(OnLevelUp);
        }

        private void OnLevelUp(LevelUpEvent evt)
        {
            if (choosing) return;
            if (panel == null)
            {
                Debug.LogWarning("[UpgradeSystem] UpgradePanel not assigned.");
                return;
            }

            choosing = true;
            Time.timeScale = 0f;
            pendingOptions = BuildOptions();
            panel.Show(pendingOptions, OnOptionChosen);

            if (autoUpgrade) StartCoroutine(AutoPickFirstOption());
        }

        private IEnumerator AutoPickFirstOption()
        {
            yield return new WaitForSecondsRealtime(0.6f);
            if (choosing && pendingOptions != null && pendingOptions.Count > 0)
            {
                OnOptionChosen(pendingOptions[0]);
            }
        }

        private void OnOptionChosen(UpgradeOption option)
        {
            Apply(option);
            panel.Hide();
            Time.timeScale = 1f;
            choosing = false;
        }

        private List<UpgradeOption> BuildOptions()
        {
            bool orbitUnlocked = orbitWeapon != null && orbitWeapon.IsActive;
            List<UpgradeOption> pool = new List<UpgradeOption>
            {
                new UpgradeOption("利爪", "伤害 +2", UpgradeType.Damage, 2f),
                new UpgradeOption("獠牙", "攻击速度 +10%", UpgradeType.AttackSpeed, 0.10f),
                new UpgradeOption("快腿", "移动速度 +8%", UpgradeType.MoveSpeed, 0.08f),
                new UpgradeOption("厚皮", "最大生命 +20（并治疗）", UpgradeType.MaxHealth, 20f),
                orbitUnlocked
                    ? new UpgradeOption("飞刀强化", "环绕飞刀伤害 +1", UpgradeType.OrbitBlades, 1f)
                    : new UpgradeOption("环绕飞刀", "解锁环绕飞刀", UpgradeType.OrbitBlades, 1f)
            };

            List<UpgradeOption> result = new List<UpgradeOption>();
            int count = Mathf.Min(3, pool.Count);
            for (int i = 0; i < count; i++)
            {
                int index = Random.Range(0, pool.Count);
                result.Add(pool[index]);
                pool.RemoveAt(index);
            }

            return result;
        }

        private void Apply(UpgradeOption option)
        {
            switch (option.Type)
            {
                case UpgradeType.Damage:
                    if (weapon != null) weapon.AddDamage((int)option.Value);
                    break;
                case UpgradeType.AttackSpeed:
                    if (weapon != null) weapon.AddAttackSpeed(option.Value);
                    break;
                case UpgradeType.MoveSpeed:
                    if (controller != null) controller.AddMoveSpeed(option.Value);
                    break;
                case UpgradeType.MaxHealth:
                    if (playerHealth != null) playerHealth.AddMaxHealth((int)option.Value, true);
                    break;
                case UpgradeType.OrbitBlades:
                    if (orbitWeapon != null)
                    {
                        if (!orbitWeapon.IsActive) orbitWeapon.Activate();
                        else orbitWeapon.AddDamage((int)option.Value);
                    }
                    break;
            }

            Debug.Log("[Upgrade] " + option.Title + " applied.");
            DungeonSurvivors.Analytics.AnalyticsService.TrackUpgrade(option.Title);
            DungeonSurvivors.Systems.CodexService.Discover("upgrade:" + option.Title);
        }
    }
}
