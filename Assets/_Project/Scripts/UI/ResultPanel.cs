using System;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonSurvivors.UI
{
    /// <summary>
    /// 结算面板（W2）：显示时间/击杀/等级 + 本局获得灵魂；支持看广告复活、重开、退出。
    /// </summary>
    public class ResultPanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Text titleText;
        [SerializeField] private Text statsText;
        [SerializeField] private Text soulsText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button reviveButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private Button quitButton;

        private Action onRetry;
        private Action onRevive;
        private Action onMenu;
        private Action onQuit;

        public void Configure(GameObject panelRoot, Text title, Text stats, Text souls, Button retry, Button revive, Button menu, Button quit)
        {
            root = panelRoot;
            titleText = title;
            statsText = stats;
            soulsText = souls;
            retryButton = retry;
            reviveButton = revive;
            menuButton = menu;
            quitButton = quit;
        }

        private void Awake()
        {
            if (root != null) root.SetActive(false);
            if (retryButton != null) retryButton.onClick.AddListener(delegate { if (onRetry != null) onRetry(); });
            if (reviveButton != null) reviveButton.onClick.AddListener(delegate { if (onRevive != null) onRevive(); });
            if (menuButton != null) menuButton.onClick.AddListener(delegate { if (onMenu != null) onMenu(); });
            if (quitButton != null) quitButton.onClick.AddListener(delegate { if (onQuit != null) onQuit(); });
        }

        public void Show(string title, string stats, int soulsEarned, bool canRevive, Action retry, Action revive, Action menu, Action quit)
        {
            onRetry = retry;
            onRevive = revive;
            onMenu = menu;
            onQuit = quit;
            if (titleText != null) titleText.text = title;
            if (statsText != null) statsText.text = stats;
            if (soulsText != null) soulsText.text = "灵魂 +" + soulsEarned;
            if (reviveButton != null) reviveButton.gameObject.SetActive(canRevive);
            if (root != null) root.SetActive(true);
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
            onRetry = null;
            onRevive = null;
            onMenu = null;
            onQuit = null;
        }
    }
}
