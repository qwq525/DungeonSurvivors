using DungeonSurvivors.Core;
using DungeonSurvivors.Gameplay.Player;
using UnityEngine;

namespace DungeonSurvivors.Gameplay.Enemies
{
    /// <summary>敌人远程投射物：直线飞行，接近玩家时造成伤害并消失。</summary>
    public class EnemyProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 7f;
        [SerializeField] private float lifeTime = 3f;

        private Vector2 direction;
        private int damage;

        public void Launch(Vector2 dir, int damageAmount)
        {
            direction = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;
            damage = Mathf.Max(1, damageAmount);
            lifeTime = 4f;
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

            transform.position += (Vector3)(direction * (speed * Time.deltaTime));
            lifeTime -= Time.deltaTime;
            if (lifeTime <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            PlayerController player = PlayerController.Instance;
            if (player == null) return;

            if (Vector2.Distance(transform.position, player.transform.position) < 0.55f)
            {
                EventBus.Publish(new PlayerDamagedEvent(damage));
                Destroy(gameObject);
            }
        }
    }
}
