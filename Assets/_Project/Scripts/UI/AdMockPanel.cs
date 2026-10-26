using System;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonSurvivors.UI
{
    /// <summary>
    /// 假广告面板（Mock）：3 秒倒计时后自动发放奖励；点 Close 表示放弃（失败回调）。
    /// 用 unscaledDeltaTime，保证 Time.timeScale=0 时倒计时仍然走。
    /// 面试可讲点：激励视频的成功/取消回调、倒计时实现、为什么用 unscaled 时间。
    /// </summary>
    public class AdMockPanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Text titleText;
        [SerializeField] private Text countdownText;
        [SerializeField] private Button closeButton;

        private float timeLeft;
        private bool running;
        private Action<bool> callback;

        public void Configure(GameObject panelRoot, Text title, Text countdown, Button close)
        {
            root = panelRoot;
            titleText = title;
            countdownText = countdown;
            closeButton = close;
        }

        private void Awake()
        {
            if (root != null) root.SetActive(false);
            if (closeButton != null) closeButton.onClick.AddListener(delegate { Finish(false); });
        }

        public void Show(string placement, Action<bool> onFinished)
        {
            callback = onFinished;
            timeLeft = 3f;
            running = true;
            if (titleText != null) titleText.text = "激励视频（模拟） - " + placement.ToUpperInvariant();
            if (countdownText != null) countdownText.text = "3 秒后发放奖励";
            if (root != null) root.SetActive(true);
        }

        private void Update()
        {
            if (!running) return;

            timeLeft -= Time.unscaledDeltaTime;
            if (countdownText != null) countdownText.text = "距发放奖励 " + Mathf.Max(0, Mathf.CeilToInt(timeLeft)) + " 秒";
            if (timeLeft <= 0f) Finish(true);
        }

        private void Finish(bool success)
        {
            if (!running) return;
            running = false;
            if (root != null) root.SetActive(false);
            Action<bool> result = callback;
            callback = null;
            if (result != null) result(success);
        }
    }
}
