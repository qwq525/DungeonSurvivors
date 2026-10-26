using DungeonSurvivors.Core;
using UnityEngine;

namespace DungeonSurvivors.Gameplay.Player
{
    /// <summary>
    /// 玩家移动：8 方向移动 + 斜向速度归一化 + 闪避期间交给 DashAbility 控制。
    /// 面试可讲点：
    /// 1) 为什么移动放 FixedUpdate（物理稳定性）、输入采样放 Update（避免漏帧）；
    /// 2) 斜向归一化防止速度变快；
    /// 3) 输入层用接口抽象，方便后面接手机虚拟摇杆。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(DashAbility))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [Header("移动")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private SpriteRenderer bodyRenderer;

        private Rigidbody2D body;
        private DashAbility dashAbility;
        private IInputSource input;
        private Vector2 moveInput;
        private Vector2 lastMoveDirection = Vector2.right;

        public Vector2 MoveInput
        {
            get { return moveInput; }
        }

        public void AddMoveSpeed(float percent)
        {
            moveSpeed *= 1f + percent;
        }

        private void Awake()
        {
            Instance = this;
            body = GetComponent<Rigidbody2D>();
            dashAbility = GetComponent<DashAbility>();
            if (bodyRenderer == null) bodyRenderer = GetComponentInChildren<SpriteRenderer>();
            input = new KeyboardInputSource();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
            {
                moveInput = Vector2.zero;
                return;
            }

            moveInput = input.Move;
            moveInput = Vector2.ClampMagnitude(moveInput, 1f);

            if (moveInput.sqrMagnitude > 0.01f)
            {
                lastMoveDirection = moveInput.normalized;
            }

            if (input.DashPressed)
            {
                Vector2 direction = moveInput.sqrMagnitude > 0.01f ? moveInput : lastMoveDirection;
                dashAbility.TryDash(direction);
            }

            if (bodyRenderer != null && Mathf.Abs(moveInput.x) > 0.01f)
            {
                bodyRenderer.flipX = moveInput.x < 0f;
            }
        }

        private void FixedUpdate()
        {
            if (dashAbility.IsDashing)
            {
                return; // 闪避期间由 DashAbility 接管速度
            }

            body.velocity = moveInput * moveSpeed;
        }
    }
}
