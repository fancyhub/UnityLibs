using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NativeLibTool.Utilities;

namespace NativeLibTool.Services
{
    internal static class LocalPodsCustomPodfileWriter
    {
        private const string UnityProjectPlaceholder = "<UnityProject>";

        public static string Write(string unityProjectRoot, string localPodsDirectory)
        {
            FileCopyUtility.EnsureDirectory(localPodsDirectory);

            var pods = FindPods(unityProjectRoot, localPodsDirectory).ToList();
            var builder = new StringBuilder();

            builder.AppendLine("target 'Add' do");
            foreach (var pod in pods)
            {
                builder.AppendLine("  pod '" + EscapeRuby(pod.Name) + "', :path => '" +
                                   EscapeRuby(UnityProjectPlaceholder + "/" + pod.RelativePath) + "'");
            }

            builder.AppendLine("end");
            builder.AppendLine();
            builder.AppendLine("target 'Remove' do");
            builder.AppendLine("end");

            var outputPath = Path.Combine(localPodsDirectory, "custom.podfile");
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

        private static string EscapeRuby(string value)
        {
            return (value ?? string.Empty).Replace("\\", "\\\\").Replace("'", "\\'");
        }

        private sealed class PodEntry
        {
            public string Name { get; set; }
            public string RelativePath { get; set; }
        }
    }
}
