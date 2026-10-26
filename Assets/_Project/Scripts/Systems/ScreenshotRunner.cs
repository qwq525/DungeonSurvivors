using System.Collections;
using System.IO;
using UnityEngine;

namespace DungeonSurvivors.Systems
{
    /// <summary>
    /// 截图辅助（开发/作品集）：启动参数 -screenshot（主菜单）或 -screenshotGameplay（战斗）时，
    /// 等待指定时间后截图到 persistentDataPath 并退出。用于生成 README/简历用图。
    /// </summary>
    public class ScreenshotRunner : MonoBehaviour
    {
        private void Start()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            bool menuShot = HasFlag(args, "-screenshot");
            bool gameplayShot = HasFlag(args, "-screenshotGameplay");

            if (menuShot) StartCoroutine(Capture("screenshot_menu.png", 2.5f));
            else if (gameplayShot) StartCoroutine(Capture("screenshot_gameplay.png", 6f));
            else
            {
                float timedDelay = GetFloatFlag(args, "-screenshotAt", 0f);
                if (timedDelay > 0f) StartCoroutine(Capture("screenshot_timed.png", timedDelay));
            }
        }

        private IEnumerator Capture(string fileName, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);

            string path = Path.Combine(Application.persistentDataPath, fileName);
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log("[Screenshot] Saved: " + path);

            yield return new WaitForSecondsRealtime(0.6f);
            Application.Quit();
        }

        private static float GetFloatFlag(string[] args, string flag, float fallback)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], flag, System.StringComparison.OrdinalIgnoreCase) &&
                    float.TryParse(args[i + 1], out float value)) return value;
            }

            return fallback;
        }

        private static bool HasFlag(string[] args, string flag)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], flag, System.StringComparison.OrdinalIgnoreCase)) return true;
            }

            return false;
        }
    }
}
