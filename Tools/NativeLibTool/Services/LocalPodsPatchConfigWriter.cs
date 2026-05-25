using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NativeLibTool.Utilities;

namespace NativeLibTool.Services
{
    internal static class LocalPodsPatchConfigWriter
    {
        public static string Write(string unityProjectRoot, string localPodsDirectory)
        {
            return Write(unityProjectRoot, localPodsDirectory, "UNITY_IOS");
        }

        public static string Write(string unityProjectRoot, string localPodsDirectory, string define)
        {
            FileCopyUtility.EnsureDirectory(localPodsDirectory);

            var pods = FindPods(unityProjectRoot, localPodsDirectory).ToList();
            var buildDefine = string.IsNullOrWhiteSpace(define) ? "UNITY_IOS" : define.Trim();
            var builder = new StringBuilder();
            builder.AppendLine("{");
            builder.AppendLine("  \"enabled\": true,");
            builder.AppendLine("  \"onlyWhenDefinesContain\": [");
            builder.AppendLine("    \"" + EscapeJson(buildDefine) + "\"");
            builder.AppendLine("  ],");
            builder.AppendLine("  \"pods\": [");

            for (var i = 0; i < pods.Count; i++)
            {
                var pod = pods[i];
                var suffix = i == pods.Count - 1 ? string.Empty : ",";
                builder.AppendLine("    {");
                builder.AppendLine("      \"name\": \"" + EscapeJson(pod.Name) + "\",");
                builder.AppendLine("      \"path\": \"" + EscapeJson(pod.RelativePath) + "\"");
                builder.AppendLine("    }" + suffix);
            }

            builder.AppendLine("  ]");
            builder.AppendLine("}");

            var outputPath = Path.Combine(localPodsDirectory, "podfile-patch_temp.json");
            File.WriteAllText(outputPath, builder.ToString(), new UTF8Encoding(false));
            return outputPath;
        }

        private static IEnumerable<PodEntry> FindPods(string unityProjectRoot, string localPodsDirectory)
        {
            if (!Directory.Exists(localPodsDirectory))
            {
                yield break;
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var podspec in Directory.GetFiles(localPodsDirectory, "*.podspec", SearchOption.AllDirectories)
                         .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                var name = TryReadPodName(podspec);
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = Path.GetFileNameWithoutExtension(podspec);
                }

                var podDirectory = Path.GetDirectoryName(podspec);
                var relative = FileCopyUtility.ToUnixPath(FileCopyUtility.GetRelativePath(unityProjectRoot, podDirectory));
                if (!seen.Add(name + "|" + relative))
                {
                    continue;
                }

                yield return new PodEntry
                {
                    Name = name.Trim(),
                    RelativePath = relative
                };
            }
        }

        private static string TryReadPodName(string podspecPath)
        {
            try
            {
                var text = File.ReadAllText(podspecPath);
                var match = Regex.Match(text, @"s\.name\s*=\s*['""]([^'""]+)['""]");
                return match.Success ? match.Groups[1].Value : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string EscapeJson(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t");
        }

        private sealed class PodEntry
        {
            public string Name { get; set; }
            public string RelativePath { get; set; }
        }
    }
}
