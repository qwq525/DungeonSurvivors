using DungeonSurvivors.Core;
using UnityEngine;

namespace DungeonSurvivors.Systems
{
    /// <summary>
    /// 单局统计：击杀数、存活时间（结算面板用）。
    /// 面试可讲点：统计与 UI 解耦；订阅敌人死亡事件计数；场景重开时随场景重建自动重置。
    /// </summary>
    public class RunStats : MonoBehaviour
    {
        public static RunStats Instance { get; private set; }

        public int Kills { get; private set; }
        public float RunTime { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
            if (Instance == this) Instance = null;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
            RunTime += Time.deltaTime;
        }

        private void OnEnemyDied(EnemyDiedEvent evt)
        {
            Kills++;
            if (Kills == 1) AchievementService.Unlock("first_blood");
            if (Kills >= 50) AchievementService.Unlock("slayer_50");

            if (evt.IsBoss) CodexService.Discover("enemy:boss");
            else if (evt.IsElite) CodexService.Discover("enemy:elite");
            else CodexService.Discover("enemy:slime");
        }
    }
}
