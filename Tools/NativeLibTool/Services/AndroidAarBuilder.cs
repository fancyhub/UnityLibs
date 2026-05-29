using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using NativeLibTool.Models;
using NativeLibTool.Utilities;

namespace NativeLibTool.Services
{
    internal sealed class AndroidAarBuilder
    {
        private static readonly string[] AbiNames =
        {
            "armeabi",
            "armeabi-v7a",
            "arm64-v8a",
            "x86",
            "x86_64",
            "mips",
            "mips64"
        };

        private static readonly HashSet<string> AbiNameSet =
            new HashSet<string>(AbiNames, StringComparer.OrdinalIgnoreCase);

        private readonly Action<string> _log;

        private enum AndroidInputKind
        {
            AndroidLibraryDirectory,
            ExistingAarFile,
            LooseContentDirectory,
            JarFile,
            NativeLibraryFile
        }

        private sealed class AndroidInput
        {
            public string Path { get; set; }
            public AndroidInputKind Kind { get; set; }
        }

        public AndroidAarBuilder(Action<string> log)
        {
            _log = log ?? delegate { };
        }

        public BuildResult BuildAar(AndroidAarPackageOptions options)
        {
            ValidatePackageOptions(options);

            var source = ResolveSource(options.SourcePath, options.AutoDetectSourceRoot);
            if (source.Kind == AndroidInputKind.ExistingAarFile)
            {
                throw new InvalidOperationException("The selected input is already an AAR. Use the Android AAR -> Local Maven tab to publish it.");
            }

            var outputDirectory = Path.GetFullPath(options.OutputDirectory);
            var artifactId = options.ArtifactId.Trim();
            var version = options.Version.Trim();
            var aarPath = Path.Combine(outputDirectory, artifactId + "-" + version + ".aar");
            var tempRoot = Path.Combine(Path.GetTempPath(), "NativeLibTool-AAR-" + Guid.NewGuid().ToString("N"));
            var aarRoot = Path.Combine(tempRoot, "aar");

            _log("Android source: " + source.Path);
            _log("Android source type: " + GetInputKindLabel(source.Kind));
            _log("AAR output: " + aarPath);

            try
            {
                FileCopyUtility.EnsureDirectory(outputDirectory);
                FileCopyUtility.EnsureCleanDirectory(aarRoot);
                CopyAarContents(source, aarRoot);
                CreateAarArchive(aarRoot, aarPath);

                return new BuildResult
                {
                    PrimaryArtifactPath = aarPath,
                    ResolvedSourceDirectory = source.Path,
                    RepositorySnippet = "AAR output:\r\n" + aarPath
                };
            }
            finally
            {
                if (Directory.Exists(tempRoot))
                {
                    Directory.Delete(tempRoot, true);
                }
            }
        }

        public BuildResult Build(AndroidAarOptions options)
        {
            ValidateOptions(options);

            var source = ResolveSource(options.SourceDirectory, options.AutoDetectSourceRoot);
            var sourcePath = source.Path;
            var outputRepository = Path.GetFullPath(options.OutputRepositoryDirectory);
            var dependencyConfiguration = string.IsNullOrWhiteSpace(options.DependencyConfiguration)
                ? "implementation"
                : options.DependencyConfiguration.Trim();

            _log("Android source: " + sourcePath);
            _log("Android source type: " + GetInputKindLabel(source.Kind));
            _log("Local Maven repo: " + outputRepository);

            var tempRoot = Path.Combine(Path.GetTempPath(), "NativeLibTool-AAR-" + Guid.NewGuid().ToString("N"));
            var aarRoot = Path.Combine(tempRoot, "aar");

            try
            {
                var groupPath = Path.Combine(options.GroupId.Trim().Split('.'));
                var artifactDirectory = Path.Combine(outputRepository, groupPath, options.ArtifactId.Trim(), options.Version.Trim());
                FileCopyUtility.EnsureDirectory(artifactDirectory);

                var aarFileName = options.ArtifactId.Trim() + "-" + options.Version.Trim() + ".aar";
                var pomFileName = options.ArtifactId.Trim() + "-" + options.Version.Trim() + ".pom";
                var aarPath = Path.Combine(artifactDirectory, aarFileName);
                var pomPath = Path.Combine(artifactDirectory, pomFileName);

                if (source.Kind == AndroidInputKind.ExistingAarFile)
                {
                    ValidateAarFile(sourcePath);
                    File.Copy(sourcePath, aarPath, true);
                    _log("Published existing AAR: " + sourcePath);
                }
                else
                {
                    FileCopyUtility.EnsureCleanDirectory(aarRoot);
                    CopyAarContents(source, aarRoot);
                    CreateAarArchive(aarRoot, aarPath);
                }

                WritePom(options, pomPath);
                var metadataPath = WriteMavenMetadata(outputRepository, groupPath, options.ArtifactId.Trim(), options.GroupId.Trim(), options.Version.Trim());

                if (options.GenerateChecksums)
                {
                    FileCopyUtility.WriteChecksumFiles(aarPath);
                    FileCopyUtility.WriteChecksumFiles(pomPath);
                    FileCopyUtility.WriteChecksumFiles(metadataPath);
                }

                var dependency = options.GroupId.Trim() + ":" + options.ArtifactId.Trim() + ":" + options.Version.Trim();
                return new BuildResult
                {
                    PrimaryArtifactPath = aarPath,
                    MetadataPath = metadataPath,
                    ResolvedSourceDirectory = sourcePath,
                    DependencyNotation = dependency,
                    RepositorySnippet = "repositories {\r\n" +
                                        "    maven { url uri(\"" + FileCopyUtility.ToUnixPath(outputRepository) + "\") }\r\n" +
                                        "}",
                    DependencySnippet = "dependencies {\r\n" +
                                        "    " + dependencyConfiguration + " \"" + dependency + "\"\r\n" +
                                        "}"
                };
            }
            finally
            {
                if (Directory.Exists(tempRoot))
                {
                    Directory.Delete(tempRoot, true);
                }
            }
        }

        public string ResolveSourceDirectory(string selectedDirectory, bool autoDetect)
        {
            return ResolveSource(selectedDirectory, autoDetect).Path;
        }

        private AndroidInput ResolveSource(string selectedPath, bool autoDetect)
        {
            IdentifierValidator.ValidateRequired("Android source path", selectedPath);

            var root = Path.GetFullPath(selectedPath.Trim().Trim('"'));
            if (File.Exists(root))
            {
                return ResolveSourceFile(root);
            }

            if (!Directory.Exists(root))
            {
                throw new FileNotFoundException("Android source path does not exist: " + root);
            }

            var direct = TryCreateInputFromDirectory(root, true);
            if (direct != null)
            {
                return direct;
            }

            if (!autoDetect)
            {
                throw new InvalidOperationException("No supported Android input was found in the selected path.");
            }

            var candidates = FindAndroidInputs(root).ToList();
            if (candidates.Count == 0)
            {
                throw new InvalidOperationException("No supported Android input was found under: " + root);
            }

            if (candidates.Count > 1)
            {
                throw new InvalidOperationException(
                    "Multiple Android inputs were found. Please select one of them directly:\r\n" +
                    string.Join("\r\n", candidates.Select(candidate => candidate.Path)));
            }

            return candidates[0];
        }

        private static AndroidInput ResolveSourceFile(string path)
        {
            var extension = Path.GetExtension(path);
            if (string.Equals(extension, ".aar", StringComparison.OrdinalIgnoreCase))
            {
                return new AndroidInput { Path = path, Kind = AndroidInputKind.ExistingAarFile };
            }

            if (string.Equals(extension, ".jar", StringComparison.OrdinalIgnoreCase))
            {
                return new AndroidInput { Path = path, Kind = AndroidInputKind.JarFile };
            }

            if (string.Equals(extension, ".so", StringComparison.OrdinalIgnoreCase))
            {
                return new AndroidInput { Path = path, Kind = AndroidInputKind.NativeLibraryFile };
            }

            throw new InvalidOperationException("Unsupported Android source file type: " + path);
        }

        private AndroidInput TryCreateInputFromDirectory(string directory, bool selectedDirectly)
        {
            if (File.Exists(Path.Combine(directory, "AndroidManifest.xml")))
            {
                return new AndroidInput { Path = directory, Kind = AndroidInputKind.AndroidLibraryDirectory };
            }

            var aarFiles = Directory.GetFiles(directory, "*.aar", SearchOption.TopDirectoryOnly)
                .Where(file => !FileCopyUtility.IsUnityMetaPath(file))
                .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (aarFiles.Count == 1)
            {
                return new AndroidInput { Path = aarFiles[0], Kind = AndroidInputKind.ExistingAarFile };
            }

            if (aarFiles.Count > 1 && selectedDirectly)
            {
                throw new InvalidOperationException(
                    "Multiple AAR files were found. Please select one AAR file directly:\r\n" +
                    string.Join("\r\n", aarFiles));
            }

            if (FindJarFiles(directory).Any() ||
                FindAbiDirectories(directory).Any() ||
                ContainsOptionalAndroidContent(directory))
            {
                return new AndroidInput { Path = directory, Kind = AndroidInputKind.LooseContentDirectory };
            }

            return null;
        }

        public IEnumerable<string> FindAndroidLibraryRoots(string root)
        {
            return FindAndroidInputs(root)
                .Where(input => input.Kind == AndroidInputKind.AndroidLibraryDirectory)
                .Select(input => input.Path);
        }

        private IEnumerable<AndroidInput> FindAndroidInputs(string root)
        {
            if (!Directory.Exists(root))
            {
                return Enumerable.Empty<AndroidInput>();
            }

            var results = new List<AndroidInput>();
            var queue = new Queue<Tuple<string, int>>();
            queue.Enqueue(Tuple.Create(Path.GetFullPath(root), 0));

            while (queue.Count > 0)
            {
                var item = queue.Dequeue();
                var directory = item.Item1;
                var depth = item.Item2;

                var input = TryCreateInputFromDirectory(directory, false);
                if (input != null)
                {
                    results.Add(input);
                    continue;
                }

                if (depth >= 4)
                {
                    continue;
                }

                foreach (var child in Directory.GetDirectories(directory))
                {
                    var name = Path.GetFileName(child);
                    if (ShouldSkipDiscoveryDirectory(name))
                    {
                        continue;
                    }

                    queue.Enqueue(Tuple.Create(child, depth + 1));
                }
            }

            return results;
        }

        private static bool ShouldSkipDiscoveryDirectory(string name)
        {
            return string.Equals(name, ".git", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(name, ".svn", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(name, "Library", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(name, "Temp", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(name, "obj", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(name, "bin", StringComparison.OrdinalIgnoreCase);
        }

        private void ValidateOptions(AndroidAarOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            IdentifierValidator.ValidateRequired("Output repository directory", options.OutputRepositoryDirectory);
            IdentifierValidator.ValidateMavenCoordinate(options.GroupId, options.ArtifactId, options.Version);
        }

        private static void ValidatePackageOptions(AndroidAarPackageOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            IdentifierValidator.ValidateRequired("Android source path", options.SourcePath);
            IdentifierValidator.ValidateRequired("Output directory", options.OutputDirectory);
            IdentifierValidator.ValidateMavenCoordinate("local", options.ArtifactId, options.Version);
        }

        private static string GetInputKindLabel(AndroidInputKind kind)
        {
            switch (kind)
            {
                case AndroidInputKind.AndroidLibraryDirectory:
                    return "Android library directory";
                case AndroidInputKind.ExistingAarFile:
                    return "AAR file";
                case AndroidInputKind.LooseContentDirectory:
                    return "loose Android content directory";
                case AndroidInputKind.JarFile:
                    return "JAR file";
                case AndroidInputKind.NativeLibraryFile:
                    return "native .so file";
                default:
                    return kind.ToString();
            }
        }

        private void CopyAarContents(AndroidInput source, string aarRoot)
        {
            switch (source.Kind)
            {
                case AndroidInputKind.AndroidLibraryDirectory:
                case AndroidInputKind.LooseContentDirectory:
                    CopyDirectoryAarContents(source.Path, aarRoot);
                    return;
                case AndroidInputKind.JarFile:
                    CopyJarFileAarContents(source.Path, aarRoot);
                    return;
                case AndroidInputKind.NativeLibraryFile:
                    CopyNativeFileAarContents(source.Path, aarRoot);
                    return;
                default:
                    throw new InvalidOperationException("Unsupported Android input type: " + source.Kind);
            }
        }

        private void CopyDirectoryAarContents(string sourceDirectory, string aarRoot)
        {
            CopyManifestOrCreate(sourceDirectory, aarRoot);
            CopyJavaBytecode(sourceDirectory, aarRoot);
            CopyNativeLibraries(sourceDirectory, aarRoot);
            CopyOptionalDirectory(sourceDirectory, aarRoot, "res");
            CopyOptionalDirectory(sourceDirectory, aarRoot, "assets");
            CopyOptionalDirectory(sourceDirectory, aarRoot, "aidl");
            CopyOptionalProguard(sourceDirectory, aarRoot);
            EnsureRText(aarRoot);
        }

        private void CopyJarFileAarContents(string jarPath, string aarRoot)
        {
            CreateMinimalManifest(aarRoot);
            File.Copy(jarPath, Path.Combine(aarRoot, "classes.jar"), true);
            _log("Using jar as classes.jar: " + jarPath);
            EnsureRText(aarRoot);
        }

        private void CopyNativeFileAarContents(string soPath, string aarRoot)
        {
            CreateMinimalManifest(aarRoot);
            CreateEmptyJar(Path.Combine(aarRoot, "classes.jar"));
            CopySingleNativeLibrary(soPath, aarRoot);
            EnsureRText(aarRoot);
        }

        private void CopyManifestOrCreate(string sourceDirectory, string aarRoot)
        {
            var manifest = Path.Combine(sourceDirectory, "AndroidManifest.xml");
            if (File.Exists(manifest))
            {
                File.Copy(manifest, Path.Combine(aarRoot, "AndroidManifest.xml"), true);
                return;
            }

            _log("No AndroidManifest.xml found. Creating minimal manifest.");
            CreateMinimalManifest(aarRoot);
        }

        private static void CreateMinimalManifest(string aarRoot)
        {
            var manifest = "<manifest xmlns:android=\"http://schemas.android.com/apk/res/android\" />\r\n";
            File.WriteAllText(Path.Combine(aarRoot, "AndroidManifest.xml"), manifest, new UTF8Encoding(false));
        }

        private void CopyJavaBytecode(string sourceDirectory, string aarRoot)
        {
            var jarFiles = FindJarFiles(sourceDirectory).ToList();
            var classesJar = Path.Combine(aarRoot, "classes.jar");

            if (jarFiles.Count == 0)
            {
                _log("No jar found. Creating empty classes.jar.");
                CreateEmptyJar(classesJar);
                return;
            }

            if (jarFiles.Count == 1)
            {
                _log("Using jar as classes.jar: " + jarFiles[0]);
                File.Copy(jarFiles[0], classesJar, true);
                return;
            }

            var existingClassesJar = jarFiles.FirstOrDefault(file =>
                string.Equals(Path.GetFileName(file), "classes.jar", StringComparison.OrdinalIgnoreCase));

            if (existingClassesJar != null)
            {
                File.Copy(existingClassesJar, classesJar, true);
                jarFiles.Remove(existingClassesJar);
            }
            else
            {
                CreateEmptyJar(classesJar);
            }

            var libsDirectory = Path.Combine(aarRoot, "libs");
            FileCopyUtility.EnsureDirectory(libsDirectory);
            foreach (var jar in jarFiles)
            {
                _log("Embedding local jar: " + jar);
                File.Copy(jar, Path.Combine(libsDirectory, Path.GetFileName(jar)), true);
            }
        }

        private static IEnumerable<string> FindJarFiles(string sourceDirectory)
        {
            var files = new List<string>();
            files.AddRange(FileCopyUtility.EnumerateFilesSafe(Path.Combine(sourceDirectory, "libs"), "*.jar", SearchOption.TopDirectoryOnly));
            files.AddRange(FileCopyUtility.EnumerateFilesSafe(sourceDirectory, "*.jar", SearchOption.TopDirectoryOnly));

            return files
                .Where(file => !FileCopyUtility.IsUnityMetaPath(file))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(file => file, StringComparer.OrdinalIgnoreCase);
        }

        private void CopyNativeLibraries(string sourceDirectory, string aarRoot)
        {
            var jniRoot = Path.Combine(aarRoot, "jni");
            var abiDirectories = FindAbiDirectories(sourceDirectory).ToList();
            if (abiDirectories.Count == 0)
            {
                _log("No native .so libraries found.");
                return;
            }

            foreach (var abiDirectory in abiDirectories)
            {
                var abiName = Path.GetFileName(abiDirectory);
                var targetAbiDirectory = Path.Combine(jniRoot, abiName);
                FileCopyUtility.EnsureDirectory(targetAbiDirectory);

                foreach (var soFile in Directory.GetFiles(abiDirectory, "*.so", SearchOption.TopDirectoryOnly))
                {
                    var target = Path.Combine(targetAbiDirectory, Path.GetFileName(soFile));
                    if (File.Exists(target))
                    {
                        if (FileCopyUtility.AreFilesEqual(soFile, target))
                        {
                            continue;
                        }

                        throw new InvalidOperationException("Duplicate native library with different content: " + target);
                    }

                    _log("Embedding native lib: " + abiName + "/" + Path.GetFileName(soFile));
                    File.Copy(soFile, target, true);
                }
            }
        }

        private void CopySingleNativeLibrary(string soPath, string aarRoot)
        {
            var abiName = InferAbiNameFromPath(soPath);
            if (string.IsNullOrWhiteSpace(abiName))
            {
                throw new InvalidOperationException(
                    "Cannot infer ABI for native library. Place the .so under an ABI directory such as arm64-v8a, armeabi-v7a, x86, or x86_64: " +
                    soPath);
            }

            var targetAbiDirectory = Path.Combine(aarRoot, "jni", abiName);
            FileCopyUtility.EnsureDirectory(targetAbiDirectory);
            File.Copy(soPath, Path.Combine(targetAbiDirectory, Path.GetFileName(soPath)), true);
            _log("Embedding native lib: " + abiName + "/" + Path.GetFileName(soPath));
        }

        private static IEnumerable<string> FindAbiDirectories(string sourceDirectory)
        {
            var directories = new List<string>();
            if (AbiNameSet.Contains(Path.GetFileName(sourceDirectory)) &&
                Directory.GetFiles(sourceDirectory, "*.so", SearchOption.TopDirectoryOnly).Length > 0)
            {
                directories.Add(sourceDirectory);
            }

            directories.AddRange(Directory
                .GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories)
                .Where(directory => AbiNameSet.Contains(Path.GetFileName(directory)))
                .Where(directory => Directory.GetFiles(directory, "*.so", SearchOption.TopDirectoryOnly).Length > 0));

            return directories
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(directory => directory, StringComparer.OrdinalIgnoreCase);
        }

        private static string InferAbiNameFromPath(string soPath)
        {
            var directory = Path.GetDirectoryName(soPath);
            if (string.IsNullOrWhiteSpace(directory))
            {
                return string.Empty;
            }

            var name = Path.GetFileName(directory);
            return AbiNameSet.Contains(name) ? name : string.Empty;
        }

        private static bool ContainsOptionalAndroidContent(string sourceDirectory)
        {
            return Directory.Exists(Path.Combine(sourceDirectory, "res")) ||
                   Directory.Exists(Path.Combine(sourceDirectory, "assets")) ||
                   Directory.Exists(Path.Combine(sourceDirectory, "aidl")) ||
                   File.Exists(Path.Combine(sourceDirectory, "proguard.txt")) ||
                   File.Exists(Path.Combine(sourceDirectory, "consumer-proguard-rules.pro")) ||
                   File.Exists(Path.Combine(sourceDirectory, "proguard-project.txt"));
        }

        private static void CopyOptionalDirectory(string sourceDirectory, string aarRoot, string name)
        {
            var source = Path.Combine(sourceDirectory, name);
            var target = Path.Combine(aarRoot, name);
            FileCopyUtility.CopyDirectory(source, target, FileCopyUtility.IsUnityMetaPath);
        }

        private static void CopyOptionalProguard(string sourceDirectory, string aarRoot)
        {
            var candidates = new[]
            {
                "proguard.txt",
                "consumer-proguard-rules.pro",
                "proguard-project.txt"
            };

            foreach (var candidate in candidates)
            {
                var path = Path.Combine(sourceDirectory, candidate);
                if (File.Exists(path))
                {
                    File.Copy(path, Path.Combine(aarRoot, "proguard.txt"), true);
                    return;
                }
            }
        }

        private static void EnsureRText(string aarRoot)
        {
            var rText = Path.Combine(aarRoot, "R.txt");
            if (!File.Exists(rText))
            {
                File.WriteAllText(rText, string.Empty, Encoding.ASCII);
            }
        }

        private static void CreateEmptyJar(string classesJar)
        {
            if (File.Exists(classesJar))
            {
                File.Delete(classesJar);
            }

            using (var stream = File.Create(classesJar))
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                var entry = archive.CreateEntry("META-INF/MANIFEST.MF", CompressionLevel.Optimal);
                using (var writer = new StreamWriter(entry.Open(), Encoding.ASCII))
                {
                    writer.Write("Manifest-Version: 1.0\r\n");
                }
            }
        }

        private static void CreateAarArchive(string aarRoot, string aarPath)
        {
            if (File.Exists(aarPath))
            {
                File.Delete(aarPath);
            }

            var tempZip = aarPath + ".ziptmp";
            if (File.Exists(tempZip))
            {
                File.Delete(tempZip);
            }

            using (var stream = File.Create(tempZip))
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                foreach (var file in Directory.GetFiles(aarRoot, "*", SearchOption.AllDirectories))
                {
                    var relative = FileCopyUtility.ToUnixPath(FileCopyUtility.GetRelativePath(aarRoot, file));
                    archive.CreateEntryFromFile(file, relative, CompressionLevel.Optimal);
                }
            }

            File.Move(tempZip, aarPath);
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

        private static void WritePom(AndroidAarOptions options, string pomPath)
        {
            var pom = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                      "<project xmlns=\"http://maven.apache.org/POM/4.0.0\"\r\n" +
                      "         xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\r\n" +
                      "         xsi:schemaLocation=\"http://maven.apache.org/POM/4.0.0 https://maven.apache.org/xsd/maven-4.0.0.xsd\">\r\n" +
                      "  <modelVersion>4.0.0</modelVersion>\r\n" +
                      "  <groupId>" + FileCopyUtility.XmlEscape(options.GroupId.Trim()) + "</groupId>\r\n" +
                      "  <artifactId>" + FileCopyUtility.XmlEscape(options.ArtifactId.Trim()) + "</artifactId>\r\n" +
                      "  <version>" + FileCopyUtility.XmlEscape(options.Version.Trim()) + "</version>\r\n" +
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
