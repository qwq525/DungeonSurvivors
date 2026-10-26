using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonSurvivors.UI
{
    /// <summary>
    /// 伤害数字（P0 打击感）：订阅 DamageDealtEvent，在命中位置弹出数字，上飘 + 淡出 + 池化。
    /// 面试可讲点：世界坐标转屏幕坐标、UI 对象池、为什么伤害数字能显著提升"打到了"的反馈。
    /// </summary>
    public class DamageNumberSpawner : MonoBehaviour
    {
        [SerializeField] private Canvas targetCanvas;
        [SerializeField] private int prewarmCount = 24;
        [SerializeField] private int maxPoolSize = 64;
        [SerializeField] private float lifetime = 0.7f;
        [SerializeField] private float riseSpeed = 60f;

        private readonly List<Text> pool = new List<Text>();
        private readonly List<ActiveNumber> active = new List<ActiveNumber>();
        private Font font;
        private RectTransform canvasRect;

        private class ActiveNumber
        {
            public Text Text;
            public Vector2 Position;
            public Vector2 Velocity;
            public float Life;
            public Color Color;
        }

        public void Configure(Canvas canvas)
        {
            targetCanvas = canvas;
        }

        private void Awake()
        {
            if (targetCanvas == null) targetCanvas = GetComponent<Canvas>();
            if (targetCanvas != null) canvasRect = targetCanvas.GetComponent<RectTransform>();
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            for (int i = 0; i < prewarmCount; i++)
            {
                pool.Add(CreateNumber());
            }
        }

        private void OnEnable()
        {
            DungeonSurvivors.Core.EventBus.Subscribe<DungeonSurvivors.Core.DamageDealtEvent>(OnDamageDealt);
        }

        private void OnDisable()
        {
            DungeonSurvivors.Core.EventBus.Unsubscribe<DungeonSurvivors.Core.DamageDealtEvent>(OnDamageDealt);
        }

        private void OnDamageDealt(DungeonSurvivors.Core.DamageDealtEvent evt)
        {
            if (canvasRect == null || Camera.main == null) return;

            Vector3 screenPoint = Camera.main.WorldToScreenPoint(evt.Position);
            Vector2 localPoint;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out localPoint)) return;

            Text text = GetNumber();
            text.transform.SetParent(targetCanvas.transform, false);
            text.rectTransform.anchoredPosition = localPoint + new Vector2(Random.Range(-10f, 10f), 10f);
            text.text = evt.Amount.ToString();
            text.color = new Color(1f, 0.96f, 0.88f, 1f);

            active.Add(new ActiveNumber
            {
                Text = text,
                Position = text.rectTransform.anchoredPosition,
                Velocity = new Vector2(Random.Range(-18f, 18f), riseSpeed),
                Life = lifetime,
                Color = text.color
            });
        }

        private Text CreateNumber()
        {
            GameObject go = new GameObject("DamageNumber", typeof(RectTransform));
            go.transform.SetParent(targetCanvas != null ? targetCanvas.transform : transform, false);
            Text text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = 26;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0.05f, 0.03f, 0.08f, 0.85f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
            go.SetActive(false);
            return text;
        }

        private Text GetNumber()
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (!pool[i].gameObject.activeSelf)
                {
                    pool[i].gameObject.SetActive(true);
                    return pool[i];
                }
            }

            if (pool.Count >= maxPoolSize) return CreateNumber();

            Text created = CreateNumber();
            pool.Add(created);
            created.gameObject.SetActive(true);
            return created;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                ActiveNumber number = active[i];
                number.Life -= dt;
                if (number.Life <= 0f || number.Text == null)
                {
                    if (number.Text != null) number.Text.gameObject.SetActive(false);
                    active.RemoveAt(i);
                    continue;
                }

                number.Position += number.Velocity * dt;
                number.Text.rectTransform.anchoredPosition = number.Position;
                Color color = number.Color;
                color.a = Mathf.Clamp01(number.Life / lifetime);
                number.Text.color = color;
                active[i] = number;
            }
        }
    }
}
