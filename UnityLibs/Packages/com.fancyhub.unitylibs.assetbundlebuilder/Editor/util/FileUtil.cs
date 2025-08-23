/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/12
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System.IO;

namespace FH.AssetBundleBuilder.Ed
{
    internal static class FileUtil
    {
        internal static void CreateFileDir(string file_path)
        {
            string full_path = Path.GetFullPath(file_path);
            _CreateDir(Path.GetDirectoryName(full_path));
        }

        internal static void CreateDir(string folder_path)
        {
            _CreateDir(Path.GetFullPath(folder_path));
        }

        private static void _CreateDir(string folder_path)
        {
            if (Directory.Exists(folder_path))
                return;

            string parent_folder_path = Path.GetDirectoryName(folder_path);
            _CreateDir(parent_folder_path);
            Directory.CreateDirectory(folder_path);
        }

        internal unsafe static string CalcFileHash(string file_path)
        {
            var hash = new UnityEngine.Hash128();
            UnityEngine.Hash128* hash2 = (UnityEngine.Hash128*)Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AddressOf(ref hash);

            byte* buff = stackalloc byte[1024];
            System.Span<byte> span_buff = new System.Span<byte>(buff, 1024);

            using var fs = System.IO.File.OpenRead(file_path);
            for (; ; )
            {
                int read = fs.Read(span_buff);
                if (read <= 0)
                    break;
                UnityEngine.HashUnsafeUtilities.ComputeHash128(buff, (ulong)read, hash2);
            }


            return hash.ToString();
        }
    }
}
