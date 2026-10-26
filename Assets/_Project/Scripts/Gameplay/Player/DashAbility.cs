using UnityEngine;

namespace DungeonSurvivors.Gameplay.Player
{
    /// <summary>
    /// 闪避：向输入方向短距离冲刺，期间获得无敌帧；有冷却。
    /// 面试可讲点：
    /// 1) 闪避手感三要素：时长、冷却、无敌帧（数值都在 Inspector 可调）；
    /// 2) 无敌帧用计时器而不是协程（避免对象禁用后协程悬挂）；
    /// 3) 表现层接入点：残影、音效、震屏、无敌闪烁。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class DashAbility : MonoBehaviour
    {
        [Header("闪避参数")]
        [SerializeField] private float dashSpeed = 16f;
        [SerializeField] private float dashDuration = 0.18f;
        [SerializeField] private float cooldown = 1.2f;
        [SerializeField] private float invulnerableDuration = 0.28f;

        [Header("表现占位")]
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private Color invulnerableColor = new Color(1f, 1f, 1f, 0.55f);

        private Rigidbody2D body;
        private float dashTimeLeft;
        private float cooldownLeft;
        private float invulnerableTimeLeft;
        private Vector2 dashDirection;
        private Color baseColor;
        private bool hasBaseColor;

        public bool IsDashing
        {
            get { return dashTimeLeft > 0f; }
        }

        public bool IsInvulnerable
        {
            get { return invulnerableTimeLeft > 0f; }
        }

        /// <summary>0 = 可用，1 = 刚进冷却。HUD 技能图标可用它做遮罩。</summary>
        public float CooldownNormalized
        {
            get { return cooldown <= 0f ? 0f : Mathf.Clamp01(cooldownLeft / cooldown); }
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            if (bodyRenderer == null) bodyRenderer = GetComponentInChildren<SpriteRenderer>();
            if (bodyRenderer != null)
            {
                baseColor = bodyRenderer.color;
                hasBaseColor = true;
            }
        }

        private void Update()
        {
            if (cooldownLeft > 0f)
            {
                cooldownLeft -= Time.deltaTime;
            }

            if (invulnerableTimeLeft > 0f)
            {
                invulnerableTimeLeft -= Time.deltaTime;
                if (invulnerableTimeLeft <= 0f) RestoreVisual();
            }
        }

        private void FixedUpdate()
        {
            if (dashTimeLeft <= 0f) return;
            dashTimeLeft -= Time.fixedDeltaTime;
            body.velocity = dashDirection * dashSpeed;
        }

        public bool TryDash(Vector2 direction)
        {
            if (cooldownLeft > 0f || dashTimeLeft > 0f)
            {
                return false;
            }

            if (direction.sqrMagnitude < 0.01f)
            {
                direction = Vector2.right;
            }

            dashDirection = direction.normalized;
            dashTimeLeft = dashDuration;
            cooldownLeft = cooldown;
            invulnerableTimeLeft = invulnerableDuration;
            ApplyInvulnerableVisual();
            return true;
        }

        /// <summary>受伤系统接入点：无敌帧内返回 false，表示这次伤害应被忽略。</summary>
        public bool TryTakeDamage()
        {
            return !IsInvulnerable;
        }

        private void ApplyInvulnerableVisual()
        {
            if (bodyRenderer != null) bodyRenderer.color = invulnerableColor;
        }

        private void RestoreVisual()
        {
            if (bodyRenderer != null && hasBaseColor) bodyRenderer.color = baseColor;
        }
    }
}
