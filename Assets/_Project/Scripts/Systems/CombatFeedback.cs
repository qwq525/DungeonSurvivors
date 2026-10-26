using System.Collections;
using DungeonSurvivors.Core;
using DungeonSurvivors.UI;
using UnityEngine;

namespace DungeonSurvivors.Systems
{
    /// <summary>
    /// 打击感反馈（P0/P2）：命中/击杀震屏 + 顿帧 + 全屏闪 + 镜头 zoom punch；玩家受伤红闪。
    /// 面试可讲点：表现层订阅事件（逻辑不引用相机/UI）；顿帧与暂停/升级面板的 timeScale 冲突处理；
    /// 为什么闪屏/顿帧要用 unscaled 时间。
    /// </summary>
    public class CombatFeedback : MonoBehaviour
    {
        [SerializeField] private CameraFollow cameraFollow;
        [SerializeField] private ScreenFlash screenFlash;
        [SerializeField] private float damageHitstop = 0.03f;
        [SerializeField] private float killHitstop = 0.06f;

        private Coroutine hitstopRoutine;

        public void Configure(CameraFollow follow, ScreenFlash flash)
        {
            cameraFollow = follow;
            screenFlash = flash;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
            EventBus.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            EventBus.Subscribe<LevelUpEvent>(OnLevelUp);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
            EventBus.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            EventBus.Unsubscribe<LevelUpEvent>(OnLevelUp);
        }

        private void OnDamageDealt(DamageDealtEvent evt)
        {
            // 普通命中只保留敌人自身闪白 + 击退（EnemyBase 内处理），不再全屏闪/震屏/顿帧。
        }

        private void OnEnemyDied(EnemyDiedEvent evt)
        {
            if (cameraFollow != null)
            {
                cameraFollow.Shake(0.12f, 0.12f);
                cameraFollow.PunchZoom(0.08f, 0.1f);
            }

            if (screenFlash != null) screenFlash.Flash(new Color(1f, 0.98f, 0.9f), 0.08f, 0.1f);
            StartHitstop(killHitstop);
        }

        private void OnPlayerDamaged(PlayerDamagedEvent evt)
        {
            if (cameraFollow != null)
            {
                cameraFollow.Shake(0.10f, 0.16f);
                cameraFollow.PunchZoom(0.08f, 0.14f);
            }

            if (screenFlash != null) screenFlash.Flash(new Color(1f, 0.15f, 0.15f), 0.28f, 0.28f);
        }

        private void OnLevelUp(LevelUpEvent evt)
        {
            if (screenFlash != null) screenFlash.Flash(new Color(1f, 0.96f, 0.88f), 0.12f, 0.2f);
        }

        private void StartHitstop(float duration)
        {
            if (Time.timeScale < 0.5f) return; // 升级面板/暂停时不抢 timeScale
            if (hitstopRoutine != null) StopCoroutine(hitstopRoutine);
            hitstopRoutine = StartCoroutine(Hitstop(duration));
        }

        private IEnumerator Hitstop(float duration)
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duration);

            if (GameManager.Instance == null || GameManager.Instance.State == GameState.Playing)
            {
                Time.timeScale = 1f;
            }

            hitstopRoutine = null;
        }
    }
}
