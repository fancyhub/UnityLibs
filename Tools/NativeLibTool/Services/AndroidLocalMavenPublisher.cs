using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NativeLibTool.Models;
using NativeLibTool.Utilities;

namespace NativeLibTool.Services
{
    internal sealed class AndroidLocalMavenPublisher
    {
        private static readonly Regex PomGroupRegex = new Regex("<groupId>\\s*([^<]+)\\s*</groupId>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex PomArtifactRegex = new Regex("<artifactId>\\s*([^<]+)\\s*</artifactId>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex PomVersionRegex = new Regex("<version>\\s*([^<]+)\\s*</version>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex FileNameCoordinateRegex = new Regex("^(.+)-([0-9][A-Za-z0-9_.-]*)$", RegexOptions.Compiled);

        private readonly Action<string> _log;

        public AndroidLocalMavenPublisher(Action<string> log)
        {
            _log = log ?? delegate { };
        }

        public BuildResult Publish(AndroidMavenPublishOptions options)
        {
            ValidateOptions(options);

            var aarPath = Path.GetFullPath(options.AarPath.Trim().Trim('"'));
            ValidateAarFile(aarPath);

            var outputRepository = Path.GetFullPath(options.OutputRepositoryDirectory);
            var groupId = options.GroupId.Trim();
            var artifactId = options.ArtifactId.Trim();
            var version = options.Version.Trim();
            var dependencyConfiguration = string.IsNullOrWhiteSpace(options.DependencyConfiguration)
                ? "implementation"
                : options.DependencyConfiguration.Trim();

            _log("AAR file: " + aarPath);
            _log("Local Maven repo: " + outputRepository);

            var groupPath = Path.Combine(groupId.Split('.'));
            var artifactDirectory = Path.Combine(outputRepository, groupPath, artifactId, version);
            FileCopyUtility.EnsureDirectory(artifactDirectory);

            var mavenAarPath = Path.Combine(artifactDirectory, artifactId + "-" + version + ".aar");
            var pomPath = Path.Combine(artifactDirectory, artifactId + "-" + version + ".pom");
            File.Copy(aarPath, mavenAarPath, true);
            WritePom(groupId, artifactId, version, pomPath);
            var metadataPath = WriteMavenMetadata(outputRepository, groupPath, artifactId, groupId, version);

            if (options.GenerateChecksums)
            {
                FileCopyUtility.WriteChecksumFiles(mavenAarPath);
                FileCopyUtility.WriteChecksumFiles(pomPath);
                FileCopyUtility.WriteChecksumFiles(metadataPath);
            }

            var dependency = groupId + ":" + artifactId + ":" + version;
            _log("Published AAR: " + mavenAarPath);
            _log("Generated POM: " + pomPath);
            _log("Generated metadata: " + metadataPath);
            if (options.GenerateChecksums)
            {
                _log("Generated checksum files: .md5, .sha1");
            }

            return new BuildResult
            {
                PrimaryArtifactPath = mavenAarPath,
                MetadataPath = metadataPath,
                ResolvedSourceDirectory = aarPath,
                DependencyNotation = dependency,
                RepositorySnippet = "repositories {\r\n" +
                                    "    maven { url uri(\"" + FileCopyUtility.ToUnixPath(outputRepository) + "\") }\r\n" +
                                    "}",
                DependencySnippet = "dependencies {\r\n" +
                                    "    " + dependencyConfiguration + " \"" + dependency + "\"\r\n" +
                                    "}"
            };
        }

        public AndroidMavenCoordinate DetectCoordinates(string aarPath)
        {
            IdentifierValidator.ValidateRequired("AAR path", aarPath);
            var path = Path.GetFullPath(aarPath.Trim().Trim('"'));
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("AAR file does not exist: " + path);
            }

            ValidateAarFile(path);

            var fromProperties = TryReadPomProperties(path);
            if (fromProperties != null)
            {
                return fromProperties;
            }

            var fromPom = TryReadPomXml(path);
            if (fromPom != null)
            {
                return fromPom;
            }

            var fileName = Path.GetFileNameWithoutExtension(path);
            var match = FileNameCoordinateRegex.Match(fileName);
            if (match.Success)
            {
                return new AndroidMavenCoordinate
                {
                    ArtifactId = match.Groups[1].Value,
                    Version = match.Groups[2].Value,
                    Source = "file name"
                };
            }

            return new AndroidMavenCoordinate
            {
                ArtifactId = fileName,
                Source = "file name"
            };
        }

        private static void ValidateOptions(AndroidMavenPublishOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            IdentifierValidator.ValidateRequired("AAR path", options.AarPath);
            if (!File.Exists(options.AarPath.Trim().Trim('"')))
            {
                throw new FileNotFoundException("AAR file does not exist: " + options.AarPath);
            }

            IdentifierValidator.ValidateRequired("Output repository directory", options.OutputRepositoryDirectory);
            IdentifierValidator.ValidateMavenCoordinate(options.GroupId, options.ArtifactId, options.Version);
        }

        private void ValidateAarFile(string aarPath)
        {
            try
            {
                using (var stream = File.OpenRead(aarPath))
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    if (archive.GetEntry("AndroidManifest.xml") == null)
                    {
                        throw new InvalidOperationException("AAR file is missing AndroidManifest.xml: " + aarPath);
                    }

                    if (archive.GetEntry("classes.jar") == null)
                    {
                        _log("AAR has no classes.jar. Publishing as-is.");
                    }
                }
            }
            catch (InvalidDataException ex)
            {
                throw new InvalidOperationException("Invalid AAR archive: " + aarPath, ex);
            }
        }

        private static AndroidMavenCoordinate TryReadPomProperties(string aarPath)
        {
            using (var stream = File.OpenRead(aarPath))
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                var entry = archive.Entries
                    .Where(item => NormalizeZipPath(item.FullName).StartsWith("META-INF/maven/", StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault(item => NormalizeZipPath(item.FullName).EndsWith("/pom.properties", StringComparison.OrdinalIgnoreCase));
                if (entry == null)
                {
                    return null;
                }

                using (var reader = new StreamReader(entry.Open(), Encoding.UTF8))
                {
                    var values = ReadProperties(reader.ReadToEnd());
                    var coordinate = new AndroidMavenCoordinate
                    {
                        GroupId = GetProperty(values, "groupId"),
                        ArtifactId = GetProperty(values, "artifactId"),
                        Version = GetProperty(values, "version"),
                        Source = entry.FullName
                    };

                    return HasArtifactAndVersion(coordinate) ? coordinate : null;
                }
            }
        }

        private static AndroidMavenCoordinate TryReadPomXml(string aarPath)
        {
            using (var stream = File.OpenRead(aarPath))
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                var entry = archive.Entries
                    .Where(item => NormalizeZipPath(item.FullName).StartsWith("META-INF/maven/", StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault(item => NormalizeZipPath(item.FullName).EndsWith("/pom.xml", StringComparison.OrdinalIgnoreCase));
                if (entry == null)
                {
                    return null;
                }

                using (var reader = new StreamReader(entry.Open(), Encoding.UTF8))
                {
                    var text = reader.ReadToEnd();
                    var coordinate = new AndroidMavenCoordinate
                    {
                        GroupId = MatchFirst(PomGroupRegex, text),
                        ArtifactId = MatchFirst(PomArtifactRegex, text),
                        Version = MatchFirst(PomVersionRegex, text),
                        Source = entry.FullName
                    };

                    return HasArtifactAndVersion(coordinate) ? coordinate : null;
                }
            }
        }

        private static System.Collections.Generic.Dictionary<string, string> ReadProperties(string text)
        {
            var values = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            using (var reader = new StringReader(text ?? string.Empty))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var trimmed = line.Trim();
                    if (trimmed.Length == 0 || trimmed.StartsWith("#", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var split = trimmed.IndexOf('=');
                    if (split < 0)
                    {
                        split = trimmed.IndexOf(':');
                    }

                    if (split <= 0)
                    {
                        continue;
                    }

                    values[trimmed.Substring(0, split).Trim()] = trimmed.Substring(split + 1).Trim();
                }
            }

            return values;
        }

        private static string GetProperty(System.Collections.Generic.Dictionary<string, string> values, string name)
        {
            string value;
            return values.TryGetValue(name, out value) ? value : string.Empty;
        }

        private static string MatchFirst(Regex regex, string text)
        {
            var match = regex.Match(text ?? string.Empty);
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }

        private static bool HasArtifactAndVersion(AndroidMavenCoordinate coordinate)
        {
            return coordinate != null &&
                   !string.IsNullOrWhiteSpace(coordinate.ArtifactId) &&
                   !string.IsNullOrWhiteSpace(coordinate.Version);
        }

        private static string NormalizeZipPath(string path)
        {
            return (path ?? string.Empty).Replace('\\', '/');
        }

        private static void WritePom(string groupId, string artifactId, string version, string pomPath)
        {
            var pom = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                      "<project xmlns=\"http://maven.apache.org/POM/4.0.0\"\r\n" +
                      "         xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\r\n" +
                      "         xsi:schemaLocation=\"http://maven.apache.org/POM/4.0.0 https://maven.apache.org/xsd/maven-4.0.0.xsd\">\r\n" +
                      "  <modelVersion>4.0.0</modelVersion>\r\n" +
                      "  <groupId>" + FileCopyUtility.XmlEscape(groupId) + "</groupId>\r\n" +
                      "  <artifactId>" + FileCopyUtility.XmlEscape(artifactId) + "</artifactId>\r\n" +
                      "  <version>" + FileCopyUtility.XmlEscape(version) + "</version>\r\n" +
                      "  <packaging>aar</packaging>\r\n" +
                      "</project>\r\n";

            File.WriteAllText(pomPath, pom, new UTF8Encoding(false));
        }

        private static string WriteMavenMetadata(string repositoryRoot, string groupPath, string artifactId, string groupId, string version)
        {
            var artifactRoot = Path.Combine(repositoryRoot, groupPath, artifactId);
            FileCopyUtility.EnsureDirectory(artifactRoot);

            var versions = Directory.GetDirectories(artifactRoot)
                .Select(Path.GetFileName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Union(new[] { version })
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var metadata = new StringBuilder();
            metadata.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            metadata.AppendLine("<metadata>");
            metadata.AppendLine("  <groupId>" + FileCopyUtility.XmlEscape(groupId) + "</groupId>");
            metadata.AppendLine("  <artifactId>" + FileCopyUtility.XmlEscape(artifactId) + "</artifactId>");
            metadata.AppendLine("  <versioning>");
            metadata.AppendLine("    <latest>" + FileCopyUtility.XmlEscape(version) + "</latest>");
            metadata.AppendLine("    <release>" + FileCopyUtility.XmlEscape(version) + "</release>");
            metadata.AppendLine("    <versions>");
            foreach (var knownVersion in versions)
            {
                metadata.AppendLine("      <version>" + FileCopyUtility.XmlEscape(knownVersion) + "</version>");
            }

            metadata.AppendLine("    </versions>");
            metadata.AppendLine("  </versioning>");
            metadata.AppendLine("</metadata>");

            var metadataPath = Path.Combine(artifactRoot, "maven-metadata.xml");
            File.WriteAllText(metadataPath, metadata.ToString(), new UTF8Encoding(false));
            return metadataPath;
        }
    }
}
