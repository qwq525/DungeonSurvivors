using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace DungeonSurvivors.EditorTools
{
    /// <summary>
    /// 一键打包 Windows x64（W1-6）。
    /// 菜单：DungeonSurvivors -> Build Windows x64
    /// 命令行：
    /// Unity.exe -batchmode -quit -projectPath "...\DungeonSurvivors" -executeMethod DungeonSurvivors.EditorTools.BuildScript.BuildWindows -logFile "...\Logs\build.log"
    /// 面试可讲点：BuildPlayerOptions、版本号/包名、构建报告（大小/错误数）、一键构建脚本。
    /// </summary>
    public static class BuildScript
    {
        private const string BuildDir = "Builds/Windows";
        private const string ExeName = "DungeonSurvivors.exe";
        private const string ScenePath = "Assets/_Project/Scenes/Main.unity";
        private const string ArtScenePath = "Assets/_Project/Scenes/MainArt.unity";

        [MenuItem("DungeonSurvivors/Build Windows x64")]
        public static void BuildWindows()
        {
            Directory.CreateDirectory(BuildDir);

            PlayerSettings.companyName = "DungeonSurvivors";
            PlayerSettings.productName = "Dungeon Survivors";
            PlayerSettings.bundleVersion = "0.2.0";

            string scenePath = File.Exists(ArtScenePath) ? ArtScenePath : ScenePath;
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { scenePath },
                locationPathName = Path.Combine(BuildDir, ExeName),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            Debug.Log("[Build] result=" + summary.result
                      + " size=" + summary.totalSize
                      + " errors=" + summary.totalErrors
                      + " output=" + options.locationPathName);

            if (summary.result != BuildResult.Succeeded)
            {
                throw new Exception("Build failed: " + summary.result);
            }
        }
    }
}
