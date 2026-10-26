using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DungeonSurvivors.Core;
using DungeonSurvivors.Gameplay.Enemies;
using DungeonSurvivors.Gameplay.Player;
using DungeonSurvivors.Gameplay.Weapons;
using Unity.Profiling;
using UnityEngine;

namespace DungeonSurvivors.Systems
{
    /// <summary>
    /// 自动压测（性能自证工具）：启动参数带 -stressTest 时，自动开局、刷 N 个敌人、采样 10 秒帧时间，
    /// 写出 CSV 并打印可复现的性能数据（FPS/帧时间分位），然后退出。
    /// 用法：DungeonSurvivors.exe -stressTest -screen-width 1280 -screen-height 720 -logFile stress.log
    /// 面试可讲点：可复现的性能测量方案、p1/p99 帧时间、测试条件记录。
    /// </summary>
    public class StressTestRunner : MonoBehaviour
    {
        [SerializeField] private EnemySpawner spawner;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private MissileWeapon playerWeapon;
        [SerializeField] private int enemyCount = 200;
        [SerializeField] private float durationSeconds = 10f;

        public void Configure(EnemySpawner enemySpawner, PlayerHealth health, MissileWeapon weapon, int count = 200, float duration = 10f)
        {
            spawner = enemySpawner;
            playerHealth = health;
            playerWeapon = weapon;
            enemyCount = Mathf.Max(1, count);
            durationSeconds = Mathf.Max(1f, duration);
        }

        private void Start()
        {
            if (!HasCommandLineFlag("-stressTest")) return;

            enemyCount = GetIntArgument("-stressEnemies", enemyCount);
            durationSeconds = GetFloatArgument("-stressDuration", durationSeconds);
            enemyCount = Mathf.Max(1, enemyCount);
            durationSeconds = Mathf.Max(1f, durationSeconds);

            StartCoroutine(RunStressTest());
        }

        private static int GetIntArgument(string key, int fallback)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(args[i + 1], out int value))
                {
                    return value;
                }
            }

            return fallback;
        }

        private static float GetFloatArgument(string key, float fallback)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase) &&
                    float.TryParse(args[i + 1], System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float value))
                {
                    return value;
                }
            }

            return fallback;
        }

        private IEnumerator RunStressTest()
        {
            yield return null;

            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
            {
                GameManager.Instance.StartRun();
            }

            if (playerHealth != null) playerHealth.SetInvincible(true);

            // 压测"敌人追击"负载：关闭玩家武器，避免击杀/特效/顿帧干扰测量
            if (playerWeapon != null) playerWeapon.enabled = false;

            yield return new WaitForSecondsRealtime(0.5f);

            if (spawner != null)
            {
                spawner.SpawnForStressTest(enemyCount);
            }

            // 预热 1 秒，避免把初始化开销算进稳态
            yield return new WaitForSecondsRealtime(1f);

            ProfilerRecorder gcRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
            ProfilerRecorder drawCallRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
            long gcTotal = 0;
            long drawCallTotal = 0;
            int gcSamples = 0;
            int drawCallSamples = 0;

            List<float> frames = new List<float>(4096);
            float elapsed = 0f;
            while (elapsed < durationSeconds)
            {
                float delta = Time.unscaledDeltaTime;
                frames.Add(delta);
                elapsed += delta;

                if (gcRecorder.Valid && gcRecorder.Count > 0)
                {
                    gcTotal += gcRecorder.LastValue;
                    gcSamples++;
                }

                if (drawCallRecorder.Valid && drawCallRecorder.Count > 0)
                {
                    drawCallTotal += drawCallRecorder.LastValue;
                    drawCallSamples++;
                }

                yield return null;
            }

            gcRecorder.Dispose();
            drawCallRecorder.Dispose();

            if (spawner != null) spawner.ClearAll();

            WriteReport(frames, enemyCount);

            if (gcSamples > 0)
            {
                Debug.Log("[StressTest] avgGcAllocKBPerFrame=" + ((float)gcTotal / gcSamples / 1024f).ToString("F3"));
            }
            else
            {
                Debug.Log("[StressTest] GC recorder unavailable.");
            }

            if (drawCallSamples > 0)
            {
                Debug.Log("[StressTest] avgDrawCalls=" + ((float)drawCallTotal / drawCallSamples).ToString("F1"));
            }
            else
            {
                Debug.Log("[StressTest] DrawCall recorder unavailable.");
            }

            yield return new WaitForSecondsRealtime(0.2f);
            Application.Quit();
        }

        private void WriteReport(List<float> frames, int count)
        {
            if (frames.Count == 0)
            {
                Debug.Log("[StressTest] No frames collected.");
                return;
            }

            float total = 0f;
            float worstDelta = 0f;
            for (int i = 0; i < frames.Count; i++)
            {
                total += frames[i];
                if (frames[i] > worstDelta) worstDelta = frames[i];
            }

            float avgDelta = total / frames.Count;
            float avgFps = 1f / Mathf.Max(0.0001f, avgDelta);
            List<float> sorted = new List<float>(frames);
            sorted.Sort();
            float p50 = Percentile(sorted, 0.50f);
            float p95 = Percentile(sorted, 0.95f);
            float p99 = Percentile(sorted, 0.99f);

            string summary = string.Format(
                "[StressTest] enemies={0} frames={1} avgFps={2:F1} avgMs={3:F2} p50Ms={4:F2} p95Ms={5:F2} p99Ms={6:F2} worstMs={7:F2}",
                count, frames.Count, avgFps, avgDelta * 1000f, p50 * 1000f, p95 * 1000f, p99 * 1000f, worstDelta * 1000f);
            Debug.Log(summary);

            try
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("frame_index,frame_ms,fps");
                for (int i = 0; i < frames.Count; i++)
                {
                    builder.AppendLine(i + "," + (frames[i] * 1000f).ToString("F3") + "," + (1f / Mathf.Max(0.0001f, frames[i])).ToString("F1"));
                }

                string path = Path.Combine(Application.persistentDataPath, "stress_test_" + count + ".csv");
                File.WriteAllText(path, builder.ToString());
                Debug.Log("[StressTest] CSV written: " + path);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[StressTest] CSV write failed: " + exception.Message);
            }
        }

        private static float Percentile(List<float> sorted, float percentile)
        {
            if (sorted.Count == 0) return 0f;
            int index = Mathf.Clamp(Mathf.FloorToInt(sorted.Count * percentile), 0, sorted.Count - 1);
            return sorted[index];
        }

        private static bool HasCommandLineFlag(string flag)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], flag, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }
    }
}
