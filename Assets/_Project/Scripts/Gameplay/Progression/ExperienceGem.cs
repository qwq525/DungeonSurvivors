using System;
using DungeonSurvivors.Core;
using DungeonSurvivors.Gameplay.Player;
using UnityEngine;

namespace DungeonSurvivors.Gameplay.Progression
{
    /// <summary>
    /// 经验宝石（勇者死亡掉落）：靠近后磁吸飞向玩家，接触后发经验事件并回收。
    /// 面试可讲点：磁吸的"半径内加速飞向玩家"实现、池化回收、事件解耦（玩家不直接引用宝石）。
    /// </summary>
    public class ExperienceGem : MonoBehaviour, IPoolable
    {
        [SerializeField] private int xpValue = 1;
        [SerializeField] private float pickupRadius = 2.2f;
        [SerializeField] private float magnetSpeed = 8f;
        [SerializeField] private float collectRadius = 0.35f;

        private Action<ExperienceGem> releaseCallback;

        public void Initialize(int value, Action<ExperienceGem> release)
        {
            xpValue = Mathf.Max(1, value);
            releaseCallback = release;
        }

        public void OnSpawn()
        {
        }

        public void OnDespawn()
        {
        }

        private void Update()
        {
            PlayerController player = PlayerController.Instance;
            if (player == null) return;

            Vector2 toPlayer = (Vector2)player.transform.position - (Vector2)transform.position;
            float distance = toPlayer.magnitude;

            if (distance <= pickupRadius)
            {
                transform.position += (Vector3)(toPlayer.normalized * (magnetSpeed * Time.deltaTime));
            }

            if (distance <= collectRadius)
            {
                EventBus.Publish(new ExperiencePickupEvent(xpValue));
                if (releaseCallback != null) releaseCallback(this);
            }
        }
    }
}
