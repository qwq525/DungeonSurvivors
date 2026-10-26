using DungeonSurvivors.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonSurvivors.UI
{
    /// <summary>
    /// 设置面板（W2 MVP）：音量循环切换 + 全屏开关，存到 SaveSystem。
    /// 面试可讲点：设置项持久化、AudioListener.volume 全局音量、Screen.fullScreen。
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Text volumeText;
        [SerializeField] private Text fullscreenText;
        [SerializeField] private Button volumeButton;
        [SerializeField] private Button fullscreenButton;
        [SerializeField] private Button backButton;

        private readonly float[] volumeSteps = { 1f, 0.75f, 0.5f, 0.25f, 0f };

        public void Configure(GameObject panelRoot, Text volume, Text fullscreen, Button volumeChange, Button fullscreenToggle, Button back)
        {
            root = panelRoot;
            volumeText = volume;
            fullscreenText = fullscreen;
            volumeButton = volumeChange;
            fullscreenButton = fullscreenToggle;
            backButton = back;
        }

        private void Awake()
        {
            if (root != null) root.SetActive(false);
            if (volumeButton != null) volumeButton.onClick.AddListener(CycleVolume);
            if (fullscreenButton != null) fullscreenButton.onClick.AddListener(ToggleFullscreen);
            if (backButton != null) backButton.onClick.AddListener(Hide);
        }

        public void Show()
        {
            ApplyFromSave();
            Refresh();
            if (root != null) root.SetActive(true);
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }

        private void ApplyFromSave()
        {
            SettingsData settings = SaveSystem.Current.settings;
            AudioListener.volume = settings.masterVolume;
            Screen.fullScreen = settings.fullscreen;
        }

        private void Refresh()
        {
            SettingsData settings = SaveSystem.Current.settings;
            if (volumeText != null) volumeText.text = "音量 " + Mathf.RoundToInt(settings.masterVolume * 100f) + "%";
            if (fullscreenText != null) fullscreenText.text = "全屏 " + (settings.fullscreen ? "开" : "关");
        }

        private void CycleVolume()
        {
            SettingsData settings = SaveSystem.Current.settings;
            int currentIndex = 0;
            for (int i = 0; i < volumeSteps.Length; i++)
            {
                if (Mathf.Abs(volumeSteps[i] - settings.masterVolume) < 0.01f)
                {
                    currentIndex = i;
                    break;
                }
            }

            currentIndex = (currentIndex + 1) % volumeSteps.Length;
            settings.masterVolume = volumeSteps[currentIndex];
            AudioListener.volume = settings.masterVolume;
            SaveSystem.Save();
            Refresh();
        }

        private void ToggleFullscreen()
        {
            SettingsData settings = SaveSystem.Current.settings;
            settings.fullscreen = !settings.fullscreen;
            Screen.fullScreen = settings.fullscreen;
            SaveSystem.Save();
            Refresh();
        }
    }
}
