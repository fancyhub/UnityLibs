using System;
using System.IO;
using NativeLibTool.Models;

namespace NativeLibTool.Services
{
    internal static class ToolCacheService
    {
        private const string DirectoryName = "NativeLibTool";
        private const string CacheFileName = "NativeLibTool.cache.json";

        public static ToolCache Load(out string cachePath)
        {
            cachePath = ResolveCachePath();
            try
            {
                return JsonFile.Read<ToolCache>(cachePath) ?? new ToolCache();
            }
            catch
            {
                return new ToolCache();
            }
        }

        public static void Save(string cachePath, ToolCache cache)
        {
            JsonFile.Write(cachePath, cache ?? new ToolCache());
        }

        private static string ResolveCachePath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, DirectoryName, CacheFileName);
        }
    }
}
