using DungeonSurvivors.Core;
using UnityEngine;

namespace DungeonSurvivors.Gameplay.Progression
{
    /// <summary>
    /// 宝石生成器：订阅"敌人死亡"事件，在死亡位置生成经验宝石（对象池）。
    /// 面试可讲点：事件解耦——敌人不需要知道宝石/玩家，死亡只发事件；生成器负责表现层。
    /// </summary>
    public class GemSpawner : MonoBehaviour
    {
        [SerializeField] private ExperienceGem gemPrefab;
        [SerializeField] private Transform poolRoot;

        private ObjectPool<ExperienceGem> pool;

        public void Configure(ExperienceGem prefab, Transform poolRootTransform)
        {
            gemPrefab = prefab;
            poolRoot = poolRootTransform;
        }

        private void Awake()
        {
            if (gemPrefab != null)
            {
                pool = new ObjectPool<ExperienceGem>(CreateGem, poolRoot, 16, 128);
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
        }

        private ExperienceGem CreateGem()
        {
            ExperienceGem gem = Instantiate(gemPrefab, poolRoot);
            gem.Initialize(1, ReleaseGem);
            return gem;
        }

        private void OnEnemyDied(EnemyDiedEvent evt)
        {
            if (pool == null) return;

            ExperienceGem gem = pool.Get();
            gem.transform.position = evt.Position;
            gem.Initialize(evt.XpValue, ReleaseGem);
        }

        private void ReleaseGem(ExperienceGem gem)
        {
            pool.Release(gem);
        }
    }
}
