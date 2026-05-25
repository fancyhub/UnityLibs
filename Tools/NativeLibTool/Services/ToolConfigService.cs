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
                AndroidSourceDirectory = "",
                AndroidGroupId = "com.garena.crashsight",
                AndroidArtifactId = "crashsight",
                AndroidVersion = "1.0.0",
                IosSourceDirectory = "",
                IosPodName = "CrashSightCN",
                IosVersion = "1.0.0",
                IosMinimumVersion = "12.0",
                IosSystemFrameworks = "Foundation, UIKit, SystemConfiguration",
                IosSystemLibraries = "z, c++",
                IosSummaryFormat = "{PodName} native SDK",
                IosHomepageFormat = "https://internal.local/{PodName}",
                IosLicenseType = "Proprietary",
                IosAuthorName = "Company",
                IosAuthorEmail = "dev@company.local",
                IosStaticFramework = false,
                IosGenerateVersionDirectory = true,
                PodfilePatchDefine = "UNITY_IOS"
            };
        }

        private static ToolConfig Normalize(ToolConfig config)
        {
            var defaults = CreateDefault();
            config.UnityProjectRoot = DefaultIfBlank(config.UnityProjectRoot, defaults.UnityProjectRoot);
            config.AndroidSourceDirectory = DefaultIfBlank(config.AndroidSourceDirectory, defaults.AndroidSourceDirectory);
            config.AndroidGroupId = DefaultIfBlank(config.AndroidGroupId, defaults.AndroidGroupId);
            config.AndroidArtifactId = DefaultIfBlank(config.AndroidArtifactId, defaults.AndroidArtifactId);
            config.AndroidVersion = DefaultIfBlank(config.AndroidVersion, defaults.AndroidVersion);
            config.IosSourceDirectory = DefaultIfBlank(config.IosSourceDirectory, defaults.IosSourceDirectory);
            config.IosPodName = DefaultIfBlank(config.IosPodName, defaults.IosPodName);
            config.IosVersion = DefaultIfBlank(config.IosVersion, defaults.IosVersion);
            config.IosMinimumVersion = DefaultIfBlank(config.IosMinimumVersion, defaults.IosMinimumVersion);
            config.IosSystemFrameworks = DefaultIfBlank(config.IosSystemFrameworks, defaults.IosSystemFrameworks);
            config.IosSystemLibraries = DefaultIfBlank(config.IosSystemLibraries, defaults.IosSystemLibraries);
            config.IosSummaryFormat = DefaultIfBlank(config.IosSummaryFormat, defaults.IosSummaryFormat);
            config.IosHomepageFormat = DefaultIfBlank(config.IosHomepageFormat, defaults.IosHomepageFormat);
            config.IosLicenseType = DefaultIfBlank(config.IosLicenseType, defaults.IosLicenseType);
            config.IosAuthorName = DefaultIfBlank(config.IosAuthorName, defaults.IosAuthorName);
            config.IosAuthorEmail = DefaultIfBlank(config.IosAuthorEmail, defaults.IosAuthorEmail);
            config.PodfilePatchDefine = DefaultIfBlank(config.PodfilePatchDefine, defaults.PodfilePatchDefine);
            return config;
        }

        private static string DefaultIfBlank(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

    }
}
