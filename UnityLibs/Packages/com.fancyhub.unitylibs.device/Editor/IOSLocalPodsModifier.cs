/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/05/19
 * Title   : 
 * Desc    : 配合NativeLibTool使用的
*************************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace FH.DI.Ed
{
    public static class IOSLocalPodsModifier
    {
        private const int CallbackOrder = 45;
        private const string ConfigRelativePath = "LocalPods/podfile-patch.json";

        [Serializable]
        private class Config
        {
            public bool enabled = true;
            public string[] onlyWhenDefinesContain;
            public PodConfig[] pods;
        }

        [Serializable]
        private class PodConfig
        {
            public string name;
            public string path;
            public string version;
            public string podspec;
        }

        [PostProcessBuild(CallbackOrder)]
        public static void OnPostProcessBuild(BuildTarget target, string buildPath)
        {
            if (target != BuildTarget.iOS)
                return;

            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var configPath = Path.Combine(projectRoot, ConfigRelativePath);

            if (!File.Exists(configPath))
            {
                Debug.Log("[LocalPods] Config not found, skip: " + configPath);
                return;
            }

            var config = JsonUtility.FromJson<Config>(File.ReadAllText(configPath));
            if (config == null || !config.enabled || config.pods == null || config.pods.Length == 0)
            {
                Debug.Log("[LocalPods] Disabled or empty config, skip.");
                return;
            }

            if (!MatchDefines(config.onlyWhenDefinesContain))
            {
                Debug.Log("[LocalPods] Current defines do not match config, skip.");
                return;
            }

            var podfilePath = Path.Combine(buildPath, "Podfile");
            if (!File.Exists(podfilePath))
            {
                Debug.LogWarning("[LocalPods] Podfile not found: " + podfilePath);
                return;
            }

            var podLines = config.pods
                .Where(IsValidPod)
                .Select(pod => BuildPodLine(pod, projectRoot))
                .ToList();

            if (podLines.Count == 0)
            {
                Debug.Log("[LocalPods] No valid pod entries.");
                return;
            }

            PatchPodfile(podfilePath, podLines);
            Debug.Log("[LocalPods] Patched Podfile: " + podfilePath);
        }

        private static bool MatchDefines(string[] requiredDefines)
        {
            if (requiredDefines == null || requiredDefines.Length == 0)
                return true;

            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS)
                .Split(';')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToHashSet();

            return requiredDefines.All(defines.Contains);
        }

        private static bool IsValidPod(PodConfig pod)
        {
            return pod != null && !string.IsNullOrWhiteSpace(pod.name);
        }

        private static string BuildPodLine(PodConfig pod, string projectRoot)
        {
            var line = "  pod '" + EscapeRuby(pod.name) + "'";

            if (!string.IsNullOrWhiteSpace(pod.version))
                line += ", '" + EscapeRuby(pod.version) + "'";

            if (!string.IsNullOrWhiteSpace(pod.path))
            {
                var path = pod.path.Replace("\\", "/");
                if (!Path.IsPathRooted(path))
                    path = Path.GetFullPath(Path.Combine(projectRoot, path)).Replace("\\", "/");

                line += ", :path => '" + EscapeRuby(path) + "'";
            }

            if (!string.IsNullOrWhiteSpace(pod.podspec))
                line += ", :podspec => '" + EscapeRuby(pod.podspec.Replace("\\", "/")) + "'";

            return line;
        }

        private static void PatchPodfile(string podfilePath, List<string> podLines)
        {
            var lines = File.ReadAllLines(podfilePath).ToList();

            foreach (var podLine in podLines)
            {
                var podName = ExtractPodName(podLine);
                lines.RemoveAll(line => line.TrimStart().StartsWith("pod '" + podName + "'", StringComparison.Ordinal));
            }

            var insertIndex = lines.FindLastIndex(line => line.Trim() == "end");
            if (insertIndex < 0)
                insertIndex = lines.Count;

            lines.Insert(insertIndex, "  # Local CN pods");
            lines.InsertRange(insertIndex + 1, podLines);

            File.WriteAllLines(podfilePath, lines);
        }

        private static string ExtractPodName(string podLine)
        {
            var start = podLine.IndexOf('\'') + 1;
            var end = podLine.IndexOf('\'', start);
            return podLine.Substring(start, end - start);
        }

        private static string EscapeRuby(string value)
        {
            return value.Replace("\\", "\\\\").Replace("'", "\\'");
        }
    }
}
