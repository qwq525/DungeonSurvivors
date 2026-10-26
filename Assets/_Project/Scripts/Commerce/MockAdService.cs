using System;
using DungeonSurvivors.UI;
using UnityEngine;

namespace DungeonSurvivors.Commerce
{
    /// <summary>
    /// 激励视频广告的 Mock 实现：3 秒假广告 + 关闭按钮（失败），演示"复活"商业化流程。
    /// 真 SDK 接入时只替换这个类（接口不变）。
    /// </summary>
    public class MockAdService : MonoBehaviour, IAdService
    {
        [SerializeField] private AdMockPanel panel;
        private Action<bool> pendingCallback;

        public void Configure(AdMockPanel adPanel)
        {
            panel = adPanel;
        }

        public void ShowRewardedAd(string placement, Action<bool> onCompleted)
        {
            if (panel == null)
            {
                Debug.LogWarning("[MockAdService] Ad panel not assigned, auto reward.");
                if (onCompleted != null) onCompleted(true);
                return;
            }

            if (pendingCallback != null)
            {
                Debug.LogWarning("[MockAdService] Ad already showing, ignore request.");
                return;
            }

            pendingCallback = onCompleted;
            panel.Show(placement, OnAdFinished);
        }

        private void OnAdFinished(bool success)
        {
            Action<bool> callback = pendingCallback;
            pendingCallback = null;
            if (callback != null) callback(success);
        }
    }
}
