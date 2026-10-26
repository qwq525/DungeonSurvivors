using DungeonSurvivors.Core;
using UnityEngine;

namespace DungeonSurvivors.Systems
{
    /// <summary>
    /// 音效服务（W3）：订阅游戏事件播放 Kenney 音效；无素材时自动静音（仓库占位版可运行）。
    /// 面试可讲点：表现层通过事件解耦；素材缺失的降级处理；随机 pitch 避免重复感。
    /// </summary>
    public class AudioService : MonoBehaviour
    {
        [SerializeField] private AudioSource source;
        [SerializeField] private AudioClip shootClip;
        [SerializeField] private AudioClip hitClip;
        [SerializeField] private AudioClip deathClip;
        [SerializeField] private AudioClip hurtClip;
        [SerializeField] private AudioClip levelUpClip;
        [SerializeField] private AudioClip pickupClip;

        private void Awake()
        {
            if (source == null) source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = 0.9f;
        }

        public void Configure(AudioClip shoot, AudioClip hit, AudioClip death, AudioClip hurt, AudioClip levelUp, AudioClip pickup)
        {
            shootClip = shoot;
            hitClip = hit;
            deathClip = death;
            hurtClip = hurt;
            levelUpClip = levelUp;
            pickupClip = pickup;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<WeaponFiredEvent>(OnWeaponFired);
            EventBus.Subscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
            EventBus.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            EventBus.Subscribe<LevelUpEvent>(OnLevelUp);
            EventBus.Subscribe<ExperiencePickupEvent>(OnPickup);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<WeaponFiredEvent>(OnWeaponFired);
            EventBus.Unsubscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
            EventBus.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            EventBus.Unsubscribe<LevelUpEvent>(OnLevelUp);
            EventBus.Unsubscribe<ExperiencePickupEvent>(OnPickup);
        }

        private void OnWeaponFired(WeaponFiredEvent evt) { Play(shootClip, 0.35f); }
        private void OnDamageDealt(DamageDealtEvent evt) { Play(hitClip, 0.45f); }
        private void OnEnemyDied(EnemyDiedEvent evt) { Play(deathClip, 0.5f); }
        private void OnPlayerDamaged(PlayerDamagedEvent evt) { Play(hurtClip, 0.8f); }
        private void OnLevelUp(LevelUpEvent evt) { Play(levelUpClip, 0.7f); }
        private void OnPickup(ExperiencePickupEvent evt) { Play(pickupClip, 0.3f); }

        private void Play(AudioClip clip, float volume)
        {
            if (clip == null || source == null) return;
            source.pitch = 1f + Random.Range(-0.06f, 0.06f);
            source.PlayOneShot(clip, volume);
        }
    }
}
