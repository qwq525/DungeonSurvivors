using DungeonSurvivors.Core;
using UnityEngine;

namespace DungeonSurvivors.Systems
{
    /// <summary>
    /// 摄像机平滑跟随 + 地图边界钳制。震屏（CameraShake）后续作为独立组件叠加 offset。
    /// 面试可讲点：为什么用 LateUpdate（等目标移动完）；SmoothDamp 与 Lerp 的区别；边界钳制防止看到地图外。
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothTime = 0.09f;
        [SerializeField] private Vector2 offset = Vector2.zero;
        [SerializeField] private float lookAheadDistance = 0.55f;
        [SerializeField] private float lookAheadSmooth = 3.5f;

        [Header("边界（勾选后生效）")]
        [SerializeField] private bool useBounds;
        [SerializeField] private Vector2 minBounds = new Vector2(-1.1f, -1f);
        [SerializeField] private Vector2 maxBounds = new Vector2(1.1f, 1f);

        [Header("震屏")]
        [SerializeField] private float shakeIntensity;
        [SerializeField] private float shakeDuration = 0.1f;
        [SerializeField] private float shakeTimer;

        [Header("Zoom Punch")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float baseOrthographicSize = 5f;
        [SerializeField] private float punchAmount;
        [SerializeField] private float punchDuration = 0.1f;
        [SerializeField] private float punchTimer;

        private Vector3 velocity;
        private Vector2 lookAhead;
        private Vector3 lastTargetPosition;

        private void Awake()
        {
            if (targetCamera == null) targetCamera = GetComponent<Camera>();
            if (targetCamera != null) baseOrthographicSize = targetCamera.orthographicSize;
        }

        /// <summary>叠加一次震屏（强度取最大值，持续时间取最大值）。</summary>
        public void Shake(float intensity, float duration)
        {
            shakeIntensity = Mathf.Max(shakeIntensity, intensity);
            shakeDuration = Mathf.Max(shakeDuration, duration);
            shakeTimer = Mathf.Max(shakeTimer, duration);
        }

        /// <summary>镜头 zoom punch：短暂缩小视野（拉近），提升击杀/受击的冲击感。</summary>
        public void PunchZoom(float amount, float duration)
        {
            punchAmount = Mathf.Max(punchAmount, amount);
            punchDuration = Mathf.Max(punchDuration, duration);
            punchTimer = Mathf.Max(punchTimer, duration);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void SetBounds(Vector2 min, Vector2 max)
        {
            useBounds = true;
            minBounds = min;
            maxBounds = max;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            bool paused = GameManager.Instance != null && GameManager.Instance.State != GameState.Playing;
            Vector3 targetPosition = target.position;
            float deltaTime = Time.deltaTime;
            Vector2 targetVelocity = deltaTime > 0f ? (Vector2)((targetPosition - lastTargetPosition) / deltaTime) : Vector2.zero;
            lastTargetPosition = targetPosition;

            Vector2 desiredLookAhead = targetVelocity.sqrMagnitude > 0.01f
                ? targetVelocity.normalized * lookAheadDistance * Mathf.Clamp01(targetVelocity.magnitude / 6f)
                : Vector2.zero;
            lookAhead = Vector2.Lerp(lookAhead, desiredLookAhead, Mathf.Clamp01(lookAheadSmooth * deltaTime));

            Vector2 desired = (Vector2)targetPosition + offset + lookAhead;
            if (useBounds)
            {
                desired.x = Mathf.Clamp(desired.x, minBounds.x, maxBounds.x);
                desired.y = Mathf.Clamp(desired.y, minBounds.y, maxBounds.y);
            }

            Vector3 desiredPosition = new Vector3(desired.x, desired.y, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);

            if (paused)
            {
                shakeTimer = 0f;
                shakeIntensity = 0f;
                punchTimer = 0f;
                if (targetCamera != null) targetCamera.orthographicSize = baseOrthographicSize;
            }
            else if (shakeTimer > 0f)
            {
                float ratio = shakeDuration <= 0f ? 0f : Mathf.Clamp01(shakeTimer / shakeDuration);
                Vector2 offset = Random.insideUnitCircle * (shakeIntensity * ratio);
                transform.position += (Vector3)offset;
                shakeTimer -= Time.unscaledDeltaTime;
                if (shakeTimer <= 0f) shakeIntensity = 0f;
            }

            if (targetCamera != null && punchTimer > 0f)
            {
                float ratio = punchDuration <= 0f ? 0f : Mathf.Clamp01(punchTimer / punchDuration);
                targetCamera.orthographicSize = baseOrthographicSize - punchAmount * ratio;
                punchTimer -= Time.unscaledDeltaTime;
                if (punchTimer <= 0f)
                {
                    targetCamera.orthographicSize = baseOrthographicSize;
                    punchAmount = 0f;
                }
            }
        }
    }
}
