using UnityEngine;

namespace DungeonSurvivors.Visual
{
    /// <summary>
    /// 四方向帧动画（精灵表切片）：根据本帧位移判断朝向/移动，播放 Idle/Run 帧。
    /// 用于玩家（史莱姆）与敌人（勇者）；零 Animator 依赖，Prefab 可复用精灵数组。
    /// </summary>
    public class DirectionalSpriteAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer target;
        [SerializeField] private Sprite[] downIdle;
        [SerializeField] private Sprite[] downRun;
        [SerializeField] private Sprite[] upIdle;
        [SerializeField] private Sprite[] upRun;
        [SerializeField] private Sprite[] sideIdle;
        [SerializeField] private Sprite[] sideRun;
        [SerializeField] private float framesPerSecond = 10f;
        [SerializeField] private float moveThreshold = 0.05f;
        [SerializeField] private bool sideRowFacesRight;

        private Sprite[] activeFrames;
        private int frameIndex;
        private float frameTimer;
        private Vector3 lastPosition;

        public void Configure(SpriteRenderer renderer, Sprite[] downIdleFrames, Sprite[] downRunFrames,
            Sprite[] upIdleFrames, Sprite[] upRunFrames, Sprite[] sideIdleFrames, Sprite[] sideRunFrames,
            float fps = 10f, bool sideFacesRight = false)
        {
            target = renderer;
            downIdle = downIdleFrames;
            downRun = downRunFrames;
            upIdle = upIdleFrames;
            upRun = upRunFrames;
            sideIdle = sideIdleFrames;
            sideRun = sideRunFrames;
            framesPerSecond = Mathf.Max(1f, fps);
            sideRowFacesRight = sideFacesRight;
            lastPosition = transform.position;

            if (target != null && downIdle != null && downIdle.Length > 0)
            {
                target.sprite = downIdle[0];
            }
        }

        private void Update()
        {
            if (target == null) target = GetComponent<SpriteRenderer>();
            if (target == null) return;

            Vector3 position = transform.position;
            Vector3 delta = position - lastPosition;
            lastPosition = position;

            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f) return;

            Vector2 velocity = new Vector2(delta.x, delta.y) / deltaTime;
            bool moving = velocity.sqrMagnitude > moveThreshold * moveThreshold;

            int direction;
            bool flip;
            if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
            {
                direction = 2;
                // side 行朝右的素材：向左移动时翻转；朝左的素材：向右移动时翻转
                flip = sideRowFacesRight ? velocity.x < 0f : velocity.x > 0f;
            }
            else if (velocity.y > 0f)
            {
                direction = 1;
                flip = false;
            }
            else
            {
                direction = 0;
                flip = false;
            }

            Sprite[] frames = PickFrames(direction, moving);
            target.flipX = flip;

            if (frames == null || frames.Length == 0) return;

            if (frames != activeFrames)
            {
                activeFrames = frames;
                frameIndex = 0;
                frameTimer = 0f;
                target.sprite = activeFrames[0];
                return;
            }

            frameTimer += deltaTime;
            float frameDuration = 1f / framesPerSecond;
            if (frameTimer < frameDuration) return;

            frameTimer -= frameDuration;
            frameIndex = (frameIndex + 1) % activeFrames.Length;
            target.sprite = activeFrames[frameIndex];
        }

        private Sprite[] PickFrames(int direction, bool moving)
        {
            switch (direction)
            {
                case 1:
                    return moving ? upRun : upIdle;
                case 2:
                    return moving ? sideRun : sideIdle;
                default:
                    return moving ? downRun : downIdle;
            }
        }
    }
}
