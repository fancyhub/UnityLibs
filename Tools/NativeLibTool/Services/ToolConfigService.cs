using System;
using System.IO;
using NativeLibTool.Models;

namespace NativeLibTool.Services
{
    internal static class ToolConfigService
    {
        private const string ConfigFileName = "NativeLibTool.config.json";

        public static ToolConfig LoadOrCreate(out string configPath)
        {
            configPath = ResolveConfigPath();
            if (!File.Exists(configPath))
            {
                var created = CreateDefault();
                Save(configPath, created);
                return created;
            }

            try
            {
                var config = JsonFile.Read<ToolConfig>(configPath);
                return Normalize(config ?? CreateDefault());
            }
            catch
            {
                var fallback = CreateDefault();
                Save(configPath, fallback);
                return fallback;
            }
        }

        public static void Save(string configPath, ToolConfig config)
        {
            JsonFile.Write(configPath, Normalize(config ?? CreateDefault()));
        }

        private static string ResolveConfigPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
        }

        private static ToolConfig CreateDefault()
        {
            return new ToolConfig
            {
                UnityProjectRoot = "",
                AndroidAarPath = "",
                AndroidSourceDirectory = "",
                AndroidAarOutputDirectory = "",
                AndroidSourceProjectDirectory = "",
                AndroidSourceGradleProjectDirectory = "",
                AndroidSourceAarOutputDirectory = "",
                AndroidGradleCommand = "gradle",
                AndroidGradlePluginVersion = "7.4.2",
                AndroidCompileSdk = "35",
                AndroidMinSdk = "21",
                AndroidNamespace = "",
                AndroidUnityDataDirectory = "",
                AndroidGroupId = "com.garena.crashsight",
                AndroidArtifactId = "crashsight",
                AndroidVersion = "1.0.0",
                IosSourceDirectory = "",
                IosPodName = "CrashSightCN",
                IosVersion = "1.0.0",
                IosMinimumVersion = "12.0",
                IosSystemFrameworks = "Foundation, UIKit, SystemConfiguration",
                IosSystemLibraries = "z, c++",
                IosPodDependencies = "",
                IosSummaryFormat = "{PodName} native SDK",
                IosHomepageFormat = "https://internal.local/{PodName}",
                IosLicenseType = "Proprietary",
                IosAuthorName = "Company",
                IosAuthorEmail = "dev@company.local",
                IosStaticFramework = false,
                IosGenerateVersionDirectory = true
            };
        }

        private static ToolConfig Normalize(ToolConfig config)
        {
            var defaults = CreateDefault();
            config.UnityProjectRoot = DefaultIfBlank(config.UnityProjectRoot, defaults.UnityProjectRoot);
            config.AndroidAarPath = DefaultIfBlank(config.AndroidAarPath, defaults.AndroidAarPath);
            config.AndroidSourceDirectory = DefaultIfBlank(config.AndroidSourceDirectory, defaults.AndroidSourceDirectory);
            config.AndroidAarOutputDirectory = DefaultIfBlank(config.AndroidAarOutputDirectory, defaults.AndroidAarOutputDirectory);
            config.AndroidSourceProjectDirectory = DefaultIfBlank(config.AndroidSourceProjectDirectory, defaults.AndroidSourceProjectDirectory);
            config.AndroidSourceGradleProjectDirectory = DefaultIfBlank(config.AndroidSourceGradleProjectDirectory, defaults.AndroidSourceGradleProjectDirectory);
            config.AndroidSourceAarOutputDirectory = DefaultIfBlank(config.AndroidSourceAarOutputDirectory, defaults.AndroidSourceAarOutputDirectory);
            config.AndroidGradleCommand = DefaultIfBlank(config.AndroidGradleCommand, defaults.AndroidGradleCommand);
            config.AndroidGradlePluginVersion = DefaultIfBlank(config.AndroidGradlePluginVersion, defaults.AndroidGradlePluginVersion);
            config.AndroidCompileSdk = DefaultIfBlank(config.AndroidCompileSdk, defaults.AndroidCompileSdk);
            config.AndroidMinSdk = DefaultIfBlank(config.AndroidMinSdk, defaults.AndroidMinSdk);
            config.AndroidNamespace = DefaultIfBlank(config.AndroidNamespace, defaults.AndroidNamespace);
            config.AndroidUnityDataDirectory = DefaultIfBlank(config.AndroidUnityDataDirectory, defaults.AndroidUnityDataDirectory);
            config.AndroidGroupId = DefaultIfBlank(config.AndroidGroupId, defaults.AndroidGroupId);
            config.AndroidArtifactId = DefaultIfBlank(config.AndroidArtifactId, defaults.AndroidArtifactId);
            config.AndroidVersion = DefaultIfBlank(config.AndroidVersion, defaults.AndroidVersion);
            config.IosSourceDirectory = DefaultIfBlank(config.IosSourceDirectory, defaults.IosSourceDirectory);
            config.IosPodName = DefaultIfBlank(config.IosPodName, defaults.IosPodName);
            config.IosVersion = DefaultIfBlank(config.IosVersion, defaults.IosVersion);
            config.IosMinimumVersion = DefaultIfBlank(config.IosMinimumVersion, defaults.IosMinimumVersion);
            config.IosSystemFrameworks = DefaultIfBlank(config.IosSystemFrameworks, defaults.IosSystemFrameworks);
            config.IosSystemLibraries = DefaultIfBlank(config.IosSystemLibraries, defaults.IosSystemLibraries);
            config.IosPodDependencies = DefaultIfBlank(config.IosPodDependencies, defaults.IosPodDependencies);
            config.IosSummaryFormat = DefaultIfBlank(config.IosSummaryFormat, defaults.IosSummaryFormat);
            config.IosHomepageFormat = DefaultIfBlank(config.IosHomepageFormat, defaults.IosHomepageFormat);
            config.IosLicenseType = DefaultIfBlank(config.IosLicenseType, defaults.IosLicenseType);
            config.IosAuthorName = DefaultIfBlank(config.IosAuthorName, defaults.IosAuthorName);
            config.IosAuthorEmail = DefaultIfBlank(config.IosAuthorEmail, defaults.IosAuthorEmail);
            return config;
        }

        private static string DefaultIfBlank(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

    }
}
