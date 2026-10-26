using UnityEngine;

namespace DungeonSurvivors.Systems
{
    /// <summary>
    /// 火把光晕闪烁（占位氛围）：Perlin 噪声驱动缩放与透明度，零美术成本提升暗黑地牢氛围。
    /// </summary>
    public class TorchFlicker : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer target;
        [SerializeField] private float baseScale = 2.5f;
        [SerializeField] private float scaleAmplitude = 0.12f;
        [SerializeField] private float speed = 1.7f;

        private float seed;
        private Color baseColor;

        private void Awake()
        {
            if (target == null) target = GetComponent<SpriteRenderer>();
            seed = Random.Range(0f, 100f);
            if (target != null) baseColor = target.color;
        }

        private void Update()
        {
            float noise = Mathf.PerlinNoise(seed, Time.time * speed);
            float scale = baseScale * (1f + (noise - 0.5f) * 2f * scaleAmplitude);
            transform.localScale = new Vector3(scale, scale, 1f);

            if (target != null)
            {
                Color color = baseColor;
                color.a = baseColor.a * (0.8f + 0.2f * noise);
                target.color = color;
            }
        }
    }
}
