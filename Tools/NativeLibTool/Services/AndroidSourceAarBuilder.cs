using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NativeLibTool.Models;
using NativeLibTool.Utilities;

namespace NativeLibTool.Services
{
    internal sealed class AndroidSourceAarBuilder
    {
        private const string GeneratedProjectMarkerFileName = ".nativelibtool-generated";
        private static readonly string[] JavaPatterns = { "*.java" };
        private static readonly string[] NativeSourcePatterns = { "*.c", "*.cc", "*.cpp", "*.cxx" };
        private static readonly string[] NativeHeaderPatterns = { "*.h", "*.hh", "*.hpp" };
        private static readonly HashSet<string> AbiNameSet = new HashSet<string>(new[]
        {
            "armeabi",
            "armeabi-v7a",
            "arm64-v8a",
            "x86",
            "x86_64",
            "mips",
            "mips64"
        }, StringComparer.OrdinalIgnoreCase);
        private static readonly Regex JavaPackageRegex = new Regex(@"^\s*package\s+([A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)*)\s*;", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex ManifestPackageRegex = new Regex(@"package\s*=\s*""([^""]+)""", RegexOptions.Compiled);
        private static readonly Regex NamespaceRegex = new Regex(@"^[A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)+$", RegexOptions.Compiled);

        private readonly Action<string> _log;

        private sealed class GeneratedProjectInfo
        {
            public string ResolvedSourceDirectory { get; set; }
            public string ProjectDirectory { get; set; }
            public string ModuleDirectory { get; set; }
            public bool HasNativeSources { get; set; }
            public int JavaSourceCount { get; set; }
            public int NativeSourceCount { get; set; }
            public int KotlinSourceCount { get; set; }
            public bool HasLocalJars { get; set; }
            public string UnityClassesJarPath { get; set; }
            public string UnityGradleLauncherPath { get; set; }
            public string GradleWrapperBatPath { get; set; }
            public string GradleWrapperJarPath { get; set; }
            public bool HasUnityClassesJar
            {
                get { return !string.IsNullOrWhiteSpace(UnityClassesJarPath); }
            }

            public bool HasUnityGradleWrapper
            {
                get
                {
                    return !string.IsNullOrWhiteSpace(GradleWrapperBatPath) &&
                           !string.IsNullOrWhiteSpace(GradleWrapperJarPath);
                }
            }

            public bool HasUnityGradleLauncher
            {
                get { return !string.IsNullOrWhiteSpace(UnityGradleLauncherPath); }
            }
        }

        public AndroidSourceAarBuilder(Action<string> log)
        {
            _log = log ?? delegate { };
        }

        public string ResolveSourceDirectory(string selectedDirectory, bool autoDetect)
        {
            return ResolveSourceRoot(selectedDirectory, autoDetect);
        }

        public BuildResult Build(AndroidSourceAarOptions options)
        {
            ValidateOptions(options);

            var sourceRoot = ResolveSourceRoot(options.SourceDirectory, options.AutoDetectSourceRoot);
            var artifactId = options.ArtifactId.Trim();
            var version = options.Version.Trim();
            var outputDirectory = Path.GetFullPath(options.OutputDirectory);
            var namespaceName = ResolveNamespace(options.Namespace, artifactId, sourceRoot);
            var hasConfiguredProjectDirectory = !string.IsNullOrWhiteSpace(options.GradleProjectDirectory);
            var projectDirectory = hasConfiguredProjectDirectory
                ? Path.GetFullPath(options.GradleProjectDirectory.Trim().Trim('"'))
                : Path.Combine(Path.GetTempPath(), "NativeLibTool-Gradle-" + Guid.NewGuid().ToString("N"));

            _log("Android source root: " + sourceRoot);
            _log("Gradle project: " + projectDirectory);
            _log("AAR output directory: " + outputDirectory);

            try
            {
                PrepareProjectDirectory(projectDirectory, hasConfiguredProjectDirectory);
                FileCopyUtility.EnsureDirectory(outputDirectory);

                var projectInfo = GenerateProject(sourceRoot, projectDirectory, options, artifactId, namespaceName);
                RunGradle(projectInfo.ProjectDirectory, ResolveGradleCommand(projectInfo, options.GradleCommand));
                var builtAar = FindBuiltAar(projectInfo.ModuleDirectory);
                var outputAar = Path.Combine(outputDirectory, artifactId + "-" + version + ".aar");
                File.Copy(builtAar, outputAar, true);

                _log("Built AAR: " + builtAar);
                _log("Copied AAR: " + outputAar);

                return new BuildResult
                {
                    PrimaryArtifactPath = outputAar,
                    MetadataPath = options.KeepGradleProject || hasConfiguredProjectDirectory ? projectDirectory : string.Empty,
                    ResolvedSourceDirectory = sourceRoot,
                    RepositorySnippet = "AAR output:\r\n" + outputAar + "\r\n\r\n" +
                                        "Next step:\r\nUse the Android AAR -> Local Maven tab to publish this AAR."
                };
            }
            finally
            {
                if (!options.KeepGradleProject && !hasConfiguredProjectDirectory && Directory.Exists(projectDirectory))
                {
                    Directory.Delete(projectDirectory, true);
                }
            }
        }

        public BuildResult GenerateGradleProject(AndroidSourceAarOptions options)
        {
            ValidateProjectOptions(options);

            var sourceRoot = ResolveSourceRoot(options.SourceDirectory, options.AutoDetectSourceRoot);
            var artifactId = options.ArtifactId.Trim();
            var namespaceName = ResolveNamespace(options.Namespace, artifactId, sourceRoot);
            var projectDirectory = Path.GetFullPath(options.GradleProjectDirectory.Trim().Trim('"'));

            _log("Android source root: " + sourceRoot);
            _log("Gradle project: " + projectDirectory);

            PrepareProjectDirectory(projectDirectory, true);
            var projectInfo = GenerateProject(sourceRoot, projectDirectory, options, artifactId, namespaceName);

            return new BuildResult
            {
                PrimaryArtifactPath = projectDirectory,
                MetadataPath = projectDirectory,
                ResolvedSourceDirectory = sourceRoot,
                RepositorySnippet = "Gradle project:\r\n" + projectDirectory + "\r\n\r\n" +
                                    "Build command:\r\n" + GetManualBuildCommand(projectInfo)
            };
        }

        private GeneratedProjectInfo GenerateProject(string sourceRoot, string projectDirectory, AndroidSourceAarOptions options, string artifactId, string namespaceName)
        {
            var moduleDirectory = Path.Combine(projectDirectory, "lib");
            var srcMain = Path.Combine(moduleDirectory, "src", "main");
            FileCopyUtility.EnsureDirectory(srcMain);

            var info = new GeneratedProjectInfo
            {
                ResolvedSourceDirectory = sourceRoot,
                ProjectDirectory = projectDirectory,
                ModuleDirectory = moduleDirectory,
                UnityClassesJarPath = ResolveUnityClassesJar(options.UnityDataDirectory)
            };

            CopyManifest(sourceRoot, srcMain);
            CopyJavaSources(sourceRoot, srcMain, info);
            CopyAndroidContent(sourceRoot, srcMain, moduleDirectory, info);
            CopyNativeSources(sourceRoot, srcMain, info, artifactId);
            WriteGradleFiles(projectDirectory, moduleDirectory, options, namespaceName, info);
            CopyUnityGradleArtifacts(projectDirectory, options.UnityDataDirectory, info);
            WriteGeneratedProjectMarker(projectDirectory);

            _log("Java source files: " + info.JavaSourceCount);
            if (info.KotlinSourceCount > 0)
            {
                _log("Kotlin source files found but not compiled by this tab: " + info.KotlinSourceCount);
            }

            _log("Native source files: " + info.NativeSourceCount);
            if (info.HasLocalJars)
            {
                _log("Local jars copied into the temporary Gradle module.");
            }

            if (info.HasUnityClassesJar)
            {
                _log("Unity classes.jar: " + info.UnityClassesJarPath);
            }

            if (info.HasUnityGradleWrapper)
            {
                _log("Unity Gradle wrapper copied: " + info.GradleWrapperBatPath);
                _log("Unity Gradle wrapper jar copied: " + info.GradleWrapperJarPath);
            }

            if (info.HasUnityGradleLauncher)
            {
                _log("Unity Gradle launcher: " + info.UnityGradleLauncherPath);
            }

            return info;
        }

        private static void CopyManifest(string sourceRoot, string srcMain)
        {
            var sourceManifest = FirstExistingFile(
                Path.Combine(sourceRoot, "src", "main", "AndroidManifest.xml"),
                Path.Combine(sourceRoot, "AndroidManifest.xml"));
            var targetManifest = Path.Combine(srcMain, "AndroidManifest.xml");
            if (!string.IsNullOrWhiteSpace(sourceManifest))
            {
                File.Copy(sourceManifest, targetManifest, true);
                return;
            }

            var manifest = "<manifest xmlns:android=\"http://schemas.android.com/apk/res/android\" />\r\n";
            File.WriteAllText(targetManifest, manifest, new UTF8Encoding(false));
        }

        private static void CopyJavaSources(string sourceRoot, string srcMain, GeneratedProjectInfo info)
        {
            var targetJava = Path.Combine(srcMain, "java");
            var standardJava = Path.Combine(sourceRoot, "src", "main", "java");
            var looseJava = Path.Combine(sourceRoot, "java");
            var oldUnitySrc = Path.Combine(sourceRoot, "src");

            if (Directory.Exists(standardJava))
            {
                FileCopyUtility.CopyDirectory(standardJava, targetJava, ShouldSkipCopyPath);
            }
            else if (Directory.Exists(looseJava))
            {
                FileCopyUtility.CopyDirectory(looseJava, targetJava, ShouldSkipCopyPath);
            }
            else if (Directory.Exists(oldUnitySrc) && !Directory.Exists(Path.Combine(oldUnitySrc, "main")))
            {
                CopyMatchingFiles(oldUnitySrc, targetJava, JavaPatterns, ShouldSkipDiscoveryPath);
            }
            else
            {
                CopyMatchingFiles(sourceRoot, targetJava, JavaPatterns, ShouldSkipDiscoveryPath);
            }

            info.JavaSourceCount = CountMatchingFiles(targetJava, JavaPatterns);
            info.KotlinSourceCount = CountMatchingFiles(sourceRoot, new[] { "*.kt" });
        }

        private static void CopyAndroidContent(string sourceRoot, string srcMain, string moduleDirectory, GeneratedProjectInfo info)
        {
            CopyFirstExistingDirectory(srcMain, "res",
                Path.Combine(sourceRoot, "src", "main", "res"),
                Path.Combine(sourceRoot, "res"));
            CopyFirstExistingDirectory(srcMain, "assets",
                Path.Combine(sourceRoot, "src", "main", "assets"),
                Path.Combine(sourceRoot, "assets"));
            CopyFirstExistingDirectory(srcMain, "aidl",
                Path.Combine(sourceRoot, "src", "main", "aidl"),
                Path.Combine(sourceRoot, "aidl"));
            CopyFirstExistingDirectory(srcMain, "jniLibs",
                Path.Combine(sourceRoot, "src", "main", "jniLibs"),
                Path.Combine(sourceRoot, "jniLibs"));
            CopyLegacyNativeLibraries(sourceRoot, srcMain);

            var sourceLibs = FirstExistingDirectory(
                Path.Combine(sourceRoot, "libs"),
                Path.Combine(sourceRoot, "src", "main", "libs"));
            if (!string.IsNullOrWhiteSpace(sourceLibs))
            {
                var jarFiles = Directory.GetFiles(sourceLibs, "*.jar", SearchOption.TopDirectoryOnly)
                    .Where(file => !FileCopyUtility.IsUnityMetaPath(file))
                    .ToList();
                if (jarFiles.Count > 0)
                {
                    var targetLibs = Path.Combine(moduleDirectory, "libs");
                    FileCopyUtility.EnsureDirectory(targetLibs);
                    foreach (var jar in jarFiles)
                    {
                        File.Copy(jar, Path.Combine(targetLibs, Path.GetFileName(jar)), true);
                    }

                    info.HasLocalJars = true;
                }
            }
        }

        private static void CopyLegacyNativeLibraries(string sourceRoot, string srcMain)
        {
            var sourceLibs = Path.Combine(sourceRoot, "libs");
            if (!Directory.Exists(sourceLibs))
            {
                return;
            }

            var targetJniLibs = Path.Combine(srcMain, "jniLibs");
            foreach (var abiDirectory in Directory.GetDirectories(sourceLibs))
            {
                var abiName = Path.GetFileName(abiDirectory);
                if (!AbiNameSet.Contains(abiName) ||
                    Directory.GetFiles(abiDirectory, "*.so", SearchOption.TopDirectoryOnly).Length == 0)
                {
                    continue;
                }

                FileCopyUtility.CopyDirectory(abiDirectory, Path.Combine(targetJniLibs, abiName), ShouldSkipCopyPath);
            }
        }

        private static void CopyNativeSources(string sourceRoot, string srcMain, GeneratedProjectInfo info, string artifactId)
        {
            var targetCpp = Path.Combine(srcMain, "cpp");
            var standardCpp = Path.Combine(sourceRoot, "src", "main", "cpp");
            var looseCpp = Path.Combine(sourceRoot, "cpp");

            if (Directory.Exists(standardCpp))
            {
                FileCopyUtility.CopyDirectory(standardCpp, targetCpp, ShouldSkipCopyPath);
            }
            else if (Directory.Exists(looseCpp))
            {
                FileCopyUtility.CopyDirectory(looseCpp, targetCpp, ShouldSkipCopyPath);
            }
            else
            {
                CopyMatchingFiles(sourceRoot, targetCpp, NativeSourcePatterns.Concat(NativeHeaderPatterns).ToArray(), ShouldSkipDiscoveryPath);
            }

            info.NativeSourceCount = CountMatchingFiles(targetCpp, NativeSourcePatterns);
            info.HasNativeSources = info.NativeSourceCount > 0;
            if (info.HasNativeSources && !File.Exists(Path.Combine(targetCpp, "CMakeLists.txt")))
            {
                WriteGeneratedCMake(targetCpp, artifactId);
            }
        }

        private static void WriteGradleFiles(string projectDirectory, string moduleDirectory, AndroidSourceAarOptions options, string namespaceName, GeneratedProjectInfo info)
        {
            var settings = "pluginManagement {\r\n" +
                           "    repositories {\r\n" +
                           "        google()\r\n" +
                           "        mavenCentral()\r\n" +
                           "        gradlePluginPortal()\r\n" +
                           "    }\r\n" +
                           "}\r\n" +
                           "dependencyResolutionManagement {\r\n" +
                           "    repositoriesMode.set(RepositoriesMode.FAIL_ON_PROJECT_REPOS)\r\n" +
                           "    repositories {\r\n" +
                           "        google()\r\n" +
                           "        mavenCentral()\r\n" +
                           "    }\r\n" +
                           "}\r\n" +
                           "rootProject.name = 'NativeLibToolSourceAar'\r\n" +
                           "include ':lib'\r\n";
            File.WriteAllText(Path.Combine(projectDirectory, "settings.gradle"), settings, new UTF8Encoding(false));

            var rootBuild = "plugins {\r\n" +
                            "    id 'com.android.library' version '" + EscapeGradleSingleQuoted(options.AndroidGradlePluginVersion.Trim()) + "' apply false\r\n" +
                            "}\r\n";
            File.WriteAllText(Path.Combine(projectDirectory, "build.gradle"), rootBuild, new UTF8Encoding(false));

            var moduleBuild = new StringBuilder();
            moduleBuild.AppendLine("plugins {");
            moduleBuild.AppendLine("    id 'com.android.library'");
            moduleBuild.AppendLine("}");
            moduleBuild.AppendLine();
            moduleBuild.AppendLine("android {");
            moduleBuild.AppendLine("    namespace '" + EscapeGradleSingleQuoted(namespaceName) + "'");
            moduleBuild.AppendLine("    compileSdk " + options.CompileSdk.Trim());
            moduleBuild.AppendLine();
            moduleBuild.AppendLine("    defaultConfig {");
            moduleBuild.AppendLine("        minSdk " + options.MinSdk.Trim());
            if (info.HasNativeSources)
            {
                moduleBuild.AppendLine("        externalNativeBuild {");
                moduleBuild.AppendLine("            cmake {");
                moduleBuild.AppendLine("            }");
                moduleBuild.AppendLine("        }");
            }

            moduleBuild.AppendLine("    }");
            moduleBuild.AppendLine();
            moduleBuild.AppendLine("    compileOptions {");
            moduleBuild.AppendLine("        sourceCompatibility JavaVersion.VERSION_1_8");
            moduleBuild.AppendLine("        targetCompatibility JavaVersion.VERSION_1_8");
            moduleBuild.AppendLine("    }");
            if (info.HasNativeSources)
            {
                moduleBuild.AppendLine();
                moduleBuild.AppendLine("    externalNativeBuild {");
                moduleBuild.AppendLine("        cmake {");
                moduleBuild.AppendLine("            path file('src/main/cpp/CMakeLists.txt')");
                moduleBuild.AppendLine("        }");
                moduleBuild.AppendLine("    }");
            }

            moduleBuild.AppendLine("}");
            if (info.HasUnityClassesJar || info.HasLocalJars)
            {
                moduleBuild.AppendLine();
                moduleBuild.AppendLine("dependencies {");
                if (info.HasUnityClassesJar)
                {
                    moduleBuild.AppendLine("    compileOnly files('" + EscapeGradleSingleQuoted(FileCopyUtility.ToUnixPath(info.UnityClassesJarPath)) + "')");
                }

                if (info.HasLocalJars)
                {
                    moduleBuild.AppendLine("    implementation fileTree(dir: 'libs', include: ['*.jar'])");
                }

                moduleBuild.AppendLine("}");
            }

            File.WriteAllText(Path.Combine(moduleDirectory, "build.gradle"), moduleBuild.ToString(), new UTF8Encoding(false));
        }

        private void CopyUnityGradleArtifacts(string projectDirectory, string unityDataDirectory, GeneratedProjectInfo info)
        {
            if (string.IsNullOrWhiteSpace(unityDataDirectory))
            {
                _log("Unity root/Data is empty. Gradle wrapper files were not copied.");
                return;
            }

            var toolDirectories = ResolveUnityAndroidToolDirectories(unityDataDirectory).ToList();
            if (toolDirectories.Count == 0)
            {
                _log("Unity Android tools directory was not found. Gradle wrapper files were not copied.");
                return;
            }

            var wrapperSourceDirectory = toolDirectories
                .Select(directory => Path.Combine(directory, "VisualStudioGradleTemplates"))
                .FirstOrDefault(directory =>
                    File.Exists(Path.Combine(directory, "gradlew.bat")) &&
                    File.Exists(Path.Combine(directory, "gradle-wrapper.jar")));

            if (!string.IsNullOrWhiteSpace(wrapperSourceDirectory))
            {
                var targetWrapperDirectory = Path.Combine(projectDirectory, "gradle", "wrapper");
                FileCopyUtility.EnsureDirectory(targetWrapperDirectory);

                var sourceBat = Path.Combine(wrapperSourceDirectory, "gradlew.bat");
                var sourceJar = Path.Combine(wrapperSourceDirectory, "gradle-wrapper.jar");
                var targetBat = Path.Combine(projectDirectory, "gradlew.bat");
                var targetJar = Path.Combine(targetWrapperDirectory, "gradle-wrapper.jar");
                File.Copy(sourceBat, targetBat, true);
                File.Copy(sourceJar, targetJar, true);

                var sourceProperties = Path.Combine(wrapperSourceDirectory, "gradle-wrapper.properties");
                if (File.Exists(sourceProperties))
                {
                    File.Copy(sourceProperties, Path.Combine(targetWrapperDirectory, "gradle-wrapper.properties"), true);
                }

                info.GradleWrapperBatPath = targetBat;
                info.GradleWrapperJarPath = targetJar;
            }
            else
            {
                _log("Unity gradlew.bat or gradle-wrapper.jar was not found.");
            }

            var launcher = ResolveUnityGradleLauncher(toolDirectories);
            if (!string.IsNullOrWhiteSpace(launcher))
            {
                WriteUnityGradleBat(projectDirectory, launcher);
                info.UnityGradleLauncherPath = launcher;
            }
            else
            {
                _log("Unity embedded Gradle launcher was not found.");
            }
        }

        private static void WriteUnityGradleBat(string projectDirectory, string gradleLauncherPath)
        {
            var bat = "@echo off\r\n" +
                      "setlocal\r\n" +
                      "set \"UNITY_GRADLE_LAUNCHER=" + gradleLauncherPath + "\"\r\n" +
                      "if defined JAVA_HOME (\r\n" +
                      "    set \"JAVA_EXE=%JAVA_HOME%\\bin\\java.exe\"\r\n" +
                      ") else (\r\n" +
                      "    set \"JAVA_EXE=java.exe\"\r\n" +
                      ")\r\n" +
                      "\"%JAVA_EXE%\" -classpath \"%UNITY_GRADLE_LAUNCHER%\" org.gradle.launcher.GradleMain %*\r\n" +
                      "exit /b %ERRORLEVEL%\r\n";
            File.WriteAllText(Path.Combine(projectDirectory, "gradle.bat"), bat, Encoding.ASCII);
        }

        private static void WriteGeneratedProjectMarker(string projectDirectory)
        {
            File.WriteAllText(
                Path.Combine(projectDirectory, GeneratedProjectMarkerFileName),
                "Generated by NativeLibTool. This directory can be cleaned and regenerated by the tool.\r\n",
                Encoding.ASCII);
        }

        private static void WriteGeneratedCMake(string targetCpp, string artifactId)
        {
            var libraryName = SanitizeCMakeLibraryName(artifactId);
            var cmake = "cmake_minimum_required(VERSION 3.10.2)\r\n" +
                        "project(" + libraryName + ")\r\n" +
                        "\r\n" +
                        "file(GLOB_RECURSE NATIVE_SOURCES CONFIGURE_DEPENDS\r\n" +
                        "    \"*.c\"\r\n" +
                        "    \"*.cc\"\r\n" +
                        "    \"*.cpp\"\r\n" +
                        "    \"*.cxx\"\r\n" +
                        ")\r\n" +
                        "\r\n" +
                        "add_library(" + libraryName + " SHARED ${NATIVE_SOURCES})\r\n" +
                        "target_include_directories(" + libraryName + " PRIVATE ${CMAKE_CURRENT_SOURCE_DIR})\r\n" +
                        "find_library(log-lib log)\r\n" +
                        "target_link_libraries(" + libraryName + " ${log-lib})\r\n";
            File.WriteAllText(Path.Combine(targetCpp, "CMakeLists.txt"), cmake, new UTF8Encoding(false));
        }

        private void RunGradle(string projectDirectory, string gradleCommand)
        {
            var startInfo = CreateGradleProcessInfo(projectDirectory, gradleCommand);
            _log("Gradle command: " + startInfo.FileName + " " + startInfo.Arguments);

            using (var process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();
                var stdoutTask = process.StandardOutput.ReadToEndAsync();
                var stderrTask = process.StandardError.ReadToEndAsync();
                process.WaitForExit();
                Task.WaitAll(stdoutTask, stderrTask);

                LogProcessOutput(stdoutTask.Result);
                LogProcessOutput(stderrTask.Result);

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException("Gradle build failed with exit code " + process.ExitCode + ".");
                }
            }
        }

        private static string ResolveGradleCommand(GeneratedProjectInfo projectInfo, string configuredCommand)
        {
            var command = (configuredCommand ?? string.Empty).Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(command) || IsDefaultGradleCommand(command))
            {
                var localGradleBat = Path.Combine(projectInfo.ProjectDirectory, "gradle.bat");
                if (Environment.OSVersion.Platform == PlatformID.Win32NT && File.Exists(localGradleBat))
                {
                    return localGradleBat;
                }

                var localGradlewBat = Path.Combine(projectInfo.ProjectDirectory, "gradlew.bat");
                if (Environment.OSVersion.Platform == PlatformID.Win32NT && File.Exists(localGradlewBat))
                {
                    return localGradlewBat;
                }

                var localGradlew = Path.Combine(projectInfo.ProjectDirectory, "gradlew");
                if (Environment.OSVersion.Platform != PlatformID.Win32NT && File.Exists(localGradlew))
                {
                    return localGradlew;
                }
            }

            return string.IsNullOrWhiteSpace(command) ? "gradle" : command;
        }

        private static bool IsDefaultGradleCommand(string command)
        {
            return string.Equals(command, "gradle", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(command, "gradle.bat", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetManualBuildCommand(GeneratedProjectInfo projectInfo)
        {
            var command = ResolveGradleCommand(projectInfo, string.Empty);
            var displayCommand = Path.GetDirectoryName(command) == projectInfo.ProjectDirectory
                ? Path.GetFileName(command)
                : command;
            return displayCommand + " :lib:assembleRelease --stacktrace";
        }

        private static ProcessStartInfo CreateGradleProcessInfo(string projectDirectory, string gradleCommand)
        {
            var command = string.IsNullOrWhiteSpace(gradleCommand) ? "gradle" : gradleCommand.Trim().Trim('"');
            var gradleArguments = ":lib:assembleRelease --stacktrace";
            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = projectDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "/c " + QuoteForCmd(command) + " " + gradleArguments;
                return startInfo;
            }

            startInfo.FileName = command;
            startInfo.Arguments = gradleArguments;
            return startInfo;
        }

        private void LogProcessOutput(string output)
        {
            if (string.IsNullOrWhiteSpace(output))
            {
                return;
            }

            using (var reader = new StringReader(output))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        _log(line);
                    }
                }
            }
        }

        private static string FindBuiltAar(string moduleDirectory)
        {
            var outputDirectory = Path.Combine(moduleDirectory, "build", "outputs", "aar");
            if (!Directory.Exists(outputDirectory))
            {
                throw new DirectoryNotFoundException("Gradle AAR output directory was not created: " + outputDirectory);
            }

            var releaseAar = Directory.GetFiles(outputDirectory, "*-release.aar", SearchOption.TopDirectoryOnly)
                .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(releaseAar))
            {
                return releaseAar;
            }

            var anyAar = Directory.GetFiles(outputDirectory, "*.aar", SearchOption.TopDirectoryOnly)
                .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
            if (string.IsNullOrWhiteSpace(anyAar))
            {
                throw new FileNotFoundException("Gradle finished, but no AAR was found under: " + outputDirectory);
            }

            return anyAar;
        }

        private string ResolveSourceRoot(string selectedDirectory, bool autoDetect)
        {
            IdentifierValidator.ValidateRequired("Android source directory", selectedDirectory);

            var root = Path.GetFullPath(selectedDirectory.Trim().Trim('"'));
            if (!Directory.Exists(root))
            {
                throw new DirectoryNotFoundException("Android source directory does not exist: " + root);
            }

            if (IsAndroidSourceRoot(root))
            {
                return root;
            }

            if (!autoDetect)
            {
                throw new InvalidOperationException("No Java or native Android source was found in the selected directory.");
            }

            var candidates = FindAndroidSourceRoots(root).ToList();
            if (candidates.Count == 0)
            {
                throw new InvalidOperationException("No Java or native Android source was found under: " + root);
            }

            if (candidates.Count > 1)
            {
                throw new InvalidOperationException(
                    "Multiple Android source roots were found. Please select one of them directly:\r\n" +
                    string.Join("\r\n", candidates));
            }

            return candidates[0];
        }

        private static IEnumerable<string> FindAndroidSourceRoots(string root)
        {
            var results = new List<string>();
            var queue = new Queue<Tuple<string, int>>();
            queue.Enqueue(Tuple.Create(Path.GetFullPath(root), 0));

            while (queue.Count > 0)
            {
                var item = queue.Dequeue();
                var directory = item.Item1;
                var depth = item.Item2;

                if (IsAndroidSourceRoot(directory))
                {
                    results.Add(directory);
                    continue;
                }

                if (depth >= 4)
                {
                    continue;
                }

                foreach (var child in Directory.GetDirectories(directory))
                {
                    if (ShouldSkipDiscoveryName(Path.GetFileName(child)))
                    {
                        continue;
                    }

                    queue.Enqueue(Tuple.Create(child, depth + 1));
                }
            }

            return results;
        }

        private static bool IsAndroidSourceRoot(string directory)
        {
            if (File.Exists(Path.Combine(directory, "src", "main", "AndroidManifest.xml")) ||
                Directory.Exists(Path.Combine(directory, "src", "main", "java")) ||
                Directory.Exists(Path.Combine(directory, "src", "main", "cpp")) ||
                Directory.Exists(Path.Combine(directory, "java")) ||
                Directory.Exists(Path.Combine(directory, "cpp")))
            {
                return ContainsAnySource(directory) || ContainsAndroidContent(directory);
            }

            return ContainsAnySource(directory);
        }

        private static void PrepareProjectDirectory(string projectDirectory, bool isUserConfiguredDirectory)
        {
            if (!isUserConfiguredDirectory)
            {
                FileCopyUtility.EnsureCleanDirectory(projectDirectory);
                return;
            }

            ValidateGeneratedProjectDirectory(projectDirectory);

            if (!Directory.Exists(projectDirectory))
            {
                Directory.CreateDirectory(projectDirectory);
                return;
            }

            if (!Directory.EnumerateFileSystemEntries(projectDirectory).Any())
            {
                return;
            }

            var marker = Path.Combine(projectDirectory, GeneratedProjectMarkerFileName);
            if (!File.Exists(marker))
            {
                throw new InvalidOperationException(
                    "Gradle project folder already contains files and was not generated by NativeLibTool. Select an empty folder or a previously generated NativeLibTool folder: " +
                    projectDirectory);
            }

            Directory.Delete(projectDirectory, true);
            Directory.CreateDirectory(projectDirectory);
        }

        private static void ValidateGeneratedProjectDirectory(string projectDirectory)
        {
            var fullPath = Path.GetFullPath(projectDirectory);
            var root = Path.GetPathRoot(fullPath);
            if (string.IsNullOrWhiteSpace(root) ||
                string.Equals(
                    FileCopyUtility.AppendDirectorySeparator(fullPath),
                    FileCopyUtility.AppendDirectorySeparator(root),
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Gradle project folder cannot be a drive root: " + fullPath);
            }
        }

        private static bool ContainsAnySource(string directory)
        {
            return CountMatchingFiles(directory, JavaPatterns.Concat(NativeSourcePatterns).ToArray(), ShouldSkipDiscoveryPath) > 0;
        }

        private static bool ContainsAndroidContent(string directory)
        {
            return File.Exists(Path.Combine(directory, "AndroidManifest.xml")) ||
                   Directory.Exists(Path.Combine(directory, "res")) ||
                   Directory.Exists(Path.Combine(directory, "assets")) ||
                   Directory.Exists(Path.Combine(directory, "aidl")) ||
                   Directory.Exists(Path.Combine(directory, "jniLibs"));
        }

        private static void ValidateProjectOptions(AndroidSourceAarOptions options)
        {
            ValidateCommonOptions(options);
            IdentifierValidator.ValidateRequired("Gradle project folder", options.GradleProjectDirectory);
        }

        private static void ValidateOptions(AndroidSourceAarOptions options)
        {
            ValidateCommonOptions(options);
            IdentifierValidator.ValidateRequired("Output directory", options.OutputDirectory);
        }

        private static void ValidateCommonOptions(AndroidSourceAarOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            IdentifierValidator.ValidateRequired("Android source directory", options.SourceDirectory);
            IdentifierValidator.ValidateMavenCoordinate("local", options.ArtifactId, options.Version);
            IdentifierValidator.ValidateRequired("Android Gradle Plugin version", options.AndroidGradlePluginVersion);
            ValidatePositiveInt("Compile SDK", options.CompileSdk);
            ValidatePositiveInt("Min SDK", options.MinSdk);
        }

        private static string ResolveUnityClassesJar(string unityDataDirectory)
        {
            if (string.IsNullOrWhiteSpace(unityDataDirectory))
            {
                return string.Empty;
            }

            var path = Path.GetFullPath(unityDataDirectory.Trim().Trim('"'));
            if (File.Exists(path))
            {
                if (string.Equals(Path.GetFileName(path), "classes.jar", StringComparison.OrdinalIgnoreCase))
                {
                    return path;
                }

                throw new InvalidOperationException("Unity root/Data path points to a file, but it is not classes.jar: " + path);
            }

            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException("Unity root/Data directory does not exist: " + path);
            }

            var relative = Path.Combine("PlaybackEngines", "AndroidPlayer", "Variations", "il2cpp", "Release", "Classes", "classes.jar");
            var candidates = new[]
            {
                Path.Combine(path, relative),
                Path.Combine(path, "Data", relative),
                Path.Combine(path, "Data", "Data", relative),
                Path.Combine(path, "Editor", "Data", relative),
                Path.Combine(path, "Editor", "Data", "Data", relative)
            };

            var classesJar = candidates.FirstOrDefault(File.Exists);
            if (string.IsNullOrWhiteSpace(classesJar))
            {
                throw new FileNotFoundException(
                    "Unity Android classes.jar was not found under Unity root/Data directory. Expected il2cpp/Release/Classes/classes.jar under: " +
                    path);
            }

            return classesJar;
        }

        private static IEnumerable<string> ResolveUnityAndroidToolDirectories(string unityDataDirectory)
        {
            if (string.IsNullOrWhiteSpace(unityDataDirectory))
            {
                return Enumerable.Empty<string>();
            }

            var path = Path.GetFullPath(unityDataDirectory.Trim().Trim('"'));
            if (File.Exists(path) || !Directory.Exists(path))
            {
                return Enumerable.Empty<string>();
            }

            var relative = Path.Combine("PlaybackEngines", "AndroidPlayer", "Tools");
            return new[]
                {
                    Path.Combine(path, relative),
                    Path.Combine(path, "Data", relative),
                    Path.Combine(path, "Data", "Data", relative),
                    Path.Combine(path, "Editor", "Data", relative),
                    Path.Combine(path, "Editor", "Data", "Data", relative)
                }
                .Where(Directory.Exists)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string ResolveUnityGradleLauncher(IEnumerable<string> androidToolDirectories)
        {
            foreach (var toolDirectory in androidToolDirectories)
            {
                var gradleLib = Path.Combine(toolDirectory, "gradle", "lib");
                if (!Directory.Exists(gradleLib))
                {
                    continue;
                }

                var launcher = Directory.GetFiles(gradleLib, "gradle-launcher-*.jar", SearchOption.TopDirectoryOnly)
                    .OrderByDescending(file => file, StringComparer.OrdinalIgnoreCase)
                    .FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(launcher))
                {
                    return launcher;
                }
            }

            return string.Empty;
        }

        private static void ValidatePositiveInt(string name, string value)
        {
            int parsed;
            if (!int.TryParse((value ?? string.Empty).Trim(), out parsed) || parsed <= 0)
            {
                throw new InvalidOperationException(name + " must be a positive integer.");
            }
        }

        private static string ResolveNamespace(string configuredNamespace, string artifactId, string sourceRoot)
        {
            var namespaceName = string.IsNullOrWhiteSpace(configuredNamespace)
                ? FirstNonBlank(ReadManifestPackage(sourceRoot), ReadFirstJavaPackage(sourceRoot), "com.nativelibtool." + SanitizeNamespacePart(artifactId))
                : configuredNamespace.Trim();

            if (!NamespaceRegex.IsMatch(namespaceName))
            {
                throw new InvalidOperationException("Namespace must be a valid Java package name: " + namespaceName);
            }

            return namespaceName;
        }

        private static string ReadManifestPackage(string sourceRoot)
        {
            var manifest = FirstExistingFile(
                Path.Combine(sourceRoot, "src", "main", "AndroidManifest.xml"),
                Path.Combine(sourceRoot, "AndroidManifest.xml"));
            if (string.IsNullOrWhiteSpace(manifest))
            {
                return string.Empty;
            }

            var match = ManifestPackageRegex.Match(File.ReadAllText(manifest));
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private static string ReadFirstJavaPackage(string sourceRoot)
        {
            var javaFile = Directory.GetFiles(sourceRoot, "*.java", SearchOption.AllDirectories)
                .Where(file => !ShouldSkipDiscoveryPath(file))
                .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
            if (string.IsNullOrWhiteSpace(javaFile))
            {
                return string.Empty;
            }

            var match = JavaPackageRegex.Match(File.ReadAllText(javaFile));
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private static void CopyFirstExistingDirectory(string targetParent, string targetName, params string[] candidates)
        {
            var source = FirstExistingDirectory(candidates);
            if (!string.IsNullOrWhiteSpace(source))
            {
                FileCopyUtility.CopyDirectory(source, Path.Combine(targetParent, targetName), ShouldSkipCopyPath);
            }
        }

        private static string FirstExistingDirectory(params string[] candidates)
        {
            return candidates.FirstOrDefault(Directory.Exists) ?? string.Empty;
        }

        private static string FirstExistingFile(params string[] candidates)
        {
            return candidates.FirstOrDefault(File.Exists) ?? string.Empty;
        }

        private static string FirstNonBlank(params string[] values)
        {
            return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
        }

        private static void CopyMatchingFiles(string sourceRoot, string targetRoot, string[] patterns, Func<string, bool> shouldSkip)
        {
            if (!Directory.Exists(sourceRoot))
            {
                return;
            }

            foreach (var pattern in patterns)
            {
                foreach (var file in Directory.GetFiles(sourceRoot, pattern, SearchOption.AllDirectories))
                {
                    if (shouldSkip != null && shouldSkip(file))
                    {
                        continue;
                    }

                    var relative = FileCopyUtility.GetRelativePath(sourceRoot, file);
                    var target = Path.Combine(targetRoot, relative);
                    FileCopyUtility.EnsureDirectory(Path.GetDirectoryName(target));
                    File.Copy(file, target, true);
                }
            }
        }

        private static int CountMatchingFiles(string root, string[] patterns, Func<string, bool> shouldSkip = null)
        {
            if (!Directory.Exists(root))
            {
                return 0;
            }

            return patterns.Sum(pattern => Directory
                .GetFiles(root, pattern, SearchOption.AllDirectories)
                .Count(file => shouldSkip == null || !shouldSkip(file)));
        }

        private static bool ShouldSkipCopyPath(string path)
        {
            return FileCopyUtility.IsUnityMetaPath(path) || ShouldSkipDiscoveryPath(path);
        }

        private static bool ShouldSkipDiscoveryPath(string path)
        {
            var normalized = FileCopyUtility.ToUnixPath(path);
            return normalized.IndexOf("/.git/", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   normalized.IndexOf("/.gradle/", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   normalized.IndexOf("/build/", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   normalized.IndexOf("/bin/", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   normalized.IndexOf("/obj/", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   FileCopyUtility.IsUnityMetaPath(path);
        }

        private static bool ShouldSkipDiscoveryName(string name)
        {
            return string.Equals(name, ".git", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(name, ".gradle", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(name, "build", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(name, "bin", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(name, "obj", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(name, "Library", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(name, "Temp", StringComparison.OrdinalIgnoreCase);
        }

        private static string EscapeGradleSingleQuoted(string value)
        {
            return (value ?? string.Empty).Replace("\\", "\\\\").Replace("'", "\\'");
        }

        private static string QuoteForCmd(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "gradle";
            }

            return value.IndexOfAny(new[] { ' ', '\t', '&', '(', ')' }) >= 0 ? "\"" + value + "\"" : value;
        }

        private static string SanitizeNamespacePart(string value)
        {
            var sanitized = Regex.Replace((value ?? string.Empty).ToLowerInvariant(), "[^a-z0-9_]+", "_").Trim('_');
            if (string.IsNullOrWhiteSpace(sanitized))
            {
                sanitized = "native_lib";
            }

            if (!char.IsLetter(sanitized[0]) && sanitized[0] != '_')
            {
                sanitized = "lib_" + sanitized;
            }

            return sanitized;
        }

        private static string SanitizeCMakeLibraryName(string value)
        {
            var sanitized = Regex.Replace((value ?? string.Empty).ToLowerInvariant(), "[^a-z0-9_]+", "_").Trim('_');
            return string.IsNullOrWhiteSpace(sanitized) ? "native_lib" : sanitized;
        }
    }
}
