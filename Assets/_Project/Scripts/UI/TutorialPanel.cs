using System;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonSurvivors.UI
{
    /// <summary>首次进入游戏的操作引导面板：点 GOT IT 后才开始第一局。</summary>
    public class TutorialPanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Button closeButton;

        private Action onClose;

        public void Configure(GameObject panelRoot, Button close)
        {
            root = panelRoot;
            closeButton = close;
        }

        private void Awake()
        {
            if (root != null) root.SetActive(false);
            if (closeButton != null) closeButton.onClick.AddListener(OnCloseClicked);
        }

        public void Show(Action callback)
        {
            onClose = callback;
            if (root != null) root.SetActive(true);
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }

        private void OnCloseClicked()
        {
            Hide();
            Action callback = onClose;
            onClose = null;
            if (callback != null) callback();
        }
    }
}
