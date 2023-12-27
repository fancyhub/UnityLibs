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
using System.IO.Compression;

namespace FH.VFSManagement.Builder
{
    [CreateAssetMenu(fileName = "VfsBuilderConfig", menuName = "fanchhub/VfsBuilderConfig")]
    [Serializable]
    public sealed class BuilderConfig : ScriptableObject
    {
        public enum EFormat
        {
            Lz4ZipStore,
            Lz4ZipCompress,
            ZipStore,
            ZipCompress,
        }

        [Serializable]
        public sealed class ZipItem
        {
            public string Name;
            public EFormat Format;
            [Header("eg: base;tag_a;tag_b ")]
            public string Tags = "";
            public List<DirItem> Dirs = new List<DirItem>();

#if UNITY_EDITOR
            public List<(string reletive_file_path, FileInfo file_info)> EdGetAllFiles()
            {
                List<(string reletive_file_path, FileInfo file_info)> ret = new List<(string reletive_file_path, FileInfo file_info)>();
                foreach (var p in Dirs)
                    p.EdGetAllFiles(ret);
                return ret;
            }

            public string EdBuildZip(string out_dir)
            {
                string path = System.IO.Path.Combine(out_dir, Name);
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);

                switch (Format)
                {
                    case EFormat.Lz4ZipStore:
                    case EFormat.Lz4ZipCompress:
                        {
                            var all_files = EdGetAllFiles();
                            Lz4ZipFile.CreateZipFile(all_files, path, Format == EFormat.Lz4ZipCompress);
                            return path;
                        }

                    case EFormat.ZipStore:
                    case EFormat.ZipCompress:
                        {
                            List<(string reletive_file_path, FileInfo file_info)> all_files = EdGetAllFiles();
                            if (all_files.Count == 0)
                            {
                                UnityEngine.Debug.LogWarning($"Build Zip, File Is Empty {Name}");
                            }

                            System.IO.Compression.CompressionLevel lvl = System.IO.Compression.CompressionLevel.Optimal;
                            if (Format == EFormat.ZipStore)
                            {
                                //这个好像无效
                                lvl = System.IO.Compression.CompressionLevel.NoCompression;
                            }

                            var zipArchive = ZipFile.Open(path, ZipArchiveMode.Create);
                            foreach (var f in all_files)
                            {
                                zipArchive.CreateEntryFromFile(f.file_info.FullName, f.reletive_file_path, lvl);                                
                            }
                            zipArchive.Dispose();
                            return path;
                        }
                }
                return null;
            }
#endif
        }

        [Serializable]
        public sealed class DirItem
        {
            /// <summary>
            /// 就是方便显示用的
            /// </summary>
            public string Name;

            /// <summary>
            /// 根目录
            /// </summary>
            public string RootDir;

            /// <summary>
            /// 根目录的子目录, 不能变成RootDir的父目录
            /// </summary>
            public string SpecSubDir = "./";


            public bool IncludeSub = true;

            [Header("eg: *.txt;*.json")]
            public string SearchPatterns;

#if UNITY_EDITOR
            public void EdGetAllFiles(List<(string reletive_file_path, FileInfo file_info)> out_list)
            {
                //1. format paths
                string root_full_dir = Path.GetFullPath(RootDir);
                string spec_sub_dir = Path.GetFullPath(Path.Combine(root_full_dir, SpecSubDir));

                //2. check Dir exist
                if (!Directory.Exists(spec_sub_dir))
                {
                    Debug.LogError($"RootDir: {RootDir}, SpecSubDir: {SpecSubDir}, SpecSubFullDir: {spec_sub_dir} 不存在");
                    return;
                }

                //3. get all file infos
                DirectoryInfo dirInfo = new DirectoryInfo(spec_sub_dir);
                Dictionary<string, FileInfo> dict = new Dictionary<string, FileInfo>();
                root_full_dir = root_full_dir.Replace('\\', '/');
                if (!root_full_dir.EndsWith('/'))
                    root_full_dir += "/";

                //4. format reletive path
                foreach (var p in SearchPatterns.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    string search_pattern = p.Trim();
                    if (string.IsNullOrEmpty(search_pattern))
                        continue;
                    FileInfo[] files = dirInfo.GetFiles(search_pattern, IncludeSub ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                    foreach (var file_info in files)
                    {
                        string file_full_name = file_info.FullName;
                        file_full_name = file_full_name.Replace('\\', '/');

                        if (!file_full_name.StartsWith(root_full_dir))
                            continue;

                        string file_name = file_full_name.Substring(root_full_dir.Length);
                        dict[file_name] = file_info;
                    }
                }


                //5. Merg
                foreach (var p in dict)
                {
                    out_list.Add((p.Key, p.Value));
                }
            }

            public List<(string reletive_file_path, FileInfo file_info)> EdGetAllFiles()
            {
                List<(string reletive_path, FileInfo file_info)> ret = new List<(string reletive_path, FileInfo file_info)>();
                EdGetAllFiles(ret);

                return ret;
            }
#endif
        }

        public List<ZipItem> Items = new List<ZipItem>();
    }
}
