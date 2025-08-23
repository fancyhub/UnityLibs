using System;
using System.Collections.Generic;
using System.IO;

namespace FH
{
    public static class FileUtil
    {
        public static void CreateFileDir(string file_path)
        {
            string full_path = Path.GetFullPath(file_path);
            _CreateDir(Path.GetDirectoryName(full_path));
        }

        public static void CreateDir(string folder_path)
        {
            _CreateDir(Path.GetFullPath(folder_path));
        }

        public static string FullAssetPath2ResourcePath(string full_asset_path, string defaultValue = null)
        {
            int index = full_asset_path.LastIndexOf("/Resources/");
            if (index < 0)
                return defaultValue;

            index = index + "/Resources/".Length;
            var ext = System.IO.Path.GetExtension(full_asset_path);
            int endIndex = full_asset_path.Length - ext.Length;
            return full_asset_path.Substring(index, endIndex - index);
        }

        private static void _CreateDir(string folder_path)
        {
            if (Directory.Exists(folder_path))
                return;

            string parent_folder_path = Path.GetDirectoryName(folder_path);
            _CreateDir(parent_folder_path);
            Directory.CreateDirectory(folder_path);
        }
    }
}
