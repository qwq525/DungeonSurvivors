using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonSurvivors.UI
{
    /// <summary>
    /// 全屏闪屏（打击感 P2）：升级白闪、受伤红闪；用 unscaledDeltaTime，暂停/顿帧时也能正常淡出。
    /// </summary>
    public class ScreenFlash : MonoBehaviour
    {
        [SerializeField] private Image flashImage;
        private Coroutine routine;

        public void Configure(Image image)
        {
            flashImage = image;
        }

        private void Awake()
        {
            if (flashImage == null) return;
            Color color = flashImage.color;
            color.a = 0f;
            flashImage.color = color;
            flashImage.raycastTarget = false;
        }

        public void Flash(Color color, float intensity, float duration)
        {
            if (flashImage == null) return;
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(Run(color, Mathf.Clamp01(intensity), Mathf.Max(0.01f, duration)));
        }

        private IEnumerator Run(Color color, float intensity, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(intensity, 0f, elapsed / duration);
                Color current = color;
                current.a = alpha;
                flashImage.color = current;
                yield return null;
            }

            Color final = color;
            final.a = 0f;
            flashImage.color = final;
            routine = null;
        }
    }
}
