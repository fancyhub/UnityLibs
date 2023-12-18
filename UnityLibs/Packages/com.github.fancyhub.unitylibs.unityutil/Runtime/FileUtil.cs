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
