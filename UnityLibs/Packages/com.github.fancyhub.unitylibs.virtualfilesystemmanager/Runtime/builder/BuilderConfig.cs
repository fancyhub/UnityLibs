/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/15
 * Title   : 
 * Desc    : 
*************************************************************************************/


using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FH.VFSManagement.Builder
{
    public enum EDestFormat
    {
        Zip,
        Lz4ZipStore,
        Lz4ZipCompress,
    }


    [Serializable]
    public sealed class BuilderConfig
    {
        public string Name;
        public EDestFormat Format;

        public List<InputDir> Dirs = new List<InputDir>();

        [Serializable]
        public class InputDir
        {
            /// <summary>
            /// 根目录
            /// </summary>
            public string RootDir;

            /// <summary>
            /// 根目录的子目录, 不能变成RootDir的父目录
            /// </summary>
            public string SpecSubDir = "./";

            public bool IncludeSub = true;

            [Header("eg: *.txt;*.json;*.lua;*.csv;")]
            public string SearchPartern;

            public List<(string reletive_file_path, FileInfo file_info)> GetAllFiles()
            {
                List<(string reletive_path, FileInfo file_info)> ret = new List<(string reletive_path, FileInfo file_info)>();

                //1. format paths
                string root_full_dir = Path.GetFullPath(RootDir);
                string spec_sub_dir = Path.GetFullPath(Path.Combine(root_full_dir, SpecSubDir));

                //2. check Dir exist
                if (!Directory.Exists(spec_sub_dir))
                {
                    Debug.LogError($"RootDir: {RootDir}, SpecSubDir: {SpecSubDir}, SpecSubFullDir: {spec_sub_dir} 不存在");
                    return ret;
                }

                //3. get all file infos
                DirectoryInfo dirInfo = new DirectoryInfo(spec_sub_dir);
                FileInfo[] files = dirInfo.GetFiles(SearchPartern, IncludeSub ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

                //4. format reletive path
                root_full_dir = root_full_dir.Replace('\\', '/');
                if (!root_full_dir.EndsWith('/'))
                    root_full_dir += "/";
                foreach (var file_info in files)
                {
                    string file_full_name = file_info.FullName;
                    file_full_name = file_full_name.Replace('\\', '/');

                    if (!file_full_name.StartsWith(root_full_dir))
                        continue;

                    string file_name = file_full_name.Substring(root_full_dir.Length);

                    ret.Add((file_name, file_info));
                }

                return ret;
            }
        }
    }
}
