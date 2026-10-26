using System;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonSurvivors.UI
{
    /// <summary>暂停面板（W1-4）：Esc 打开，Resume 继续，Restart 重开。</summary>
    public class PausePanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;

        private Action onResume;
        private Action onRestart;
        private Action onMenu;

        public void Configure(GameObject panelRoot, Button resume, Button restart, Button menu)
        {
            root = panelRoot;
            resumeButton = resume;
            restartButton = restart;
            menuButton = menu;
        }

        private void Awake()
        {
            if (root != null) root.SetActive(false);
            if (resumeButton != null) resumeButton.onClick.AddListener(delegate { if (onResume != null) onResume(); });
            if (restartButton != null) restartButton.onClick.AddListener(delegate { if (onRestart != null) onRestart(); });
            if (menuButton != null) menuButton.onClick.AddListener(delegate { if (onMenu != null) onMenu(); });
        }

        public void Show(Action resume, Action restart, Action menu)
        {
            onResume = resume;
            onRestart = restart;
            onMenu = menu;
            if (root != null) root.SetActive(true);
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
            onResume = null;
            onRestart = null;
            onMenu = null;
        }
    }
}
