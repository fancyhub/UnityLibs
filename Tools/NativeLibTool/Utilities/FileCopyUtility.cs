using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NativeLibTool.Utilities
{
    internal static class FileCopyUtility
    {
        public static void EnsureCleanDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);
        }

        public static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static void CopyDirectory(string source, string target, Func<string, bool> shouldSkip = null)
        {
            if (!Directory.Exists(source))
            {
                return;
            }

            EnsureDirectory(target);

            foreach (var directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            {
                if (shouldSkip != null && shouldSkip(directory))
                {
                    continue;
                }

                var relative = GetRelativePath(source, directory);
                EnsureDirectory(Path.Combine(target, relative));
            }

            foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                if (shouldSkip != null && shouldSkip(file))
                {
                    continue;
                }

                var relative = GetRelativePath(source, file);
                var destination = Path.Combine(target, relative);
                EnsureDirectory(Path.GetDirectoryName(destination));
                File.Copy(file, destination, true);
            }
        }

        public static IEnumerable<string> EnumerateFilesSafe(string root, string pattern, SearchOption option)
        {
            if (!Directory.Exists(root))
            {
                return Enumerable.Empty<string>();
            }

            return Directory.GetFiles(root, pattern, option);
        }

        public static string GetRelativePath(string root, string path)
        {
            var rootUri = new Uri(AppendDirectorySeparator(Path.GetFullPath(root)));
            var pathUri = new Uri(Path.GetFullPath(path));
            return Uri.UnescapeDataString(rootUri.MakeRelativeUri(pathUri).ToString())
                .Replace('/', Path.DirectorySeparatorChar);
        }

        public static string ToUnixPath(string path)
        {
            return path.Replace('\\', '/');
        }

        public static string AppendDirectorySeparator(string path)
        {
            if (path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ||
                path.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                return path;
            }

            return path + Path.DirectorySeparatorChar;
        }

        public static bool IsUnityMetaPath(string path)
        {
            return path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase);
        }

        public static string XmlEscape(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }

        public static string RubySingleQuoteEscape(string value)
        {
            return (value ?? string.Empty).Replace("\\", "\\\\").Replace("'", "\\'");
        }

        public static void WriteChecksumFiles(string file)
        {
            WriteHash(file, SHA1.Create(), ".sha1");
            WriteHash(file, MD5.Create(), ".md5");
        }

        private static void WriteHash(string file, HashAlgorithm algorithm, string extension)
        {
            using (algorithm)
            using (var stream = File.OpenRead(file))
            {
                var hash = algorithm.ComputeHash(stream);
                var builder = new StringBuilder(hash.Length * 2);
                foreach (var b in hash)
                {
                    builder.Append(b.ToString("x2"));
                }

                File.WriteAllText(file + extension, builder.ToString(), Encoding.ASCII);
            }
        }

        public static bool AreFilesEqual(string left, string right)
        {
            var leftInfo = new FileInfo(left);
            var rightInfo = new FileInfo(right);
            if (leftInfo.Length != rightInfo.Length)
            {
                return false;
            }

            using (var leftStream = File.OpenRead(left))
            using (var rightStream = File.OpenRead(right))
            using (var sha1 = SHA1.Create())
            {
                var leftHash = sha1.ComputeHash(leftStream);
                rightStream.Position = 0;
                var rightHash = sha1.ComputeHash(rightStream);
                return leftHash.SequenceEqual(rightHash);
            }
        }
    }
}
