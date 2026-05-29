using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NativeLibTool.Models;
using NativeLibTool.Utilities;

namespace NativeLibTool.Services
{
    internal sealed class IosPodBuilder
    {
        private static readonly string[] NativeSourceFilePatterns =
        {
            "*.h",
            "*.hh",
            "*.hpp",
            "*.m",
            "*.mm",
            "*.c",
            "*.cc",
            "*.cpp",
            "*.cxx"
        };

        private static readonly string[] PublicHeaderPatterns =
        {
            "*.h",
            "*.hh",
            "*.hpp"
        };

        private static readonly string[] CompileSourceFilePatterns =
        {
            "*.m",
            "*.mm",
            "*.c",
            "*.cc",
            "*.cpp",
            "*.cxx"
        };

        private readonly Action<string> _log;

        private sealed class PodDependency
        {
            public string Name { get; set; }
            public string Version { get; set; }
        }

        public IosPodBuilder(Action<string> log)
        {
            _log = log ?? delegate { };
        }

        public BuildResult Build(IosPodOptions options)
        {
            ValidateOptions(options);

            var sourceDirectory = ResolveSourceDirectory(options.SourceDirectory, options.AutoDetectSourceRoot);
            var podRoot = ResolvePodRoot(options);
            var vendorRoot = Path.Combine(podRoot, "Vendor");

            _log("iOS source: " + sourceDirectory);
            _log("Local Pod: " + podRoot);

            FileCopyUtility.EnsureCleanDirectory(vendorRoot);
            FileCopyUtility.CopyDirectory(sourceDirectory, vendorRoot, ShouldSkipCopyPath);

            var podspecPath = Path.Combine(podRoot, options.PodName.Trim() + ".podspec");
            WritePodspec(options, podspecPath, vendorRoot);

            var podfileSnippet = "pod '" + FileCopyUtility.RubySingleQuoteEscape(options.PodName.Trim()) +
                                 "', :path => '" + FileCopyUtility.RubySingleQuoteEscape(FileCopyUtility.ToUnixPath(podRoot)) + "'";

            return new BuildResult
            {
                PrimaryArtifactPath = podspecPath,
                MetadataPath = podRoot,
                ResolvedSourceDirectory = sourceDirectory,
                DependencyNotation = options.PodName.Trim(),
                RepositorySnippet = "# Podfile\r\n" + podfileSnippet,
                DependencySnippet = podfileSnippet
            };
        }

        public string ResolveSourceDirectory(string selectedDirectory, bool autoDetect)
        {
            IdentifierValidator.ValidateRequired("iOS source directory", selectedDirectory);

            var root = Path.GetFullPath(selectedDirectory);
            if (!Directory.Exists(root))
            {
                throw new DirectoryNotFoundException("iOS source directory does not exist: " + root);
            }

            if (ContainsPodContent(root, SearchOption.TopDirectoryOnly) ||
                ContainsNativeCompileSource(root, SearchOption.AllDirectories))
            {
                return root;
            }

            if (!autoDetect)
            {
                throw new InvalidOperationException("No framework, xcframework, static library, bundle, privacy manifest, or native source file was found in the selected directory.");
            }

            var candidates = FindPodSourceRoots(root).ToList();
            if (candidates.Count == 0)
            {
                throw new InvalidOperationException("No iOS pod source root was found under: " + root);
            }

            if (candidates.Count > 1)
            {
                throw new InvalidOperationException(
                    "Multiple iOS pod source roots were found. Please select one of them directly:\r\n" +
                    string.Join("\r\n", candidates));
            }

            return candidates[0];
        }

        public IEnumerable<string> FindPodSourceRoots(string root)
        {
            if (!Directory.Exists(root))
            {
                return Enumerable.Empty<string>();
            }

            var results = new List<string>();
            var queue = new Queue<Tuple<string, int>>();
            queue.Enqueue(Tuple.Create(Path.GetFullPath(root), 0));

            while (queue.Count > 0)
            {
                var item = queue.Dequeue();
                var directory = item.Item1;
                var depth = item.Item2;

                if (ContainsPodContent(directory, SearchOption.TopDirectoryOnly))
                {
                    results.Add(directory);
                    continue;
                }

                if (depth >= 3)
                {
                    continue;
                }

                foreach (var child in Directory.GetDirectories(directory))
                {
                    var name = Path.GetFileName(child);
                    if (name.EndsWith(".framework", StringComparison.OrdinalIgnoreCase) ||
                        name.EndsWith(".xcframework", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(name, ".git", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(name, ".svn", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    queue.Enqueue(Tuple.Create(child, depth + 1));
                }
            }

            return results;
        }

        private static bool ContainsPodContent(string directory, SearchOption searchOption)
        {
            return Directory.GetDirectories(directory, "*.framework", searchOption).Length > 0 ||
                   Directory.GetDirectories(directory, "*.xcframework", searchOption).Length > 0 ||
                   Directory.GetFiles(directory, "*.a", searchOption).Length > 0 ||
                   Directory.GetDirectories(directory, "*.bundle", searchOption).Length > 0 ||
                   Directory.GetFiles(directory, "*.xcprivacy", searchOption).Length > 0 ||
                   ContainsNativeCompileSource(directory, searchOption);
        }

        private static bool ContainsNativeCompileSource(string directory, SearchOption searchOption)
        {
            return CompileSourceFilePatterns.Any(pattern =>
                Directory.GetFiles(directory, pattern, searchOption).Any(file => !FileCopyUtility.IsUnityMetaPath(file)));
        }

        private static bool ShouldSkipCopyPath(string path)
        {
            return FileCopyUtility.IsUnityMetaPath(path) ||
                   path.IndexOf(Path.DirectorySeparatorChar + ".git" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   path.IndexOf(Path.DirectorySeparatorChar + ".svn" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void ValidateOptions(IosPodOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            IdentifierValidator.ValidateRequired("Output Pods directory", options.OutputPodsDirectory);
            IdentifierValidator.ValidatePodName(options.PodName);
            IdentifierValidator.ValidateRequired("Version", options.Version);
            IdentifierValidator.ValidateRequired("Minimum iOS version", options.MinimumIosVersion);
        }

        private static string ResolvePodRoot(IosPodOptions options)
        {
            var root = Path.GetFullPath(options.OutputPodsDirectory);
            return options.GenerateVersionDirectory
                ? Path.Combine(root, options.PodName.Trim(), options.Version.Trim())
                : Path.Combine(root, options.PodName.Trim());
        }

        private void WritePodspec(IosPodOptions options, string podspecPath, string vendorRoot)
        {
            var podRoot = Path.GetDirectoryName(podspecPath);
            FileCopyUtility.EnsureDirectory(podRoot);

            var vendoredFrameworks = FindRelativeDirectories(vendorRoot, new[] { "*.framework", "*.xcframework" }).ToList();
            var vendoredLibraries = FindRelativeFiles(vendorRoot, "*.a").ToList();
            var resourceEntries = FindResourceEntries(vendorRoot).ToList();
            var sourceFiles = FindRelativeFiles(vendorRoot, NativeSourceFilePatterns)
                .Where(path => !IsInsideVendoredContainer(path))
                .Where(path => !IsInsideResourceContainer(path))
                .ToList();
            var publicHeaders = FindRelativeFiles(vendorRoot, PublicHeaderPatterns)
                .Where(path => !IsInsideVendoredContainer(path))
                .Where(path => !IsInsideResourceContainer(path))
                .ToList();

            var builder = new StringBuilder();
            builder.AppendLine("Pod::Spec.new do |s|");
            builder.AppendLine("  s.name = '" + R(options.PodName.Trim()) + "'");
            builder.AppendLine("  s.version = '" + R(options.Version.Trim()) + "'");
            builder.AppendLine("  s.summary = '" + R(DefaultIfBlank(options.Summary, options.PodName.Trim() + " native SDK")) + "'");
            builder.AppendLine("  s.homepage = '" + R(DefaultIfBlank(options.Homepage, "https://internal.local/" + options.PodName.Trim())) + "'");
            builder.AppendLine("  s.license = { :type => '" + R(DefaultIfBlank(options.LicenseType, "Proprietary")) + "' }");
            builder.AppendLine("  s.author = { '" + R(DefaultIfBlank(options.AuthorName, "Company")) + "' => '" + R(DefaultIfBlank(options.AuthorEmail, "dev@company.local")) + "' }");
            builder.AppendLine("  s.platform = :ios, '" + R(options.MinimumIosVersion.Trim()) + "'");
            builder.AppendLine("  s.source = { :path => '.' }");

            if (options.StaticFramework)
            {
                builder.AppendLine("  s.static_framework = true");
            }

            AppendRubyArray(builder, "s.vendored_frameworks", vendoredFrameworks);
            AppendRubyArray(builder, "s.vendored_libraries", vendoredLibraries);

            if (publicHeaders.Count > 0)
            {
                AppendRubyArray(builder, "s.public_header_files", publicHeaders);
            }

            if (sourceFiles.Count > 0)
            {
                AppendRubyArray(builder, "s.source_files", sourceFiles);
            }

            AppendRubyArray(builder, "s.resources", resourceEntries);
            AppendCommaList(builder, "s.frameworks", options.SystemFrameworks);
            AppendCommaList(builder, "s.libraries", options.SystemLibraries);
            AppendPodDependencies(builder, options.PodDependencies);
            builder.AppendLine("end");

            File.WriteAllText(podspecPath, builder.ToString(), new UTF8Encoding(false));

            _log("Vendored frameworks: " + vendoredFrameworks.Count);
            _log("Vendored libraries: " + vendoredLibraries.Count);
            _log("Source files: " + sourceFiles.Count);
            _log("Public headers: " + publicHeaders.Count);
            _log("Resources: " + resourceEntries.Count);
            _log("Pod dependencies: " + ParsePodDependencies(options.PodDependencies).Count);
        }

        private static IEnumerable<string> FindRelativeDirectories(string root, IEnumerable<string> patterns)
        {
            foreach (var pattern in patterns)
            {
                foreach (var directory in Directory.GetDirectories(root, pattern, SearchOption.AllDirectories))
                {
                    yield return FileCopyUtility.ToUnixPath(FileCopyUtility.GetRelativePath(Path.GetDirectoryName(root), directory));
                }
            }
        }

        private static IEnumerable<string> FindRelativeFiles(string root, string pattern)
        {
            foreach (var file in Directory.GetFiles(root, pattern, SearchOption.AllDirectories))
            {
                yield return FileCopyUtility.ToUnixPath(FileCopyUtility.GetRelativePath(Path.GetDirectoryName(root), file));
            }
        }

        private static IEnumerable<string> FindRelativeFiles(string root, IEnumerable<string> patterns)
        {
            foreach (var pattern in patterns)
            {
                foreach (var file in FindRelativeFiles(root, pattern))
                {
                    yield return file;
                }
            }
        }

        private static IEnumerable<string> FindResourceEntries(string root)
        {
            var directoryPatterns = new[] { "*.bundle", "*.xcassets" };
            foreach (var directory in FindRelativeDirectories(root, directoryPatterns))
            {
                yield return directory;
            }

            var filePatterns = new[] { "*.plist", "*.xcprivacy", "*.storyboard", "*.xib" };
            foreach (var pattern in filePatterns)
            {
                foreach (var file in FindRelativeFiles(root, pattern))
                {
                    if (file.EndsWith("/Info.plist", StringComparison.OrdinalIgnoreCase) ||
                        IsInsideVendoredContainer(file) ||
                        file.IndexOf(".bundle/", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        file.IndexOf(".xcassets/", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        continue;
                    }

                    yield return file;
                }
            }
        }

        private static bool IsInsideVendoredContainer(string relativePath)
        {
            return relativePath.IndexOf(".framework/", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   relativePath.IndexOf(".xcframework/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsInsideResourceContainer(string relativePath)
        {
            return relativePath.IndexOf(".bundle/", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   relativePath.IndexOf(".xcassets/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void AppendRubyArray(StringBuilder builder, string name, IList<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return;
            }

            var distinct = values
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (distinct.Count == 1)
            {
                builder.AppendLine("  " + name + " = '" + R(distinct[0]) + "'");
                return;
            }

            builder.AppendLine("  " + name + " = [");
            for (var i = 0; i < distinct.Count; i++)
            {
                var suffix = i == distinct.Count - 1 ? string.Empty : ",";
                builder.AppendLine("    '" + R(distinct[i]) + "'" + suffix);
            }

            builder.AppendLine("  ]");
        }

        private static void AppendCommaList(StringBuilder builder, string name, string value)
        {
            var items = SplitCommaList(value).ToList();
            if (items.Count == 0)
            {
                return;
            }

            builder.AppendLine("  " + name + " = " + string.Join(", ", items.Select(item => "'" + R(item) + "'")));
        }

        private static void AppendPodDependencies(StringBuilder builder, string value)
        {
            foreach (var dependency in ParsePodDependencies(value))
            {
                if (string.IsNullOrWhiteSpace(dependency.Version))
                {
                    builder.AppendLine("  s.dependency '" + R(dependency.Name) + "'");
                }
                else
                {
                    builder.AppendLine("  s.dependency '" + R(dependency.Name) + "', '" + R(dependency.Version) + "'");
                }
            }
        }

        private static IList<PodDependency> ParsePodDependencies(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return new List<PodDependency>();
            }

            var dependencies = new List<PodDependency>();
            foreach (var item in value.Split(new[] { '\r', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var dependency = ParsePodDependency(item);
                if (dependency != null &&
                    !dependencies.Any(existing =>
                        string.Equals(existing.Name, dependency.Name, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(existing.Version ?? string.Empty, dependency.Version ?? string.Empty, StringComparison.OrdinalIgnoreCase)))
                {
                    dependencies.Add(dependency);
                }
            }

            return dependencies;
        }

        private static PodDependency ParsePodDependency(string value)
        {
            var text = (value ?? string.Empty).Trim();
            if (text.Length == 0)
            {
                return null;
            }

            var rubyMatch = Regex.Match(
                text,
                @"^(?:pod|s\.dependency)\s+['""]([^'""]+)['""]\s*(?:,\s*['""]([^'""]+)['""])?",
                RegexOptions.IgnoreCase);
            if (rubyMatch.Success)
            {
                return CreatePodDependency(rubyMatch.Groups[1].Value, rubyMatch.Groups[2].Value);
            }

            var separatorIndex = FindDependencySeparator(text);
            if (separatorIndex < 0)
            {
                return CreatePodDependency(text, string.Empty);
            }

            return CreatePodDependency(text.Substring(0, separatorIndex), text.Substring(separatorIndex + 1));
        }

        private static int FindDependencySeparator(string value)
        {
            var comma = value.IndexOf(',');
            var pipe = value.IndexOf('|');
            var colon = value.IndexOf(':');
            return new[] { comma, pipe, colon }
                .Where(index => index >= 0)
                .DefaultIfEmpty(-1)
                .Min();
        }

        private static PodDependency CreatePodDependency(string name, string version)
        {
            var dependencyName = TrimDependencyToken(name);
            if (string.IsNullOrWhiteSpace(dependencyName))
            {
                return null;
            }

            return new PodDependency
            {
                Name = dependencyName,
                Version = TrimDependencyToken(version)
            };
        }

        private static string TrimDependencyToken(string value)
        {
            return (value ?? string.Empty).Trim().Trim('\'', '"');
        }

        private static IEnumerable<string> SplitCommaList(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                yield break;
            }

            foreach (var item in value.Split(','))
            {
                var trimmed = item.Trim();
                if (trimmed.Length > 0)
                {
                    yield return trimmed;
                }
            }
        }

        private static string DefaultIfBlank(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private static string R(string value)
        {
            return FileCopyUtility.RubySingleQuoteEscape(value);
        }
    }
}
