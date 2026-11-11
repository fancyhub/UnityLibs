/*************************************************************************************
 * Author  : cunyu.fan
 * Title   : AssetDatabase dependency cache
 * Desc    : 独立的 Editor 依赖缓存工具，不参与打包流程。
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace FH.AssetBundleBuilder.Ed
{
    /// <summary>
    /// AssetDatabase.GetDependencies 的持久化缓存。
    /// 这里只缓存 Unity 返回的直接依赖，不额外过滤资源。
    /// </summary>
    public sealed class AssetDependencyCache
    {
        private const int CacheVersion = 1;
        private const string DefaultCachePath =
            "Library/FancyHub/AssetBundleBuilder/asset_dependency_cache.bytes";

        private sealed class Record
        {
            public string DependencyHash;
            public string[] Dependencies;
        }

        private readonly string _cachePath;
        private readonly Dictionary<string, Record> _records =
            new Dictionary<string, Record>(StringComparer.Ordinal);
        private bool _dirty;

        private AssetDependencyCache(string cachePath)
        {
            _cachePath = Path.GetFullPath(cachePath.Replace('/', Path.DirectorySeparatorChar));
        }

        /// <summary>
        /// 创建依赖缓存。
        /// 依赖会在 GetDependencies 调用时按需搜集。
        /// </summary>
        public static AssetDependencyCache Create(string cachePath = DefaultCachePath)
        {
            var cache = new AssetDependencyCache(cachePath);
            cache.Load();
            return cache;
        }

        /// <summary>
        /// 保存缓存。
        /// </summary>
        public void Save()
        {
            if (!_dirty)
                return;

            var directory = Path.GetDirectoryName(_cachePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var tempPath = _cachePath + ".tmp";
            try
            {
                using (var stream = File.Create(tempPath))
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(CacheVersion);
                    writer.Write(_records.Count);

                    foreach (var pair in _records.OrderBy(pair => pair.Key, StringComparer.Ordinal))
                    {
                        writer.Write(pair.Key);
                        writer.Write(pair.Value.DependencyHash ?? string.Empty);

                        var dependencies = pair.Value.Dependencies ?? Array.Empty<string>();
                        writer.Write(dependencies.Length);
                        foreach (var dependency in dependencies)
                            writer.Write(dependency ?? string.Empty);
                    }
                }

                if (File.Exists(_cachePath))
                    File.Delete(_cachePath);
                File.Move(tempPath, _cachePath);
                _dirty = false;
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        /// <summary>
        /// 获取资源依赖。
        /// 首次查询或依赖 Hash 发生变化时，调用 Unity 获取依赖并更新内存缓存。
        /// </summary>
        public string[] GetDependencies(string assetPath)
        {
            assetPath = NormalizeAssetPath(assetPath);
            if (string.IsNullOrEmpty(assetPath))
                return Array.Empty<string>();

            var dependencyHash = AssetDatabase.GetAssetDependencyHash(assetPath).ToString();
            if (_records.TryGetValue(assetPath, out var record)
                && string.Equals(record.DependencyHash, dependencyHash, StringComparison.Ordinal))
            {
                return record.Dependencies ?? Array.Empty<string>();
            }

            record = new Record
            {
                DependencyHash = dependencyHash,
                Dependencies = AssetDatabase.GetDependencies(assetPath, false)
                    .Select(NormalizeAssetPath)
                    .ToArray()
            };
            _records[assetPath] = record;
            _dirty = true;
            return record.Dependencies;
        }         

        private void Load()
        {
            if (!File.Exists(_cachePath))
                return;

            try
            {
                using (var stream = File.OpenRead(_cachePath))
                using (var reader = new BinaryReader(stream))
                {
                    if (reader.ReadInt32() != CacheVersion)
                        return;

                    var count = reader.ReadInt32();
                    for (var i = 0; i < count; i++)
                    {
                        var path = NormalizeAssetPath(reader.ReadString());
                        var record = new Record
                        {
                            DependencyHash = reader.ReadString(),
                            Dependencies = new string[reader.ReadInt32()]
                        };

                        for (var j = 0; j < record.Dependencies.Length; j++)
                            record.Dependencies[j] = NormalizeAssetPath(reader.ReadString());

                        _records[path] = record;
                    }
                }
                _dirty = false;
            }
            catch (Exception exception)
            {
                _records.Clear();
                UnityEngine.Debug.LogWarning(
                    $"Load asset dependency cache failed: {_cachePath}\n{exception.Message}");
            }
        }

        private static string NormalizeAssetPath(string path)
        {
            return string.IsNullOrEmpty(path) ? string.Empty : path.Replace('\\', '/');
        }
    }
}
